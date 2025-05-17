#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Newtonsoft.Json;

public static class IntentDetection
{
    public static async Task<IntentDetectionResult> Execute(ChatHistory chatHistory)
    {
        var kernel = KernelFactory.DefaultKernel();

        ChatCompletionAgent agent = new()
        {
            Name = "IntentDetectionAgent",
            Instructions = """
                Your tasks are:
                    # Detect the intent of the user input.
                    Classify the input into one of the possible intents. 
                    # Rewrite query, if needed, so that it can be understood without the chat history.
                    If the user message can be understood without the chat history, leave it as is.
                    If the user message cannot be understood without the chat history, rewrite it so that it can be understood without the chat history.
                """,
            Kernel = kernel,
            Arguments = new(new GeminiPromptExecutionSettings()
            {
                ResponseSchema = typeof(IntentDetectionResult),
                ResponseMimeType = "application/json",
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
        Console.WriteLine("Raw result:");
        Console.WriteLine(result.ToString());
        var resultTyped = JsonConvert.DeserializeObject<IntentDetectionResult>(result.ToString());
        return resultTyped;


        // var kernel = KernelFactory.DefaultKernel();
        // var chatService = kernel.GetRequiredService<IChatCompletionService>();
        // ChatHistory chat = new("Your'e a helpful assistant that can answer questions. ONLY call functions if the user asks for the current date or time. Do not call functions for any other reason.");
        // GeminiPromptExecutionSettings promptExecutionSettings = new()
        // {
        //     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        //     ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
        // };

        // while (true)
        // {
        //     Console.Write("User>");
        //     var userInput = Console.ReadLine()!;
        //     chat.AddUserMessage(userInput);
        //     var result = await chatService.GetChatMessageContentAsync(chat, executionSettings: promptExecutionSettings, kernel: kernel);
        //     Console.WriteLine($"Assistant> {result}");
        //     chat.AddAssistantMessage(result.ToString());
        // }
    }
}

public record IntentDetectionResult([Description("The user intent")] Intent Intent, [Description("the user query")] string Query);

public enum Intent
{
    SmallTalk,
    Other
}