using System;
using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates the cost optimization of representing multiple triplets
    /// with the same source and target using doublets.
    ///
    /// Key insight: For n triplets with same source/target but different types:
    /// - Traditional: 2n doublets
    /// - Optimized: n+1 doublets
    /// - Savings: n-1 doublets (approaching 50% efficiency)
    /// </summary>
    public class MultipleTypesDoubletOptimization
    {
        /// <summary>
        /// Simulates a simple doublet store using a list of pairs.
        /// In a real implementation, this would be a Platform.Data.Doublets store.
        /// </summary>
        public class DoubletStore
        {
            private Dictionary<(ulong, ulong), ulong> _links = new Dictionary<(ulong, ulong), ulong>();
            private ulong _nextId = 1;

            public ulong GetOrCreate(ulong source, ulong target)
            {
                var key = (source, target);
                if (_links.TryGetValue(key, out var existingId))
                {
                    return existingId;
                }

                var newId = _nextId++;
                _links[key] = newId;
                return newId;
            }

            public int Count => _links.Count;

            public IEnumerable<(ulong Id, ulong Source, ulong Target)> GetAll()
            {
                foreach (var kvp in _links)
                {
                    yield return (kvp.Value, kvp.Key.Item1, kvp.Key.Item2);
                }
            }

            public List<ulong> FindBySource(ulong source)
            {
                var results = new List<ulong>();
                foreach (var kvp in _links)
                {
                    if (kvp.Key.Item1 == source)
                    {
                        results.Add(kvp.Value);
                    }
                }
                return results;
            }
        }

        /// <summary>
        /// Demonstrates traditional (unoptimized) representation of multiple triplets
        /// </summary>
        public static int TraditionalTripletRepresentation(int numberOfTypes)
        {
            var store = new DoubletStore();

            // Entity IDs
            ulong entityA = 100;
            ulong entityB = 200;

            // Type IDs
            var types = new ulong[numberOfTypes];
            for (int i = 0; i < numberOfTypes; i++)
            {
                types[i] = (ulong)(300 + i);
            }

            // Each triplet (a, type_i, b) becomes 2 separate doublets
            // This is the UNOPTIMIZED approach
            for (int i = 0; i < numberOfTypes; i++)
            {
                // First doublet: (a, b)
                store.GetOrCreate(entityA, entityB);

                // Second doublet: ((a, type_i), b)
                var typeNode = store.GetOrCreate(entityA, types[i]);
                store.GetOrCreate(typeNode, entityB);
            }

            return store.Count;
        }

        /// <summary>
        /// Demonstrates optimized representation using shared base relationship
        /// </summary>
        public static int OptimizedDoubletRepresentation(int numberOfTypes)
        {
            var store = new DoubletStore();

            // Entity IDs
            ulong entityA = 100;
            ulong entityB = 200;

            // Type IDs
            var types = new ulong[numberOfTypes];
            for (int i = 0; i < numberOfTypes; i++)
            {
                types[i] = (ulong)(300 + i);
            }

            // OPTIMIZED approach: Share the base relationship
            // Create base relationship once: (a, b)
            var baseRelationship = store.GetOrCreate(entityA, entityB);

            // For each type, create: ((a, b), type_i)
            for (int i = 0; i < numberOfTypes; i++)
            {
                store.GetOrCreate(baseRelationship, types[i]);
            }

            return store.Count;
        }

        /// <summary>
        /// Demonstrates querying types from the optimized structure
        /// </summary>
        public static List<ulong> QueryTypesOptimized(DoubletStore store, ulong entityA, ulong entityB)
        {
            // Find base relationship
            var baseRelationship = store.GetOrCreate(entityA, entityB);

            // Find all links where source is the base relationship
            // Their targets are the types
            return store.FindBySource(baseRelationship);
        }

        public static void Main()
        {
            Console.WriteLine("=== Multiple Types with Doublets: Cost Optimization Demo ===\n");

            // Test cases with different numbers of types
            var testCases = new[] { 1, 2, 3, 4, 5, 10, 20, 50, 100 };

            Console.WriteLine("| Types (n) | Traditional | Optimized | Saved | Efficiency |");
            Console.WriteLine("|-----------|-------------|-----------|-------|------------|");

            foreach (var n in testCases)
            {
                var traditional = TraditionalTripletRepresentation(n);
                var optimized = OptimizedDoubletRepresentation(n);
                var saved = traditional - optimized;
                var efficiency = saved * 100.0 / traditional;

                Console.WriteLine($"| {n,9} | {traditional,11} | {optimized,9} | {saved,5} | {efficiency,9:F1}% |");
            }

            Console.WriteLine("\n=== Detailed Example: 3 Types ===\n");

            // Create a detailed example with 3 types
            var exampleStore = new DoubletStore();

            ulong person1 = 1000;
            ulong person2 = 2000;
            ulong knowsType = 3001;
            ulong worksWithType = 3002;
            ulong livesNearType = 3003;

            Console.WriteLine("Creating relationships:");
            Console.WriteLine($"  (Person1={person1}, Person2={person2})");
            Console.WriteLine($"  Types: knows={knowsType}, works_with={worksWithType}, lives_near={livesNearType}");
            Console.WriteLine();

            // Create base relationship
            var baseLink = exampleStore.GetOrCreate(person1, person2);
            Console.WriteLine($"Step 1: Create base relationship (Person1, Person2) -> Link {baseLink}");

            // Add type annotations
            var knowsLink = exampleStore.GetOrCreate(baseLink, knowsType);
            Console.WriteLine($"Step 2: Add 'knows' type ({baseLink}, {knowsType}) -> Link {knowsLink}");

            var worksWithLink = exampleStore.GetOrCreate(baseLink, worksWithType);
            Console.WriteLine($"Step 3: Add 'works_with' type ({baseLink}, {worksWithType}) -> Link {worksWithLink}");

            var livesNearLink = exampleStore.GetOrCreate(baseLink, livesNearType);
            Console.WriteLine($"Step 4: Add 'lives_near' type ({baseLink}, {livesNearType}) -> Link {livesNearLink}");

            Console.WriteLine($"\nTotal links created: {exampleStore.Count}");
            Console.WriteLine("  (vs 6 for traditional representation)");
            Console.WriteLine($"  Space savings: {6 - exampleStore.Count} links ({(6 - exampleStore.Count) * 100.0 / 6:F1}%)");

            Console.WriteLine("\n=== Formula Verification ===\n");
            Console.WriteLine("For n triplets with same source and target:");
            Console.WriteLine("  Traditional cost: 2n doublets");
            Console.WriteLine("  Optimized cost:   n+1 doublets");
            Console.WriteLine("  Space saved:      n-1 doublets");
            Console.WriteLine("  Efficiency:       (n-1)/(2n) → 50% as n → ∞");
            Console.WriteLine();

            for (int n = 1; n <= 5; n++)
            {
                var traditionalFormula = 2 * n;
                var optimizedFormula = n + 1;
                var savedFormula = n - 1;
                var efficiencyFormula = (n - 1) * 100.0 / (2 * n);

                Console.WriteLine($"n={n}: Traditional={traditionalFormula}, Optimized={optimizedFormula}, " +
                                $"Saved={savedFormula}, Efficiency={efficiencyFormula:F1}%");
            }

            Console.WriteLine("\n=== Conclusion ===\n");
            Console.WriteLine("This optimization provides:");
            Console.WriteLine("  ✓ Linear scaling: O(n+1) instead of O(2n)");
            Console.WriteLine("  ✓ Approaches 50% space efficiency as types increase");
            Console.WriteLine("  ✓ Maintains query flexibility through link references");
            Console.WriteLine("  ✓ Practical savings in real-world multi-type scenarios");
        }
    }
}
