using System.Reflection;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Platform.Data.Triplets;
using Platform.Examples;

namespace Platform.Data.WebTerminal
{
    public class Program : Terminal
    {
        public Program() : base(GetDatabaseFilePath())
        {
        }

        private static string GetDatabaseFilePath()
        {
            var databaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"data.dat");
#if DEBUG
            File.Delete(databaseFile);
#endif
            return databaseFile;
        }

        protected override void OnStart()
        {
            if (!string.IsNullOrWhiteSpace(DatabaseFilePath))
            {
                Link.StartMemoryManager(DatabaseFilePath);
            }
        }

        protected override void OnStop()
        {
            Link.StopMemoryManager();
        }

        public static void Main(string[] args)
        {
            var program = new Program();
            program.Start();
            CreateWebHostBuilder(args).Build().Run();
            program.Stop();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
    }
}
