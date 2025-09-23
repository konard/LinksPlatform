using System;
using Platform.Examples;
using Xunit;

namespace Platform.Examples.Tests
{
    /// <summary>
    /// Unit tests for the JsonSchemaGenerator class.
    /// </summary>
    public class JsonSchemaGeneratorTests
    {
        [Fact]
        public void AnalyzeSample_WithValidJson_ShouldNotThrow()
        {
            var generator = new JsonSchemaGenerator();
            var jsonSample = """
                {
                  "id": 1,
                  "name": "John Doe"
                }
                """;

            var exception = Record.Exception(() => generator.AnalyzeSample(jsonSample));

            Assert.Null(exception);
        }

        [Fact]
        public void AnalyzeSample_WithInvalidJson_ShouldThrowArgumentException()
        {
            var generator = new JsonSchemaGenerator();
            var invalidJson = "{ invalid json }";

            Assert.Throws<ArgumentException>(() => generator.AnalyzeSample(invalidJson));
        }

        [Fact]
        public void GenerateSchema_WithObjectSample_ShouldReturnValidSchema()
        {
            var generator = new JsonSchemaGenerator();
            var jsonSample = """
                {
                  "id": 1,
                  "name": "John Doe",
                  "email": "john@example.com",
                  "isActive": true
                }
                """;

            generator.AnalyzeSample(jsonSample);
            var schema = generator.GenerateSchema();

            Assert.Contains("\"type\": \"object\"", schema);
            Assert.Contains("\"id\"", schema);
            Assert.Contains("\"name\"", schema);
            Assert.Contains("\"email\"", schema);
            Assert.Contains("\"isActive\"", schema);
            Assert.Contains("\"format\": \"email\"", schema);
        }

        [Fact]
        public void GenerateSchema_WithMultipleSamples_ShouldMergeProperties()
        {
            var generator = new JsonSchemaGenerator();
            var sample1 = """
                {
                  "id": 1,
                  "name": "John Doe"
                }
                """;
            var sample2 = """
                {
                  "id": 2,
                  "name": "Jane Smith",
                  "phone": "+1-555-0123"
                }
                """;

            generator.AnalyzeSample(sample1);
            generator.AnalyzeSample(sample2);
            var schema = generator.GenerateSchema();

            Assert.Contains("\"id\"", schema);
            Assert.Contains("\"name\"", schema);
            Assert.Contains("\"phone\"", schema);
        }

        [Fact]
        public void GenerateSchema_WithNestedObject_ShouldHandleNesting()
        {
            var generator = new JsonSchemaGenerator();
            var jsonSample = """
                {
                  "user": {
                    "name": "John",
                    "age": 30
                  }
                }
                """;

            generator.AnalyzeSample(jsonSample);
            var schema = generator.GenerateSchema();

            Assert.Contains("\"user\"", schema);
            Assert.Contains("\"properties\"", schema);
            Assert.Contains("\"name\"", schema);
            Assert.Contains("\"age\"", schema);
        }

        [Fact]
        public void GenerateSchema_WithArray_ShouldHandleArrayItems()
        {
            var generator = new JsonSchemaGenerator();
            var jsonSample = """
                {
                  "hobbies": ["reading", "cycling"]
                }
                """;

            generator.AnalyzeSample(jsonSample);
            var schema = generator.GenerateSchema();

            Assert.Contains("\"hobbies\"", schema);
            Assert.Contains("\"type\": \"array\"", schema);
            Assert.Contains("\"items\"", schema);
        }
    }
}