namespace _aiLabApp.Services.Ai.Google
{
    /// <summary>
    /// Configuration for Google (Vertex AI) service provider.
    /// </summary>
    public class GoogleConfig : IAiServiceConfig
    {
        public AiServiceProvider Provider => AiServiceProvider.Anthropic;
        public string ApiKey { get; }
        public Dictionary<string, object?> Parameters { get; } = [];

        public GoogleConfig(string apiKey, Dictionary<string, object?>? parameters = null)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    Parameters[kvp.Key] = kvp.Value;
            }
        }
    }
}
