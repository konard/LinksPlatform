using System;
using Platform.Examples;

namespace Platform.Examples
{
    /// <summary>
    /// Example showing how virtual links structures can integrate with real links storage.
    /// This demonstrates the concept from issue #599: each table as a virtual links structure.
    /// </summary>
    /// <remarks>
    /// Use case: You have a links database for storing relationships, but you also want
    /// to store strings efficiently. Instead of converting each string into links,
    /// you can use a virtual string table that presents a links-like interface
    /// while storing strings in an optimized format.
    /// </remarks>
    public class VirtualLinksIntegrationExample
    {
        /// <summary>
        /// Demonstrates integration of virtual links with a hypothetical links storage system.
        /// </summary>
        public static void RunExample()
        {
            Console.WriteLine("=== Virtual Links Integration Example ===");
            Console.WriteLine();

            // Scenario: We want to store relationships between entities and attach
            // string labels to them, but we don't want to store strings as links.

            Console.WriteLine("Scenario: Storing entity relationships with string labels");
            Console.WriteLine();

            // Real links space: IDs 1-999,999 (for relationships)
            // Virtual string table: IDs 1,000,000+ (for string data)

            var virtualStringTable = new VirtualStringTable<long>(
                baseId: 1_000_000L,
                increment: x => x + 1,
                greaterThanOrEqual: (a, b) => a >= b,
                lessThan: (a, b) => a < b,
                subtract: (a, b) => a - b,
                add: (a, n) => a + n
            );

            // Add some strings to the virtual table
            var nameLabel = virtualStringTable.Add("Name");
            var personLabel = virtualStringTable.Add("Person");
            var johnValue = virtualStringTable.Add("John Doe");
            var emailLabel = virtualStringTable.Add("Email");
            var emailValue = virtualStringTable.Add("john@example.com");

            Console.WriteLine("Added virtual string entries:");
            Console.WriteLine($"  'Name' -> Link {nameLabel}");
            Console.WriteLine($"  'Person' -> Link {personLabel}");
            Console.WriteLine($"  'John Doe' -> Link {johnValue}");
            Console.WriteLine($"  'Email' -> Link {emailLabel}");
            Console.WriteLine($"  'john@example.com' -> Link {emailValue}");
            Console.WriteLine();

            // In a real system, you might have:
            // - Real links: (Entity1, Relationship, Entity2)
            // - Virtual links: (EntityID, StringLabel) where StringLabel is virtual

            Console.WriteLine("Benefits of Virtual Links Structures:");
            Console.WriteLine();
            Console.WriteLine("1. Efficient Storage:");
            Console.WriteLine("   Strings are stored as strings, not decomposed into character links.");
            Console.WriteLine();
            Console.WriteLine("2. Uniform Access Pattern:");
            Console.WriteLine("   Both real and virtual links use the same interface.");
            Console.WriteLine();
            Console.WriteLine("3. Traversability:");
            Console.WriteLine("   Virtual structures can be traversed just like real links:");

            virtualStringTable.Traverse(johnValue, (link, source, target) =>
            {
                var value = virtualStringTable.GetValue(link);
                Console.WriteLine($"   Link {link}: '{value}' (Source: {source}, Target: {target})");
            });

            Console.WriteLine();
            Console.WriteLine("4. Non-Physical Storage:");
            Console.WriteLine("   Virtual links don't occupy space in the physical links database.");
            Console.WriteLine($"   String count: {virtualStringTable.Count}");
            Console.WriteLine("   These exist in the virtual string table, not in links space.");
            Console.WriteLine();

            Console.WriteLine("5. Type-Specific Optimization:");
            Console.WriteLine("   Different data types can have their own virtual structures:");
            Console.WriteLine("   - VirtualStringTable for strings");
            Console.WriteLine("   - VirtualNumberTable for numbers");
            Console.WriteLine("   - VirtualBinaryTable for binary data");
            Console.WriteLine("   Each optimized for its specific use case.");
            Console.WriteLine();

            Console.WriteLine("=== End of Example ===");
        }
    }
}
