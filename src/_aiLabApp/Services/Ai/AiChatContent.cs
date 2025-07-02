namespace _aiLabApp.Services
{
    /// <summary>
    /// Abstract base class for AI chat message content (text, image, tool call, etc).
    /// </summary>
    public abstract class AiChatContent
    {
        /// <summary>
        /// The modality/type of this content (text, image, etc).
        /// </summary>
        public abstract AiContentType ContentType { get; }
    }
}
