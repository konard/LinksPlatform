using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    public class YamlExporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var yamlFile = ConsoleHelpers.GetOrReadArgument(1, "Yaml file", args);
            var unicodeMapped = ConsoleHelpers.GetOrDefaultArgument(2, "Unicode mapped", false, args);
            var convertUnicodeLinksToCharacters = ConsoleHelpers.GetOrDefaultArgument(3, "Convert unicode links to characters", false, args);

            using (var cancellation = new ConsoleCancellation())
            using (var links = new UnitedMemoryLinks<ulong>(linksFile))
            {
                var synchronizedLinks = new SynchronizedLinks<ulong>(links);
                var exporter = new YamlExporter();
                Console.WriteLine("Exporting to YAML...");
                exporter.Export(synchronizedLinks, yamlFile, unicodeMapped, convertUnicodeLinksToCharacters, cancellation.Token);
                Console.WriteLine($"Export completed: {yamlFile}");
            }
        }
    }
}
