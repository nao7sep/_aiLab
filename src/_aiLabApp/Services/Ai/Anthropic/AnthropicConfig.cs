namespace _aiLabApp.Services.Ai.Anthropic
{
    /// <summary>
    /// Configuration for Anthropic service provider.
    /// </summary>
    public class AnthropicConfig : IAiServiceConfig
    {
        public AiServiceProvider Provider => AiServiceProvider.Anthropic;
        public string ApiKey { get; }
        public Dictionary<string, object?> Parameters { get; } = [];

        public AnthropicConfig(string apiKey, Dictionary<string, object?>? parameters = null)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    Parameters[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
