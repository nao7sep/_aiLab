namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Unified interface for all AI multimodal operations (text, image, audio, video, etc).
    /// Uses AiChatRequest and AiChatResponse for all modalities.
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// Generates one or more text/multimodal responses (e.g., chat completions or candidates) from a conversation prompt.
        /// The response may contain multiple messages if the provider supports it; otherwise, a single message is returned in a list.
        /// </summary>
        Task<AiChatResponse> GenerateTextsAsync(AiChatRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates one or more images from a prompt using a dedicated image generation API (e.g., DALL·E, Gemini Image).
        /// </summary>
        Task<AiChatResponse> GenerateImagesAsync(AiChatRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates one or more audio outputs from a prompt (text-to-speech or similar).
        /// </summary>
        Task<AiChatResponse> GenerateAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes one or more audio inputs to text (speech-to-text).
        /// </summary>
        Task<AiChatResponse> TranscribeAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates one or more videos from a prompt.
        /// </summary>
        Task<AiChatResponse> GenerateVideosAsync(AiChatRequest request, CancellationToken cancellationToken = default);
    }
}
