namespace _aiLabApp.Services.Ai.OpenAI
{
    /// <summary>
    /// Configuration for OpenAI service provider.
    /// </summary>
    public class OpenAiConfig : AiServiceConfig
    {
        public OpenAiConfig(ApiKeyStore apiKeyStore, JsonNode? configNode = null)
            : base(AiServiceProvider.OpenAI, apiKeyStore, configNode)
        {
        }
    }
}
