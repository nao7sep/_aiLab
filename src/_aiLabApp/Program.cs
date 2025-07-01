using Microsoft.Extensions.DependencyInjection;
using _aiLabApp.Services;

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
