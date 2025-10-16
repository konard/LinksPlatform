using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    public class FileSystemImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var path = ConsoleHelpers.GetOrReadArgument(1, "Path to import", args);
            var includeContents = ConsoleHelpers.GetOrReadArgument(2, "Include file contents? (yes/no)", args);

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Console.WriteLine("Entered path does not exist.");
            }
            else
            {
                const long gb32 = 34359738368;
                bool shouldIncludeContents = includeContents.ToLower() == "yes" || includeContents.ToLower() == "y";

                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UnitedMemoryLinks<uint>(linksFile, gb32))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var links = memoryAdapter.DecorateWithAutomaticUniquenessAndUsagesResolution();
                    var indexer = new HierarchicalIndexer<uint>(links);
                    var cache = indexer.Cache;
                    Console.WriteLine("Frequencies cache ready.");
                    var storage = new LinksHierarchicalStorage<uint>(links, false, cache);
                    var importer = new FileSystemImporter<uint>(storage, shouldIncludeContents);
                    importer.Import(path, cancellation.Token).Wait();
                    Console.WriteLine("File system import completed.");
                }
            }
        }
    }
}
