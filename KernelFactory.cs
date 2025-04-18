#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Microsoft.SemanticKernel;

public static class KernelFactory
{
    public static Kernel DefaultOllamaKernel()
    {
        return Kernel.CreateBuilder()
            .AddOllamaChatCompletion(modelId: "llama3.2", endpoint: new("http://localhost:11434"))
            .Build();
    }
}