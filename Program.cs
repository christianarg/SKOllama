#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable CS8321 // Local function is declared but never used

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

await BasicQALootAgentFramework();
//await BasicQALoopChat();
//await SimplestSample();

async Task BasicQALootAgentFramework()
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
        var result = await agent.InvokeAsync(userInput, agentThread).ToListAsync();
        Console.WriteLine($"Assistant> {result.First().Message}");
    }
}

async Task BasicQALoopChat()
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

Kernel DefaultOllamaKernel()
{
    return Kernel.CreateBuilder()
        .AddOllamaChatCompletion(modelId: "llama3.2", endpoint: new("http://localhost:11434"))
        .Build();
}

Console.ReadLine();