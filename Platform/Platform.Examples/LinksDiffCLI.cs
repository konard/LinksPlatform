using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for comparing two links databases and detecting differences.
    /// </summary>
    public class LinksDiffCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var i = 0;
            var oldLinksFile = ConsoleHelpers.GetOrReadArgument(i++, "Old links file (baseline)", args);
            var newLinksFile = ConsoleHelpers.GetOrReadArgument(i++, "New links file (current)", args);
            var outputFile = ConsoleHelpers.GetOrReadArgument(i++, "Output diff file", args);

            if (!File.Exists(oldLinksFile))
            {
                Console.WriteLine($"Error: Old links file '{oldLinksFile}' does not exist.");
                return;
            }

            if (!File.Exists(newLinksFile))
            {
                Console.WriteLine($"Error: New links file '{newLinksFile}' does not exist.");
                return;
            }

            try
            {
                // Create output file
                File.Create(outputFile).Dispose();

                if (!File.Exists(outputFile))
                {
                    Console.WriteLine($"Error: Cannot create output file '{outputFile}'.");
                    return;
                }

                Console.WriteLine("Loading old links database...");
                using (var cancellation = new ConsoleCancellation())
                using (var oldMemoryAdapter = new UInt64UnitedMemoryLinks(oldLinksFile))
                using (var oldLinksStore = new UInt64Links(oldMemoryAdapter))
                {
                    Console.WriteLine("Loading new links database...");
                    using (var newMemoryAdapter = new UInt64UnitedMemoryLinks(newLinksFile))
                    using (var newLinksStore = new UInt64Links(newMemoryAdapter))
                    {
                        Console.WriteLine("Comparing databases...");
                        Console.WriteLine("Press CTRL+C to stop.");

                        var oldLinks = new SynchronizedLinks<ulong>(oldLinksStore);
                        var newLinks = new SynchronizedLinks<ulong>(newLinksStore);

                        var diff = new LinksDiff();
                        diff.CompareAndExport(oldLinks, newLinks, outputFile, cancellation.Token);

                        Console.WriteLine($"Diff report written to: {outputFile}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
