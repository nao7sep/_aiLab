using _aiLabApp.Services.Ai;
using System.Text;
using System.Text.Json;

namespace _aiLabApp.Services
{
    public class ApiKeyStore
    {
        private readonly Dictionary<AiServiceProvider, string> _apiKeys = [];

        public void LoadFromFile(string filePath)
        {
            if (Path.IsPathFullyQualified(filePath) == false)
                throw new ArgumentException("File path must be fully qualified.", nameof(filePath));
            if (!File.Exists(filePath)) return;
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict == null) return;
            foreach (var kvp in dict)
            {
                if (Enum.TryParse<AiServiceProvider>(kvp.Key, out var provider))
                    _apiKeys[provider] = kvp.Value; // Overwrite if exists
                else
                    throw new InvalidOperationException($"Invalid service provider '{kvp.Key}' in API key file.");
            }
        }

        public string GetApiKey(AiServiceProvider provider)
        {
            if (_apiKeys.TryGetValue(provider, out var key))
                return key;
            throw new KeyNotFoundException($"API key for {provider} not found.");
        }
    }
}
