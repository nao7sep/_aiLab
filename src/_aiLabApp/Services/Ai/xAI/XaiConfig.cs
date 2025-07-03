namespace _aiLabApp.Services.Ai.xAI
{
    /// <summary>
    /// Configuration for xAI (Grok) service provider.
    /// </summary>
    public class XaiConfig : IAiServiceConfig
    {
        public AiServiceProvider Provider => AiServiceProvider.Anthropic;
        public string ApiKey { get; }
        public Dictionary<string, object?> Parameters { get; } = [];

        public XaiConfig(string apiKey, Dictionary<string, object?>? parameters = null)
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
