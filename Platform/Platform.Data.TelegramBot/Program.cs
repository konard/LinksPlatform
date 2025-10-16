using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Platform.Data.TelegramBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var bot = new TelegramBotInterface(configuration);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("\nStopping bot...");
                bot.Stop();
                eventArgs.Cancel = true;
            };

            await bot.RunAsync();
        }
    }
}
