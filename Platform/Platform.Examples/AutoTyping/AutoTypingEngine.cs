using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples.AutoTyping
{
    /// <summary>
    /// Minimal engine that generates type definitions/descriptions based on observed data examples.
    /// Implements duck typing approach: "if it walks like a duck and quacks like a duck, it's a duck".
    /// </summary>
    public class AutoTypingEngine
    {
        private readonly Dictionary<string, TypeDescriptor> _inferredTypes;

        public AutoTypingEngine()
        {
            _inferredTypes = new Dictionary<string, TypeDescriptor>();
        }

        /// <summary>
        /// Analyzes a data sample and infers its type structure.
        /// </summary>
        /// <param name="data">The data object to analyze</param>
        /// <param name="typeName">Optional name for the inferred type</param>
        /// <returns>A TypeDescriptor describing the inferred type</returns>
        public TypeDescriptor InferType(object data, string typeName = null)
        {
            if (data == null)
            {
                return new TypeDescriptor("Null", TypeKind.Null);
            }

            var dataType = data.GetType();

            // Handle primitive types
            if (IsPrimitive(dataType))
            {
                return new TypeDescriptor(dataType.Name, TypeKind.Primitive)
                {
                    UnderlyingType = dataType
                };
            }

            // Handle collections
            if (data is System.Collections.IEnumerable enumerable && !(data is string))
            {
                return InferCollectionType(enumerable, typeName);
            }

            // Handle object/record types
            return InferObjectType(data, typeName ?? dataType.Name);
        }

        /// <summary>
        /// Infers type from multiple data examples and creates a unified type definition.
        /// </summary>
        /// <param name="examples">Multiple data examples</param>
        /// <param name="typeName">Name for the inferred type</param>
        /// <returns>A unified TypeDescriptor</returns>
        public TypeDescriptor InferTypeFromExamples(IEnumerable<object> examples, string typeName)
        {
            var descriptors = examples.Select(e => InferType(e)).ToList();

            if (descriptors.Count == 0)
            {
                return new TypeDescriptor(typeName, TypeKind.Unknown);
            }

            if (descriptors.Count == 1)
            {
                var descriptor = descriptors[0];
                descriptor.Name = typeName;
                return descriptor;
            }

            // Merge multiple descriptors into a unified type
            return MergeTypeDescriptors(descriptors, typeName);
        }

        /// <summary>
        /// Registers a type descriptor for future reference.
        /// </summary>
        public void RegisterType(TypeDescriptor descriptor)
        {
            _inferredTypes[descriptor.Name] = descriptor;
        }

        /// <summary>
        /// Gets a previously inferred type by name.
        /// </summary>
        public TypeDescriptor GetType(string name)
        {
            return _inferredTypes.TryGetValue(name, out var descriptor) ? descriptor : null;
        }

        /// <summary>
        /// Gets all inferred types.
        /// </summary>
        public IReadOnlyDictionary<string, TypeDescriptor> GetAllTypes()
        {
            return _inferredTypes;
        }

        private bool IsPrimitive(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        private TypeDescriptor InferCollectionType(System.Collections.IEnumerable enumerable, string typeName)
        {
            var items = enumerable.Cast<object>().ToList();

            if (items.Count == 0)
            {
                return new TypeDescriptor(typeName ?? "EmptyCollection", TypeKind.Collection)
                {
                    ElementType = new TypeDescriptor("Unknown", TypeKind.Unknown)
                };
            }

            // Infer element types from samples
            var elementDescriptors = items.Take(100).Select(item => InferType(item)).ToList();

            // Check if all elements have the same type
            var unifiedElementType = MergeTypeDescriptors(elementDescriptors, "Element");

            return new TypeDescriptor(typeName ?? "Collection", TypeKind.Collection)
            {
                ElementType = unifiedElementType,
                SampleCount = items.Count
            };
        }

        private TypeDescriptor InferObjectType(object obj, string typeName)
        {
            var descriptor = new TypeDescriptor(typeName, TypeKind.Object);
            var properties = obj.GetType().GetProperties();

            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj);
                    var propertyType = InferType(value, prop.Name);
                    descriptor.AddProperty(prop.Name, propertyType);
                }
                catch
                {
                    // Skip properties that can't be accessed
                    descriptor.AddProperty(prop.Name, new TypeDescriptor("Inaccessible", TypeKind.Unknown));
                }
            }

            return descriptor;
        }

        private TypeDescriptor MergeTypeDescriptors(List<TypeDescriptor> descriptors, string typeName)
        {
            if (descriptors.Count == 0)
            {
                return new TypeDescriptor(typeName, TypeKind.Unknown);
            }

            if (descriptors.Count == 1)
            {
                var single = descriptors[0];
                single.Name = typeName;
                return single;
            }

            // Group by kind
            var kindGroups = descriptors.GroupBy(d => d.Kind).ToList();

            if (kindGroups.Count == 1)
            {
                // All same kind - merge based on kind
                var kind = kindGroups[0].Key;

                switch (kind)
                {
                    case TypeKind.Primitive:
                        return MergePrimitiveTypes(descriptors, typeName);

                    case TypeKind.Object:
                        return MergeObjectTypes(descriptors, typeName);

                    case TypeKind.Collection:
                        return MergeCollectionTypes(descriptors, typeName);

                    default:
                        return new TypeDescriptor(typeName, kind);
                }
            }

            // Mixed kinds - create a union type
            return new TypeDescriptor(typeName, TypeKind.Union)
            {
                UnionTypes = descriptors
            };
        }

        private TypeDescriptor MergePrimitiveTypes(List<TypeDescriptor> descriptors, string typeName)
        {
            // If all primitives are the same type, use that type
            var uniqueTypes = descriptors.Select(d => d.UnderlyingType?.Name ?? d.Name).Distinct().ToList();

            if (uniqueTypes.Count == 1)
            {
                return new TypeDescriptor(uniqueTypes[0], TypeKind.Primitive)
                {
                    UnderlyingType = descriptors[0].UnderlyingType
                };
            }

            // Different primitive types - create union
            return new TypeDescriptor(typeName, TypeKind.Union)
            {
                UnionTypes = descriptors
            };
        }

        private TypeDescriptor MergeObjectTypes(List<TypeDescriptor> descriptors, string typeName)
        {
            var merged = new TypeDescriptor(typeName, TypeKind.Object);

            // Collect all property names
            var allProperties = descriptors
                .SelectMany(d => d.Properties.Keys)
                .Distinct()
                .ToList();

            foreach (var propName in allProperties)
            {
                var propDescriptors = descriptors
                    .Where(d => d.Properties.ContainsKey(propName))
                    .Select(d => d.Properties[propName])
                    .ToList();

                if (propDescriptors.Count == 0)
                    continue;

                var mergedPropType = MergeTypeDescriptors(propDescriptors, propName);

                // Mark as optional if not present in all examples
                mergedPropType.IsOptional = propDescriptors.Count < descriptors.Count;

                merged.AddProperty(propName, mergedPropType);
            }

            return merged;
        }

        private TypeDescriptor MergeCollectionTypes(List<TypeDescriptor> descriptors, string typeName)
        {
            var elementTypes = descriptors
                .Where(d => d.ElementType != null)
                .Select(d => d.ElementType)
                .ToList();

            if (elementTypes.Count == 0)
            {
                return new TypeDescriptor(typeName, TypeKind.Collection)
                {
                    ElementType = new TypeDescriptor("Unknown", TypeKind.Unknown)
                };
            }

            var mergedElementType = MergeTypeDescriptors(elementTypes, "Element");

            return new TypeDescriptor(typeName, TypeKind.Collection)
            {
                ElementType = mergedElementType
            };
        }
    }
}
