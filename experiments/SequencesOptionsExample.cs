using System;
using System.IO;
using System.Diagnostics;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates various SequencesOptions configurations.
    ///
    /// This example shows:
    /// - Different SequencesOptions settings and their effects
    /// - Performance implications of different options
    /// - Compression quality tradeoffs
    /// - When to use each configuration
    /// </summary>
    public class SequencesOptionsExample
    {
        private const string DbFilename = "sequences-options-test.links";

        public static void Run()
        {
            Console.WriteLine("=== SequencesOptions Configuration Example ===\n");

            Console.WriteLine("SequencesOptions controls:");
            Console.WriteLine("  - Performance characteristics");
            Console.WriteLine("  - Compression quality");
            Console.WriteLine("  - Garbage collection behavior");
            Console.WriteLine("  - Sequence identification\n");

            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(DbFilename, 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);

                // Initialize Unicode
                var unicodeMap = new UnicodeMap(syncLinks);
                unicodeMap.Init();

                DemonstrateNoCompressionNoMarker(syncLinks);
                DemonstrateWithMarkerOnly(syncLinks);
                DemonstrateWithCompressionOnly(syncLinks);
                DemonstrateFullyOptimized(syncLinks);
                ComparePerformance(syncLinks);
            }

            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Configuration 1: No compression, no marker - simplest, fastest
        /// </summary>
        private static void DemonstrateNoCompressionNoMarker(ILinks<ulong> links)
        {
            Console.WriteLine("--- Configuration 1: No Compression, No Marker ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = false,
                UseCompression = false
            });

            var testText = "Simple test sequence";
            var chars = UnicodeMap.FromStringToLinkArray(testText);

            var linksBefore = links.Count();
            var sequenceLink = sequences.Create(chars);
            var linksAfter = links.Count();

            Console.WriteLine($"Created: {sequences.FormatSequence(sequenceLink, AppendLink, true)}");
            Console.WriteLine($"Links added: {linksAfter - linksBefore}");
            Console.WriteLine("Use case: Fastest creation, no garbage collection needed");
            Console.WriteLine("Tradeoff: Uses more storage, no compression benefits\n");
        }

        /// <summary>
        /// Configuration 2: Marker only - enables garbage collection without compression overhead
        /// </summary>
        private static void DemonstrateWithMarkerOnly(ILinks<ulong> links)
        {
            Console.WriteLine("--- Configuration 2: With Marker, No Compression ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = true,
                SequenceMarkerLink = 70000,  // Custom marker link
                UseCompression = false
            });

            var testText = "Marked sequence";
            var chars = UnicodeMap.FromStringToLinkArray(testText);

            var linksBefore = links.Count();
            var sequenceLink = sequences.Create(chars);
            var linksAfter = links.Count();

            Console.WriteLine($"Created: {sequences.FormatSequence(sequenceLink, AppendLink, true)}");
            Console.WriteLine($"Links added: {linksAfter - linksBefore}");
            Console.WriteLine($"Marker link: {70000}");
            Console.WriteLine($"Is sequence (marked): {sequences.IsSequence(sequenceLink)}");
            Console.WriteLine("Use case: Need to distinguish sequences, enable garbage collection");
            Console.WriteLine("Tradeoff: Small overhead for marker, no compression\n");
        }

        /// <summary>
        /// Configuration 3: Compression only - best storage efficiency
        /// </summary>
        private static void DemonstrateWithCompressionOnly(ILinks<ulong> links)
        {
            Console.WriteLine("--- Configuration 3: With Compression, No Marker ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = false,
                UseCompression = true
            });

            var testText = "compression test test compression";
            var chars = UnicodeMap.FromStringToLinkArray(testText);

            var linksBefore = links.Count();
            var sequenceLink = sequences.Create(chars);
            var linksAfter = links.Count();

            Console.WriteLine($"Created: {sequences.FormatSequence(sequenceLink, AppendLink, true)}");
            Console.WriteLine($"Links added: {linksAfter - linksBefore}");
            Console.WriteLine("Use case: Maximize storage efficiency, repeated patterns");
            Console.WriteLine("Tradeoff: Slower creation, can't distinguish subsequences\n");
        }

        /// <summary>
        /// Configuration 4: Both marker and compression - fully optimized
        /// </summary>
        private static void DemonstrateFullyOptimized(ILinks<ulong> links)
        {
            Console.WriteLine("--- Configuration 4: Marker + Compression (Fully Optimized) ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = true,
                SequenceMarkerLink = 80000,
                UseCompression = true
            });

            var testText = "optimized sequence with repeated optimized patterns";
            var chars = UnicodeMap.FromStringToLinkArray(testText);

            var linksBefore = links.Count();
            var sequenceLink = sequences.Create(chars);
            var linksAfter = links.Count();

            Console.WriteLine($"Created: {sequences.FormatSequence(sequenceLink, AppendLink, true)}");
            Console.WriteLine($"Links added: {linksAfter - linksBefore}");
            Console.WriteLine($"Marker link: {80000}");
            Console.WriteLine($"Is sequence (marked): {sequences.IsSequence(sequenceLink)}");
            Console.WriteLine("Use case: Production systems, best overall balance");
            Console.WriteLine("Benefits:");
            Console.WriteLine("  - Compressed storage");
            Console.WriteLine("  - Garbage collection enabled");
            Console.WriteLine("  - Distinguish intended sequences from internal subsequences");
            Console.WriteLine("  - Safe cleanup of unused compression artifacts\n");
        }

        /// <summary>
        /// Compare performance characteristics of different configurations
        /// </summary>
        private static void ComparePerformance(ILinks<ulong> links)
        {
            Console.WriteLine("--- Performance Comparison ---\n");

            var testText = "Performance test text with some repeated patterns and text";
            var chars = UnicodeMap.FromStringToLinkArray(testText);

            // Test each configuration
            var configs = new[]
            {
                new { Name = "No Marker, No Compression", Marker = false, Compression = false },
                new { Name = "With Marker Only", Marker = true, Compression = false },
                new { Name = "With Compression Only", Marker = false, Compression = true },
                new { Name = "Marker + Compression", Marker = true, Compression = true }
            };

            Console.WriteLine($"Test text length: {testText.Length} characters\n");

            int configNum = 1;
            foreach (var config in configs)
            {
                var sequences = new Sequences(links, new SequencesOptions<ulong>
                {
                    UseSequenceMarker = config.Marker,
                    SequenceMarkerLink = config.Marker ? (ulong)(90000 + configNum) : 0,
                    UseCompression = config.Compression
                });

                var linksBefore = links.Count();
                var sw = Stopwatch.StartNew();
                var sequenceLink = sequences.Create(chars);
                sw.Stop();
                var linksAfter = links.Count();

                Console.WriteLine($"{configNum}. {config.Name}:");
                Console.WriteLine($"   Creation time: {sw.Elapsed.TotalMilliseconds:F2} ms");
                Console.WriteLine($"   Links created: {linksAfter - linksBefore}");
                Console.WriteLine();

                configNum++;
            }

            Console.WriteLine("General observations:");
            Console.WriteLine("  - No compression: Fastest, most links");
            Console.WriteLine("  - With compression: Slower, fewer links");
            Console.WriteLine("  - Marker adds minimal overhead");
            Console.WriteLine("  - Best choice depends on use case\n");
        }

        /// <summary>
        /// Helper method to append link representation to string
        /// </summary>
        private static void AppendLink(System.Text.StringBuilder sb, ulong link)
        {
            if (link <= (char.MaxValue + 1))
            {
                sb.Append(UnicodeMap.FromLinkToChar(link));
            }
            else
            {
                sb.Append($"({link})");
            }
        }
    }
}
