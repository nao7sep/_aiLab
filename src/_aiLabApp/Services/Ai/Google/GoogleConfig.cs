namespace _aiLabApp.Services.Ai.Google
{
    /// <summary>
    /// Configuration for Google (Vertex AI) service provider.
    /// </summary>
    public class GoogleConfig : AiServiceConfig
    {
        public GoogleConfig(ApiKeyStore apiKeyStore, JsonNode? configNode = null)
            : base(AiServiceProvider.Google, apiKeyStore, configNode)
        {
        }
    }
}
