using System;
using System.Diagnostics;
using System.IO;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Frequencies.Counters;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Experiments
{
    /// <summary>
    /// Benchmark to compare local compression effectiveness vs balanced variant creation.
    /// Addresses issue #95 requirement: "Check if local compression actually helpful when main sequences are loaded"
    /// </summary>
    public static class CompressionBenchmark
    {
        public static void CompareCompressionVsBalancedVariant()
        {
            const string testFile = "benchmark.links";
            const int iterations = 5;

            // Test data - various patterns to test compression effectiveness
            var testStrings = new[]
            {
                "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.",
                "AAAABBBBCCCCAAAABBBBCCCCAAAABBBBCCCC",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet.",
                "abcdefghijklmnopqrstuvwxyz0123456789",
                new string('a', 1000) // Highly repetitive
            };

            Console.WriteLine("=== Compression vs Balanced Variant Benchmark ===\n");

            foreach (var testString in testStrings)
            {
                Console.WriteLine($"Test string length: {testString.Length}");
                Console.WriteLine($"Preview: {(testString.Length > 50 ? testString.Substring(0, 50) + "..." : testString)}\n");

                var balancedTimes = new long[iterations];
                var balancedLinksCount = new long[iterations];

                var compressedTimes = new long[iterations];
                var compressedLinksCount = new long[iterations];

                // Test Balanced Variant (without compression)
                for (int i = 0; i < iterations; i++)
                {
                    File.Delete(testFile);

                    using (var memoryManager = new UInt64UnitedMemoryLinks(testFile, 8 * 1024 * 1024))
                    using (var links = new UInt64Links(memoryManager))
                    {
                        var syncLinks = new SynchronizedLinks<ulong>(links);
                        links.UseUnicode();

                        var sourceArray = UnicodeMap.FromStringToLinkArray(testString);
                        var balancedVariantConverter = new BalancedVariantConverter<ulong>(syncLinks);

                        var sw = Stopwatch.StartNew();
                        balancedVariantConverter.Convert(sourceArray);
                        sw.Stop();

                        balancedTimes[i] = sw.ElapsedMilliseconds;
                        balancedLinksCount[i] = syncLinks.Count() - UnicodeMap.MapSize;
                    }
                }

                // Test Compression + Balanced Variant
                for (int i = 0; i < iterations; i++)
                {
                    File.Delete(testFile);

                    using (var memoryManager = new UInt64UnitedMemoryLinks(testFile, 8 * 1024 * 1024))
                    using (var links = new UInt64Links(memoryManager))
                    {
                        var syncLinks = new SynchronizedLinks<ulong>(links);
                        links.UseUnicode();

                        var sourceArray = UnicodeMap.FromStringToLinkArray(testString);
                        var balancedVariantConverter = new BalancedVariantConverter<ulong>(syncLinks);
                        var frequencyCounter = new TotalSequenceSymbolFrequencyCounter<ulong>(syncLinks);
                        var doubletFrequenciesCache = new LinkFrequenciesCache<ulong>(syncLinks, frequencyCounter);
                        var compressingConverter = new CompressingConverter<ulong>(syncLinks, balancedVariantConverter, doubletFrequenciesCache);

                        var sw = Stopwatch.StartNew();
                        compressingConverter.Convert(sourceArray);
                        sw.Stop();

                        compressedTimes[i] = sw.ElapsedMilliseconds;
                        compressedLinksCount[i] = syncLinks.Count() - UnicodeMap.MapSize;
                    }
                }

                // Calculate averages
                long avgBalancedTime = Average(balancedTimes);
                long avgBalancedLinks = Average(balancedLinksCount);
                long avgCompressedTime = Average(compressedTimes);
                long avgCompressedLinks = Average(compressedLinksCount);

                // Display results
                Console.WriteLine($"Balanced Variant Only:");
                Console.WriteLine($"  Avg Time: {avgBalancedTime}ms");
                Console.WriteLine($"  Avg Links: {avgBalancedLinks}");

                Console.WriteLine($"With Compression:");
                Console.WriteLine($"  Avg Time: {avgCompressedTime}ms");
                Console.WriteLine($"  Avg Links: {avgCompressedLinks}");

                double compressionRatio = (double)avgBalancedLinks / avgCompressedLinks;
                double timeOverhead = ((double)avgCompressedTime / avgBalancedTime - 1.0) * 100;

                Console.WriteLine($"\nCompression Ratio: {compressionRatio:F2}x");
                Console.WriteLine($"Time Overhead: {timeOverhead:F1}%");
                Console.WriteLine($"Verdict: Compression is {(compressionRatio > 1.2 ? "BENEFICIAL" : "NOT BENEFICIAL")} " +
                                 $"(saves {(1 - 1/compressionRatio) * 100:F1}% space at {timeOverhead:F1}% time cost)\n");
                Console.WriteLine(new string('-', 60) + "\n");

                File.Delete(testFile);
            }
        }

        private static long Average(long[] values)
        {
            long sum = 0;
            foreach (var v in values)
            {
                sum += v;
            }
            return sum / values.Length;
        }
    }
}
