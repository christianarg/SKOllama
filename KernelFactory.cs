#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Microsoft.SemanticKernel;

public static class KernelFactory
{
    public static Kernel DefaultKernel()
        => GeminiKernel();

    public static Kernel OllamaKernel()
    {
        return Kernel.CreateBuilder()
            //.AddOllamaChatCompletion(modelId: "llama3.1", endpoint: new("http://localhost:11434"))
            .AddOllamaChatCompletion(modelId: "llama3.2", endpoint: new("http://localhost:11434"))
            .Build();
    }

    public static Kernel GeminiKernel()
    {
        return Kernel.CreateBuilder()
            //.AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.0-flash", Environment.GetEnvironmentVariable("googleAiStudioApiKey")!)
            .AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", Environment.GetEnvironmentVariable("googleAiStudioApiKey")!)
            .Build();
    }
}