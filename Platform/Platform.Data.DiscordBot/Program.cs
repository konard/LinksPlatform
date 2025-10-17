using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Platform.Data.DiscordBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("LinksPlatform Discord Error Search Bot");
            Console.WriteLine("======================================");

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Get bot token from configuration or environment variable
            var botToken = configuration["DiscordBotToken"] ?? Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

            if (string.IsNullOrWhiteSpace(botToken))
            {
                Console.WriteLine("Error: Discord bot token not found!");
                Console.WriteLine("Please set the DISCORD_BOT_TOKEN environment variable or add it to appsettings.json");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  Linux/macOS: export DISCORD_BOT_TOKEN=your_token_here");
                Console.WriteLine("  Windows: set DISCORD_BOT_TOKEN=your_token_here");
                Console.WriteLine("  appsettings.json: { \"DiscordBotToken\": \"your_token_here\" }");
                return;
            }

            try
            {
                var bot = new DiscordBot(botToken);
                Console.WriteLine("Starting bot...");
                await bot.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting bot: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
