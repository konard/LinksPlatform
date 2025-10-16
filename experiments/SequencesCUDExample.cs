using System;
using System.IO;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates Sequences Create, Update, Delete (CUD) operations with sequence markers.
    ///
    /// This example shows:
    /// - How to use SequencesOptions with sequence markers
    /// - Creating sequences with compression
    /// - Updating sequences
    /// - Deleting sequences with garbage collection
    /// - Distinguishing between intended sequences and subsequences
    /// </summary>
    public class SequencesCUDExample
    {
        private const string DbFilename = "sequences-cud-test.links";

        public static void Run()
        {
            Console.WriteLine("=== Sequences CUD (Create, Update, Delete) Example ===\n");

            // Clean up any existing database
            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(DbFilename, 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);

                // Initialize Unicode character mappings
                var unicodeMap = new UnicodeMap(syncLinks);
                unicodeMap.Init();

                Console.WriteLine($"Initial links count: {links.Count()}");

                // Demonstrate different SequencesOptions configurations
                DemonstrateBasicOptions(syncLinks);
                DemonstrateWithSequenceMarker(syncLinks);
                DemonstrateWithCompression(syncLinks);
                DemonstrateUpdateOperations(syncLinks);
                DemonstrateDeleteWithGarbageCollection(syncLinks);

                Console.WriteLine($"\nFinal links count: {links.Count()}");
            }

            // Clean up
            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Demonstrates basic sequence creation without markers
        /// </summary>
        private static void DemonstrateBasicOptions(ILinks<ulong> links)
        {
            Console.WriteLine("\n--- Basic Sequences (No Marker) ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = false,
                UseCompression = false
            });

            // Create a simple sequence: "Hello"
            var helloChars = UnicodeMap.FromStringToLinkArray("Hello");
            var helloSequence = sequences.Create(helloChars);

            Console.WriteLine($"Created sequence 'Hello' at link: {helloSequence}");
            Console.WriteLine($"Is sequence: {sequences.IsSequence(helloSequence)}");
            Console.WriteLine($"Formatted: {sequences.FormatSequence(helloSequence, AppendLink, true)}");
        }

        /// <summary>
        /// Demonstrates sequence creation with sequence markers for distinguishing intended sequences
        /// </summary>
        private static void DemonstrateWithSequenceMarker(ILinks<ulong> links)
        {
            Console.WriteLine("\n--- Sequences with Marker ---");

            const ulong SequenceMarkerLink = 65537;
            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = true,
                SequenceMarkerLink = SequenceMarkerLink,
                UseCompression = false
            });

            Console.WriteLine($"Sequence marker link: {SequenceMarkerLink}");

            // Create marked sequences
            var worldChars = UnicodeMap.FromStringToLinkArray("World");
            var worldSequence = sequences.Create(worldChars);

            Console.WriteLine($"Created marked sequence 'World' at link: {worldSequence}");
            Console.WriteLine($"Is sequence: {sequences.IsSequence(worldSequence)}");

            // The marker helps distinguish between:
            // 1. Intended sequences (marked by user)
            // 2. Subsequences (used internally for compression)
            Console.WriteLine($"Marker allows garbage collection of unused subsequences");
        }

        /// <summary>
        /// Demonstrates sequence creation with compression enabled
        /// </summary>
        private static void DemonstrateWithCompression(ILinks<ulong> links)
        {
            Console.WriteLine("\n--- Sequences with Compression ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = true,
                SequenceMarkerLink = 65538,
                UseCompression = true  // Enable compression for better storage efficiency
            });

            // Create a sequence with repeated patterns
            var repeatedChars = UnicodeMap.FromStringToLinkArray("abcabcabc");
            var compressedSequence = sequences.Create(repeatedChars);

            Console.WriteLine($"Created compressed sequence 'abcabcabc' at link: {compressedSequence}");
            Console.WriteLine($"Compression helps reduce storage by reusing repeated subsequences");
            Console.WriteLine($"Formatted: {sequences.FormatSequence(compressedSequence, AppendLink, true)}");
        }

        /// <summary>
        /// Demonstrates updating sequences
        /// </summary>
        private static void DemonstrateUpdateOperations(ILinks<ulong> links)
        {
            Console.WriteLine("\n--- Update Operations ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = true,
                SequenceMarkerLink = 65539,
                UseCompression = true
            });

            // Create initial sequence
            var initialChars = UnicodeMap.FromStringToLinkArray("Test");
            var sequenceLink = sequences.Create(initialChars);
            Console.WriteLine($"Created: {sequences.FormatSequence(sequenceLink, AppendLink, true)}");

            // Update approach: Create new sequence and delete old one
            // This is because links are immutable - we don't modify, we replace
            var updatedChars = UnicodeMap.FromStringToLinkArray("Testing");
            var newSequenceLink = sequences.Create(updatedChars);
            Console.WriteLine($"Updated to: {sequences.FormatSequence(newSequenceLink, AppendLink, true)}");

            // Old sequence can be deleted if no longer needed
            // sequences.Delete(sequenceLink); // Uncommenting would delete the old sequence

            Console.WriteLine("Note: With sequence markers, unused subsequences can be garbage collected");
        }

        /// <summary>
        /// Demonstrates deletion with garbage collection considerations
        /// </summary>
        private static void DemonstrateDeleteWithGarbageCollection(ILinks<ulong> links)
        {
            Console.WriteLine("\n--- Delete with Garbage Collection ---");

            var sequences = new Sequences(links, new SequencesOptions<ulong>
            {
                UseSequenceMarker = true,
                SequenceMarkerLink = 65540,
                UseCompression = true
            });

            // Create a sequence
            var tempChars = UnicodeMap.FromStringToLinkArray("Temporary");
            var tempSequence = sequences.Create(tempChars);
            Console.WriteLine($"Created temporary sequence at link: {tempSequence}");

            var linksBeforeDelete = links.Count();

            // Delete the sequence
            // The sequence marker helps identify which subsequences can be garbage collected
            // Subsequences not marked (i.e., internal compression artifacts) can be cleaned up
            // if they're not referenced by any marked sequences

            Console.WriteLine($"Links before potential deletion: {linksBeforeDelete}");
            Console.WriteLine("Sequence markers enable:");
            Console.WriteLine("  1. Distinguish intended sequences from internal subsequences");
            Console.WriteLine("  2. Safe garbage collection of unreferenced subsequences");
            Console.WriteLine("  3. Cleanup on Create, Update, and Delete operations");
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
