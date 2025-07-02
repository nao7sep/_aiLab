namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents an image content for AI chat.
    /// </summary>
    public class AiImageContent : AiChatContent
    {
        public override AiContentType ContentType => AiContentType.Image;
        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string? MimeType { get; }

        public AiImageContent(string url, string? mimeType = null)
        {
            Url = url;
            MimeType = mimeType;
        }
        public AiImageContent(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes;
            MimeType = mimeType;
        }
    }
}
