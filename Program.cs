#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable CS8321 // Local function is declared but never used

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

await AgentDelegationSample.RunAsync();
//await BasicQALoopAgentFrameworkWithFunctions();
//await BasicQALoopAgentFramework();
//await BasicQALoopWithFunctions();
//await BasicQALoopChat();
//await SimplestSample();

Console.ReadLine();


async Task BasicQALoopAgentFrameworkWithFunctions()
{
    var kernel = DefaultOllamaKernel();
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
        KernelArguments = new(new PromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
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
    var kernel = DefaultOllamaKernel();
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
    var kernel = DefaultOllamaKernel();
    kernel.Plugins.AddFromType<DatePlugin>();
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    ChatHistory chat = new();

    PromptExecutionSettings promptExecutionSettings = new()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
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
    var kernel = DefaultOllamaKernel();

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
    var kernel = DefaultOllamaKernel();
    var result = await kernel.InvokePromptAsync("Hola crack");
    Console.WriteLine(result);

}

Kernel DefaultOllamaKernel() => KernelFactory.DefaultOllamaKernel();

public class DatePlugin
{

    [KernelFunction("get_date_time"), Description("Get the current date")]
    public string GetDateTime()
    {
        return DateTime.Now.ToString();
    }
}
