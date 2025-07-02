using Microsoft.Extensions.DependencyInjection;
using _aiLabApp.Services;
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
            var services = new ServiceCollection();

            // console
            var consoleWriter = new ConsoleWriter();
            services.AddSingleton(consoleWriter);

            // logger
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string logFilePath = Path.Combine(desktopPath, $"_aiLab-{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}.log");
            var logger = new Logger(logFilePath);
            services.AddSingleton(logger);

            try
            {
                // settings
                string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                var settings = JsonNode.LoadFromFile(settingsPath);
                services.AddSingleton(settings);

                // api keys
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
                services.AddSingleton(apiKeyStore);

                // OpenAI config
                var openAiConfigNode = settings.Get("OpenAiConfig");
                var openAiConfig = new OpenAiConfig(apiKeyStore, openAiConfigNode);
                services.AddSingleton(openAiConfig);

                // Anthropic config
                var anthropicConfigNode = settings.Get("AnthropicConfig");
                var anthropicConfig = new AnthropicConfig(apiKeyStore, anthropicConfigNode);
                services.AddSingleton(anthropicConfig);

                // Google config
                var googleConfigNode = settings.Get("GoogleConfig");
                var googleConfig = new GoogleConfig(apiKeyStore, googleConfigNode);
                services.AddSingleton(googleConfig);

                // xAI config
                var xaiConfigNode = settings.Get("XaiConfig");
                var xaiConfig = new XaiConfig(apiKeyStore, xaiConfigNode);
                services.AddSingleton(xaiConfig);

                // services
                var tempProvider = services.BuildServiceProvider();
                // ...
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
