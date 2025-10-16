using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    public class JsonImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var file = ConsoleHelpers.GetOrReadArgument(1, "Json file", args);

            if (!File.Exists(file))
            {
                Console.WriteLine("Entered json file does not exist.");
            }
            else
            {
                const long gb32 = 34359738368;

                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UnitedMemoryLinks<uint>(linksFile, gb32))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var links = memoryAdapter.DecorateWithAutomaticUniquenessAndUsagesResolution();
                    var indexer = new HierarchicalIndexer<uint>(links);
                    var indexingImporter = new JsonImporter<uint>(indexer);
                    indexingImporter.Import(file, cancellation.Token).Wait();
                    if (cancellation.NotRequested)
                    {
                        var cache = indexer.Cache;
                        Console.WriteLine("Frequencies cache ready.");
                        var storage = new LinksHierarchicalStorage<uint>(links, false, cache);
                        var importer = new JsonImporter<uint>(storage);
                        importer.Import(file, cancellation.Token).Wait();
                        Console.WriteLine("JSON import completed.");
                    }
                }
            }
        }
    }
}
