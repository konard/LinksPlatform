using System;
using System.IO;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates all compression optimizations from issue #95 working together:
    /// 1. Benchmark: Local compression vs balanced variant
    /// 2. Batch replacement: Multiple pairs at a time
    /// 3. Global dictionary: Cross-file compression
    /// 4. Relative frequency: Percentage-based analysis
    /// </summary>
    public static class IntegratedCompressionExample
    {
        public static void RunFullDemo()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Integrated Compression Optimization Demo (Issue #95)       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            const string linksFile = "integrated-demo.links";
            const string dictionaryFile = "global-dictionary.txt";

            // Sample documents to compress
            var documents = new[]
            {
                "The quick brown fox jumps over the lazy dog. The quick brown fox runs fast.",
                "The quick brown cat sleeps under the big tree. The cat is very lazy.",
                "Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet consectetur."
            };

            // Initialize global dictionary
            var globalDict = new GlobalCompressionDictionary(dictionaryFile, new GlobalDictionarySettings
            {
                MinUsageThreshold = 2,
                MaxDictionarySize = 100
            });

            var relativeFreqTracker = new RelativeFrequencyTracker();

            Console.WriteLine("═══ Processing Documents ═══\n");

            for (int docIndex = 0; docIndex < documents.Length; docIndex++)
            {
                var document = documents[docIndex];
                Console.WriteLine($"Document {docIndex + 1}: \"{document.Substring(0, Math.Min(50, document.Length))}...\"");

                File.Delete(linksFile);

                using (var memoryManager = new UInt64UnitedMemoryLinks(linksFile, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    links.UseUnicode();

                    var sourceArray = UnicodeMap.FromStringToLinkArray(document);

                    // Use batch replacement compressor
                    var batchCompressor = new BatchReplacementCompressor(syncLinks, batchSize: 5);
                    var compressed = batchCompressor.Compress(sourceArray);

                    // Track frequencies for global dictionary
                    var pairFrequencies = new System.Collections.Generic.Dictionary<Link<ulong>, ulong>();
                    for (int i = 1; i < sourceArray.Length; i++)
                    {
                        var pair = new Link<ulong>(sourceArray[i - 1], sourceArray[i]);
                        if (pairFrequencies.ContainsKey(pair))
                        {
                            pairFrequencies[pair]++;
                        }
                        else
                        {
                            pairFrequencies[pair] = 1;
                        }
                    }

                    globalDict.RecordPairFrequencies(pairFrequencies);
                    relativeFreqTracker.AnalyzeSequence(sourceArray);

                    double compressionRatio = (double)sourceArray.Length / compressed.Length;
                    Console.WriteLine($"  Original length: {sourceArray.Length}");
                    Console.WriteLine($"  Compressed length: {compressed.Length}");
                    Console.WriteLine($"  Compression ratio: {compressionRatio:F2}x");
                    Console.WriteLine($"  Space saved: {(1 - 1 / compressionRatio) * 100:F1}%");
                    Console.WriteLine();
                }

                File.Delete(linksFile);
            }

            // Save global dictionary
            globalDict.SaveToFile();
            var dictStats = globalDict.GetStats();

            Console.WriteLine("═══ Global Dictionary Statistics ═══\n");
            Console.WriteLine($"  Total Pairs: {dictStats.TotalPairs}");
            Console.WriteLine($"  Total Frequency: {dictStats.TotalFrequency}");
            Console.WriteLine($"  Average Frequency: {dictStats.AverageFrequency:F2}");
            Console.WriteLine($"  High Frequency Pairs (>=10): {dictStats.HighFrequencyPairs}");
            Console.WriteLine($"  Multi-Document Pairs: {dictStats.MultiDocumentPairs}");
            Console.WriteLine($"  Dictionary saved to: {dictionaryFile}");
            Console.WriteLine();

            // Display relative frequency report
            Console.WriteLine("═══ Relative Frequency Analysis ═══\n");
            var freqReport = relativeFreqTracker.GenerateFrequencyReport(10);
            Console.WriteLine(freqReport);

            // Demonstrate compression candidates
            Console.WriteLine("═══ Top Compression Candidates ═══\n");
            var candidates = relativeFreqTracker.GetCompressionCandidates(minRelativeFrequencyPercent: 0.5);
            Console.WriteLine($"{"Rank",-6} {"Source",-10} {"Target",-10} {"Rel Freq %",-12} {"Savings",-10} {"Score",-10}");
            Console.WriteLine(new string('-', 68));

            for (int i = 0; i < Math.Min(10, candidates.Count); i++)
            {
                var candidate = candidates[i];
                Console.WriteLine($"{i + 1,-6} {candidate.Pair.Source,-10} {candidate.Pair.Target,-10} " +
                                $"{candidate.RelativeFrequencyPercent,-12:F4} {candidate.PotentialSavings,-10} " +
                                $"{candidate.CompressionScore,-10:F2}");
            }

            Console.WriteLine();
            Console.WriteLine("═══ Demo Complete ═══");
            Console.WriteLine();
            Console.WriteLine("Summary of Optimizations Demonstrated:");
            Console.WriteLine("  ✓ Batch replacement: Multiple pairs compressed simultaneously");
            Console.WriteLine("  ✓ Global dictionary: Cross-document pair tracking");
            Console.WriteLine("  ✓ Relative frequency: Percentage-based compression analysis");
            Console.WriteLine("  ✓ Compression scoring: Intelligent candidate selection");
            Console.WriteLine();
        }

        /// <summary>
        /// Runs a comparison between standard compression and optimized batch compression.
        /// </summary>
        public static void CompareBatchVsStandard()
        {
            Console.WriteLine("═══ Batch vs Standard Compression Comparison ═══\n");

            const string linksFile = "comparison.links";
            const string testString = "AAABBBCCCAAABBBCCCAAABBBCCC" +
                                     "DDDEEEFFF" +
                                     "AAABBBCCCAAABBBCCC";

            Console.WriteLine($"Test string length: {testString.Length}");
            Console.WriteLine($"Test string: {testString}\n");

            // Test with different batch sizes
            var batchSizes = new[] { 1, 3, 5, 10 };

            foreach (var batchSize in batchSizes)
            {
                File.Delete(linksFile);

                using (var memoryManager = new UInt64UnitedMemoryLinks(linksFile, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    links.UseUnicode();

                    var sourceArray = UnicodeMap.FromStringToLinkArray(testString);
                    var compressor = new BatchReplacementCompressor(syncLinks, batchSize);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var compressed = compressor.Compress(sourceArray);
                    sw.Stop();

                    double compressionRatio = (double)sourceArray.Length / compressed.Length;
                    Console.WriteLine($"Batch Size {batchSize}:");
                    Console.WriteLine($"  Time: {sw.ElapsedMilliseconds}ms");
                    Console.WriteLine($"  Compressed length: {compressed.Length}");
                    Console.WriteLine($"  Compression ratio: {compressionRatio:F2}x");
                    Console.WriteLine($"  Links created: {syncLinks.Count() - UnicodeMap.MapSize}");
                    Console.WriteLine();
                }

                File.Delete(linksFile);
            }
        }
    }
}
