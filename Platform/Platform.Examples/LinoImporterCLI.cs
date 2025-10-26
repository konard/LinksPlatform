using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for importing Lino (Links Notation) files.
    /// </summary>
    public class LinoImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var file = ConsoleHelpers.GetOrReadArgument(1, "Lino file", args);

            if (!File.Exists(file))
            {
                Console.WriteLine("Entered Lino file does not exist.");
            }
            else
            {
                const long gb32 = 34359738368;

                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UnitedMemoryLinks<uint>(linksFile, gb32))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var links = memoryAdapter.DecorateWithAutomaticUniquenessAndUsagesResolution();
                    var indexer = new LinoIndexer<uint>(links);
                    var indexingImporter = new LinoImporter<uint>(indexer);
                    indexingImporter.Import(file, cancellation.Token).Wait();

                    if (cancellation.NotRequested)
                    {
                        var cache = indexer.Cache;
                        Console.WriteLine("Frequencies cache ready.");
                        var storage = new LinksLinoStorage<uint>(links, false, cache);
                        var importer = new LinoImporter<uint>(storage);
                        importer.Import(file, cancellation.Token).Wait();
                    }
                }
            }
        }
    }
}
