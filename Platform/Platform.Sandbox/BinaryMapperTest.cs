using System;
using System.IO;
using System.Linq;
using Platform.Examples;

namespace Platform.Sandbox
{
    public class BinaryMapperTest
    {
        public static void Run()
        {
            Console.WriteLine("=== Binary Mapper Tests ===\n");

            TestBinarySchema();
            TestBinaryMapper();
            TestBinaryDiff();
            TestSchemaInference();
            TestCommonSchemas();

            Console.WriteLine("\n=== All tests completed ===");
        }

        private static void TestBinarySchema()
        {
            Console.WriteLine("--- Testing BinarySchema ---");

            var schema = new BinarySchema("TestSchema", "Test schema for validation");
            schema.MagicBytes = new byte[] { 0xFF, 0xFE };
            schema.AddField(new BinaryField("Header", "hex", 0, 2));

            var matchingData = new byte[] { 0xFF, 0xFE, 0x01, 0x02 };
            var nonMatchingData = new byte[] { 0x00, 0x00, 0x01, 0x02 };

            Console.WriteLine($"Schema name: {schema.Name}");
            Console.WriteLine($"Matches valid data: {schema.IsMatch(matchingData)}");
            Console.WriteLine($"Matches invalid data: {schema.IsMatch(nonMatchingData)}");
            Console.WriteLine();
        }

        private static void TestBinaryMapper()
        {
            Console.WriteLine("--- Testing BinaryMapper ---");

            // Create test schema
            var schema = new BinarySchema("SimpleHeader");
            schema.AddField(new BinaryField("Magic", "hex", 0, 2));
            schema.AddField(new BinaryField("Version", "uint16", 2, 2));
            schema.AddField(new BinaryField("Size", "uint32", 4, 4));

            // Create test data
            var testData = new byte[]
            {
                0xCA, 0xFE,                     // Magic
                0x01, 0x00,                     // Version (1)
                0x64, 0x00, 0x00, 0x00         // Size (100)
            };

            var mapper = new BinaryMapper(schema);
            var result = mapper.MapData(testData);

            Console.WriteLine("Mapped data:");
            Console.WriteLine(mapper.FormatMapping(result));
        }

        private static void TestBinaryDiff()
        {
            Console.WriteLine("--- Testing BinaryDiff ---");

            var data1 = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            var data2 = new byte[] { 0x01, 0xFF, 0x03, 0x04, 0x06 };

            var differ = new BinaryDiff();
            var differences = differ.CompareBinaryData(data1, data2);

            Console.WriteLine($"Found {differences.Count} differences:");
            foreach (var diff in differences)
            {
                Console.WriteLine($"  {diff}");
            }
            Console.WriteLine();
        }

        private static void TestSchemaInference()
        {
            Console.WriteLine("--- Testing Schema Inference ---");

            // Create similar binary files
            var file1 = new byte[] { 0xCA, 0xFE, 0x01, 0x00, 0x10, 0x20, 0x30 };
            var file2 = new byte[] { 0xCA, 0xFE, 0x01, 0x00, 0x40, 0x50, 0x60 };
            var file3 = new byte[] { 0xCA, 0xFE, 0x01, 0x00, 0x70, 0x80, 0x90 };

            var inference = new SchemaInference();
            var schema = inference.InferSchemaFromData(new[] { file1, file2, file3 }, "InferredTest");

            Console.WriteLine($"Inferred schema: {schema.Name}");
            Console.WriteLine($"Description: {schema.Description}");
            Console.WriteLine($"Fields: {schema.Fields.Count}");
            foreach (var field in schema.Fields)
            {
                Console.WriteLine($"  - {field.Name}: offset={field.Offset}, size={field.Size}, type={field.Type}");
            }
            Console.WriteLine();
        }

        private static void TestCommonSchemas()
        {
            Console.WriteLine("--- Testing Common Schemas ---");

            // Test PNG detection
            var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var pngSchema = CommonBinarySchemas.DetectSchema(pngHeader);
            Console.WriteLine($"PNG detection: {pngSchema?.Name ?? "Not detected"}");

            // Test BMP detection
            var bmpHeader = new byte[] { 0x42, 0x4D };
            var bmpSchema = CommonBinarySchemas.DetectSchema(bmpHeader);
            Console.WriteLine($"BMP detection: {bmpSchema?.Name ?? "Not detected"}");

            // Test ZIP detection
            var zipHeader = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
            var zipSchema = CommonBinarySchemas.DetectSchema(zipHeader);
            Console.WriteLine($"ZIP detection: {zipSchema?.Name ?? "Not detected"}");

            // Test ELF detection
            var elfHeader = new byte[] { 0x7F, 0x45, 0x4C, 0x46 };
            var elfSchema = CommonBinarySchemas.DetectSchema(elfHeader);
            Console.WriteLine($"ELF detection: {elfSchema?.Name ?? "Not detected"}");

            Console.WriteLine();
        }
    }
}
