namespace _aiLabApp.Services.Ai.xAI
{
    public class XaiService : IAiService
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
            => throw new NotSupportedException();

        public Task<AiChatResponse> TranscribeAudiosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AiChatResponse> GenerateVideosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public XaiService(IAiServiceConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}
