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
        public string? MimeType { get; }

        public bool IsUrl => !string.IsNullOrEmpty(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiVideoContent(string url, string? mimeType = null)
        {
            Url = url;
            MimeType = mimeType;
        }

        public AiVideoContent(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes;
            MimeType = mimeType;
        }
    }
}
