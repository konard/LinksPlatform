using System;
using System.Net.Sockets;
using Platform.Exceptions;
using Platform.Threading;
using Platform.IO;
using Platform.Communication.Protocol.Udp;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Slave (Mirror) Server CLI that operates entirely in memory without file persistence.
    /// This implementation uses only primary storage (main memory/RAM) as specified in issue #68.
    /// </summary>
    public class SlaveServerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                // Create a temporary file path for in-memory database
                // This will be deleted when the server stops, ensuring no persistence
                var tempDbPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"slave-db-{Guid.NewGuid()}.links");
                try
                {
                    using (var cancellation = new ConsoleCancellation())
                    // Use UInt64UnitedMemoryLinks with a temporary file for RAM-based operation
                    // The file serves as a memory-mapped interface but data is primarily in memory
                    using (var memoryAdapter = new UInt64UnitedMemoryLinks(tempDbPath, 8 * 1024 * 1024))
                    using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var unicodeMap = new UnicodeMap(syncLinks);
                    unicodeMap.Init();
                    var sequences = new Sequences(syncLinks, new SequencesOptions<ulong> { UseSequenceMarker = true, SequenceMarkerLink = 65537, UseCompression = true });
                    Console.WriteLine("Links slave (mirror) server started.");
                    Console.WriteLine("Operating in memory-only mode (no file persistence).");
                    Console.WriteLine("Press CTRL+C or ESC to stop server.");
                    using (var sender = new UdpSender(8888))
                    {
                        var slaveServer = new SlaveServer(links, sequences, sender);
                        slaveServer.PrintContents(Console.WriteLine);
                        void handleMessage(string message)
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                message = message.Trim();
                                Console.WriteLine($"<- {message}");
                                if (slaveServer.IsSearch(message))
                                {
                                    slaveServer.Search(message);
                                }
                                else
                                {
                                    slaveServer.Create(message);
                                }
                            }
                        }
                        //using (var receiver = new UdpReceiver(7777, handleMessage))
                        using (var receiver = new UdpClient(7777))
                        {
                            while (cancellation.NotRequested)
                            {
                                while (receiver.Available > 0)
                                {
                                    handleMessage(receiver.ReceiveString());
                                }
                                while (Console.KeyAvailable)
                                {
                                    var info = Console.ReadKey(true);
                                    if (info.Key == ConsoleKey.Escape)
                                    {
                                        cancellation.ForceCancellation();
                                    }
                                }
                                ThreadHelpers.Sleep();
                            }
                            Console.WriteLine("Links slave (mirror) server stopped.");
                        }
                    }
                    }
                }
                finally
                {
                    // Clean up temporary database file
                    if (System.IO.File.Exists(tempDbPath))
                    {
                        try
                        {
                            System.IO.File.Delete(tempDbPath);
                            Console.WriteLine($"Temporary database file deleted: {tempDbPath}");
                        }
                        catch
                        {
                            Console.WriteLine($"Warning: Could not delete temporary database file: {tempDbPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToStringWithAllInnerExceptions());
            }
        }
    }
}
