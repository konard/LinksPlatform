using System;
using System.Linq;
using Platform.Sandbox;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstration and experimentation with alternative sequence variants
    /// as described in GitHub issue #139.
    ///
    /// This example shows practical usage of four different approaches to
    /// representing sequences using doublet (pair) structures.
    /// </summary>
    class AlternativeSequencesDemo
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Alternative Sequence Variants - Issue #139 Demo          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            // Run the main demonstration
            AlternativeSequenceVariants.DemonstrateVariants();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Detailed Examples:");
            Console.WriteLine(new string('═', 60) + "\n");

            // Detailed example 1: Comparing compression
            CompressionComparison();

            // Detailed example 2: Endpoint lookup performance
            EndpointLookupDemo();

            // Detailed example 3: Bidirectional navigation
            BidirectionalNavigationDemo();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Demonstrates the compression advantages of the Sequences Map approach.
        /// </summary>
        static void CompressionComparison()
        {
            Console.WriteLine("📊 Compression Comparison:");
            Console.WriteLine("─────────────────────────\n");

            var seq1 = new[] { 'a', 'b', 'c' };
            var seq2 = new[] { 'a', 'b', 'd' };
            var seq3 = new[] { 'a', 'b', 'e' };

            Console.WriteLine("Creating 3 sequences with common prefix 'ab':");
            Console.WriteLine($"  Seq1: {string.Join("", seq1)}");
            Console.WriteLine($"  Seq2: {string.Join("", seq2)}");
            Console.WriteLine($"  Seq3: {string.Join("", seq3)}");

            var seqMap = new AlternativeSequenceVariants.SequencesMap<char>('⊳', '⊲');
            var pairs1 = seqMap.CreateSequence(seq1);
            var pairs2 = seqMap.CreateSequence(seq2);
            var pairs3 = seqMap.CreateSequence(seq3);

            Console.WriteLine("\nWith Sequences Map, the pairs (⊳→a) and (a→b) are reused:");
            Console.WriteLine($"  Total unique pairs needed: Less due to prefix sharing");
            Console.WriteLine($"  Compression achieved: Common prefix stored once\n");
        }

        /// <summary>
        /// Demonstrates the O(1) lookup advantage of Insertion Sequences.
        /// </summary>
        static void EndpointLookupDemo()
        {
            Console.WriteLine("🔍 Endpoint Lookup Demo:");
            Console.WriteLine("─────────────────────────\n");

            var insertSeq = new AlternativeSequenceVariants.InsertionSequences<string>();

            // Create several word sequences
            var words = new[]
            {
                new[] { "the", "quick", "brown", "fox" },
                new[] { "the", "lazy", "dog" },
                new[] { "quick", "brown", "bear" }
            };

            Console.WriteLine("Storing sequences:");
            foreach (var seq in words)
            {
                insertSeq.CreateSequence(seq);
                Console.WriteLine($"  {string.Join(" → ", seq)}");
            }

            Console.WriteLine("\nLookup by endpoints (O(1) complexity):");
            var found1 = insertSeq.FindByEndpoints("the", "fox");
            var found2 = insertSeq.FindByEndpoints("quick", "bear");

            Console.WriteLine($"  Find ('the' ... 'fox'): {string.Join(" → ", found1)}");
            Console.WriteLine($"  Find ('quick' ... 'bear'): {string.Join(" → ", found2)}");
            Console.WriteLine("\nAdvantage: Instant lookup knowing only first and last elements!\n");
        }

        /// <summary>
        /// Demonstrates bidirectional navigation with Doubly-Linked Lists.
        /// </summary>
        static void BidirectionalNavigationDemo()
        {
            Console.WriteLine("↔️  Bidirectional Navigation Demo:");
            Console.WriteLine("───────────────────────────────────\n");

            var dll = new AlternativeSequenceVariants.DoublyLinkedSequences<int>();
            var numbers = new[] { 10, 20, 30, 40, 50 };

            Console.WriteLine($"Creating sequence: {string.Join(" → ", numbers)}");
            var head = dll.CreateSequence(numbers);

            Console.WriteLine("\nForward traversal from head:");
            Console.WriteLine($"  {string.Join(" → ", dll.TraverseForward(head))}");

            var middleNode = dll.FindNode(30);
            Console.WriteLine("\nBackward traversal from middle (30):");
            Console.WriteLine($"  {string.Join(" ← ", dll.TraverseBackward(middleNode))}");

            Console.WriteLine("\nForward traversal from middle (30):");
            Console.WriteLine($"  {string.Join(" → ", dll.TraverseForward(middleNode))}");

            Console.WriteLine("\nAdvantage: Can navigate in both directions from any point!\n");
        }
    }
}
