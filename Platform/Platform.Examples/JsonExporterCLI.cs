using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    public class JsonExporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var jsonFile = ConsoleHelpers.GetOrReadArgument(1, "Json file", args);
            var unicodeMapped = ConsoleHelpers.GetOrDefaultArgument(2, "Unicode mapped", false, args);
            var convertUnicodeLinksToCharacters = ConsoleHelpers.GetOrDefaultArgument(3, "Convert unicode links to characters", false, args);

            using (var cancellation = new ConsoleCancellation())
            using (var links = new UnitedMemoryLinks<ulong>(linksFile))
            {
                var synchronizedLinks = new SynchronizedLinks<ulong>(links);
                var exporter = new JsonExporter();
                Console.WriteLine("Exporting to JSON...");
                exporter.Export(synchronizedLinks, jsonFile, unicodeMapped, convertUnicodeLinksToCharacters, cancellation.Token);
                Console.WriteLine($"Export completed: {jsonFile}");
            }
        }
    }
}
