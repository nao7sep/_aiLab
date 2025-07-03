using Flurl.Http;
using System.Text.Json.Nodes;

namespace _aiLabApp.Services.Ai.Anthropic
{
    public static class AnthropicApiClient
    {
        public static async Task<JsonObject> PostAsync(string url, string apiKey, string anthropicVersion, JsonObject payload, CancellationToken cancellationToken)
        {
            try
            {
                var flurlResp = await url
                    .WithHeader("x-api-key", apiKey)
                    .WithHeader("anthropic-version", anthropicVersion)
                    .WithHeader("Content-Type", "application/json")
                    .PostJsonAsync(payload, cancellationToken: cancellationToken);
                var response = await flurlResp.GetJsonAsync<JsonObject>();
                return response;
            }
            catch (FlurlHttpException ex)
            {
                string? body = null;
                try
                {
                    body = await ex.GetResponseStringAsync();
                }
                catch {}
                // Use ex.Message here to avoid duplicating stack traces and exception details in the final exception output.
                // The full exception information is still available via the InnerException property.
                var message = $"Anthropic API error: {ex.Message}{Environment.NewLine}Response body: {body}";
                throw new AiServiceException(message, ex);
            }
        }
    }
}
