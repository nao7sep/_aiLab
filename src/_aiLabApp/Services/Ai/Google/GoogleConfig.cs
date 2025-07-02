namespace _aiLabApp.Services.Ai.Google
{
    /// <summary>
    /// Configuration for Google (Vertex AI) service provider.
    /// </summary>
    public class GoogleConfig : AiServiceConfig
    {
        public GoogleConfig(string apiKey, Dictionary<string, object?>? parameters = null)
            : base(AiServiceProvider.Google, apiKey, parameters)
        {
        }
    }
}
