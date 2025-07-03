namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents an image content for AI chat.
    /// </summary>
    public class AiImageContent : IAiChatContent, IAiBinaryData
    {
        public AiContentType ContentType => AiContentType.Image;

        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string? MimeType { get; }

        public bool IsUrl => !string.IsNullOrWhiteSpace(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiImageContent(string url, string? mimeType = null)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            MimeType = mimeType;
        }

        public AiImageContent(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            MimeType = mimeType;
        }
    }
}
