using System;
using Platform.Examples;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates how virtual links structures work.
    /// This experiment shows that traditional table data can be accessed through
    /// a links-like interface without being physically stored in the links space.
    /// </summary>
    public class VirtualLinksStructureDemo
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Virtual Links Structure Demonstration ===");
            Console.WriteLine();
            Console.WriteLine("This demo shows how a table (e.g., strings table) can be");
            Console.WriteLine("represented as a virtual links structure that can be traversed");
            Console.WriteLine("but is not actually stored in the physical links space.");
            Console.WriteLine();

            // Create a virtual string table with base ID of 1000000
            // This ensures virtual links don't conflict with real links
            var baseId = 1000000L;
            var stringTable = new VirtualStringTable<long>(
                baseId,
                increment: x => x + 1,
                greaterThanOrEqual: (a, b) => a >= b,
                lessThan: (a, b) => a < b,
                subtract: (a, b) => a - b,
                add: (a, n) => a + n
            );

            Console.WriteLine("Step 1: Adding strings to the virtual table");
            Console.WriteLine("-------------------------------------------");
            var link1 = stringTable.Add("Hello");
            var link2 = stringTable.Add("World");
            var link3 = stringTable.Add("Virtual");
            var link4 = stringTable.Add("Links");

            Console.WriteLine($"Added 'Hello' with virtual link ID: {link1}");
            Console.WriteLine($"Added 'World' with virtual link ID: {link2}");
            Console.WriteLine($"Added 'Virtual' with virtual link ID: {link3}");
            Console.WriteLine($"Added 'Links' with virtual link ID: {link4}");
            Console.WriteLine($"Total count: {stringTable.Count}");
            Console.WriteLine();

            Console.WriteLine("Step 2: Accessing strings through virtual link interface");
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine($"Value at link {link1}: {stringTable.GetValue(link1)}");
            Console.WriteLine($"Value at link {link2}: {stringTable.GetValue(link2)}");
            Console.WriteLine();

            Console.WriteLine("Step 3: Traversing virtual links structure");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Each virtual link has a source and target, just like real links:");
            Console.WriteLine();

            stringTable.Traverse(link1, (link, source, target) =>
            {
                Console.WriteLine($"Link: {link}");
                Console.WriteLine($"  Value: '{stringTable.GetValue(link)}'");
                Console.WriteLine($"  Source: {source} (beginning of string)");
                Console.WriteLine($"  Target: {target} (end marker/metadata)");
            });

            Console.WriteLine();
            stringTable.Traverse(link2, (link, source, target) =>
            {
                Console.WriteLine($"Link: {link}");
                Console.WriteLine($"  Value: '{stringTable.GetValue(link)}'");
                Console.WriteLine($"  Source: {source}");
                Console.WriteLine($"  Target: {target}");
            });

            Console.WriteLine();
            Console.WriteLine("Step 4: Checking link membership");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Is {link1} in virtual structure? {stringTable.Contains(link1)}");
            Console.WriteLine($"Is {link3} in virtual structure? {stringTable.Contains(link3)}");
            Console.WriteLine($"Is 42 in virtual structure? {stringTable.Contains(42)}");
            Console.WriteLine($"Is 999999 in virtual structure? {stringTable.Contains(999999)}");
            Console.WriteLine();

            Console.WriteLine("=== Key Concepts ===");
            Console.WriteLine();
            Console.WriteLine("1. Virtual Storage: Strings are stored in a table (Dictionary),");
            Console.WriteLine("   not in the physical links space.");
            Console.WriteLine();
            Console.WriteLine("2. Traversable: Despite not being real links, we can traverse");
            Console.WriteLine("   the structure using GetSource() and GetTarget().");
            Console.WriteLine();
            Console.WriteLine("3. Uniform Access: The same interface can be used for both");
            Console.WriteLine("   real links and virtual links structures.");
            Console.WriteLine();
            Console.WriteLine("4. Efficient Storage: Specialized data (like strings) is stored");
            Console.WriteLine("   optimally while still being accessible through the links API.");
            Console.WriteLine();
            Console.WriteLine("=== End of Demonstration ===");
        }
    }
}
