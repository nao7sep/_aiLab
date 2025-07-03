namespace _aiLabApp.Services.Ai.OpenAI
{
    /// <summary>
    /// Configuration for OpenAI service provider.
    /// </summary>
    public class OpenAiConfig : IAiServiceConfig
    {
        public AiServiceProvider Provider => AiServiceProvider.Anthropic;
        public string ApiKey { get; }
        public Dictionary<string, object?> Parameters { get; } = [];

        public OpenAiConfig(string apiKey, Dictionary<string, object?>? parameters = null)
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
