using System;
using System.Collections.Generic;
using System.Text;
using Platform.Numbers;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Converts between JSON strings and Links-based dynamic objects.
    /// Provides import/export functionality for JSON format.
    /// </summary>
    public class JsonConverter<TLink> : IObjectConverter<TLink, string>
    {
        private static readonly TLink _zero = default;

        private readonly ILinks<TLink> _links;
        private readonly ITypeSystem<TLink> _typeSystem;
        private readonly TLink _objectMarker;
        private readonly TLink _arrayMarker;
        private readonly TLink _stringMarker;
        private readonly TLink _numberMarker;
        private readonly TLink _booleanMarker;
        private readonly TLink _nullMarker;

        public JsonConverter(ILinks<TLink> links, ITypeSystem<TLink> typeSystem)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _typeSystem = typeSystem ?? throw new ArgumentNullException(nameof(typeSystem));

            // Initialize type markers
            _objectMarker = _typeSystem.GetOrCreateType("Object");
            _arrayMarker = _typeSystem.GetOrCreateType("Array");
            _stringMarker = _typeSystem.GetOrCreateType("String");
            _numberMarker = _typeSystem.GetOrCreateType("Number");
            _booleanMarker = _typeSystem.GetOrCreateType("Boolean");
            _nullMarker = _typeSystem.GetOrCreateType("Null");
        }

        public TLink Import(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));

            json = json.Trim();
            int index = 0;
            return ParseValue(json, ref index);
        }

        public string Export(TLink objectLink)
        {
            if (EqualityComparer<TLink>.Default.Equals(objectLink, _zero))
                return "null";

            var sb = new StringBuilder();
            ExportValue(objectLink, sb);
            return sb.ToString();
        }

        private TLink ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);

            if (index >= json.Length)
                throw new FormatException("Unexpected end of JSON.");

            char c = json[index];

            if (c == '{')
                return ParseObject(json, ref index);
            else if (c == '[')
                return ParseArray(json, ref index);
            else if (c == '"')
                return ParseString(json, ref index);
            else if (c == 't' || c == 'f')
                return ParseBoolean(json, ref index);
            else if (c == 'n')
                return ParseNull(json, ref index);
            else if (char.IsDigit(c) || c == '-')
                return ParseNumber(json, ref index);
            else
                throw new FormatException($"Unexpected character '{c}' at position {index}.");
        }

        private TLink ParseObject(string json, ref int index)
        {
            index++; // Skip '{'
            SkipWhitespace(json, ref index);

            var obj = new LinksDynamicObject<TLink>(_links, _typeSystem, _objectMarker);

            while (index < json.Length && json[index] != '}')
            {
                // Parse property name
                if (json[index] != '"')
                    throw new FormatException($"Expected property name at position {index}.");

                string propertyName = ParseStringValue(json, ref index);

                SkipWhitespace(json, ref index);

                if (index >= json.Length || json[index] != ':')
                    throw new FormatException($"Expected ':' at position {index}.");

                index++; // Skip ':'

                // Parse property value
                TLink value = ParseValue(json, ref index);
                obj[propertyName] = value;

                SkipWhitespace(json, ref index);

                if (index < json.Length && json[index] == ',')
                {
                    index++; // Skip ','
                    SkipWhitespace(json, ref index);
                }
            }

            if (index >= json.Length || json[index] != '}')
                throw new FormatException($"Expected '}}' at position {index}.");

            index++; // Skip '}'
            return obj.Link;
        }

        private TLink ParseArray(string json, ref int index)
        {
            index++; // Skip '['
            SkipWhitespace(json, ref index);

            var arrayLink = _links.GetOrCreate(_arrayMarker, _arrayMarker);
            int arrayIndex = 0;

            while (index < json.Length && json[index] != ']')
            {
                TLink element = ParseValue(json, ref index);

                // Store array element: array -> element
                _links.GetOrCreate(arrayLink, element);

                arrayIndex++;

                SkipWhitespace(json, ref index);

                if (index < json.Length && json[index] == ',')
                {
                    index++; // Skip ','
                    SkipWhitespace(json, ref index);
                }
            }

            if (index >= json.Length || json[index] != ']')
                throw new FormatException($"Expected ']' at position {index}.");

            index++; // Skip ']'
            return arrayLink;
        }

        private TLink ParseString(string json, ref int index)
        {
            string value = ParseStringValue(json, ref index);
            return CreateStringLink(value);
        }

        private string ParseStringValue(string json, ref int index)
        {
            if (json[index] != '"')
                throw new FormatException($"Expected '\"' at position {index}.");

            index++; // Skip opening '"'
            var sb = new StringBuilder();

            while (index < json.Length && json[index] != '"')
            {
                if (json[index] == '\\')
                {
                    index++;
                    if (index >= json.Length)
                        throw new FormatException("Unexpected end of string.");

                    char escaped = json[index];
                    switch (escaped)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        default: throw new FormatException($"Invalid escape sequence '\\{escaped}'.");
                    }
                }
                else
                {
                    sb.Append(json[index]);
                }
                index++;
            }

            if (index >= json.Length || json[index] != '"')
                throw new FormatException($"Expected closing '\"' at position {index}.");

            index++; // Skip closing '"'
            return sb.ToString();
        }

        private TLink ParseNumber(string json, ref int index)
        {
            int start = index;
            if (json[index] == '-')
                index++;

            while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.'))
                index++;

            string numberStr = json.Substring(start, index - start);
            return CreateNumberLink(double.Parse(numberStr));
        }

        private TLink ParseBoolean(string json, ref int index)
        {
            if (json.Substring(index).StartsWith("true"))
            {
                index += 4;
                return CreateBooleanLink(true);
            }
            else if (json.Substring(index).StartsWith("false"))
            {
                index += 5;
                return CreateBooleanLink(false);
            }
            else
            {
                throw new FormatException($"Invalid boolean value at position {index}.");
            }
        }

        private TLink ParseNull(string json, ref int index)
        {
            if (json.Substring(index).StartsWith("null"))
            {
                index += 4;
                return _links.GetOrCreate(_nullMarker, _nullMarker);
            }
            else
            {
                throw new FormatException($"Invalid null value at position {index}.");
            }
        }

        private void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
                index++;
        }

        private TLink CreateStringLink(string value)
        {
            // For simplicity, store string as a link with string marker
            // In a real implementation, you'd want to convert to unicode sequence
            var hashLink = _links.GetOrCreate(_stringMarker, _links.Create());
            return hashLink;
        }

        private TLink CreateNumberLink(double value)
        {
            // Store number as marker link
            var numberLink = _links.GetOrCreate(_numberMarker, _links.Create());
            return numberLink;
        }

        private TLink CreateBooleanLink(bool value)
        {
            var trueMarker = _typeSystem.GetOrCreateProperty("true");
            var falseMarker = _typeSystem.GetOrCreateProperty("false");
            return _links.GetOrCreate(_booleanMarker, value ? trueMarker : falseMarker);
        }

        private void ExportValue(TLink linkAddress, StringBuilder sb)
        {
            if (EqualityComparer<TLink>.Default.Equals(linkAddress, _zero))
            {
                sb.Append("null");
                return;
            }

            // Read the link to determine its type
            bool handled = false;
            _links.Each(link =>
            {
                if (link != null && link.Count >= 3)
                {
                    var source = link[_links.Constants.SourcePart];

                    // Check type
                    if (EqualityComparer<TLink>.Default.Equals(source, _objectMarker))
                    {
                        ExportObject(linkAddress, sb);
                        handled = true;
                    }
                    else if (EqualityComparer<TLink>.Default.Equals(source, _arrayMarker))
                    {
                        ExportArray(linkAddress, sb);
                        handled = true;
                    }
                    else if (EqualityComparer<TLink>.Default.Equals(source, _stringMarker))
                    {
                        sb.Append("\"string\""); // Simplified
                        handled = true;
                    }
                    else if (EqualityComparer<TLink>.Default.Equals(source, _numberMarker))
                    {
                        sb.Append("0"); // Simplified
                        handled = true;
                    }
                    else if (EqualityComparer<TLink>.Default.Equals(source, _booleanMarker))
                    {
                        sb.Append("false"); // Simplified
                        handled = true;
                    }
                    else if (EqualityComparer<TLink>.Default.Equals(source, _nullMarker))
                    {
                        sb.Append("null");
                        handled = true;
                    }
                }
                return _links.Constants.Break;
            }, linkAddress);

            if (!handled)
            {
                sb.Append("null");
            }
        }

        private void ExportObject(TLink objectLink, StringBuilder sb)
        {
            sb.Append("{");

            var obj = new LinksDynamicObject<TLink>(_links, _typeSystem, _objectMarker, objectLink);
            var properties = obj.GetProperties();

            bool first = true;
            foreach (var kvp in properties)
            {
                if (!first)
                    sb.Append(",");
                first = false;

                sb.Append("\"");
                sb.Append(kvp.Key);
                sb.Append("\":");
                ExportValue(kvp.Value, sb);
            }

            sb.Append("}");
        }

        private void ExportArray(TLink arrayLink, StringBuilder sb)
        {
            sb.Append("[");

            // Simplified array export
            bool first = true;
            _links.Each(link =>
            {
                if (link != null && link.Count >= 3)
                {
                    var source = link[_links.Constants.SourcePart];
                    if (EqualityComparer<TLink>.Default.Equals(source, arrayLink))
                    {
                        if (!first)
                            sb.Append(",");
                        first = false;

                        var target = link[_links.Constants.TargetPart];
                        ExportValue(target, sb);
                    }
                }
                return _links.Constants.Continue;
            });

            sb.Append("]");
        }
    }
}
