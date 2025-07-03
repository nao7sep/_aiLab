using System.Text.Json.Nodes;

namespace _aiLabApp.Services.Ai.Anthropic
{
    public static class AnthropicResponseParser
    {
        public static AiChatResponse Parse(JsonObject json)
        {
            var messages = new List<AiChatMessage>();
            if (json["content"] is JsonArray contentArray)
            {
                var contents = ParseContents(contentArray);
                messages.Add(new AiChatMessage(AiChatRole.Assistant, contents.ToArray()));
            }
            else
                throw new AiServiceException("Invalid response format: 'content' must be an array of content objects.");

            var metadata = new Dictionary<string, object?>();
            foreach (var kvp in json)
            {
                if (kvp.Key != "content")
                    metadata[kvp.Key] = kvp.Value;
            }
            return new AiChatResponse(messages, metadata);
        }

        private static List<IAiChatContent> ParseContents(JsonArray array)
        {
            var result = new List<IAiChatContent>();
            foreach (var part in array)
            {
                if (part is JsonObject textObj && textObj["type"]?.ToString() == "text")
                {
                    var text = textObj["text"]?.ToString() ?? throw new AiServiceException("Text content is missing in the response.");
                    result.Add(new AiTextContent(text));
                }
                else
                    throw new AiServiceException("Invalid content format: each part must be a text object with a 'type' of 'text'.");
            }
            return result;
        }
    }
}
