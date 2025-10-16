using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for packing links database into compact binary format.
    /// </summary>
    public class LinksPackerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var i = 0;
            var linksFile = ConsoleHelpers.GetOrReadArgument(i++, "Links database file", args);
            var packTo = ConsoleHelpers.GetOrReadArgument(i++, "Pack to file", args);
            var bytesPerIndexStr = ConsoleHelpers.GetOrReadArgument(i++, "Bytes per index (0 for auto-detect, 1-8)", args);

            if (!byte.TryParse(bytesPerIndexStr, out byte bytesPerIndex))
            {
                Console.WriteLine("Invalid bytes per index value. Using auto-detect (0).");
                bytesPerIndex = 0;
            }

            if (!File.Exists(linksFile))
            {
                Console.WriteLine("Entered links file does not exist.");
                return;
            }

            try
            {
                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var packer = new LinksPacker();

                    // Count links
                    ulong linkCount = 0;
                    syncLinks.Each(link => { linkCount++; return syncLinks.Constants.Continue; });

                    Console.WriteLine($"Packing {linkCount} links...");
                    var startTime = DateTime.Now;

                    packer.Pack(syncLinks, packTo, bytesPerIndex, cancellation.Token);

                    var elapsed = DateTime.Now - startTime;
                    var fileInfo = new FileInfo(packTo);

                    Console.WriteLine($"Successfully packed to: {packTo}");
                    Console.WriteLine($"File size: {fileInfo.Length} bytes");
                    Console.WriteLine($"Time elapsed: {elapsed.TotalSeconds:F2} seconds");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
