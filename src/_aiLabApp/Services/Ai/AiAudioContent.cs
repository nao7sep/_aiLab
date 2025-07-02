namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents an audio content for AI chat.
    /// </summary>
    public class AiAudioContent : AiChatContent
    {
        public override AiContentType ContentType => AiContentType.Audio;
        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string? MimeType { get; }

        public AiAudioContent(string url, string? mimeType = null)
        {
            Url = url;
            MimeType = mimeType;
        }

        public AiAudioContent(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes;
            MimeType = mimeType;
        }
    }
}
