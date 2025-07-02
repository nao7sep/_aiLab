namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Base configuration for any AI service provider.
    /// </summary>
    public abstract class AiServiceConfig
    {
        /// <summary>
        /// The AI service provider type.
        /// </summary>
        public AiServiceProvider Provider { get; }

        /// <summary>
        /// The API key for the provider.
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// Arbitrary parameters for the provider (e.g., endpoints, model names, etc).
        /// </summary>
        public Dictionary<string, object?>? Parameters { get; private set; }

        protected AiServiceConfig(AiServiceProvider provider, ApiKeyStore apiKeyStore, JsonNode? configNode = null)
        {
            if (!Enum.IsDefined(typeof(AiServiceProvider), provider))
                throw new ArgumentException($"Invalid AI service provider: {provider}", nameof(provider));
            Provider = provider;
            ApiKey = apiKeyStore.GetApiKey(provider) ?? throw new KeyNotFoundException($"API key for {provider} not found.");
            Parameters = configNode?.GetChildrenAsDictionary();
        }
    }
}
