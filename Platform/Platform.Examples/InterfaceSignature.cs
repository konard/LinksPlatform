using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Platform.Examples
{
    /// <summary>
    /// Represents the signature of a class interface, ignoring the class name.
    /// Used to group classes by their interface structure.
    /// </summary>
    public class InterfaceSignature : IEquatable<InterfaceSignature>
    {
        private readonly string _normalizedSignature;
        private readonly int _hashCode;

        public InterfaceSignature(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _normalizedSignature = GenerateNormalizedSignature(type);
            _hashCode = _normalizedSignature.GetHashCode();
        }

        private static string GenerateNormalizedSignature(Type type)
        {
            var sb = new StringBuilder();

            // Get all public methods (excluding inherited object methods)
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                             .OrderBy(m => m.Name)
                             .ThenBy(m => m.GetParameters().Length);

            // Get all public properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                .OrderBy(p => p.Name);

            // Get all public fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                            .OrderBy(f => f.Name);

            // Build signature from methods
            foreach (var method in methods)
            {
                sb.Append("M:");
                sb.Append(method.ReturnType.Name);
                sb.Append(" ");
                sb.Append(method.Name);
                sb.Append("(");

                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(parameters[i].ParameterType.Name);
                }

                sb.Append(");");
            }

            // Build signature from properties
            foreach (var property in properties)
            {
                sb.Append("P:");
                sb.Append(property.PropertyType.Name);
                sb.Append(" ");
                sb.Append(property.Name);
                sb.Append("{");
                if (property.CanRead) sb.Append("get;");
                if (property.CanWrite) sb.Append("set;");
                sb.Append("}");
            }

            // Build signature from fields
            foreach (var field in fields)
            {
                sb.Append("F:");
                sb.Append(field.FieldType.Name);
                sb.Append(" ");
                sb.Append(field.Name);
                sb.Append(";");
            }

            return sb.ToString();
        }

        public string GetSignature() => _normalizedSignature;

        public override bool Equals(object obj)
        {
            return Equals(obj as InterfaceSignature);
        }

        public bool Equals(InterfaceSignature other)
        {
            if (other == null) return false;
            return _normalizedSignature == other._normalizedSignature;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return _normalizedSignature;
        }

        public static bool operator ==(InterfaceSignature left, InterfaceSignature right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(InterfaceSignature left, InterfaceSignature right)
        {
            return !(left == right);
        }
    }
}
