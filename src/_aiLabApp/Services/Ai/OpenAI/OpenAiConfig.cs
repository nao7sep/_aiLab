namespace _aiLabApp.Services.Ai.OpenAI
{
    /// <summary>
    /// Configuration for OpenAI service provider.
    /// </summary>
    public class OpenAiConfig : AiServiceConfig
    {
        public OpenAiConfig(string apiKey, Dictionary<string, object?>? parameters = null)
            : base(AiServiceProvider.OpenAI, apiKey, parameters)
        {
        }
    }
}
