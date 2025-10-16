using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for importing links from Universal Links String Format.
    /// </summary>
    public class UniversalLinksStringFormatImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var i = 0;
            var linksFile = ConsoleHelpers.GetOrReadArgument(i++, "Links file (will be created or updated)", args);
            var importFrom = ConsoleHelpers.GetOrReadArgument(i++, "Import from", args);
            var useLineNumbers = ConsoleHelpers.GetOrReadArgument(i++, "Use line numbers as link references", args);
            bool.TryParse(useLineNumbers, out bool doUseLineNumbers);

            if (!File.Exists(importFrom))
            {
                Console.WriteLine("Entered import file does not exist.");
            }
            else
            {
                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var importer = new UniversalLinksStringFormatImporter(syncLinks, doUseLineNumbers);
                    try
                    {
                        importer.Import(importFrom, cancellation.Token);
                        Console.WriteLine($"Import completed successfully. Links stored in: {linksFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during import: {ex.Message}");
                    }
                }
            }
        }
    }
}
