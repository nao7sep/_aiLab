namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a multi-modal AI chat response, including all returned messages and optional metadata.
    /// Note: Most AI providers return only a single message per response (with possibly multiple contents).
    /// This class supports multiple messages for future-proofing and flexibility, but in practice,
    /// usually only the first message will contain meaningful values.
    /// </summary>
    public class AiChatResponse
    {
        /// <summary>
        /// The list of assistant (or system) messages returned by the AI provider.
        /// </summary>
        public List<AiChatMessage> Messages { get; } = [];

        /// <summary>
        /// Optional: Provider-specific metadata (e.g., usage, finish reason, safety ratings, etc.)
        /// </summary>
        public Dictionary<string, object?> Metadata { get; } = [];

        public AiChatResponse() {}

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
