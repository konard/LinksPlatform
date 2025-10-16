using System;
using Platform.Examples;

namespace Platform.Experiments
{
    /// <summary>
    /// Example demonstrating the usage of ILinksMap for mapping link indices between databases.
    /// <para>Пример демонстрирующий использование ILinksMap для отображения индексов связей между базами данных.</para>
    /// </summary>
    public class LinksMapExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== ILinksMap Example ===");
            Console.WriteLine();

            // Create a links map
            var map = new MemoryLinksMap<ulong>();

            // Scenario: Two databases need to synchronize
            // Database A has local indices: 1, 2, 3
            // Permanent/shared indices: 100, 200, 300
            Console.WriteLine("Creating mappings between local and permanent indices:");
            map.Map(1, 100);
            map.Map(2, 200);
            map.Map(3, 300);
            Console.WriteLine($"Mapped 1 -> 100, 2 -> 200, 3 -> 300");
            Console.WriteLine($"Total mappings: {map.Count}");
            Console.WriteLine();

            // Translate local to permanent
            Console.WriteLine("Translating local indices to permanent indices:");
            for (ulong i = 1; i <= 3; i++)
            {
                if (map.TryGetPermanentIndex(i, out var permanent))
                {
                    Console.WriteLine($"Local {i} -> Permanent {permanent}");
                }
            }
            Console.WriteLine();

            // Translate permanent to local
            Console.WriteLine("Translating permanent indices to local indices:");
            ulong[] permanentIndices = { 100, 200, 300 };
            foreach (var permanent in permanentIndices)
            {
                if (map.TryGetLocalIndex(permanent, out var local))
                {
                    Console.WriteLine($"Permanent {permanent} -> Local {local}");
                }
            }
            Console.WriteLine();

            // Check if mapping exists
            Console.WriteLine("Checking if mappings exist:");
            Console.WriteLine($"Has mapping for local 1: {map.HasMapping(1)}");
            Console.WriteLine($"Has mapping for local 999: {map.HasMapping(999)}");
            Console.WriteLine();

            // Update a mapping
            Console.WriteLine("Updating mapping for local 2 from permanent 200 to 250:");
            map.Map(2, 250);
            if (map.TryGetPermanentIndex(2, out var perm2))
            {
                Console.WriteLine($"Local 2 -> Permanent {perm2}");
            }
            Console.WriteLine($"Permanent 200 -> Local {(map.TryGetLocalIndex(200, out var loc200) ? loc200.ToString() : "null")}");
            if (map.TryGetLocalIndex(250, out var loc250))
            {
                Console.WriteLine($"Permanent 250 -> Local {loc250}");
            }
            Console.WriteLine();

            // Remove a mapping
            Console.WriteLine("Removing mapping for local 3:");
            bool removed = map.Unmap(3);
            Console.WriteLine($"Removed: {removed}");
            Console.WriteLine($"Has mapping for local 3: {map.HasMapping(3)}");
            Console.WriteLine($"Total mappings: {map.Count}");
            Console.WriteLine();

            // Use case: Decentralized network synchronization
            Console.WriteLine("=== Use Case: Decentralized Network Synchronization ===");
            Console.WriteLine();
            Console.WriteLine("Scenario: Two users (Alice and Bob) each have their own database.");
            Console.WriteLine("They want to synchronize a shared concept 'User' with permanent index 1000.");
            Console.WriteLine();

            var aliceMap = new MemoryLinksMap<ulong>();
            var bobMap = new MemoryLinksMap<ulong>();

            // Alice's database: 'User' concept is at local index 5
            aliceMap.Map(5, 1000);
            Console.WriteLine($"Alice's database: Local index 5 represents 'User' (permanent 1000)");

            // Bob's database: 'User' concept is at local index 42
            bobMap.Map(42, 1000);
            Console.WriteLine($"Bob's database: Local index 42 represents 'User' (permanent 1000)");
            Console.WriteLine();

            // When they communicate, they use permanent indices
            Console.WriteLine("Communication between Alice and Bob:");
            if (aliceMap.TryGetPermanentIndex(5, out var alicePerm))
            {
                Console.WriteLine($"Alice says: 'Check my link 5' -> translates to permanent {alicePerm}");
            }
            if (bobMap.TryGetLocalIndex(1000, out var bobLocal))
            {
                Console.WriteLine($"Bob receives permanent 1000 -> translates to his local {bobLocal}");
                Console.WriteLine($"Bob understands: 'Alice is talking about my link {bobLocal}'");
            }
            Console.WriteLine();

            Console.WriteLine("This allows efficient synchronization even when internal indices differ!");
        }
    }
}
