#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

/// <summary>
/// Delegating Agent using tools/function calling. Inspired on https://ai.pydantic.dev/multi-agent-applications/#agent-delegation
/// </summary>
public static class AgentDelegationSample
{
    public static async Task RunAsync()
    {
        var kernel = KernelFactory.DefaultOllamaKernel();
        kernel.Plugins.AddFromType<JokesAgent>();
        ChatCompletionAgent agent = new()
        {
            Name = "DelegationAgent",
            Instructions = "You're a delegation agent. You can delegate to other agents.",
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
}

public class JokesAgent
{
    [KernelFunction("get_jokes"), Description("Get response from Jokes agent")]
    public async Task<object> GetJokesAgentResponse(string input)
    {
        Console.WriteLine("Invoking Jokes agent...");
        var kernel = KernelFactory.DefaultOllamaKernel();

        ChatCompletionAgent agent = new()
        {
            Name = "JokesAgent",
            Instructions = "You tell jokes.",
            Kernel = kernel
        };
        // so ugly

        ChatHistoryAgentThread agentThread = new();
        var response = await agent.InvokeAsync(input, agentThread).ToListAsync();
        var simplifiedResponse = response.Select(x => new 
        {
            x.Message.AuthorName,
            Message = x.Message.ToString(),
        });
        return simplifiedResponse;
    }
}
