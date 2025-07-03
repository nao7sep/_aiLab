namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a generic file content for AI chat.
    /// </summary>
    public class AiFileContent : IAiChatContent, IAiBinaryData
    {
        public AiContentType ContentType => AiContentType.File;

        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string MimeType { get; }

        public bool IsUrl => !string.IsNullOrWhiteSpace(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiFileContent(string url, string mimeType)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }

        public AiFileContent(byte[] bytes, string mimeType)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }
    }
}
