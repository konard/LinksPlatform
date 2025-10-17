using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating BitString indexing.
    /// Provides an interactive way to test and compare BitString index performance
    /// against traditional indexing methods.
    /// </summary>
    public class BitStringIndexCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            Console.WriteLine("=== BitString Index Demo ===");
            Console.WriteLine("A space-efficient indexing layer using compact binary representations");
            Console.WriteLine();

            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links database file", args);
            var testMode = ConsoleHelpers.GetOrReadArgument(1, "Test mode (1=Interactive, 2=File, 3=Benchmark)", args);

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile, UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();
                UnicodeMap.InitNew(syncLinks);

                var bitStringIndex = new BitStringIndex<ulong>(syncLinks);

                switch (testMode)
                {
                    case "1":
                        RunInteractiveMode(bitStringIndex, syncLinks);
                        break;
                    case "2":
                        var inputFile = ConsoleHelpers.GetOrReadArgument(2, "Input file to index", args);
                        RunFileIndexingMode(bitStringIndex, syncLinks, inputFile);
                        break;
                    case "3":
                        RunBenchmarkMode(bitStringIndex, syncLinks);
                        break;
                    default:
                        Console.WriteLine("Invalid test mode. Using interactive mode.");
                        RunInteractiveMode(bitStringIndex, syncLinks);
                        break;
                }
            }
        }

        private void RunInteractiveMode(BitStringIndex<ulong> index, SynchronizedLinks<ulong> links)
        {
            Console.WriteLine("\n=== Interactive Mode ===");
            Console.WriteLine("Enter text sequences to index. Type 'quit' to exit, 'stats' for statistics.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter sequence: ");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (input.ToLower() == "quit")
                {
                    break;
                }

                if (input.ToLower() == "stats")
                {
                    Console.WriteLine(index.GetStatistics());
                    continue;
                }

                try
                {
                    // Convert string to link array
                    var linkArray = UnicodeMap.FromStringToLinkArray(input);

                    // Check if already indexed
                    var existing = index.Search(linkArray);
                    if (!EqualityComparer<ulong>.Default.Equals(existing, default))
                    {
                        Console.WriteLine($"  ✓ Sequence already indexed as link: {existing}");
                    }
                    else
                    {
                        // Add to index
                        var link = index.Add(linkArray);
                        Console.WriteLine($"  ✓ Indexed sequence as link: {link}");
                    }

                    Console.WriteLine($"  Total sequences in index: {index.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error: {ex.Message}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("\nFinal statistics:");
            Console.WriteLine(index.GetStatistics());
        }

        private void RunFileIndexingMode(BitStringIndex<ulong> index, SynchronizedLinks<ulong> links, string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            Console.WriteLine($"\n=== File Indexing Mode ===");
            Console.WriteLine($"Indexing file: {filePath}");
            Console.WriteLine();

            try
            {
                var lines = File.ReadAllLines(filePath);
                var startTime = DateTime.Now;
                int indexedCount = 0;
                int duplicateCount = 0;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var linkArray = UnicodeMap.FromStringToLinkArray(line);

                    if (index.Contains(linkArray))
                    {
                        duplicateCount++;
                    }
                    else
                    {
                        index.Add(linkArray);
                        indexedCount++;
                    }

                    if ((indexedCount + duplicateCount) % 100 == 0)
                    {
                        Console.Write($"\rProcessed: {indexedCount + duplicateCount} lines ({indexedCount} new, {duplicateCount} duplicates)");
                    }
                }

                var elapsed = DateTime.Now - startTime;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"✓ Indexing complete!");
                Console.WriteLine($"  Time elapsed: {elapsed.TotalSeconds:F2} seconds");
                Console.WriteLine($"  Lines processed: {lines.Length}");
                Console.WriteLine($"  Unique sequences indexed: {indexedCount}");
                Console.WriteLine($"  Duplicate sequences: {duplicateCount}");
                Console.WriteLine($"  Indexing rate: {lines.Length / elapsed.TotalSeconds:F0} lines/second");
                Console.WriteLine();
                Console.WriteLine(index.GetStatistics());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during file indexing: {ex.Message}");
            }
        }

        private void RunBenchmarkMode(BitStringIndex<ulong> index, SynchronizedLinks<ulong> links)
        {
            Console.WriteLine("\n=== Benchmark Mode ===");
            Console.WriteLine("Comparing BitString index with traditional methods");
            Console.WriteLine();

            var testSequences = GenerateTestSequences(1000);

            // Benchmark BitString index
            Console.WriteLine("Testing BitString index...");
            var bitStringTime = BenchmarkIndexing(index, testSequences);

            Console.WriteLine();
            Console.WriteLine("=== Benchmark Results ===");
            Console.WriteLine($"BitString index:");
            Console.WriteLine($"  Total time: {bitStringTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"  Average per sequence: {bitStringTime.TotalMilliseconds / testSequences.Length:F4} ms");
            Console.WriteLine($"  Sequences per second: {testSequences.Length / bitStringTime.TotalSeconds:F0}");
            Console.WriteLine();
            Console.WriteLine(index.GetStatistics());
            Console.WriteLine();
            Console.WriteLine("Note: BitString indexes provide significant space savings compared to tries.");
            Console.WriteLine("      Space complexity: O(k) where k is total bits for all sequences");
            Console.WriteLine("      vs. Trie space complexity: O(n*m) where n is number of sequences, m is avg length");
        }

        private string[] GenerateTestSequences(int count)
        {
            var random = new System.Random(42); // Fixed seed for reproducibility
            var sequences = new string[count];

            for (int i = 0; i < count; i++)
            {
                var length = random.Next(5, 50);
                var chars = new char[length];

                for (int j = 0; j < length; j++)
                {
                    chars[j] = (char)random.Next('a', 'z' + 1);
                }

                sequences[i] = new string(chars);
            }

            return sequences;
        }

        private TimeSpan BenchmarkIndexing(BitStringIndex<ulong> index, string[] sequences)
        {
            var startTime = DateTime.Now;

            foreach (var sequence in sequences)
            {
                var linkArray = UnicodeMap.FromStringToLinkArray(sequence);
                index.Add(linkArray);
            }

            return DateTime.Now - startTime;
        }
    }
}
