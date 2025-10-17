using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for JSON schema inference.
    /// </summary>
    public class JsonSchemaInferrerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args == null || args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "infer":
                    InferSchemaCommand(args.Skip(1).ToArray());
                    break;

                case "help":
                case "-h":
                case "--help":
                    PrintUsage();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    PrintUsage();
                    break;
            }
        }

        private void InferSchemaCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No input files or JSON strings provided.");
                Console.WriteLine("Usage: infer <file1> [file2] [file3] ... [-o <output-file>] [--use-any]");
                return;
            }

            var files = new List<string>();
            string outputFile = null;
            bool useUnionTypes = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" || args[i] == "--output")
                {
                    if (i + 1 < args.Length)
                    {
                        outputFile = args[++i];
                    }
                    else
                    {
                        Console.WriteLine("Error: -o flag requires an output file path.");
                        return;
                    }
                }
                else if (args[i] == "--use-any")
                {
                    useUnionTypes = false;
                }
                else
                {
                    files.Add(args[i]);
                }
            }

            if (files.Count == 0)
            {
                Console.WriteLine("Error: No input files provided.");
                return;
            }

            try
            {
                var jsonDocuments = new List<string>();

                foreach (var file in files)
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"Warning: File not found: {file}");
                        continue;
                    }

                    var content = File.ReadAllText(file);
                    jsonDocuments.Add(content);
                    Console.WriteLine($"Loaded: {file}");
                }

                if (jsonDocuments.Count == 0)
                {
                    Console.WriteLine("Error: No valid JSON files loaded.");
                    return;
                }

                Console.WriteLine($"\nInferring schema from {jsonDocuments.Count} document(s)...");

                var inferrer = new JsonSchemaInferrer(new JsonSchemaInferrerOptions
                {
                    UseUnionTypes = useUnionTypes
                });

                var schema = inferrer.InferSchema(jsonDocuments);
                var schemaJson = inferrer.ToJsonSchemaString(schema);

                if (outputFile != null)
                {
                    File.WriteAllText(outputFile, schemaJson);
                    Console.WriteLine($"\nSchema written to: {outputFile}");
                }
                else
                {
                    Console.WriteLine("\nInferred JSON Schema:");
                    Console.WriteLine(schemaJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine("JSON Schema Inferrer - Infer JSON schema from multiple JSON documents");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  infer <file1> [file2] [file3] ... [-o <output-file>] [--use-any]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  infer           Infer schema from JSON files");
            Console.WriteLine("  help            Display this help message");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -o, --output    Output file for the inferred schema (optional)");
            Console.WriteLine("  --use-any       Use 'any' type instead of union types for conflicting types");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  infer doc1.json doc2.json doc3.json");
            Console.WriteLine("  infer doc1.json doc2.json -o schema.json");
            Console.WriteLine("  infer *.json --use-any -o inferred-schema.json");
        }
    }
}
