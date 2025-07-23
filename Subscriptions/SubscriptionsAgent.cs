#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using System.ComponentModel;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Newtonsoft.Json;

namespace Subscriptions;

public class SubscriptionsAgent
{
    /// <summary>
    /// Some prompts:
    /// - details of Office 365 E3 subscriptions => should return the details of the subscriptions
    /// - what's the price of Office 365 E1 => should ask for clarification
    /// - what are my annual subscriptions => should return the annual subscriptions. Ideally should clarify if the user means billing cycle or commitment, I could add some prompt to this. For now it query either one. The description of the returned query should help the user understand what the agent has done.
    /// - what are my unassigned licenses
    /// - cost of my subscriptions + total price I pay monthly for all my subscriptions + this doesn't seem to include annual subscriptions => I should improve prompt because it initially returned only monthly subscriptions.
    /// - what subscriptions will renew in the next 10 months
    /// - subscriptions that will renew on 15/03/2026
    /// </summary>
    /// <returns></returns>
    public static async Task RunAgent()
    {
        var subscriptionsAgent = SetupSubscriptionsAgent();
        ChatHistory chatHistory = new();

        while (true)
        {
            Console.Write("User>");
            var userInput = Console.ReadLine()!;
            chatHistory.AddUserMessage(userInput);
            var result = await ExecuteAgent(subscriptionsAgent, chatHistory);
            Console.Write("Assistant> ");
            Console.WriteLine(result.ToString());
        }
    }

    private static async Task<string> ExecuteAgent(ChatCompletionAgent agent, ChatHistory chatHistory)
    {
        StringBuilder result = new();
        ChatHistoryAgentThread agentThread = new(chatHistory);
        await foreach (var message in agent.InvokeAsync(agentThread))
        {
            result.AppendLine(message.Message.ToString());
        }
        return result.ToString();
    }

    private static ChatCompletionAgent SetupSubscriptionsAgent()
    {
        var kernel = KernelFactory.DefaultKernel();
        kernel.Plugins.AddFromType<SubscriptionQueryExecutor>();

        ChatCompletionAgent agent = new()
        {
            Name = "SubscriptionsAgent",
            Instructions = $"""
                Answer the user's questions about licenses and subscriptions.
                Use the `GenerateAndExecuteQuery` function to generate and execute SQL queries.
                The subscriptions database contains the following tables: 
                - Subscriptions. The subscription to licenses.
                - Skus. The stock keeping units for the license. A Sku can have multiple subscriptions. Note: Sku is an internal technical term. The user will most likely never use it. Normally the user will refer it as a license.
                - CatalogProducts. The complete product catalog the user can subscribe to.
                
                DO NOT return the SQL query to the user. 
                Understand the user's intent, generate SQL queries, and return the results.
                You must also show the query description to the user. This can help the user understand what you have done and refine their query if needed.
                
                You can generate and execute multiple multiple times to get the correct result. For example, if a query fails or the result is not what the user expected, you can try again with a different query.
                You can generate and execute multiple queries if needed to get the correct result.
                
                Since the terminology can be ambiguous, you should ask the user for clarification if needed, either before or after executing the query. For example, if the user asks "what is the price of Office 365 E3", it may refer to a subscription the user owns, or a product the user can subscribe to. In this case you should ask the user if they mean a subscription they own or license from the product catalog.
                
                This is the Entity Framework model of the database:
                {SubscriptionAgentHelper.DescribeDatabase()}
                
                For enum properties, the database stores the values as integers. For example, the BillingCycle property can have the values 0 (Monthly) or 1 (Annual). The Commitment property can have the values 0 (Monthly) or 1 (Annual).
                """,
            Kernel = kernel,
            Arguments = new(new GeminiPromptExecutionSettings()
            {
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.5,
                ThinkingConfig = new GeminiThinkingConfig
                {
                    ThinkingBudget = -1, // -1 means automatically determined by the model, 0 disabled, rest are the ammount of tokens the model can use to think before generating a response.
                },
            })
        };
        // note: instructions do not include restrictions. In a real app you would do this if this was a user facing agent. If this was a "sub-agent" you would proably not include restrictions as the parent agent would have already done this.
        return agent;
    }
    #region Alternative instructions for the agent separated in sections with headers
/*
Alternative instructions for the agent separated in sections with headers. Separation made by gpt4.1
In my experience I have had time defining sections as of example many or most are rules. 
I tested it a bit and I'm not conviced, I think I got worse results with sections.
While models can benefit from sections I think it's mostly useful for humans to read the instructions.
I will leave it here for now commented out.

# Objective
Answer the user's questions about licenses and subscriptions.

# Rules
- Use the `GenerateAndExecuteQuery` function to generate and execute SQL queries.
- DO NOT return the SQL query to the user.
- Understand the user's intent, generate SQL queries, and return the results.
- Return also the query description to help the user understand what you have done.
- You can try multiple times to get the correct result.
- Since the terminology can be ambiguous, you can ask the user for clarification if needed, either before or after executing the query.

# Database Information
The subscriptions database contains the following tables: 
- Subscriptions: The subscription to licenses.
- Skus: The stock keeping units for the license. A Sku can have multiple subscriptions. Note: Sku is an internal technical term. The user will most likely never use it. Normally the user will refer to it as a license.
- CatalogProducts: The complete product catalog the user can subscribe to.

# Entity Framework Model
{SubscriptionAgentHelper.DescribeDatabase()}

# Enum Properties
For enum properties, the database stores the values as integers. For example, the BillingCycle property can have the values 0 (Monthly) or 1 (Annual). The Commitment property can have the values 0 (Monthly) or 1 (Annual).

# Clarification Guidance
If the user's question is ambiguous (e.g., "what is the price of Office 365 E3"), clarify whether they mean a subscription they own or a product they can subscribe to.
""",
*/
    #endregion
}

