#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable CS8321 // Local function is declared but never used

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

await BasicQALoop();

async Task BasicQALoop()
{
    var kernel = Kernel.CreateBuilder()
        .AddOllamaChatCompletion(modelId: "llama3.2", endpoint: new("http://localhost:11434"))
        .Build();

    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    ChatHistory chat = new();

    while(true)
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
    var kernel = Kernel.CreateBuilder()
        .AddOllamaChatCompletion(modelId: "llama3.2", endpoint: new("http://localhost:11434"))
        .Build();

    var result = await kernel.InvokePromptAsync("Hola crack");
    Console.WriteLine(result);

}

Console.ReadLine();