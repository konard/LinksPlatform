using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for unpacking links database from compact binary format.
    /// </summary>
    public class LinksUnpackerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var i = 0;
            var packFile = ConsoleHelpers.GetOrReadArgument(i++, "Pack file to restore from", args);
            var linksFile = ConsoleHelpers.GetOrReadArgument(i++, "Links database file to create/restore", args);

            if (!File.Exists(packFile))
            {
                Console.WriteLine("Entered pack file does not exist.");
                return;
            }

            try
            {
                // Create new links database file if it doesn't exist
                var isNewFile = !File.Exists(linksFile);

                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var unpacker = new LinksUnpacker();

                    Console.WriteLine($"Unpacking from: {packFile}");
                    var startTime = DateTime.Now;

                    var restoredCount = unpacker.Unpack(syncLinks, packFile, cancellation.Token);

                    var elapsed = DateTime.Now - startTime;

                    Console.WriteLine($"Successfully restored {restoredCount} links to: {linksFile}");
                    Console.WriteLine($"Time elapsed: {elapsed.TotalSeconds:F2} seconds");

                    if (isNewFile)
                    {
                        Console.WriteLine("New database file created.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
