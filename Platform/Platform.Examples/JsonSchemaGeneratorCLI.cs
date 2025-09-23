using System;
using System.IO;
using System.Linq;
using Platform.IO;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for generating JSON Schema from multiple JSON samples.
    /// </summary>
    public class JsonSchemaGeneratorCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var i = 0;
            var inputPath = ConsoleHelpers.GetOrReadArgument(i++, "Input path (file or directory with JSON samples)", args);
            var outputPath = ConsoleHelpers.GetOrReadArgument(i++, "Output path for generated schema", args);

            try
            {
                var generator = new JsonSchemaGenerator();
                var sampleCount = 0;

                if (File.Exists(inputPath))
                {
                    // Single file
                    Console.WriteLine($"Analyzing JSON file: {inputPath}");
                    var content = File.ReadAllText(inputPath);
                    generator.AnalyzeSample(content);
                    sampleCount = 1;
                }
                else if (Directory.Exists(inputPath))
                {
                    // Directory with multiple JSON files
                    var jsonFiles = Directory.GetFiles(inputPath, "*.json", SearchOption.AllDirectories);

                    if (!jsonFiles.Any())
                    {
                        Console.WriteLine("No JSON files found in the specified directory.");
                        return;
                    }

                    Console.WriteLine($"Found {jsonFiles.Length} JSON files. Analyzing...");

                    foreach (var file in jsonFiles)
                    {
                        try
                        {
                            Console.WriteLine($"  Processing: {Path.GetFileName(file)}");
                            var content = File.ReadAllText(file);
                            generator.AnalyzeSample(content);
                            sampleCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Error processing {file}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Specified input path does not exist.");
                    return;
                }

                if (sampleCount == 0)
                {
                    Console.WriteLine("No valid JSON samples were processed.");
                    return;
                }

                Console.WriteLine($"Generating schema from {sampleCount} sample(s)...");
                var schema = generator.GenerateSchema();

                // Ensure output directory exists
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                File.WriteAllText(outputPath, schema);
                Console.WriteLine($"Schema generated successfully and saved to: {outputPath}");

                // Display the generated schema
                Console.WriteLine("\nGenerated JSON Schema:");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine(schema);
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
    }
}