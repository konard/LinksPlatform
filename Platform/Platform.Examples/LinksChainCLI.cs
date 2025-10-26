using System;
using System.IO;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating LinksChain functionality.
    /// This CLI creates a blockchain-like chain structure stored in associative memory.
    /// </summary>
    public class LinksChainCLI : ICommandLineInterface
    {
        private const string DefaultDatabaseFilename = "linkschain.db";

        public void Run(params string[] args)
        {
            var filename = args.Length > 0 ? args[0] : DefaultDatabaseFilename;

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║     LinksChain - Blockchain in Associative DB      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                // Clean up for demonstration purposes
#if DEBUG
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                    Console.WriteLine($"Deleted existing database: {filename}");
                }
#endif

                // Initialize the links database
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(filename, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);

                    Console.WriteLine($"Database initialized: {filename}");

                    // Count initial links
                    var initialCount = syncLinks.Count();
                    Console.WriteLine($"Initial link count: {initialCount}\n");

                    // Run all examples
                    LinksChainExperiment.RunAllExamples(syncLinks);

                    // Display final statistics
                    Console.WriteLine("=== Final Statistics ===");
                    var finalCount = syncLinks.Count();
                    Console.WriteLine($"Total links in database: {finalCount}");

                    // Show all links in the database
                    Console.WriteLine("\n--- All Links in Database ---");
                    syncLinks.Each(link =>
                    {
                        Console.WriteLine($"{link[syncLinks.Constants.IndexPart]}: [{link[syncLinks.Constants.SourcePart]}] -> [{link[syncLinks.Constants.TargetPart]}]");
                        return syncLinks.Constants.Continue;
                    });
                }

                Console.WriteLine("\nLinksChain demonstration completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
