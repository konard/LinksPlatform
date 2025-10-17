using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    public class BinarySequencesExporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: BinarySequencesExporterCLI <db-file-path> <output-file-path>");
                return;
            }

            var dbPath = args[0];
            var outputPath = args[1];

            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"Error: Database file not found: {dbPath}");
                return;
            }

            Console.WriteLine($"Opening database: {dbPath}");
            using (var links = new UnitedMemoryLinks<ulong>(dbPath))
            {
                var synchronizedLinks = new SynchronizedLinks<ulong>(links);

                Console.WriteLine($"Exporting to binary sequences format: {outputPath}");
                var exporter = new BinarySequencesExporter();
                exporter.Export(synchronizedLinks, outputPath);

                Console.WriteLine($"Export completed successfully. File saved to: {outputPath}");
            }
        }
    }
}
