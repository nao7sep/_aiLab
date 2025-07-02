namespace _aiLabApp.Services
{
    /// <summary>
    /// Base class for any binary object (file, image, audio, etc.) that may be a URL or a byte array.
    /// </summary>
    public abstract class AiBinaryData
    {
        /// <summary>
        /// Optional public URL to the binary data (preferred if available for some providers).
        /// </summary>
        public string? Url { get; init; }

        /// <summary>
        /// Optional raw bytes of the binary data (used for inline upload or data URLs).
        /// </summary>
        public byte[]? Bytes { get; init; }

        /// <summary>
        /// Optional MIME type (e.g., "image/png", "audio/wav").
        /// </summary>
        public string? MimeType { get; init; }

        /// <summary>
        /// Returns true if this object represents a URL source.
        /// </summary>
        public bool IsUrl => !string.IsNullOrEmpty(Url);

        /// <summary>
        /// Returns true if this object represents a byte array source.
        /// </summary>
        public bool IsBytes => Bytes != null && Bytes.Length > 0;
    }
}
