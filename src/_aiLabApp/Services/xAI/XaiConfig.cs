namespace _aiLabApp.Services.xAI
{
    /// <summary>
    /// Configuration for xAI (Grok) service provider.
    /// </summary>
    public class XaiConfig : AiServiceConfig
    {
        public XaiConfig(ApiKeyStore apiKeyStore, JsonNode? configNode = null)
            : base(AiServiceProvider.xAI, apiKeyStore, configNode)
        {
        }
    }
}
