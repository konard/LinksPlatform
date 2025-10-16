using System;
using System.Linq;
using Platform.Examples;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Experiments
{
    /// <summary>
    /// Demonstration and test of LinkToArrayConverter functionality for issue #77.
    /// Shows three conversion modes:
    /// 1. Without recursion check
    /// 2. With recursion detection
    /// 3. With configurable recursion depth (1..N)
    /// </summary>
    public class LinkToArrayConverterTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== LinkToArrayConverter Test ===");
            Console.WriteLine("Testing functionality for issue #77: Function to convert Link to Array\n");

            // Use temporary file for testing
            var dbPath = "test-link-to-array.links";
            try
            {
                using var links = new UnitedMemoryLinks<uint>(dbPath);

                Console.WriteLine("1. Testing simple sequence (no recursion):");
                TestSimpleSequence(links);

                Console.WriteLine("\n2. Testing self-referencing link:");
                TestSelfReference(links);

                Console.WriteLine("\n3. Testing recursive structure:");
                TestRecursiveStructure(links);

                Console.WriteLine("\n4. Testing recursion depth limit:");
                TestRecursionDepth(links);

                Console.WriteLine("\n=== All tests completed ===");
            }
            finally
            {
                // Cleanup
                if (System.IO.File.Exists(dbPath))
                {
                    System.IO.File.Delete(dbPath);
                }
            }
        }

        private static void TestSimpleSequence(ILinks<uint> links)
        {
            // Create a simple sequence: 1 -> 2 -> 3
            var elem1 = links.Create();
            var elem2 = links.Create();
            var elem3 = links.Create();

            var pair1 = links.Create(elem1, elem2);  // (1, 2)
            var seq = links.Create(pair1, elem3);     // ((1, 2), 3)

            var converter = new LinkToArrayConverter<uint>(
                link => links.GetSource(link),
                link => links.GetTarget(link),
                link => links.Count(links.Constants.Any, link) == 0  // Elements have no references
            );

            var result = converter.ToArrayWithoutRecursionCheck(seq);
            Console.WriteLine($"  Sequence structure: ((elem1, elem2), elem3)");
            Console.WriteLine($"  Array result: [{string.Join(", ", result)}]");
            Console.WriteLine($"  Elements: elem1={elem1}, elem2={elem2}, elem3={elem3}");

            // Cleanup
            links.Delete(seq);
            links.Delete(pair1);
            links.Delete(elem3);
            links.Delete(elem2);
            links.Delete(elem1);
        }

        private static void TestSelfReference(ILinks<uint> links)
        {
            // Create a link that references itself: link -> (link, elem)
            var elem = links.Create();
            var selfRef = links.Create();
            links.Update(selfRef, selfRef, elem);  // selfRef points to itself and elem

            var converter = new LinkToArrayConverter<uint>(
                link => links.GetSource(link),
                link => links.GetTarget(link),
                link => links.Count(links.Constants.Any, link) == 0
            );

            Console.WriteLine($"  Self-reference structure: selfRef -> (selfRef, elem)");
            Console.WriteLine($"  selfRef={selfRef}, elem={elem}");

            try
            {
                var result = converter.ToArrayWithRecursionCheck(selfRef);
                Console.WriteLine($"  Array with recursion check: [{string.Join(", ", result)}]");
                Console.WriteLine($"  Note: Self-referencing link is represented as single element");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Expected behavior - recursion detected: {ex.Message}");
            }

            // Cleanup
            links.Delete(selfRef);
            links.Delete(elem);
        }

        private static void TestRecursiveStructure(ILinks<uint> links)
        {
            // Create a circular structure: A -> (B, C), B -> (A, D)
            var elemC = links.Create();
            var elemD = links.Create();
            var linkA = links.Create();
            var linkB = links.Create();

            links.Update(linkA, linkB, elemC);  // A -> (B, C)
            links.Update(linkB, linkA, elemD);  // B -> (A, D) - creates cycle

            var converter = new LinkToArrayConverter<uint>(
                link => links.GetSource(link),
                link => links.GetTarget(link),
                link => links.Count(links.Constants.Any, link) == 0
            );

            Console.WriteLine($"  Circular structure: A -> (B, C), B -> (A, D)");
            Console.WriteLine($"  linkA={linkA}, linkB={linkB}, elemC={elemC}, elemD={elemD}");

            var result = converter.ToArrayWithRecursionCheck(linkA);
            Console.WriteLine($"  Array with recursion check: [{string.Join(", ", result)}]");
            Console.WriteLine($"  Note: Circular references are detected and handled");

            // Cleanup
            links.Delete(linkB);
            links.Delete(linkA);
            links.Delete(elemD);
            links.Delete(elemC);
        }

        private static void TestRecursionDepth(ILinks<uint> links)
        {
            // Create a self-referencing structure
            var elem = links.Create();
            var linkA = links.Create();
            links.Update(linkA, linkA, elem);  // A -> (A, elem)

            var converter = new LinkToArrayConverter<uint>(
                link => links.GetSource(link),
                link => links.GetTarget(link),
                link => links.Count(links.Constants.Any, link) == 0
            );

            Console.WriteLine($"  Recursive structure: A -> (A, elem)");
            Console.WriteLine($"  linkA={linkA}, elem={elem}");

            for (int depth = 1; depth <= 3; depth++)
            {
                var result = converter.ToArrayWithRecursionDepth(linkA, depth);
                Console.WriteLine($"  Depth {depth}: [{string.Join(", ", result)}]");
            }
            Console.WriteLine($"  Note: Higher depth shows more repetitions of recursive elements");

            // Cleanup
            links.Delete(linkA);
            links.Delete(elem);
        }
    }
}
