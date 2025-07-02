namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a multi-modal AI chat request (conversation + options).
    /// </summary>
    public class AiChatRequest
    {
        public IList<AiChatMessage> Messages = [];
        public Dictionary<string, object?> Options = [];
    }
}
