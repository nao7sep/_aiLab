namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents an audio content for AI chat.
    /// </summary>
    public class AiAudioContent : IAiChatContent, IAiBinaryData
    {
        public AiContentType ContentType => AiContentType.Audio;

        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string? MimeType { get; }

        public bool IsUrl => !string.IsNullOrEmpty(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiAudioContent(string url, string? mimeType = null)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            MimeType = mimeType;
        }

        public AiAudioContent(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            MimeType = mimeType;
        }
    }
}
