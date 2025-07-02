namespace _aiLabApp.Services.Ai.xAI
{
    /// <summary>
    /// Configuration for xAI (Grok) service provider.
    /// </summary>
    public class XaiConfig : AiServiceConfig
    {
        public XaiConfig(string apiKey, Dictionary<string, object?>? parameters = null)
            : base(AiServiceProvider.xAI, apiKey, parameters)
        {
        }
    }
}
