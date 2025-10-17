using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Platform.Examples
{
    /// <summary>
    /// Maps binary file data according to a schema
    /// </summary>
    public class BinaryMapper
    {
        private readonly BinarySchema _schema;

        public BinaryMapper(BinarySchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public Dictionary<string, object> MapFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var data = File.ReadAllBytes(filePath);
            return MapData(data);
        }

        public Dictionary<string, object> MapData(byte[] data)
        {
            var result = new Dictionary<string, object>
            {
                ["Schema"] = _schema.Name,
                ["FileSize"] = data.Length
            };

            if (_schema.MagicBytes != null && !_schema.IsMatch(data))
            {
                result["Warning"] = "File does not match expected magic bytes";
            }

            var fields = new Dictionary<string, object>();
            foreach (var field in _schema.Fields)
            {
                if (field.Offset + field.Size <= data.Length)
                {
                    fields[field.Name] = ExtractField(data, field);
                }
                else
                {
                    fields[field.Name] = $"<out of bounds: offset {field.Offset}, size {field.Size}>";
                }
            }
            result["Fields"] = fields;

            return result;
        }

        private object ExtractField(byte[] data, BinaryField field)
        {
            var bytes = new byte[field.Size];
            Array.Copy(data, field.Offset, bytes, 0, field.Size);

            if (field.SubFields != null && field.SubFields.Length > 0)
            {
                var subResult = new Dictionary<string, object>();
                foreach (var subField in field.SubFields)
                {
                    if (subField.Offset + subField.Size <= bytes.Length)
                    {
                        var subBytes = new byte[subField.Size];
                        Array.Copy(bytes, subField.Offset, subBytes, 0, subField.Size);
                        subResult[subField.Name] = InterpretBytes(subBytes, subField.Type);
                    }
                }
                return subResult;
            }

            return InterpretBytes(bytes, field.Type);
        }

        private object InterpretBytes(byte[] bytes, string type)
        {
            switch (type?.ToLowerInvariant())
            {
                case "uint8":
                case "byte":
                    return bytes.Length > 0 ? bytes[0] : (byte)0;

                case "uint16":
                case "ushort":
                    return bytes.Length >= 2 ? BitConverter.ToUInt16(bytes, 0) : (ushort)0;

                case "uint32":
                case "uint":
                    return bytes.Length >= 4 ? BitConverter.ToUInt32(bytes, 0) : 0u;

                case "uint64":
                case "ulong":
                    return bytes.Length >= 8 ? BitConverter.ToUInt64(bytes, 0) : 0ul;

                case "int8":
                case "sbyte":
                    return bytes.Length > 0 ? (sbyte)bytes[0] : (sbyte)0;

                case "int16":
                case "short":
                    return bytes.Length >= 2 ? BitConverter.ToInt16(bytes, 0) : (short)0;

                case "int32":
                case "int":
                    return bytes.Length >= 4 ? BitConverter.ToInt32(bytes, 0) : 0;

                case "int64":
                case "long":
                    return bytes.Length >= 8 ? BitConverter.ToInt64(bytes, 0) : 0L;

                case "float":
                    return bytes.Length >= 4 ? BitConverter.ToSingle(bytes, 0) : 0f;

                case "double":
                    return bytes.Length >= 8 ? BitConverter.ToDouble(bytes, 0) : 0d;

                case "string":
                case "ascii":
                    return Encoding.ASCII.GetString(bytes).TrimEnd('\0');

                case "utf8":
                    return Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                case "hex":
                    return BitConverter.ToString(bytes).Replace("-", " ");

                default:
                    return BitConverter.ToString(bytes).Replace("-", " ");
            }
        }

        public string FormatMapping(Dictionary<string, object> mapping, int indent = 0)
        {
            var sb = new StringBuilder();
            var indentStr = new string(' ', indent);

            foreach (var kvp in mapping)
            {
                if (kvp.Value is Dictionary<string, object> dict)
                {
                    sb.AppendLine($"{indentStr}{kvp.Key}:");
                    sb.Append(FormatMapping(dict, indent + 2));
                }
                else
                {
                    sb.AppendLine($"{indentStr}{kvp.Key}: {kvp.Value}");
                }
            }

            return sb.ToString();
        }
    }
}
