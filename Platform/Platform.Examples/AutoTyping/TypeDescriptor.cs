using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Examples.AutoTyping
{
    /// <summary>
    /// Describes an inferred type structure based on observed data.
    /// </summary>
    public class TypeDescriptor
    {
        public string Name { get; set; }
        public TypeKind Kind { get; set; }
        public Type UnderlyingType { get; set; }
        public bool IsOptional { get; set; }
        public int SampleCount { get; set; }

        // For object types
        public Dictionary<string, TypeDescriptor> Properties { get; private set; }

        // For collection types
        public TypeDescriptor ElementType { get; set; }

        // For union types
        public List<TypeDescriptor> UnionTypes { get; set; }

        public TypeDescriptor(string name, TypeKind kind)
        {
            Name = name;
            Kind = kind;
            Properties = new Dictionary<string, TypeDescriptor>();
            UnionTypes = new List<TypeDescriptor>();
        }

        public void AddProperty(string propertyName, TypeDescriptor propertyType)
        {
            Properties[propertyName] = propertyType;
        }

        /// <summary>
        /// Generates a human-readable description of this type.
        /// </summary>
        public string GenerateDescription()
        {
            var sb = new StringBuilder();
            GenerateDescription(sb, 0);
            return sb.ToString();
        }

        private void GenerateDescription(StringBuilder sb, int indent)
        {
            var indentStr = new string(' ', indent * 2);

            switch (Kind)
            {
                case TypeKind.Primitive:
                    sb.Append($"{indentStr}{Name}");
                    if (IsOptional)
                        sb.Append("?");
                    break;

                case TypeKind.Object:
                    sb.AppendLine($"{indentStr}{Name} {{");
                    foreach (var prop in Properties.OrderBy(p => p.Key))
                    {
                        sb.Append($"{indentStr}  {prop.Key}: ");
                        prop.Value.GenerateDescription(sb, indent + 1);
                        if (prop.Value.IsOptional)
                            sb.Append(" (optional)");
                        sb.AppendLine();
                    }
                    sb.Append($"{indentStr}}}");
                    break;

                case TypeKind.Collection:
                    sb.Append($"{indentStr}Collection<");
                    if (ElementType != null)
                    {
                        ElementType.GenerateDescription(sb, 0);
                    }
                    else
                    {
                        sb.Append("Unknown");
                    }
                    sb.Append(">");
                    if (SampleCount > 0)
                    {
                        sb.Append($" (observed {SampleCount} items)");
                    }
                    break;

                case TypeKind.Union:
                    sb.Append($"{indentStr}(");
                    for (int i = 0; i < UnionTypes.Count; i++)
                    {
                        if (i > 0)
                            sb.Append(" | ");
                        UnionTypes[i].GenerateDescription(sb, 0);
                    }
                    sb.Append(")");
                    break;

                case TypeKind.Null:
                    sb.Append($"{indentStr}null");
                    break;

                default:
                    sb.Append($"{indentStr}{Name ?? "Unknown"}");
                    break;
            }
        }

        /// <summary>
        /// Generates a code-like type definition (similar to TypeScript or C# interface).
        /// </summary>
        public string GenerateDefinition()
        {
            var sb = new StringBuilder();

            switch (Kind)
            {
                case TypeKind.Object:
                    sb.AppendLine($"type {Name} = {{");
                    foreach (var prop in Properties.OrderBy(p => p.Key))
                    {
                        var optionalMarker = prop.Value.IsOptional ? "?" : "";
                        var propTypeName = GetTypeName(prop.Value);
                        sb.AppendLine($"  {prop.Key}{optionalMarker}: {propTypeName};");
                    }
                    sb.AppendLine("}");
                    break;

                case TypeKind.Collection:
                    var elementTypeName = ElementType != null ? GetTypeName(ElementType) : "unknown";
                    sb.AppendLine($"type {Name} = {elementTypeName}[];");
                    break;

                case TypeKind.Union:
                    var unionTypeNames = string.Join(" | ", UnionTypes.Select(GetTypeName));
                    sb.AppendLine($"type {Name} = {unionTypeNames};");
                    break;

                default:
                    sb.AppendLine($"type {Name} = {GetTypeName(this)};");
                    break;
            }

            return sb.ToString();
        }

        private string GetTypeName(TypeDescriptor descriptor)
        {
            switch (descriptor.Kind)
            {
                case TypeKind.Primitive:
                    return NormalizePrimitiveTypeName(descriptor.Name);

                case TypeKind.Object:
                    return descriptor.Name;

                case TypeKind.Collection:
                    var elementType = descriptor.ElementType != null
                        ? GetTypeName(descriptor.ElementType)
                        : "unknown";
                    return $"{elementType}[]";

                case TypeKind.Union:
                    return $"({string.Join(" | ", descriptor.UnionTypes.Select(GetTypeName))})";

                case TypeKind.Null:
                    return "null";

                default:
                    return descriptor.Name ?? "unknown";
            }
        }

        private string NormalizePrimitiveTypeName(string typeName)
        {
            // Normalize .NET type names to more universal names
            switch (typeName?.ToLower())
            {
                case "int32":
                case "int64":
                case "int16":
                case "byte":
                case "sbyte":
                    return "number";

                case "single":
                case "double":
                case "decimal":
                    return "number";

                case "boolean":
                    return "boolean";

                case "string":
                    return "string";

                case "datetime":
                    return "date";

                case "guid":
                    return "guid";

                default:
                    return typeName?.ToLower() ?? "unknown";
            }
        }

        public override string ToString()
        {
            return GenerateDescription();
        }
    }
}
