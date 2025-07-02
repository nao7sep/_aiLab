namespace _aiLabApp.Services.Ai.Anthropic
{
    /// <summary>
    /// Configuration for Anthropic service provider.
    /// </summary>
    public class AnthropicConfig : AiServiceConfig
    {
        public AnthropicConfig(string apiKey, Dictionary<string, object?>? parameters = null)
            : base(AiServiceProvider.Anthropic, apiKey, parameters)
        {
        }
    }
}
