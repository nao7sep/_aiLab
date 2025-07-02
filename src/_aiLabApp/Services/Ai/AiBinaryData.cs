namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Base class for any binary object (file, image, audio, etc.) that may be a URL or a byte array.
    /// </summary>
    public abstract class AiBinaryData
    {
        /// <summary>
        /// Optional public URL to the binary data (preferred if available for some providers).
        /// </summary>

        public string? Url { get; }
        public byte[]? Bytes { get; }
        public string? MimeType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AiBinaryData"/> with a URL.
        /// </summary>
        /// <param name="url">The public URL to the binary data.</param>
        /// <param name="mimeType">The MIME type of the data.</param>
        protected AiBinaryData(string url, string? mimeType = null)
        {
            Url = url;
            MimeType = mimeType;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AiBinaryData"/> with raw bytes.
        /// </summary>
        /// <param name="bytes">The raw bytes of the binary data.</param>
        /// <param name="mimeType">The MIME type of the data.</param>
        protected AiBinaryData(byte[] bytes, string? mimeType = null)
        {
            Bytes = bytes;
            MimeType = mimeType;
        }

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
