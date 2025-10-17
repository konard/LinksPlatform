using System;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    public class BinaryMapperCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "map":
                    MapCommand(args.Skip(1).ToArray());
                    break;

                case "diff":
                    DiffCommand(args.Skip(1).ToArray());
                    break;

                case "infer":
                    InferCommand(args.Skip(1).ToArray());
                    break;

                case "detect":
                    DetectCommand(args.Skip(1).ToArray());
                    break;

                case "help":
                case "--help":
                case "-h":
                    PrintUsage();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    PrintUsage();
                    break;
            }
        }

        private void MapCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: map <file> [schema]");
                Console.WriteLine("  schema: png, bmp, pdf, zip, elf, or auto (default)");
                return;
            }

            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            var schemaName = args.Length > 1 ? args[1].ToLowerInvariant() : "auto";
            var data = File.ReadAllBytes(filePath);

            BinarySchema schema;
            if (schemaName == "auto")
            {
                schema = CommonBinarySchemas.DetectSchema(data);
                if (schema == null)
                {
                    Console.WriteLine("Unable to detect file format. Please specify a schema.");
                    return;
                }
                Console.WriteLine($"Detected format: {schema.Name}");
            }
            else
            {
                schema = CommonBinarySchemas.GetSchemaByExtension($".{schemaName}");
                if (schema == null)
                {
                    Console.WriteLine($"Unknown schema: {schemaName}");
                    return;
                }
            }

            var mapper = new BinaryMapper(schema);
            var mapping = mapper.MapData(data);
            Console.WriteLine(mapper.FormatMapping(mapping));
        }

        private void DiffCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: diff <file1> <file2> [schema]");
                Console.WriteLine("  schema: png, bmp, pdf, zip, elf (optional)");
                return;
            }

            var file1 = args[0];
            var file2 = args[1];

            if (!File.Exists(file1))
            {
                Console.WriteLine($"File not found: {file1}");
                return;
            }
            if (!File.Exists(file2))
            {
                Console.WriteLine($"File not found: {file2}");
                return;
            }

            var differ = new BinaryDiff();

            if (args.Length > 2)
            {
                var schemaName = args[2].ToLowerInvariant();
                var schema = CommonBinarySchemas.GetSchemaByExtension($".{schemaName}");
                if (schema == null)
                {
                    Console.WriteLine($"Unknown schema: {schemaName}");
                    return;
                }

                var result = differ.CompareWithSchema(file1, file2, schema);
                var mapper = new BinaryMapper(schema);
                Console.WriteLine(mapper.FormatMapping(result));
            }
            else
            {
                var differences = differ.CompareBinaryFiles(file1, file2);
                Console.WriteLine(differ.FormatDifferences(differences));
            }
        }

        private void InferCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: infer <file1> <file2> [file3...]");
                Console.WriteLine("  Infer schema from 2 or more similar files");
                return;
            }

            foreach (var file in args)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"File not found: {file}");
                    return;
                }
            }

            var inference = new SchemaInference();
            var schema = inference.InferSchemaFromFiles(args, "InferredSchema");

            Console.WriteLine($"Inferred Schema: {schema.Name}");
            Console.WriteLine($"Description: {schema.Description}");
            Console.WriteLine();

            if (schema.MagicBytes != null && schema.MagicBytes.Length > 0)
            {
                Console.WriteLine($"Magic Bytes: {BitConverter.ToString(schema.MagicBytes).Replace("-", " ")}");
            }

            Console.WriteLine($"\nFields ({schema.Fields.Count}):");
            foreach (var field in schema.Fields)
            {
                Console.WriteLine($"  {field.Name} (offset: {field.Offset}, size: {field.Size}, type: {field.Type})");
                if (!string.IsNullOrEmpty(field.Description))
                {
                    Console.WriteLine($"    {field.Description}");
                }
            }
        }

        private void DetectCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: detect <file>");
                return;
            }

            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            var data = File.ReadAllBytes(filePath);
            var schema = CommonBinarySchemas.DetectSchema(data);

            if (schema == null)
            {
                Console.WriteLine("Unable to detect file format.");
                Console.WriteLine($"First 16 bytes: {BitConverter.ToString(data.Take(16).ToArray()).Replace("-", " ")}");
            }
            else
            {
                Console.WriteLine($"Detected format: {schema.Name}");
                Console.WriteLine($"Description: {schema.Description}");
                if (!string.IsNullOrEmpty(schema.FileExtension))
                {
                    Console.WriteLine($"Extension: {schema.FileExtension}");
                }
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine("Binary Mapper - Map and compare binary files using schemas");
            Console.WriteLine();
            Console.WriteLine("Usage: BinaryMapperCLI <command> [arguments]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  map <file> [schema]           Map binary file using schema");
            Console.WriteLine("  diff <file1> <file2> [schema] Compare two binary files");
            Console.WriteLine("  infer <file1> <file2> [...]   Infer schema from multiple files");
            Console.WriteLine("  detect <file>                 Detect file format");
            Console.WriteLine("  help                          Show this help message");
            Console.WriteLine();
            Console.WriteLine("Supported schemas: png, bmp, pdf, zip, elf");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  BinaryMapperCLI map image.png");
            Console.WriteLine("  BinaryMapperCLI diff old.bin new.bin");
            Console.WriteLine("  BinaryMapperCLI infer file1.dat file2.dat file3.dat");
            Console.WriteLine("  BinaryMapperCLI detect unknown.bin");
        }
    }
}
