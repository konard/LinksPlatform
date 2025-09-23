using System;
using System.IO;
using Platform.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing JSON Schema Generation...");

        var cli = new JsonSchemaGeneratorCLI();
        var samplesDir = Path.Combine(Directory.GetCurrentDirectory(), "examples", "json-samples");
        var outputFile = Path.Combine(Directory.GetCurrentDirectory(), "examples", "generated-schema.json");

        if (!Directory.Exists(samplesDir))
        {
            Console.WriteLine($"Samples directory not found: {samplesDir}");
            return;
        }

        Console.WriteLine($"Input directory: {samplesDir}");
        Console.WriteLine($"Output file: {outputFile}");

        cli.Run(samplesDir, outputFile);
    }
}