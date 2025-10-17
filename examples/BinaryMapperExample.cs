using System;
using System.IO;
using Platform.Examples;

namespace Examples
{
    /// <summary>
    /// Example usage of the Binary Mapper tool
    /// Demonstrates how to map, compare, and infer schemas from binary files
    /// </summary>
    public class BinaryMapperExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Binary Mapper Example ===\n");

            // Example 1: Detect and map a binary file
            Example1_DetectAndMap();

            // Example 2: Compare two binary files
            Example2_CompareBinaryFiles();

            // Example 3: Infer schema from multiple files
            Example3_InferSchema();

            // Example 4: Create custom schema
            Example4_CustomSchema();

            Console.WriteLine("\n=== Examples completed ===");
        }

        private static void Example1_DetectAndMap()
        {
            Console.WriteLine("Example 1: Detect and Map Binary File");
            Console.WriteLine("---------------------------------------");

            // Create a sample PNG-like file
            var pngData = new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,  // PNG signature
                0x00, 0x00, 0x00, 0x0D,                          // IHDR length
                0x49, 0x48, 0x44, 0x52,                          // "IHDR"
                0x00, 0x00, 0x02, 0x00,                          // Width: 512
                0x00, 0x00, 0x01, 0x80,                          // Height: 384
                0x08,                                             // Bit depth: 8
                0x02,                                             // Color type: 2 (RGB)
                0x00,                                             // Compression: 0
                0x00,                                             // Filter: 0
                0x00                                              // Interlace: 0
            };

            // Detect schema
            var schema = CommonBinarySchemas.DetectSchema(pngData);
            if (schema != null)
            {
                Console.WriteLine($"Detected: {schema.Name}");

                // Map the file
                var mapper = new BinaryMapper(schema);
                var mapping = mapper.MapData(pngData);
                Console.WriteLine(mapper.FormatMapping(mapping));
            }
        }

        private static void Example2_CompareBinaryFiles()
        {
            Console.WriteLine("Example 2: Compare Binary Files");
            Console.WriteLine("--------------------------------");

            var version1 = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            var version2 = new byte[] { 0x01, 0x02, 0xFF, 0x04, 0x06 };

            var differ = new BinaryDiff();
            var differences = differ.CompareBinaryData(version1, version2);

            Console.WriteLine($"Found {differences.Count} differences:");
            Console.WriteLine(differ.FormatDifferences(differences, 10));
        }

        private static void Example3_InferSchema()
        {
            Console.WriteLine("Example 3: Infer Schema from Multiple Files");
            Console.WriteLine("--------------------------------------------");

            // Create similar binary files with common structure
            var file1 = new byte[] { 0xCA, 0xFE, 0x01, 0x00, 0xAA, 0xBB, 0xCC };
            var file2 = new byte[] { 0xCA, 0xFE, 0x01, 0x00, 0x11, 0x22, 0x33 };
            var file3 = new byte[] { 0xCA, 0xFE, 0x01, 0x00, 0xFF, 0xEE, 0xDD };

            var inference = new SchemaInference();
            var schema = inference.InferSchemaFromData(
                new[] { file1, file2, file3 },
                "CustomFormat"
            );

            Console.WriteLine($"Inferred Schema: {schema.Name}");
            Console.WriteLine($"Description: {schema.Description}");
            Console.WriteLine($"\nFields ({schema.Fields.Count}):");
            foreach (var field in schema.Fields)
            {
                Console.WriteLine($"  {field.Name}:");
                Console.WriteLine($"    Offset: {field.Offset}");
                Console.WriteLine($"    Size: {field.Size}");
                Console.WriteLine($"    Type: {field.Type}");
                if (!string.IsNullOrEmpty(field.Description))
                {
                    Console.WriteLine($"    Description: {field.Description}");
                }
            }
            Console.WriteLine();
        }

        private static void Example4_CustomSchema()
        {
            Console.WriteLine("Example 4: Create Custom Schema");
            Console.WriteLine("--------------------------------");

            // Define a custom binary format schema
            var schema = new BinarySchema(
                "CustomHeader",
                "Custom file format with header",
                ".cst",
                new byte[] { 0x43, 0x53, 0x54, 0x00 } // "CST\0"
            );

            schema.AddField(new BinaryField("Magic", "ascii", 0, 4, "File signature"));
            schema.AddField(new BinaryField("Version", "uint16", 4, 2, "Format version"));
            schema.AddField(new BinaryField("Flags", "uint16", 6, 2, "Feature flags"));
            schema.AddField(new BinaryField("DataSize", "uint32", 8, 4, "Data section size"));
            schema.AddField(new BinaryField("Checksum", "uint32", 12, 4, "CRC32 checksum"));

            // Create sample data matching the schema
            var customData = new byte[]
            {
                0x43, 0x53, 0x54, 0x00,          // Magic: "CST\0"
                0x01, 0x00,                      // Version: 1
                0x03, 0x00,                      // Flags: 3
                0x00, 0x10, 0x00, 0x00,         // DataSize: 4096
                0xAB, 0xCD, 0xEF, 0x12          // Checksum: 0x12EFCDAB
            };

            // Map the data
            var mapper = new BinaryMapper(schema);
            var mapping = mapper.MapData(customData);

            Console.WriteLine("Custom format mapped:");
            Console.WriteLine(mapper.FormatMapping(mapping));
        }
    }
}
