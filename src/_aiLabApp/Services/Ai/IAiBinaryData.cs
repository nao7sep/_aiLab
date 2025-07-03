namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Interface for any binary object (file, image, audio, etc.) that may be a URL or a byte array.
    /// </summary>
    public interface IAiBinaryData
    {
        /// <summary>
        /// Optional public URL to the binary data (preferred if available for some providers).
        /// </summary>
        string? Url { get; }

        /// <summary>
        /// Optional raw bytes of the binary data (used for inline upload or data URLs).
        /// </summary>
        byte[]? Bytes { get; }

        /// <summary>
        /// Optional MIME type (e.g., "image/png", "audio/wav").
        /// </summary>
        string? MimeType { get; }

        /// <summary>
        /// Returns true if this object represents a URL source.
        /// </summary>
        bool IsUrl { get; }

        /// <summary>
        /// Returns true if this object represents a byte array source.
        /// </summary>
        bool IsBytes { get; }
    }
}
