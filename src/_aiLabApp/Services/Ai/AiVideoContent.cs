namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a video content for AI chat.
    /// </summary>
    public class AiVideoContent : IAiChatContent, IAiBinaryData
    {
        public AiContentType ContentType => AiContentType.Video;

        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string MimeType { get; }

        public bool IsUrl => !string.IsNullOrWhiteSpace(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiVideoContent(string url, string mimeType)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }

        public AiVideoContent(byte[] bytes, string mimeType)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }
    }
}
