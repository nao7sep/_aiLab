namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a single message in a multi-modal AI chat (role + content parts).
    /// </summary>
    public class AiChatMessage
    {
        public AiChatRole Role { get; set; }
        public IList<IAiChatContent> Contents { get; } = [];

        public AiChatMessage()
        {
        }

        public AiChatMessage(AiChatRole role, params IAiChatContent[] contents)
        {
            Role = role;
            foreach (var content in contents)
                Contents.Add(content);
        }
    }
}
