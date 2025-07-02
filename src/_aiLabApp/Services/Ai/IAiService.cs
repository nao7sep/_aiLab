namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Unified interface for all AI multimodal operations (text, image, audio, video).
    /// Each method supports multiple outputs (candidates) where supported by the provider.
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// Generates one or more text/multimodal responses (e.g., chat completions or candidates) from a conversation prompt.
        /// The response may contain multiple messages if the provider supports it; otherwise, a single message is returned in a list.
        /// </summary>
        Task<AiChatResponse> GenerateTextsAsync(AiChatRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates one or more images from a prompt. The response contains all generated images as a list.
        /// </summary>
        Task<AiImageResponse> GenerateImagesAsync(AiImageRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates one or more audio outputs from a prompt (text-to-speech or similar). The response contains all generated audios as a list.
        /// </summary>
        Task<AiAudioResponse> GenerateAudiosAsync(AiAudioRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes one or more audio inputs to text (speech-to-text). The response contains all transcripts as a list.
        /// </summary>
        Task<AiAudioResponse> TranscribeAudiosAsync(AiAudioRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates one or more videos from a prompt. The response contains all generated videos as a list.
        /// </summary>
        Task<AiVideoResponse> GenerateVideosAsync(AiVideoRequest request, CancellationToken cancellationToken = default);
    }
}
