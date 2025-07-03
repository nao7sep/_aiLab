namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a multi-modal AI chat request (conversation + parameters).
    /// </summary>
    public class AiChatRequest
    {
        public IList<AiChatMessage> Messages { get; } = [];
        public Dictionary<string, object?> Parameters { get; } = [];

        public AiChatRequest() {}

        public AiChatRequest(IEnumerable<AiChatMessage> messages, Dictionary<string, object?>? parameters = null)
        {
            foreach (var msg in messages)
                Messages.Add(msg);
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    Parameters[kvp.Key] = kvp.Value;
            }
        }
    }
}
