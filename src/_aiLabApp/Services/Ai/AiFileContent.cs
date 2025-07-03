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
        public string? MimeType { get; }

        public bool IsUrl => !string.IsNullOrEmpty(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiFileContent(string url, string? mimeType = null)
        {
            Url = url;
            MimeType = mimeType;
        }

        public AiFileContent(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes;
            MimeType = mimeType;
        }
    }
}
