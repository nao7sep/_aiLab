namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a multi-modal AI chat response, including all returned messages and optional metadata.
    /// </summary>
    public class AiChatResponse
    {
        /// <summary>
        /// The list of assistant (or system) messages returned by the AI provider.
        /// </summary>
        public IList<AiChatMessage> Messages { get; } = new List<AiChatMessage>();

        /// <summary>
        /// Optional: Provider-specific metadata (e.g., usage, finish reason, safety ratings, etc.)
        /// </summary>
        public Dictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

        public AiChatResponse()
        {
        }

        public AiChatResponse(IEnumerable<AiChatMessage> messages, Dictionary<string, object?>? metadata = null)
        {
            foreach (var msg in messages)
                Messages.Add(msg);
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                    Metadata[kvp.Key] = kvp.Value;
            }
        }
    }
}
