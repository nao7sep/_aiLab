namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Interface for AI chat message content (text, image, tool call, etc).
    /// </summary>
    public interface IAiChatContent
    {
        /// <summary>
        /// The modality/type of this content (text, image, etc).
        /// </summary>
        AiContentType ContentType { get; }
    }
}
