namespace _aiLabApp.Services.Ai.Anthropic
{
    public class AnthropicService : IAiService
    {
        public IAiServiceConfig Config { get; }

        public Task<AiChatResponse> GenerateTextsAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AiChatResponse> GenerateImagesAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AiChatResponse> GenerateAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AiChatResponse> TranscribeAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AiChatResponse> GenerateVideosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public AnthropicService(IAiServiceConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config), "AnthropicService requires a valid configuration.");
        }
    }
}
