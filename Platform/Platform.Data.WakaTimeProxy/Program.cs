using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Platform.Data.WakaTimeProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:52595"); // WakaTime default API endpoint port
    }
}
