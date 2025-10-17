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
            var i = 0;
            var linksFile = ConsoleHelpers.GetOrReadArgument(i++, "Links file", args);
            var jsonFile = ConsoleHelpers.GetOrReadArgument(i++, "Json file", args);
            var unicodeMapped = ConsoleHelpers.GetOrReadArgument(i++, "Unicode mapped", args);
            var convertUnicodeLinksToCharacters = ConsoleHelpers.GetOrReadArgument(i++, "Convert unicode links to characters", args);
            bool.TryParse(unicodeMapped, out bool isUnicodeMapped);
            bool.TryParse(convertUnicodeLinksToCharacters, out bool doConvertUnicodeLinksToCharacters);

            using (var cancellation = new ConsoleCancellation())
            using (var links = new UnitedMemoryLinks<ulong>(linksFile))
            {
                var synchronizedLinks = new SynchronizedLinks<ulong>(links);
                var exporter = new JsonExporter();
                Console.WriteLine("Exporting to JSON...");
                exporter.Export(synchronizedLinks, jsonFile, isUnicodeMapped, doConvertUnicodeLinksToCharacters, cancellation.Token);
                Console.WriteLine($"Export completed: {jsonFile}");
            }
        }
    }
}
