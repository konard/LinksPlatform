using System;
using System.Threading;

namespace Platform.CachingWebProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 8080;
            var cacheExpiration = TimeSpan.FromHours(1);

            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "-p" || args[i] == "--port") && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out var parsedPort))
                    {
                        port = parsedPort;
                    }
                    i++;
                }
                else if ((args[i] == "-e" || args[i] == "--expiration") && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out var hours))
                    {
                        cacheExpiration = TimeSpan.FromHours(hours);
                    }
                    i++;
                }
                else if (args[i] == "-h" || args[i] == "--help")
                {
                    PrintHelp();
                    return;
                }
            }

            Console.WriteLine("=== LinksPlatform Caching Web Proxy ===");
            Console.WriteLine($"Cache expiration: {cacheExpiration.TotalHours} hours");
            Console.WriteLine();

            var cache = new MemoryResponseCache(cacheExpiration);
            var prefix = $"http://localhost:{port}/";

            using (var server = new CachingProxyServer(prefix, cache))
            {
                server.Start();

                // Wait for Ctrl+C
                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    exitEvent.Set();
                };

                exitEvent.WaitOne();
                Console.WriteLine("\nShutting down...");
                server.Stop();
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("LinksPlatform Caching Web Proxy");
            Console.WriteLine();
            Console.WriteLine("Usage: Platform.CachingWebProxy [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -p, --port <port>           Port to listen on (default: 8080)");
            Console.WriteLine("  -e, --expiration <hours>    Cache expiration time in hours (default: 1)");
            Console.WriteLine("  -h, --help                  Show this help message");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  Platform.CachingWebProxy -p 8888 -e 24");
            Console.WriteLine();
            Console.WriteLine("To use the proxy, configure your browser or application to use:");
            Console.WriteLine("  HTTP Proxy: localhost:<port>");
            Console.WriteLine();
            Console.WriteLine("Or access URLs directly:");
            Console.WriteLine("  http://localhost:<port>/?url=<target-url>");
        }
    }
}
