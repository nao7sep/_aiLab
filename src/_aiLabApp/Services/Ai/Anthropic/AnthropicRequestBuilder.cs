using System.Text.Json.Nodes;

namespace _aiLabApp.Services.Ai.Anthropic
{
    public static class AnthropicRequestBuilder
    {
        /// <summary>
        /// Builds the Anthropic API request JSON object from AiChatRequest.
        /// </summary>
        public static JsonObject BuildRequest(AiChatRequest request)
        {
            var json = new JsonObject();
            foreach (var kvp in request.Parameters)
                json[kvp.Key] = DictionaryJsonConverter.ToJsonNode(kvp.Value);
            var messages = new JsonArray();
            foreach (var msg in request.Messages)
            {
                var msgObj = new JsonObject
                {
                    ["role"] = AnthropicConverter.RoleToString(msg.Role)
                };
                var contentArray = new JsonArray();
                foreach (var content in msg.Contents)
                {
                    switch (content)
                    {
                        case AiTextContent text:
                            contentArray.Add(new JsonObject {
                                ["type"] = "text",
                                ["text"] = text.Text
                            });
                            break;
                        case AiImageContent image:
                            if (image.IsUrl)
                                throw new NotSupportedException("Anthropic API does not support image URLs in requests.");
                            else if (image.IsBytes)
                            {
                                var base64Data = Convert.ToBase64String(image.Bytes!);
                                contentArray.Add(new JsonObject
                                {
                                    ["type"] = "image",
                                    ["source"] = new JsonObject
                                    {
                                        ["type"] = "base64",
                                        ["media_type"] = image.MimeType ?? throw new ArgumentNullException(nameof(image.MimeType)),
                                        ["data"] = base64Data
                                    }
                                });
                            }
                            break;
                        case AiFileContent file:
                            // Anthropic API does not currently support direct file content in request bodies.
                            // While their Files API allows pre-uploading files and referencing them via `file_id`,
                            // our implementation does not yet support this workflow.
                            throw new NotImplementedException("We do not support file content in Anthropic API requests yet.");
                        default:
                            throw new NotSupportedException($"Content type '{content.GetType().Name}' is not supported by Anthropic API.");
                    }
                }
                msgObj["content"] = contentArray;
                messages.Add(msgObj);
            }
            json["messages"] = messages;
            return json;
        }
    }
}
