#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable CS8321 // Local function is declared but never used

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Subscriptions;

//SubscriptionsSeeder.InitDb();
//SubscriptinTestQueries.Run();
SubscriptinTestQueries.RunWithDapper();
//SubscriptinTestQueries.DescribeDatabase();

//await RunIntentDetection();
//await AgentDelegationSample.RunAsync();
//await BasicQALoopAgentFrameworkWithFunctions();
//await BasicQALoopAgentFramework();
//await BasicQALoopWithFunctions();
//await BasicQALoop();
//await SimplestSample();

Console.ReadLine();


async Task RunIntentDetection()
{
    ChatHistory chatHistory = new();
    while (true)
    {
        Console.Write("User>");
        var userInput = Console.ReadLine()!;
        chatHistory.AddUserMessage(userInput);
        var result = await IntentDetection.Execute(chatHistory);
        Console.WriteLine("Intent detected:" + result.Intent);
        Console.WriteLine("Rewritten query:" + result.Query);
    }
}

async Task BasicQALoopAgentFrameworkWithFunctions()
{
    var kernel = DefaultKernel();
    kernel.Plugins.AddFromType<DatePlugin>();
    ChatCompletionAgent agent = new()
    {
        Name = null,
        Instructions = null,
        Kernel = kernel
    };
    // so ugly
    AgentInvokeOptions agentInvokeOptions = new()
    {
        KernelArguments = new(new GeminiPromptExecutionSettings()
        {
            //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
        })
    };

    ChatHistoryAgentThread agentThread = new();

    while (true)
    {
        Console.Write("User>");
        var userInput = Console.ReadLine()!;
        bool responseStarted = false;
        await foreach (var message in agent.InvokeAsync(userInput, agentThread, agentInvokeOptions))
        {
            if (!responseStarted)
            {
                responseStarted = true;
                Console.WriteLine("Assistant>");
            }
            Console.WriteLine(message.Message);
        }
    }
}

async Task BasicQALoopAgentFramework()
{
    var kernel = DefaultKernel();
    ChatCompletionAgent agent = new()
    {
        Name = null,
        Instructions = null,
        Kernel = kernel
    };
    ChatHistoryAgentThread agentThread = new();
    while (true)
    {
        Console.Write("User>");
        var userInput = Console.ReadLine()!;
        bool responseStarted = false;
        await foreach (var message in agent.InvokeAsync(userInput, agentThread))
        {
            if (!responseStarted)
            {
                responseStarted = true;
                Console.WriteLine("Assistant>");
            }
            Console.WriteLine(message.Message);
        }
    }
}

async Task BasicQALoopWithFunctions()
{
    var kernel = DefaultKernel();
    kernel.Plugins.AddFromType<DatePlugin>();
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    ChatHistory chat = new("Your'e a helpful assistant that can answer questions. ONLY call functions if the user asks for the current date or time. Do not call functions for any other reason.");

    GeminiPromptExecutionSettings promptExecutionSettings = new()
    {
        //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
    };

    while (true)
    {
        Console.Write("User>");
        var userInput = Console.ReadLine()!;
        chat.AddUserMessage(userInput);
        var result = await chatService.GetChatMessageContentAsync(chat, executionSettings: promptExecutionSettings, kernel: kernel);
        Console.WriteLine($"Assistant> {result}");
        chat.AddAssistantMessage(result.ToString());
    }
}


async Task BasicQALoop()
{
    var kernel = DefaultKernel();

    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    ChatHistory chat = new();

    while (true)
    {
        Console.Write("User>");
        var userInput = Console.ReadLine()!;
        chat.AddUserMessage(userInput);
        var result = await chatService.GetChatMessageContentAsync(chat);
        Console.WriteLine($"Assistant> {result}");
        chat.AddAssistantMessage(result.ToString());
    }
}

async Task SimplestSample()
{
    var kernel = DefaultKernel();
    var result = await kernel.InvokePromptAsync("Hola crack");
    Console.WriteLine(result);

}

Kernel DefaultKernel() => KernelFactory.DefaultKernel();

public class DatePlugin
{

    [KernelFunction("get_date_time"), Description("Get the current date.")]
    public string GetDateTime()
    {
        return DateTime.Now.ToString();
    }
}
