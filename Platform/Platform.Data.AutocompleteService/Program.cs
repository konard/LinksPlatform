using System.Reflection;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.AutocompleteService
{
    public class Program
    {
        public static ILinks<ulong> Links { get; private set; }

        public static void Main(string[] args)
        {
            var databaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"autocomplete.dat");

            // Initialize Links storage
            Links = new UnitedMemoryLinks<ulong>(databaseFile);

            CreateWebHostBuilder(args).Build().Run();

            // Dispose Links on shutdown
            (Links as UnitedMemoryLinks<ulong>)?.Dispose();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
