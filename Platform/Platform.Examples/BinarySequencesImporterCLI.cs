using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    public class BinarySequencesImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: BinarySequencesImporterCLI <binary-file-path> <db-file-path>");
                return;
            }

            var binaryPath = args[0];
            var dbPath = args[1];

            if (!File.Exists(binaryPath))
            {
                Console.WriteLine($"Error: Binary file not found: {binaryPath}");
                return;
            }

            Console.WriteLine($"Opening/creating database: {dbPath}");
            using var links = new UnitedMemoryLinks<ulong>(dbPath);
            var synchronizedLinks = new SynchronizedLinks<ulong>(links);

            Console.WriteLine($"Importing from binary sequences format: {binaryPath}");
            var importer = new BinarySequencesImporter();
            importer.Import(synchronizedLinks, binaryPath);

            Console.WriteLine($"Import completed successfully. Data loaded into: {dbPath}");
        }
    }
}
