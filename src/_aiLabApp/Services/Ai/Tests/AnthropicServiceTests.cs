using _aiLabApp.Services.Ai.Anthropic;

namespace _aiLabApp.Services.Ai.Tests
{
    public static class AnthropicServiceTests
    {
        public static void TestGenerateTextsAsync(Logger logger, ConsoleWriter consoleWriter, AnthropicService anthropicService)
        {
            var userMsg = "Hello, Claude! Can you tell me a fun fact about AI?";
            var chatRequest = new AiChatRequest(
                [
                    new AiChatMessage(AiChatRole.User, new AiTextContent(userMsg))
                ]
            );
            Console.WriteLine($"{AiChatRole.User}: {userMsg}");

            try
            {
                // Use GetAwaiter().GetResult() to avoid AggregateException wrapping (throws original exception instead of AggregateException)
                var response = anthropicService.GenerateTextsAsync(chatRequest).GetAwaiter().GetResult();

                foreach (var msg in response.Messages)
                {
                    foreach (var content in msg.Contents)
                    {
                        if (content is AiTextContent text)
                            // role not converted to provider-specific string
                            Console.WriteLine($"{AiChatRole.Assistant}: {text.Text}");
                        else
                            throw new NotSupportedException($"Unsupported content type: {content.GetType().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = $"Error: {ex}";
                logger.WriteError(msg);
                consoleWriter.WriteError(msg);
            }
        }
    }
}
