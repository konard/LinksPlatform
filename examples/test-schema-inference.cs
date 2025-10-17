using System;
using System.Collections.Generic;
using System.IO;
using Platform.Examples;

namespace SchemaInferenceTest
{
    /// <summary>
    /// Test script to demonstrate and verify JSON schema inference functionality.
    /// This script can be run to test the schema inference algorithm with sample data.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== JSON Schema Inference Test ===\n");

            // Test 1: Simple schema inference from multiple documents
            Test1_BasicSchemaInference();

            // Test 2: Schema inference with optional fields
            Test2_OptionalFields();

            // Test 3: Schema inference with arrays
            Test3_ArrayHandling();

            // Test 4: Schema inference with type conflicts
            Test4_TypeConflicts();

            // Test 5: Schema inference with nested objects
            Test5_NestedObjects();

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        static void Test1_BasicSchemaInference()
        {
            Console.WriteLine("Test 1: Basic Schema Inference");
            Console.WriteLine("--------------------------------");

            var json1 = @"{""name"": ""John"", ""age"": 30}";
            var json2 = @"{""name"": ""Jane"", ""age"": 25}";

            var inferrer = new JsonSchemaInferrer();
            var schema = inferrer.InferSchema(new[] { json1, json2 });
            var schemaJson = inferrer.ToJsonSchemaString(schema);

            Console.WriteLine("Input documents:");
            Console.WriteLine(json1);
            Console.WriteLine(json2);
            Console.WriteLine("\nInferred schema:");
            Console.WriteLine(schemaJson);
            Console.WriteLine();
        }

        static void Test2_OptionalFields()
        {
            Console.WriteLine("Test 2: Optional Fields");
            Console.WriteLine("------------------------");

            var json1 = @"{""id"": 1, ""name"": ""John"", ""email"": ""john@example.com""}";
            var json2 = @"{""id"": 2, ""name"": ""Jane""}";

            var inferrer = new JsonSchemaInferrer();
            var schema = inferrer.InferSchema(new[] { json1, json2 });
            var schemaJson = inferrer.ToJsonSchemaString(schema);

            Console.WriteLine("Input documents:");
            Console.WriteLine(json1);
            Console.WriteLine(json2);
            Console.WriteLine("\nInferred schema (email should not be required):");
            Console.WriteLine(schemaJson);
            Console.WriteLine();
        }

        static void Test3_ArrayHandling()
        {
            Console.WriteLine("Test 3: Array Handling");
            Console.WriteLine("----------------------");

            var json1 = @"{""items"": [1, 2, 3]}";
            var json2 = @"{""items"": [4, 5]}";

            var inferrer = new JsonSchemaInferrer();
            var schema = inferrer.InferSchema(new[] { json1, json2 });
            var schemaJson = inferrer.ToJsonSchemaString(schema);

            Console.WriteLine("Input documents:");
            Console.WriteLine(json1);
            Console.WriteLine(json2);
            Console.WriteLine("\nInferred schema:");
            Console.WriteLine(schemaJson);
            Console.WriteLine();
        }

        static void Test4_TypeConflicts()
        {
            Console.WriteLine("Test 4: Type Conflicts");
            Console.WriteLine("----------------------");

            var json1 = @"{""value"": 123}";
            var json2 = @"{""value"": ""text""}";

            var inferrer = new JsonSchemaInferrer(new JsonSchemaInferrerOptions { UseUnionTypes = true });
            var schema = inferrer.InferSchema(new[] { json1, json2 });
            var schemaJson = inferrer.ToJsonSchemaString(schema);

            Console.WriteLine("Input documents:");
            Console.WriteLine(json1);
            Console.WriteLine(json2);
            Console.WriteLine("\nInferred schema (with union types):");
            Console.WriteLine(schemaJson);
            Console.WriteLine();
        }

        static void Test5_NestedObjects()
        {
            Console.WriteLine("Test 5: Nested Objects");
            Console.WriteLine("-----------------------");

            var json1 = @"{""user"": {""name"": ""John"", ""age"": 30}, ""status"": ""active""}";
            var json2 = @"{""user"": {""name"": ""Jane"", ""age"": 25}, ""status"": ""inactive""}";

            var inferrer = new JsonSchemaInferrer();
            var schema = inferrer.InferSchema(new[] { json1, json2 });
            var schemaJson = inferrer.ToJsonSchemaString(schema);

            Console.WriteLine("Input documents:");
            Console.WriteLine(json1);
            Console.WriteLine(json2);
            Console.WriteLine("\nInferred schema:");
            Console.WriteLine(schemaJson);
            Console.WriteLine();
        }
    }
}
