using System;
using System.Diagnostics;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Sandbox;

namespace Platform.Experiments
{
    /// <summary>
    /// Experiment to compare performance and correctness of Compressor vs ConcurrentCompressor
    /// </summary>
    public static class ConcurrentCompressorTest
    {
        public static void Run()
        {
            Console.WriteLine("=== ConcurrentCompressor Performance Test ===\n");

            // Test data
            var testStrings = new[]
            {
                "aaabbbcccdddeeefff",  // Simple repeated pattern
                "abcabcabcabcabc",      // Repeated sequence
                "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.", // Realistic text
                GenerateLargeTestString(10000) // Large test
            };

            for (int i = 0; i < testStrings.Length; i++)
            {
                Console.WriteLine($"\n--- Test {i + 1}: String length {testStrings[i].Length} ---");
                CompareCompressors(testStrings[i]);
            }
        }

        private static void CompareCompressors(string input)
        {
            // Test with regular Compressor
            var regularResult = TestCompressor(input, useRegular: true);

            // Test with ConcurrentCompressor
            var concurrentResult = TestCompressor(input, useRegular: false);

            // Compare results
            Console.WriteLine($"\nComparison:");
            Console.WriteLine($"  Regular Time:     {regularResult.elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"  Concurrent Time:  {concurrentResult.elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"  Speedup:          {regularResult.elapsed.TotalMilliseconds / concurrentResult.elapsed.TotalMilliseconds:F2}x");
            Console.WriteLine($"  Regular Size:     {regularResult.compressedSize}");
            Console.WriteLine($"  Concurrent Size:  {concurrentResult.compressedSize}");
            Console.WriteLine($"  Results Match:    {regularResult.compressedSize == concurrentResult.compressedSize}");
        }

        private static (TimeSpan elapsed, int compressedSize) TestCompressor(string input, bool useRegular)
        {
            var dbFile = useRegular ? "test_regular.links" : "test_concurrent.links";
            File.Delete(dbFile);

            using (var memoryManager = new UInt64UnitedMemoryLinks(dbFile, 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryManager))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();

                var inputArray = UnicodeMap.FromStringToLinkArray(input);

                var sw = Stopwatch.StartNew();
                ulong[] compressed;

                if (useRegular)
                {
                    var compressor = new CompressionExperiments.Compressor(syncLinks);
                    compressed = compressor.Precompress0(inputArray);
                }
                else
                {
                    var compressor = new CompressionExperiments.ConcurrentCompressor(syncLinks);
                    compressed = compressor.Precompress0(inputArray);
                }

                sw.Stop();

                return (sw.Elapsed, compressed?.Length ?? 0);
            }
        }

        private static string GenerateLargeTestString(int size)
        {
            var words = new[] { "hello", "world", "test", "data", "compression", "performance", "parallel", "concurrent" };
            var random = new Random(42); // Fixed seed for reproducibility
            var result = new System.Text.StringBuilder();

            while (result.Length < size)
            {
                result.Append(words[random.Next(words.Length)]);
                result.Append(" ");
            }

            return result.ToString().Substring(0, Math.Min(size, result.Length));
        }
    }
}