public class SubscriptionQueryExecutor
{
    [KernelFunction, Description("Generates and Executes a SELECT query against the subscriptions database and returns the results. The tables include Subscriptions, CatalogProducts, and Skus.")]
    public async Task<object> GenerateAndExecuteQuery([Description("The SQL SELECT query. Must be valid SQL for SQLite")] string sqlQuery, [Description("The description of the generated query. Use this to reason about the generated query")]string queryDescription)
    {
        // Note: in a real application, you would validate the SQL ensure it's a select query and not a destructive query.
        Console.WriteLine($"Query Description: {queryDescription}");
        Console.WriteLine($"Executing query: {sqlQuery}");
        try
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=subscriptions.db");
            await connection.OpenAsync();

            var result = await connection.QueryAsync(sqlQuery);
            // Serialize with enum as string using JsonNet setting
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
            var serializedResult = JsonConvert.SerializeObject(result, settings);
            return $"""
                # Query Result
                {serializedResult}
                
                # Query Description
                {queryDescription}
                Remember to replace the term "Sku" with "License" in the results, as the user will not understand the technical term "Sku".
                """;
        }
        catch (Exception ex)
        {
            return ex.Message;  // Return the error message if the query fails to allow the agent to handle it.
        }
    }
}

public class SubscriptionAgentHelper
{
    public static string DescribeDatabase()
    {
        using var context = new SubscriptionsDbContext();
        var model = context.Model;
        var sb = new StringBuilder();
        foreach (var entityType in model.GetEntityTypes())
        {
            sb.AppendLine($"Entity: {entityType.Name}");
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType.IsEnum)
                {
                    var enumNames = Enum.GetNames(property.ClrType);
                    var enumValues = Enum.GetValues(property.ClrType);
                    var enumDict = new Dictionary<string, int>();
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        enumDict[enumNames[i]] = (int)enumValues.GetValue(i)!;
                    }

                    sb.AppendLine($"  Property: {property.Name}, Type: Enum, Values: {JsonConvert.SerializeObject(enumDict)}");
                }
                else
                {
                    sb.AppendLine($"  Property: {property.Name}, Type: {property.ClrType.Name}");
                }
            }
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                sb.AppendLine($"  Foreign Key: {string.Join(", ", foreignKey.Properties.Select(p => p.Name))} -> {foreignKey.PrincipalEntityType.Name}");
            }
        }
        return sb.ToString();
    }
}