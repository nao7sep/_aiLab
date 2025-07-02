namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a multi-modal AI chat request (conversation + parameters).
    /// </summary>
    public class AiChatRequest
    {
        public IList<AiChatMessage> Messages { get; } = new List<AiChatMessage>();
        public Dictionary<string, object?> Parameters { get; } = new Dictionary<string, object?>();
    }
}
