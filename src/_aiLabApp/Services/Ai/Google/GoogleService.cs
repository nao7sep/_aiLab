namespace _aiLabApp.Services.Ai.Google
{
    public class GoogleService : IAiService
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

        public Task<AiChatResponse> GenerateVideosAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public GoogleService(IAiServiceConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config), "GoogleService requires a valid configuration.");
        }
    }
}
