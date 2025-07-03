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
        public string MimeType { get; }

        public bool IsUrl => !string.IsNullOrWhiteSpace(Url);
        public bool IsBytes => Bytes != null && Bytes.Length > 0;

        public AiAudioContent(string url, string mimeType)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }

        public AiAudioContent(byte[] bytes, string mimeType)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }
    }
}
