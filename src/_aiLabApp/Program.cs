using _aiLabApp.Services;
using _aiLabApp.Services.Ai;
using _aiLabApp.Services.Ai.Anthropic;
using _aiLabApp.Services.Ai.Google;
using _aiLabApp.Services.Ai.OpenAI;
using _aiLabApp.Services.Ai.xAI;

namespace _aiLabApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var consoleWriter = new ConsoleWriter();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string logFilePath = Path.Combine(desktopPath, $"_aiLab-{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}.log");
            var logger = new Logger(logFilePath);

            try
            {
                string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                var settings = JsonNode.LoadFromFile(settingsPath);

                var apiKeyStore = new ApiKeyStore();
                var apiKeyFilesNode = settings.Get("ApiKeyFiles");
                var apiKeyFilesArray = apiKeyFilesNode?.AsArray();
                if (apiKeyFilesArray != null)
                {
                    foreach (var fileNode in apiKeyFilesArray)
                    {
                        var filePath = fileNode.AsString();
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            apiKeyStore.LoadFromFile(filePath);
                        }
                    }
                }

                var openAiApiKey = apiKeyStore.GetApiKey(AiServiceProvider.OpenAI);
                var openAiConfigNode = settings.Get("OpenAiConfig");
                var openAiParameters = openAiConfigNode?.GetChildrenAsDictionary();
                var openAiConfig = new OpenAiConfig(openAiApiKey, openAiParameters);
                var openAiService = new OpenAiService(openAiConfig);

                var anthropicApiKey = apiKeyStore.GetApiKey(AiServiceProvider.Anthropic);
                var anthropicConfigNode = settings.Get("AnthropicConfig");
                var anthropicParameters = anthropicConfigNode?.GetChildrenAsDictionary();
                var anthropicConfig = new AnthropicConfig(anthropicApiKey, anthropicParameters);
                var anthropicService = new AnthropicService(anthropicConfig);

                var googleApiKey = apiKeyStore.GetApiKey(AiServiceProvider.Google);
                var googleConfigNode = settings.Get("GoogleConfig");
                var googleParameters = googleConfigNode?.GetChildrenAsDictionary();
                var googleConfig = new GoogleConfig(googleApiKey, googleParameters);
                var googleService = new GoogleService(googleConfig);

                var xaiApiKey = apiKeyStore.GetApiKey(AiServiceProvider.xAI);
                var xaiConfigNode = settings.Get("XaiConfig");
                var xaiParameters = xaiConfigNode?.GetChildrenAsDictionary();
                var xaiConfig = new XaiConfig(xaiApiKey, xaiParameters);
                var xaiService = new XaiService(xaiConfig);
            }
            catch (Exception ex)
            {
                var msg = $"Error: {ex}";
                logger.WriteError(msg);
                consoleWriter.WriteError(msg);
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
