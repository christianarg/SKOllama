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
    public static async Task RunAgent()
    {
        ChatHistory chatHistory = new();
        while (true)
        {
            Console.Write("User>");
            var userInput = Console.ReadLine()!;
            chatHistory.AddUserMessage(userInput);
            var result = await ExecuteAgent(chatHistory);
            Console.Write("Assistant> ");
            Console.WriteLine(result.ToString());
        }
    }

    public static async Task<string> ExecuteAgent(ChatHistory chatHistory)
    {
        var kernel = KernelFactory.DefaultKernel();
        kernel.Plugins.AddFromType<SubscriptionQueryExecutor>();

        SubscriptionAgentHelper.DescribeDatabase();

        ChatCompletionAgent agent = new()
        {
            Name = "SubscriptionsAgent",
            Instructions = $"""
                You're an expect SQL agent. You can execute SQL queries against the subscriptions database.
                Use the `execute_select_query` function to generate and execute SQL queries. Return the results to the user.
                The database contains the following tables: Subscriptions, CatalogProducts, Skus.
                This is the Entity Framework model of the database:
                {SubscriptionAgentHelper.DescribeDatabase()}
                """,
            Kernel = kernel,
            Arguments = new(new GeminiPromptExecutionSettings()
            {
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
                //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            })
        };
        StringBuilder result = new();
        ChatHistoryAgentThread agentThread = new(chatHistory);
        await foreach (var message in agent.InvokeAsync(agentThread))
        {
            result.AppendLine(message.Message.ToString());
        }
        return result.ToString();
    }
}

public class SubscriptionQueryExecutor
{
    [KernelFunction("execute_select_query"), Description("Execute a SELECT query against the subscriptions database. The tables include Subscriptions, CatalogProducts, and Skus.")]
    public async Task<object> ExecuteQuery([Description("The SQL SELECT query. Must be valid SQL for SQLite")] string sqlQuery)
    {
        Console.WriteLine($"Executing query: {sqlQuery}");
        try
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=subscriptions.db");
            await connection.OpenAsync();

            // Dapper returns IEnumerable<dynamic>, convert to List<Dictionary<string, object>>
            var result = await connection.QueryAsync(sqlQuery);

            // generated code, have to check if it's needed
            // var list = new List<Dictionary<string, object>>();
            // foreach (var row in result)
            // {
            //     var dict = new Dictionary<string, object>();
            //     foreach (var prop in ((IDictionary<string, object>)row))
            //     {
            //         dict[prop.Key] = prop.Value;
            //     }
            //     list.Add(dict);
            // }
            return result;
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
        var sb = new System.Text.StringBuilder();
        foreach (var entityType in model.GetEntityTypes())
        {
            sb.AppendLine($"Entity: {entityType.Name}");
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType.IsEnum)
                {
                    var enumValues = Enum.GetNames(property.ClrType);
                    sb.AppendLine($"  Property: {property.Name}, Type: {property.ClrType.Name}, Values: {string.Join(";", enumValues)}");
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