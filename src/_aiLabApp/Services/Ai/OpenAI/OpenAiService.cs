namespace _aiLabApp.Services.Ai.OpenAI
{
    public class OpenAiService : IAiService
    {
        public IAiServiceConfig Config { get; }

        public Task<AiChatResponse> GenerateTextsAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AiChatResponse> GenerateImagesAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AiChatResponse> GenerateAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AiChatResponse> TranscribeAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // OpenAI.Sora: Video generation is available via Azure OpenAI Service,
        // but there is currently *no public endpoint* on the standard OpenAI API.
        public Task<AiChatResponse> GenerateVideosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public OpenAiService(IAiServiceConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config), "OpenAiService requires a valid configuration.");
        }
    }
}
