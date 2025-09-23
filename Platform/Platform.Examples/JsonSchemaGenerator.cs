using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Platform.Examples
{
    /// <summary>
    /// Generates JSON Schema from multiple JSON samples by analyzing their structure and data types.
    /// </summary>
    public class JsonSchemaGenerator
    {
        private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
        private readonly HashSet<JsonValueKind> _rootTypes = new HashSet<JsonValueKind>();

        /// <summary>
        /// Analyzes a JSON sample and accumulates information for schema generation.
        /// </summary>
        /// <param name="jsonSample">A JSON string sample to analyze</param>
        public void AnalyzeSample(string jsonSample)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonSample);
                var root = document.RootElement;
                _rootTypes.Add(root.ValueKind);

                if (root.ValueKind == JsonValueKind.Object)
                {
                    AnalyzeObject(root, "");
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    AnalyzeArray(root, "");
                }
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON sample: {ex.Message}", nameof(jsonSample));
            }
        }

        /// <summary>
        /// Generates a JSON Schema based on all analyzed samples.
        /// </summary>
        /// <returns>JSON Schema as a string</returns>
        public string GenerateSchema()
        {
            var schema = new JsonObject
            {
                ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
                ["title"] = "Generated Schema",
                ["description"] = "Schema generated from JSON samples"
            };

            if (_rootTypes.Count == 1)
            {
                var rootType = _rootTypes.First();
                if (rootType == JsonValueKind.Object)
                {
                    schema["type"] = "object";
                    if (_properties.Any())
                    {
                        schema["properties"] = GeneratePropertiesObject();
                        var requiredProperties = _properties.Where(p => p.Value.IsRequired && !p.Key.Contains(".") && !p.Key.Contains("[]")).Select(p => p.Key).ToList();
                        if (requiredProperties.Any())
                        {
                            schema["required"] = new JsonArray(requiredProperties.Select(p => JsonValue.Create(p)).ToArray());
                        }
                    }
                }
                else if (rootType == JsonValueKind.Array)
                {
                    schema["type"] = "array";
                    // For arrays at root level, we'd need to analyze item types
                    schema["items"] = new JsonObject { ["type"] = "object" };
                }
                else
                {
                    schema["type"] = GetJsonSchemaType(rootType);
                }
            }
            else if (_rootTypes.Count > 1)
            {
                // Multiple root types - use anyOf
                var anyOf = new JsonArray();
                foreach (var rootType in _rootTypes.OrderBy(t => t.ToString()))
                {
                    anyOf.Add(new JsonObject { ["type"] = GetJsonSchemaType(rootType) });
                }
                schema["anyOf"] = anyOf;
            }

            return schema.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        private void AnalyzeObject(JsonElement element, string basePath)
        {
            foreach (var property in element.EnumerateObject())
            {
                var propertyPath = string.IsNullOrEmpty(basePath) ? property.Name : $"{basePath}.{property.Name}";

                if (!_properties.ContainsKey(propertyPath))
                {
                    _properties[propertyPath] = new PropertyInfo();
                }

                var propInfo = _properties[propertyPath];
                propInfo.Types.Add(property.Value.ValueKind);
                propInfo.SampleCount++;

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    AnalyzeObject(property.Value, propertyPath);
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    AnalyzeArray(property.Value, propertyPath);
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    propInfo.StringFormats.Add(AnalyzeStringFormat(property.Value.GetString()));
                }
                else if (property.Value.ValueKind == JsonValueKind.Number)
                {
                    if (property.Value.TryGetInt32(out _))
                    {
                        propInfo.NumberTypes.Add("integer");
                    }
                    else
                    {
                        propInfo.NumberTypes.Add("number");
                    }
                }
            }
        }

        private void AnalyzeArray(JsonElement element, string basePath)
        {
            var arrayItemPath = $"{basePath}[]";

            if (!_properties.ContainsKey(arrayItemPath))
            {
                _properties[arrayItemPath] = new PropertyInfo();
            }

            var itemInfo = _properties[arrayItemPath];

            foreach (var item in element.EnumerateArray())
            {
                itemInfo.Types.Add(item.ValueKind);

                if (item.ValueKind == JsonValueKind.Object)
                {
                    AnalyzeObject(item, arrayItemPath);
                }
                else if (item.ValueKind == JsonValueKind.Array)
                {
                    AnalyzeArray(item, arrayItemPath);
                }
                else if (item.ValueKind == JsonValueKind.String)
                {
                    itemInfo.StringFormats.Add(AnalyzeStringFormat(item.GetString()));
                }
                else if (item.ValueKind == JsonValueKind.Number)
                {
                    if (item.TryGetInt32(out _))
                    {
                        itemInfo.NumberTypes.Add("integer");
                    }
                    else
                    {
                        itemInfo.NumberTypes.Add("number");
                    }
                }
            }
        }

        private string AnalyzeStringFormat(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "string";

            // Basic format detection
            if (DateTime.TryParse(value, out _))
                return "date-time";

            if (Uri.TryCreate(value, UriKind.Absolute, out _))
                return "uri";

            if (value.Contains("@") && value.Contains("."))
                return "email";

            return "string";
        }

        private JsonObject GeneratePropertiesObject()
        {
            var properties = new JsonObject();
            var topLevelProps = _properties.Where(p => !p.Key.Contains(".") && !p.Key.Contains("[]")).ToList();

            foreach (var prop in topLevelProps)
            {
                properties[prop.Key] = GeneratePropertySchema(prop.Value, prop.Key);
            }

            return properties;
        }

        private JsonObject GeneratePropertySchema(PropertyInfo propInfo, string propertyPath)
        {
            var propertySchema = new JsonObject();

            if (propInfo.Types.Count == 1)
            {
                var type = propInfo.Types.First();
                propertySchema["type"] = GetJsonSchemaType(type);

                if (type == JsonValueKind.String && propInfo.StringFormats.Any())
                {
                    var mostCommonFormat = propInfo.StringFormats.GroupBy(f => f)
                        .OrderByDescending(g => g.Count())
                        .First().Key;

                    if (mostCommonFormat != "string")
                    {
                        propertySchema["format"] = mostCommonFormat;
                    }
                }
                else if (type == JsonValueKind.Number && propInfo.NumberTypes.Any())
                {
                    var mostCommonNumberType = propInfo.NumberTypes.GroupBy(t => t)
                        .OrderByDescending(g => g.Count())
                        .First().Key;
                    propertySchema["type"] = mostCommonNumberType;
                }
                else if (type == JsonValueKind.Object)
                {
                    var nestedProps = _properties.Where(p => p.Key.StartsWith($"{propertyPath}.") &&
                                                           !p.Key.Substring(propertyPath.Length + 1).Contains(".") &&
                                                           !p.Key.Substring(propertyPath.Length + 1).Contains("[]")).ToList();

                    if (nestedProps.Any())
                    {
                        var nestedPropsObj = new JsonObject();
                        var nestedRequired = new List<string>();

                        foreach (var nestedProp in nestedProps)
                        {
                            var nestedPropName = nestedProp.Key.Substring(propertyPath.Length + 1);
                            nestedPropsObj[nestedPropName] = GeneratePropertySchema(nestedProp.Value, nestedProp.Key);
                            if (nestedProp.Value.IsRequired)
                            {
                                nestedRequired.Add(nestedPropName);
                            }
                        }

                        propertySchema["properties"] = nestedPropsObj;
                        if (nestedRequired.Any())
                        {
                            propertySchema["required"] = new JsonArray(nestedRequired.Select(p => JsonValue.Create(p)).ToArray());
                        }
                    }
                }
                else if (type == JsonValueKind.Array)
                {
                    var arrayItemPath = $"{propertyPath}[]";
                    if (_properties.ContainsKey(arrayItemPath))
                    {
                        var itemInfo = _properties[arrayItemPath];
                        propertySchema["items"] = GeneratePropertySchema(itemInfo, arrayItemPath);
                    }
                    else
                    {
                        propertySchema["items"] = new JsonObject { ["type"] = "object" };
                    }
                }
            }
            else if (propInfo.Types.Count > 1)
            {
                var distinctTypes = propInfo.Types.Select(GetJsonSchemaType).Distinct().OrderBy(t => t).ToList();
                if (distinctTypes.Count == 1)
                {
                    propertySchema["type"] = distinctTypes[0];
                }
                else
                {
                    var typeArray = new JsonArray();
                    foreach (var type in distinctTypes)
                    {
                        typeArray.Add(JsonValue.Create(type));
                    }
                    propertySchema["type"] = typeArray;
                }
            }

            return propertySchema;
        }

        private string GetJsonSchemaType(JsonValueKind valueKind)
        {
            return valueKind switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Array => "array",
                JsonValueKind.Object => "object",
                JsonValueKind.Null => "null",
                _ => "string"
            };
        }

        private class PropertyInfo
        {
            public HashSet<JsonValueKind> Types { get; } = new HashSet<JsonValueKind>();
            public HashSet<string> StringFormats { get; } = new HashSet<string>();
            public HashSet<string> NumberTypes { get; } = new HashSet<string>();
            public int SampleCount { get; set; }
            public bool IsRequired => SampleCount > 0; // For now, consider all seen properties as required
        }
    }
}