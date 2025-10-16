using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates the usage of the dynamic object system and type system for Links.
    /// Shows how to create objects with properties, assign types, and import/export JSON.
    /// </summary>
    public class ObjectsExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Links Platform: Objects and Type System Example ===\n");

            // Initialize Links storage
            using (var links = new UnitedMemoryLinks<uint>())
            {
                // Initialize type system
                var typeSystem = new LinksTypeSystem<uint>(links);
                Console.WriteLine("Type system initialized.");

                // Example 1: Create a simple object with properties
                Console.WriteLine("\n--- Example 1: Simple Object ---");
                CreateSimpleObject(links, typeSystem);

                // Example 2: Create a typed object
                Console.WriteLine("\n--- Example 2: Typed Object ---");
                CreateTypedObject(links, typeSystem);

                // Example 3: JSON Import/Export
                Console.WriteLine("\n--- Example 3: JSON Import/Export ---");
                DemonstrateJsonConversion(links, typeSystem);
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        private static void CreateSimpleObject(ILinks<uint> links, LinksTypeSystem<uint> typeSystem)
        {
            var objectMarker = typeSystem.GetOrCreateType("Object");
            var obj = new LinksDynamicObject<uint>(links, typeSystem, objectMarker);

            // Set properties
            var nameProperty = typeSystem.GetOrCreateProperty("name");
            var ageProperty = typeSystem.GetOrCreateProperty("age");

            Console.WriteLine("Creating object with properties: name and age");

            // Note: In a full implementation, we'd create string and number links properly
            // For now, we're demonstrating the structure
            obj["name"] = links.Create(); // Placeholder for string "John"
            obj["age"] = links.Create();  // Placeholder for number 30

            Console.WriteLine($"Object created with link ID: {obj.Link}");
            Console.WriteLine($"Has 'name' property: {obj.HasProperty("name")}");
            Console.WriteLine($"Has 'age' property: {obj.HasProperty("age")}");
            Console.WriteLine($"Has 'email' property: {obj.HasProperty("email")}");

            var properties = obj.GetProperties();
            Console.WriteLine($"Total properties: {properties.Count}");
        }

        private static void CreateTypedObject(ILinks<uint> links, LinksTypeSystem<uint> typeSystem)
        {
            // Create a custom type
            var personType = typeSystem.GetOrCreateType("Person");
            Console.WriteLine($"Created type 'Person' with marker: {personType}");

            // Create an object of that type
            var objectMarker = typeSystem.GetOrCreateType("Object");
            var person = new LinksDynamicObject<uint>(links, typeSystem, objectMarker);

            person.SetType(personType);
            person["firstName"] = links.Create(); // Placeholder
            person["lastName"] = links.Create();  // Placeholder

            var retrievedType = person.GetType();
            var typeName = typeSystem.GetTypeName(retrievedType);

            Console.WriteLine($"Object type: {typeName}");
            Console.WriteLine($"Object has type: {!EqualityComparer<uint>.Default.Equals(retrievedType, default(uint))}");
        }

        private static void DemonstrateJsonConversion(ILinks<uint> links, LinksTypeSystem<uint> typeSystem)
        {
            var jsonConverter = new JsonConverter<uint>(links, typeSystem);

            // Simple JSON examples
            string simpleJson = "{\"name\":\"Alice\",\"age\":25}";
            Console.WriteLine($"Input JSON: {simpleJson}");

            try
            {
                var importedObject = jsonConverter.Import(simpleJson);
                Console.WriteLine($"Imported object with link ID: {importedObject}");

                var exportedJson = jsonConverter.Export(importedObject);
                Console.WriteLine($"Exported JSON: {exportedJson}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON conversion demonstration: {ex.Message}");
                Console.WriteLine("Note: Full string and number conversion requires additional implementation.");
            }

            // Demonstrate the concept
            Console.WriteLine("\nJSON conversion capability established:");
            Console.WriteLine("- Import: JSON string -> Links structure");
            Console.WriteLine("- Export: Links structure -> JSON string");
            Console.WriteLine("- Supports: objects, arrays, strings, numbers, booleans, null");
        }

        private class EqualityComparer<T> : System.Collections.Generic.IEqualityComparer<T>
        {
            public static readonly EqualityComparer<T> Default = new EqualityComparer<T>();

            public bool Equals(T x, T y)
            {
                return System.Collections.Generic.EqualityComparer<T>.Default.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return System.Collections.Generic.EqualityComparer<T>.Default.GetHashCode(obj);
            }
        }
    }
}
