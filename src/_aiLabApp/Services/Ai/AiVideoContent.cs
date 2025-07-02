namespace _aiLabApp.Services
{
    /// <summary>
    /// Represents a video content for AI chat.
    /// </summary>
    public class AiVideoContent : AiChatContent
    {
        public override AiContentType ContentType => AiContentType.Video;
        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string? MimeType { get; }

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
