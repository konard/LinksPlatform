using System;
using System.Collections.Generic;
using Platform.Examples.AutoTyping;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates the AutoTyping engine with various data examples.
    /// </summary>
    public class AutoTypingExample
    {
        public static void Main(string[] args)
        {
            var engine = new AutoTypingEngine();

            Console.WriteLine("=== AutoTyping Engine Demo ===\n");

            // Example 1: Infer type from simple objects
            Console.WriteLine("Example 1: Simple Object Type Inference");
            Console.WriteLine("----------------------------------------");

            var person1 = new { Name = "Alice", Age = 30, Email = "alice@example.com" };
            var person2 = new { Name = "Bob", Age = 25, Email = "bob@example.com" };
            var person3 = new { Name = "Charlie", Age = 35 }; // Missing Email

            var personType = engine.InferTypeFromExamples(
                new object[] { person1, person2, person3 },
                "Person"
            );

            Console.WriteLine(personType.GenerateDefinition());
            Console.WriteLine("Description:");
            Console.WriteLine(personType.GenerateDescription());
            Console.WriteLine();

            // Example 2: Infer type from collections
            Console.WriteLine("Example 2: Collection Type Inference");
            Console.WriteLine("-------------------------------------");

            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            var numberListType = engine.InferType(numbers, "NumberList");

            Console.WriteLine(numberListType.GenerateDefinition());
            Console.WriteLine();

            // Example 3: Mixed types (Union)
            Console.WriteLine("Example 3: Union Type Inference");
            Console.WriteLine("--------------------------------");

            var mixedData = new object[] { 42, "hello", 3.14, true };
            var mixedType = engine.InferTypeFromExamples(mixedData, "MixedType");

            Console.WriteLine(mixedType.GenerateDefinition());
            Console.WriteLine();

            // Example 4: Nested objects
            Console.WriteLine("Example 4: Nested Object Type Inference");
            Console.WriteLine("----------------------------------------");

            var user1 = new
            {
                Id = 1,
                Name = "Alice",
                Address = new { Street = "123 Main St", City = "Springfield", Zip = "12345" }
            };

            var user2 = new
            {
                Id = 2,
                Name = "Bob",
                Address = new { Street = "456 Oak Ave", City = "Shelbyville", Zip = "67890" }
            };

            var userType = engine.InferTypeFromExamples(
                new object[] { user1, user2 },
                "User"
            );

            Console.WriteLine(userType.GenerateDefinition());
            Console.WriteLine();

            // Example 5: Collection of objects
            Console.WriteLine("Example 5: Collection of Objects");
            Console.WriteLine("---------------------------------");

            var products = new[]
            {
                new { Id = 1, Name = "Laptop", Price = 999.99 },
                new { Id = 2, Name = "Mouse", Price = 29.99 },
                new { Id = 3, Name = "Keyboard", Price = 79.99 }
            };

            var productListType = engine.InferType(products, "ProductList");

            Console.WriteLine(productListType.GenerateDefinition());
            Console.WriteLine();

            // Example 6: Duck typing demonstration
            Console.WriteLine("Example 6: Duck Typing - Different classes, same structure");
            Console.WriteLine("-----------------------------------------------------------");

            var bird = new Bird { Name = "Tweety", CanFly = true };
            var plane = new Plane { Name = "Boeing 747", CanFly = true };

            var flyableType = engine.InferTypeFromExamples(
                new object[] { bird, plane },
                "Flyable"
            );

            Console.WriteLine("If it has 'Name' and 'CanFly', it's a Flyable thing:");
            Console.WriteLine(flyableType.GenerateDefinition());
            Console.WriteLine();

            // Example 7: Type registry
            Console.WriteLine("Example 7: Type Registry");
            Console.WriteLine("------------------------");

            engine.RegisterType(personType);
            engine.RegisterType(userType);
            engine.RegisterType(flyableType);

            Console.WriteLine("Registered types:");
            foreach (var typeName in engine.GetAllTypes().Keys)
            {
                Console.WriteLine($"  - {typeName}");
            }
            Console.WriteLine();

            Console.WriteLine("=== Demo Complete ===");
        }
    }

    // Helper classes for duck typing example
    public class Bird
    {
        public string Name { get; set; }
        public bool CanFly { get; set; }
    }

    public class Plane
    {
        public string Name { get; set; }
        public bool CanFly { get; set; }
    }
}
