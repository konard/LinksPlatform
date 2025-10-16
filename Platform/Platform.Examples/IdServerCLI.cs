using System;
using System.Net.Sockets;
using Platform.IO;
using Platform.Threading;
using Platform.Communication.Protocol.Udp;
using Platform.Exceptions;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the IdServer.
    /// Manages ID distribution for cluster servers via UDP communication.
    /// </summary>
    public class IdServerCLI : ICommandLineInterface
    {
        private const int DefaultReceivePort = 9999;
        private const int DefaultSendPort = 9998;
        private const ulong DefaultStartId = 1;
        private const ulong DefaultBlockSize = 1000;

        public void Run(params string[] args)
        {
            try
            {
                var receivePort = DefaultReceivePort;
                var sendPort = DefaultSendPort;
                var startId = DefaultStartId;
                var blockSize = DefaultBlockSize;

                // Parse command-line arguments
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--receive-port" && i + 1 < args.Length)
                    {
                        if (int.TryParse(args[i + 1], out var port))
                        {
                            receivePort = port;
                            i++;
                        }
                    }
                    else if (args[i] == "--send-port" && i + 1 < args.Length)
                    {
                        if (int.TryParse(args[i + 1], out var port))
                        {
                            sendPort = port;
                            i++;
                        }
                    }
                    else if (args[i] == "--start-id" && i + 1 < args.Length)
                    {
                        if (ulong.TryParse(args[i + 1], out var id))
                        {
                            startId = id;
                            i++;
                        }
                    }
                    else if (args[i] == "--block-size" && i + 1 < args.Length)
                    {
                        if (ulong.TryParse(args[i + 1], out var size))
                        {
                            blockSize = size;
                            i++;
                        }
                    }
                    else if (args[i] == "--help" || args[i] == "-h")
                    {
                        PrintUsage();
                        return;
                    }
                }

                Console.WriteLine("=== Links Platform - ID Server ===");
                Console.WriteLine($"Configuration:");
                Console.WriteLine($"  Receive Port: {receivePort}");
                Console.WriteLine($"  Send Port: {sendPort}");
                Console.WriteLine($"  Start ID: {startId}");
                Console.WriteLine($"  Block Size: {blockSize}");
                Console.WriteLine();

                using (var cancellation = new ConsoleCancellation())
                using (var sender = new UdpSender(sendPort))
                {
                    var idServer = new IdServer(sender, startId, blockSize);

                    Console.WriteLine("ID Server started.");
                    Console.WriteLine("Press CTRL+C or ESC to stop server.");
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  STATUS - Display current server status");
                    Console.WriteLine();

                    using (var receiver = new UdpClient(receivePort))
                    {
                        while (cancellation.NotRequested)
                        {
                            // Process incoming UDP messages
                            while (receiver.Available > 0)
                            {
                                var message = receiver.ReceiveString();
                                if (!string.IsNullOrWhiteSpace(message))
                                {
                                    Console.WriteLine($"<- {message}");
                                    idServer.HandleRequest(message);
                                }
                            }

                            // Process console input
                            while (Console.KeyAvailable)
                            {
                                var info = Console.ReadKey(true);
                                if (info.Key == ConsoleKey.Escape)
                                {
                                    cancellation.ForceCancellation();
                                }
                            }

                            // Check for console commands
                            if (Console.In.Peek() != -1)
                            {
                                var line = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    line = line.Trim().ToUpper();
                                    if (line == "STATUS")
                                    {
                                        idServer.SendStatus();
                                    }
                                    else if (line == "EXIT" || line == "QUIT")
                                    {
                                        cancellation.ForceCancellation();
                                    }
                                }
                            }

                            ThreadHelpers.Sleep();
                        }

                        Console.WriteLine("ID Server stopped.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToStringWithAllInnerExceptions());
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: Platform.Data.IdServer [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --receive-port <port>    Port to receive requests on (default: 9999)");
            Console.WriteLine("  --send-port <port>       Port to send responses to (default: 9998)");
            Console.WriteLine("  --start-id <id>          Starting ID for allocation (default: 1)");
            Console.WriteLine("  --block-size <size>      Size of ID blocks to allocate (default: 1000)");
            Console.WriteLine("  --help, -h               Display this help message");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("  IdServer distributes unique ID ranges to servers in a cluster,");
            Console.WriteLine("  enabling a shared global address space across multiple servers.");
        }
    }
}
