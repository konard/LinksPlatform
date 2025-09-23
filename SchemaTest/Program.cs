using Platform.Examples;

Console.WriteLine("=== Testing JSON Schema Generator CLI ===");

try
{
    var cli = new JsonSchemaGeneratorCLI();

    // Test with our sample JSON files directory
    var samplesDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "examples", "json-samples");
    var outputFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "examples", "generated-schema.json");

    Console.WriteLine($"Input directory: {samplesDir}");
    Console.WriteLine($"Output file: {outputFile}");

    // Run the CLI with our test arguments
    cli.Run(samplesDir, outputFile);

    Console.WriteLine("\n=== CLI Test completed! ===");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}
