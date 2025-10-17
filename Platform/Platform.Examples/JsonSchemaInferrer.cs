using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Platform.Examples
{
    /// <summary>
    /// Infers a JSON schema from multiple JSON document examples.
    /// Uses a merge-based approach where individual schemas are inferred and then merged.
    /// </summary>
    public class JsonSchemaInferrer
    {
        private readonly JsonSchemaInferrerOptions _options;

        public JsonSchemaInferrer() : this(new JsonSchemaInferrerOptions()) { }

        public JsonSchemaInferrer(JsonSchemaInferrerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Infers a schema from multiple JSON document strings.
        /// </summary>
        public JsonSchema InferSchema(IEnumerable<string> jsonDocuments)
        {
            if (jsonDocuments == null || !jsonDocuments.Any())
            {
                throw new ArgumentException("At least one JSON document is required for schema inference.", nameof(jsonDocuments));
            }

            JsonSchema mergedSchema = null;

            foreach (var jsonDoc in jsonDocuments)
            {
                using var document = JsonDocument.Parse(jsonDoc);
                var schema = InferSchemaFromElement(document.RootElement);

                mergedSchema = mergedSchema == null ? schema : MergeSchemas(mergedSchema, schema);
            }

            return mergedSchema;
        }

        /// <summary>
        /// Infers schema from a single JSON element.
        /// </summary>
        private JsonSchema InferSchemaFromElement(JsonElement element)
        {
            var schema = new JsonSchema();

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    schema.Type = JsonSchemaType.Object;
                    schema.Properties = new Dictionary<string, JsonSchema>();
                    schema.Required = new HashSet<string>();

                    foreach (var property in element.EnumerateObject())
                    {
                        schema.Properties[property.Name] = InferSchemaFromElement(property.Value);
                        schema.Required.Add(property.Name);
                    }
                    break;

                case JsonValueKind.Array:
                    schema.Type = JsonSchemaType.Array;
                    var itemSchemas = new List<JsonSchema>();

                    foreach (var item in element.EnumerateArray())
                    {
                        itemSchemas.Add(InferSchemaFromElement(item));
                    }

                    // Merge all item schemas to find common structure
                    if (itemSchemas.Count > 0)
                    {
                        schema.Items = itemSchemas[0];
                        for (int i = 1; i < itemSchemas.Count; i++)
                        {
                            schema.Items = MergeSchemas(schema.Items, itemSchemas[i]);
                        }
                    }
                    else
                    {
                        // Empty array - default to allowing any type
                        schema.Items = new JsonSchema { Type = JsonSchemaType.Any };
                    }
                    break;

                case JsonValueKind.String:
                    schema.Type = JsonSchemaType.String;
                    break;

                case JsonValueKind.Number:
                    // Determine if integer or number
                    if (element.TryGetInt64(out _))
                    {
                        schema.Type = JsonSchemaType.Integer;
                    }
                    else
                    {
                        schema.Type = JsonSchemaType.Number;
                    }
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    schema.Type = JsonSchemaType.Boolean;
                    break;

                case JsonValueKind.Null:
                    schema.Type = JsonSchemaType.Null;
                    break;

                default:
                    schema.Type = JsonSchemaType.Any;
                    break;
            }

            return schema;
        }

        /// <summary>
        /// Merges two schemas into a single schema that encompasses both.
        /// </summary>
        private JsonSchema MergeSchemas(JsonSchema schema1, JsonSchema schema2)
        {
            if (schema1 == null) return schema2;
            if (schema2 == null) return schema1;

            var merged = new JsonSchema();

            // If types are different, create a union type or use "any"
            if (schema1.Type != schema2.Type)
            {
                if (_options.UseUnionTypes)
                {
                    merged.Type = JsonSchemaType.Union;
                    merged.UnionTypes = new HashSet<JsonSchemaType> { schema1.Type, schema2.Type };
                }
                else
                {
                    merged.Type = JsonSchemaType.Any;
                }
                return merged;
            }

            merged.Type = schema1.Type;

            // Merge based on type
            switch (merged.Type)
            {
                case JsonSchemaType.Object:
                    merged.Properties = new Dictionary<string, JsonSchema>();
                    merged.Required = new HashSet<string>();

                    // Merge properties
                    var allKeys = new HashSet<string>();
                    if (schema1.Properties != null) allKeys.UnionWith(schema1.Properties.Keys);
                    if (schema2.Properties != null) allKeys.UnionWith(schema2.Properties.Keys);

                    foreach (var key in allKeys)
                    {
                        var hasInSchema1 = schema1.Properties?.ContainsKey(key) ?? false;
                        var hasInSchema2 = schema2.Properties?.ContainsKey(key) ?? false;

                        if (hasInSchema1 && hasInSchema2)
                        {
                            merged.Properties[key] = MergeSchemas(schema1.Properties[key], schema2.Properties[key]);

                            // Property is required only if it's in both schemas
                            if ((schema1.Required?.Contains(key) ?? false) && (schema2.Required?.Contains(key) ?? false))
                            {
                                merged.Required.Add(key);
                            }
                        }
                        else if (hasInSchema1)
                        {
                            merged.Properties[key] = schema1.Properties[key];
                            // Not required since it's not in schema2
                        }
                        else if (hasInSchema2)
                        {
                            merged.Properties[key] = schema2.Properties[key];
                            // Not required since it's not in schema1
                        }
                    }
                    break;

                case JsonSchemaType.Array:
                    if (schema1.Items != null && schema2.Items != null)
                    {
                        merged.Items = MergeSchemas(schema1.Items, schema2.Items);
                    }
                    else
                    {
                        merged.Items = schema1.Items ?? schema2.Items ?? new JsonSchema { Type = JsonSchemaType.Any };
                    }
                    break;

                case JsonSchemaType.Integer:
                case JsonSchemaType.Number:
                    // If one is integer and being compared with number, promote to number
                    if ((schema1.Type == JsonSchemaType.Integer && schema2.Type == JsonSchemaType.Number) ||
                        (schema1.Type == JsonSchemaType.Number && schema2.Type == JsonSchemaType.Integer))
                    {
                        merged.Type = JsonSchemaType.Number;
                    }
                    break;

                // For primitive types, the types already match, so no additional merging needed
                case JsonSchemaType.String:
                case JsonSchemaType.Boolean:
                case JsonSchemaType.Null:
                    break;
            }

            return merged;
        }

        /// <summary>
        /// Converts the inferred schema to JSON Schema format (as JSON string).
        /// </summary>
        public string ToJsonSchemaString(JsonSchema schema)
        {
            var schemaObject = ConvertToJsonSchemaObject(schema);
            return JsonSerializer.Serialize(schemaObject, new JsonSerializerOptions { WriteIndented = true });
        }

        private Dictionary<string, object> ConvertToJsonSchemaObject(JsonSchema schema)
        {
            var result = new Dictionary<string, object>
            {
                ["$schema"] = "http://json-schema.org/draft-07/schema#"
            };

            AddTypeToSchema(result, schema);

            if (schema.Type == JsonSchemaType.Object && schema.Properties != null && schema.Properties.Count > 0)
            {
                var properties = new Dictionary<string, object>();
                foreach (var prop in schema.Properties)
                {
                    var propSchema = new Dictionary<string, object>();
                    AddTypeToSchema(propSchema, prop.Value);

                    if (prop.Value.Type == JsonSchemaType.Object && prop.Value.Properties != null)
                    {
                        propSchema["properties"] = ConvertPropertiesToJsonSchema(prop.Value.Properties);
                        if (prop.Value.Required != null && prop.Value.Required.Count > 0)
                        {
                            propSchema["required"] = prop.Value.Required.ToList();
                        }
                    }
                    else if (prop.Value.Type == JsonSchemaType.Array && prop.Value.Items != null)
                    {
                        var itemsSchema = new Dictionary<string, object>();
                        AddTypeToSchema(itemsSchema, prop.Value.Items);
                        if (prop.Value.Items.Properties != null)
                        {
                            itemsSchema["properties"] = ConvertPropertiesToJsonSchema(prop.Value.Items.Properties);
                            if (prop.Value.Items.Required != null && prop.Value.Items.Required.Count > 0)
                            {
                                itemsSchema["required"] = prop.Value.Items.Required.ToList();
                            }
                        }
                        propSchema["items"] = itemsSchema;
                    }

                    properties[prop.Key] = propSchema;
                }
                result["properties"] = properties;

                if (schema.Required != null && schema.Required.Count > 0)
                {
                    result["required"] = schema.Required.ToList();
                }
            }
            else if (schema.Type == JsonSchemaType.Array && schema.Items != null)
            {
                var itemsSchema = new Dictionary<string, object>();
                AddTypeToSchema(itemsSchema, schema.Items);
                if (schema.Items.Properties != null)
                {
                    itemsSchema["properties"] = ConvertPropertiesToJsonSchema(schema.Items.Properties);
                    if (schema.Items.Required != null && schema.Items.Required.Count > 0)
                    {
                        itemsSchema["required"] = schema.Items.Required.ToList();
                    }
                }
                result["items"] = itemsSchema;
            }

            return result;
        }

        private Dictionary<string, object> ConvertPropertiesToJsonSchema(Dictionary<string, JsonSchema> properties)
        {
            var result = new Dictionary<string, object>();
            foreach (var prop in properties)
            {
                result[prop.Key] = ConvertToJsonSchemaObject(prop.Value);
            }
            return result;
        }

        private void AddTypeToSchema(Dictionary<string, object> schemaDict, JsonSchema schema)
        {
            if (schema.Type == JsonSchemaType.Union && schema.UnionTypes != null)
            {
                schemaDict["type"] = schema.UnionTypes.Select(t => t.ToString().ToLowerInvariant()).ToArray();
            }
            else
            {
                schemaDict["type"] = schema.Type.ToString().ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// Options for schema inference.
    /// </summary>
    public class JsonSchemaInferrerOptions
    {
        /// <summary>
        /// If true, uses union types when types differ. If false, uses "any" type.
        /// Default: true
        /// </summary>
        public bool UseUnionTypes { get; set; } = true;
    }

    /// <summary>
    /// Represents an inferred JSON schema.
    /// </summary>
    public class JsonSchema
    {
        public JsonSchemaType Type { get; set; }
        public Dictionary<string, JsonSchema> Properties { get; set; }
        public HashSet<string> Required { get; set; }
        public JsonSchema Items { get; set; }
        public HashSet<JsonSchemaType> UnionTypes { get; set; }
    }

    /// <summary>
    /// JSON schema types.
    /// </summary>
    public enum JsonSchemaType
    {
        Object,
        Array,
        String,
        Number,
        Integer,
        Boolean,
        Null,
        Any,
        Union
    }
}
