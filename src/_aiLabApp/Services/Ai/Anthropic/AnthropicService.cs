namespace _aiLabApp.Services.Ai.Anthropic
{
    public class AnthropicService : IAiService
    {
        public IAiServiceConfig Config { get; }

        public async Task<AiChatResponse> GenerateTextsAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            var apiHost = Config.Parameters["ApiHost"] as string;
            var endpoint = $"https://{apiHost}/v1/messages";
            var apiKey = Config.ApiKey;
            var chatModel = Config.Parameters["ChatModel"] as string;
            // to avoid compiler warning
            var chatApiVersion = Config.Parameters["ChatApiVersion"] as string ?? throw new AiServiceException("ChatApiVersion is not configured.");
            int timeoutSeconds = Convert.ToInt32(Config.Parameters["TimeoutSeconds"]);

            if (string.IsNullOrWhiteSpace(request.Parameters["model"] as string))
                request.Parameters["model"] = chatModel;

            var payload = AnthropicRequestBuilder.BuildRequest(request);
            var jsonResponse = await AnthropicApiClient.PostAsync(endpoint, apiKey, chatApiVersion, payload, timeoutSeconds, cancellationToken);
            return AnthropicResponseParser.Parse(jsonResponse);
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
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}
