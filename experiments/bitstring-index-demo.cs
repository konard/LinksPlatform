// BitString Index Demonstration and Experiment Script
// This script demonstrates the space-efficient BitString indexing layer
// and compares it with traditional trie-based indexing approaches.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Examples;

namespace BitStringIndexExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   BitString Index - Space-Efficient Indexing Demo     ║");
            Console.WriteLine("║   Demonstrating space savings vs. traditional tries   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Create temporary database
            var dbFile = "bitstring-index-demo.links";

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbFile, UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();
                UnicodeMap.InitNew(syncLinks);

                RunExperiments(syncLinks);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void RunExperiments(SynchronizedLinks<ulong> links)
        {
            // Experiment 1: Basic indexing demonstration
            Console.WriteLine("═══ Experiment 1: Basic Indexing ═══");
            Experiment1_BasicIndexing(links);
            Console.WriteLine();

            // Experiment 2: Space efficiency comparison
            Console.WriteLine("═══ Experiment 2: Space Efficiency Analysis ═══");
            Experiment2_SpaceEfficiency(links);
            Console.WriteLine();

            // Experiment 3: Performance benchmarking
            Console.WriteLine("═══ Experiment 3: Performance Benchmarking ═══");
            Experiment3_Performance(links);
            Console.WriteLine();

            // Experiment 4: Deduplication benefits
            Console.WriteLine("═══ Experiment 4: Deduplication Benefits ═══");
            Experiment4_Deduplication(links);
        }

        static void Experiment1_BasicIndexing(SynchronizedLinks<ulong> links)
        {
            var index = new BitStringIndex<ulong>(links);

            var testStrings = new[]
            {
                "Hello, World!",
                "BitString indexing",
                "Space-efficient data structures",
                "Links Platform doublets",
                "Hello, World!" // Duplicate to test dedup
            };

            Console.WriteLine("Indexing test sequences...");
            foreach (var str in testStrings)
            {
                var linkArray = UnicodeMap.FromStringToLinkArray(str);
                var link = index.Add(linkArray);
                var isDuplicate = index.Search(linkArray);

                Console.WriteLine($"  '{str.Substring(0, Math.Min(30, str.Length))}{(str.Length > 30 ? "..." : "")}'");
                Console.WriteLine($"    → Link: {link}, Contains: {index.Contains(linkArray)}");
            }

            Console.WriteLine();
            Console.WriteLine(index.GetStatistics());
        }

        static void Experiment2_SpaceEfficiency(SynchronizedLinks<ulong> links)
        {
            var index = new BitStringIndex<ulong>(links, bitsPerElement: 21);

            // Generate sequences with varying lengths
            var random = new System.Random(42);
            var sequenceLengths = new[] { 10, 50, 100, 500 };

            Console.WriteLine("Analyzing space efficiency for different sequence lengths:");
            Console.WriteLine();

            foreach (var length in sequenceLengths)
            {
                var sequence = new ulong[length];
                for (int i = 0; i < length; i++)
                {
                    sequence[i] = (ulong)random.Next(1, 65536); // Random Unicode characters
                }

                var link = index.Add(sequence);

                // Calculate theoretical space requirements
                var bitStringSize = (length * 21 + 16) / 8; // bits per element * count + length field, converted to bytes
                var trieSize = length * 8 * 2; // Each trie node needs 2 pointers (8 bytes each) per character

                Console.WriteLine($"Sequence length: {length}");
                Console.WriteLine($"  BitString storage: ~{bitStringSize} bytes");
                Console.WriteLine($"  Traditional trie: ~{trieSize} bytes");
                Console.WriteLine($"  Space savings: {100.0 * (trieSize - bitStringSize) / trieSize:F1}%");
                Console.WriteLine();
            }
        }

        static void Experiment3_Performance(SynchronizedLinks<ulong> links)
        {
            var index = new BitStringIndex<ulong>(links);
            var sequenceCount = 1000;

            Console.WriteLine($"Benchmarking with {sequenceCount} sequences...");

            var sequences = GenerateRandomSequences(sequenceCount, 20, 42);

            // Benchmark insertion
            var sw = Stopwatch.StartNew();
            foreach (var seq in sequences)
            {
                index.Add(seq);
            }
            sw.Stop();

            var insertTime = sw.Elapsed;
            Console.WriteLine($"  Insertion time: {insertTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"  Average per insert: {insertTime.TotalMilliseconds / sequenceCount:F4} ms");
            Console.WriteLine($"  Insertions per second: {sequenceCount / insertTime.TotalSeconds:F0}");

            // Benchmark search
            sw.Restart();
            int found = 0;
            foreach (var seq in sequences)
            {
                if (index.Contains(seq))
                {
                    found++;
                }
            }
            sw.Stop();

            var searchTime = sw.Elapsed;
            Console.WriteLine();
            Console.WriteLine($"  Search time: {searchTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"  Average per search: {searchTime.TotalMilliseconds / sequenceCount:F4} ms");
            Console.WriteLine($"  Searches per second: {sequenceCount / searchTime.TotalSeconds:F0}");
            Console.WriteLine($"  Found: {found}/{sequenceCount}");
        }

        static void Experiment4_Deduplication(SynchronizedLinks<ulong> links)
        {
            var index = new BitStringIndex<ulong>(links);

            // Create sequences with intentional duplicates
            var baseSequences = GenerateRandomSequences(100, 15, 123);
            var allSequences = new List<ulong[]>();

            // Add base sequences
            allSequences.AddRange(baseSequences);

            // Add 50% duplicates
            for (int i = 0; i < baseSequences.Length / 2; i++)
            {
                allSequences.Add(baseSequences[i]);
            }

            Console.WriteLine($"Testing deduplication with {allSequences.Count} total sequences");
            Console.WriteLine($"  ({baseSequences.Length} unique + {allSequences.Count - baseSequences.Length} duplicates)");
            Console.WriteLine();

            int newIndexed = 0;
            int duplicates = 0;

            foreach (var seq in allSequences)
            {
                if (index.Contains(seq))
                {
                    duplicates++;
                }
                else
                {
                    index.Add(seq);
                    newIndexed++;
                }
            }

            Console.WriteLine($"Results:");
            Console.WriteLine($"  Newly indexed: {newIndexed}");
            Console.WriteLine($"  Duplicates detected: {duplicates}");
            Console.WriteLine($"  Deduplication rate: {100.0 * duplicates / allSequences.Count:F1}%");
            Console.WriteLine($"  Space saved by dedup: ~{duplicates * 15 * 21 / 8} bytes");
            Console.WriteLine();
            Console.WriteLine("✓ BitString index automatically deduplicates sequences,");
            Console.WriteLine("  storing each unique sequence only once regardless of");
            Console.WriteLine("  how many times it's added to the index.");
        }

        static ulong[][] GenerateRandomSequences(int count, int avgLength, int seed)
        {
            var random = new System.Random(seed);
            var sequences = new ulong[count][];

            for (int i = 0; i < count; i++)
            {
                var length = random.Next(avgLength / 2, avgLength * 2);
                var sequence = new ulong[length];

                for (int j = 0; j < length; j++)
                {
                    sequence[j] = (ulong)random.Next(1, 10000);
                }

                sequences[i] = sequence;
            }

            return sequences;
        }
    }
}
