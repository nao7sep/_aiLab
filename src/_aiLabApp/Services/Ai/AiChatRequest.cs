namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a multi-modal AI chat request (conversation + options).
    /// </summary>
    public class AiChatRequest
    {
        public IList<AiChatMessage> Messages { get; } = new List<AiChatMessage>();
        public Dictionary<string, object?> Options { get; } = new Dictionary<string, object?>();
    }
}
