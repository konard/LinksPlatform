using System;
using System.IO;
using Platform.Examples;

class TestSchemaGenerator
{
    static void Main()
    {
        Console.WriteLine("=== Testing JSON Schema Generator ===");

        try
        {
            var generator = new JsonSchemaGenerator();

            // Test with sample JSON files
            var sampleFiles = new string[]
            {
                """
                {
                  "id": 1,
                  "name": "John Doe",
                  "email": "john@example.com",
                  "age": 30,
                  "isActive": true
                }
                """,
                """
                {
                  "id": 2,
                  "name": "Jane Smith",
                  "email": "jane@example.com",
                  "age": 25,
                  "isActive": false,
                  "phone": "+1-555-0123"
                }
                """
            };

            Console.WriteLine("Analyzing samples...");
            foreach (var sample in sampleFiles)
            {
                generator.AnalyzeSample(sample);
            }

            Console.WriteLine("Generating schema...");
            var schema = generator.GenerateSchema();

            Console.WriteLine("\nGenerated Schema:");
            Console.WriteLine("=================");
            Console.WriteLine(schema);

            Console.WriteLine("\n=== Test completed successfully! ===");
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