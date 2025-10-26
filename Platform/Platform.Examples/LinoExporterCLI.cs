using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for exporting Links to Lino (Links Notation) format.
    /// </summary>
    public class LinoExporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var outputFile = ConsoleHelpers.GetOrReadArgument(1, "Output Lino file", args);
            var unicodeMappedStr = ConsoleHelpers.GetOrReadArgument(2, "Unicode mapped (true/false)", args);
            var convertToTextStr = ConsoleHelpers.GetOrReadArgument(3, "Convert unicode to text (true/false)", args);

            if (!File.Exists(linksFile))
            {
                Console.WriteLine("Entered links file does not exist.");
            }
            else
            {
                bool.TryParse(unicodeMappedStr, out var unicodeMapped);
                bool.TryParse(convertToTextStr, out var convertToText);

                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var exporter = new LinoExporter();
                    exporter.Export(syncLinks, outputFile, unicodeMapped, convertToText, cancellation.Token);
                    Console.WriteLine($"Export completed. Output file: {outputFile}");
                }
            }
        }
    }
}
