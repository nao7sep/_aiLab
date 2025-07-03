namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Interface for AI service configuration for any provider.
    /// </summary>
    public interface IAiServiceConfig
    {
        /// <summary>
        /// The AI service provider type.
        /// </summary>
        AiServiceProvider Provider { get; }

        /// <summary>
        /// The API key for the provider.
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// Arbitrary parameters for the provider (e.g., endpoints, model names, etc).
        /// </summary>
        Dictionary<string, object?> Parameters { get; }
    }
}
