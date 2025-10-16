using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var databaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Directory.GetCurrentDirectory()) ?? Directory.GetCurrentDirectory(), @"data.dat");
#if DEBUG
            if (File.Exists(databaseFile))
            {
                File.Delete(databaseFile);
            }
#endif
            Link.StartMemoryManager(databaseFile);
            CreateHostBuilder(args).Build().Run();
            Link.StopMemoryManager();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
