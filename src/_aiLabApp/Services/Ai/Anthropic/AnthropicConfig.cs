namespace _aiLabApp.Services.Ai.Anthropic
{
    /// <summary>
    /// Configuration for Anthropic service provider.
    /// </summary>
    public class AnthropicConfig : AiServiceConfig
    {
        public AnthropicConfig(ApiKeyStore apiKeyStore, JsonNode? configNode = null)
            : base(AiServiceProvider.Anthropic, apiKeyStore, configNode)
        {
        }
    }
}
