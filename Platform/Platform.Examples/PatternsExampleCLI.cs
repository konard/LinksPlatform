using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating pattern matching examples.
    /// Addresses issue #110: Patterns
    /// </summary>
    public class PatternsExampleCLI : ICommandLineInterface
    {
        private const string DefaultDatabaseFile = "patterns-example.links";

        public void Run(params string[] args)
        {
            var databaseFile = args.Length > 0 ? args[0] : DefaultDatabaseFile;

            Console.WriteLine($"LinksPlatform Pattern Matching Examples");
            Console.WriteLine($"Database: {databaseFile}\n");

            // Clean up previous database for fresh start
            if (File.Exists(databaseFile))
            {
                File.Delete(databaseFile);
            }

            // Initialize links database
            using (var memoryManager = new UInt64UnitedMemoryLinks(databaseFile, 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryManager))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);

                // Initialize Unicode support
                syncLinks.UseUnicode();

                // Create sequences handler
                var sequences = new Sequences(syncLinks);

                // Create and run examples
                var patternsExample = new PatternsExample(syncLinks, sequences);
                patternsExample.DemonstrateAllPatterns();

                var totalLinks = syncLinks.Count(new Link<ulong>(syncLinks.Constants.Any, syncLinks.Constants.Any, syncLinks.Constants.Any));
                Console.WriteLine($"\nTotal links in database: {totalLinks}");
                Console.WriteLine($"Database file: {databaseFile}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
