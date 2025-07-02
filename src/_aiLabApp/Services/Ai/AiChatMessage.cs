namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a single message in a multi-modal AI chat (role + content parts).
    /// </summary>
    public class AiChatMessage
    {
        public AiChatRole Role { get; set; }
        public IList<AiChatContent> Contents = [];

        public AiChatMessage() { }
        public AiChatMessage(AiChatRole role, params AiChatContent[] contents)
        {
            Role = role;
            Contents = contents;
        }
    }
}
