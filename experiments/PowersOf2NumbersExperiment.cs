using System;
using System.Numerics;
using System.Collections.Generic;
using Platform.Converters;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Examples;

namespace Experiments
{
    /// <summary>
    /// Demonstrates the use of powers of 2 representation for integer numbers.
    /// This experiment shows how integers can be represented as sequences of powers of 2.
    /// For example: 13 = 2^3 + 2^2 + 2^0 = [3, 2, 0]
    /// </summary>
    public class PowersOf2NumbersExperiment
    {
        public static void Run()
        {
            Console.WriteLine("=== Powers of 2 Integer Numbers Experiment ===");
            Console.WriteLine();

            // Create an in-memory links storage
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);

            // Initialize markers
            var meaningRoot = links.GetOrCreate(1, 1);
            var powersOf2SequenceMarker = links.GetOrCreate(meaningRoot, 2);

            // Create converters for powers and sequences
            var powerToLinkConverter = new TrivialConverter<uint>();
            var linkToPowerConverter = new TrivialConverter<uint>();
            var listToSequenceConverter = new ListToSequenceConverter<uint>(links);
            var sequenceWalker = new SequenceWalker<uint>(links);

            // Create the bidirectional converters
            var integerToPowersOf2 = new IntegerToPowersOf2SequenceConverter<uint>(
                links,
                powerToLinkConverter,
                listToSequenceConverter,
                powersOf2SequenceMarker
            );

            var powersOf2ToInteger = new PowersOf2SequenceToIntegerConverter<uint>(
                links,
                linkToPowerConverter,
                sequenceWalker,
                powersOf2SequenceMarker
            );

            // Test various numbers
            TestNumber(13, integerToPowersOf2, powersOf2ToInteger); // Binary: 1101 = 8 + 4 + 1
            TestNumber(255, integerToPowersOf2, powersOf2ToInteger); // Binary: 11111111 = all bits set
            TestNumber(1024, integerToPowersOf2, powersOf2ToInteger); // Binary: 10000000000 = single bit
            TestNumber(42, integerToPowersOf2, powersOf2ToInteger); // Binary: 101010 = 32 + 8 + 2
            TestNumber(12345, integerToPowersOf2, powersOf2ToInteger); // Larger number

            Console.WriteLine();
            Console.WriteLine($"Total links created: {links.Count()}");
            Console.WriteLine();
            Console.WriteLine("Experiment completed successfully!");
        }

        private static void TestNumber(
            BigInteger number,
            IntegerToPowersOf2SequenceConverter<uint> toConverter,
            PowersOf2SequenceToIntegerConverter<uint> fromConverter)
        {
            Console.WriteLine($"Testing number: {number}");
            Console.WriteLine($"Binary representation: {Convert.ToString((long)number, 2)}");

            // Convert to powers of 2 sequence
            var sequenceLink = toConverter.Convert(number);
            Console.WriteLine($"Sequence link created: {sequenceLink}");

            // Convert back to integer
            var reconstructed = fromConverter.Convert(sequenceLink);
            Console.WriteLine($"Reconstructed number: {reconstructed}");

            // Verify
            var success = number == reconstructed;
            Console.WriteLine($"Verification: {(success ? "PASSED ✓" : "FAILED ✗")}");
            Console.WriteLine();
        }
    }

    // Helper converter that just passes through the value
    public class TrivialConverter<T> : IConverter<T> where T : struct
    {
        public T Convert(T source) => source;
    }

    // Simple list to sequence converter
    public class ListToSequenceConverter<TLink> : LinksOperatorBase<TLink>, IConverter<IList<TLink>, TLink>
        where TLink : struct, IUnsignedNumber<TLink>, IComparisonOperators<TLink, TLink, bool>
    {
        public ListToSequenceConverter(ILinks<TLink> links) : base(links) { }

        public TLink Convert(IList<TLink> source)
        {
            if (source.Count == 0)
            {
                return TLink.Zero;
            }
            if (source.Count == 1)
            {
                return source[0];
            }

            // Build sequence as a chain of links
            var current = source[source.Count - 1];
            for (int i = source.Count - 2; i >= 0; i--)
            {
                current = _links.GetOrCreate(source[i], current);
            }
            return current;
        }
    }

    // Simple sequence walker
    public class SequenceWalker<TLink> : LinksOperatorBase<TLink>, ISequenceWalker<TLink>
        where TLink : struct, IUnsignedNumber<TLink>, IComparisonOperators<TLink, TLink, bool>
    {
        public SequenceWalker(ILinks<TLink> links) : base(links) { }

        public void Walk(TLink sequence, Func<TLink, bool> handler)
        {
            var link = _links.GetLink(sequence);
            if (link == null || link.Count < 3)
            {
                handler(sequence);
                return;
            }

            var source = link[_links.Constants.SourcePart];
            var target = link[_links.Constants.TargetPart];

            if (handler(source))
            {
                Walk(target, handler);
            }
        }
    }
}
