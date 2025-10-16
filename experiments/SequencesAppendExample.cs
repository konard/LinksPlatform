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
    /// Demonstrates Sequence Append/Continue operations with optimization.
    ///
    /// This example shows:
    /// - Appending elements to existing sequences
    /// - Optimization by finding longest repeated subsequences
    /// - Incremental sequence building
    /// - Compression during append operations
    /// </summary>
    public class SequencesAppendExample
    {
        private const string DbFilename = "sequences-append-test.links";

        public static void Run()
        {
            Console.WriteLine("=== Sequences Append/Continue Example ===\n");

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

                var sequences = new Sequences(syncLinks, new SequencesOptions<ulong>
                {
                    UseSequenceMarker = true,
                    SequenceMarkerLink = 65537,
                    UseCompression = true
                });

                DemonstrateBasicAppend(sequences, syncLinks);
                DemonstrateAppendWithRepeatedPatterns(sequences, syncLinks);
                DemonstrateIncrementalBuilding(sequences, syncLinks);
                DemonstrateAppendOptimization(sequences, syncLinks);

                Console.WriteLine($"\nTotal links in database: {links.Count()}");
            }

            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Demonstrates basic append operation - creating a new sequence by appending elements
        /// </summary>
        private static void DemonstrateBasicAppend(Sequences sequences, ILinks<ulong> links)
        {
            Console.WriteLine("--- Basic Append Operation ---\n");

            // Create initial sequence "Hello"
            var helloChars = UnicodeMap.FromStringToLinkArray("Hello");
            var helloSequence = sequences.Create(helloChars);

            Console.WriteLine($"Initial sequence: {sequences.FormatSequence(helloSequence, AppendLink, true)}");
            Console.WriteLine($"  Link: {helloSequence}");

            // Simulate append by creating new sequence with additional elements
            // In practice, this is "appending" by creating: existing + new
            var worldChars = UnicodeMap.FromStringToLinkArray(" World");
            var combinedChars = helloChars.Concat(worldChars).ToArray();
            var appendedSequence = sequences.Create(combinedChars);

            Console.WriteLine($"After append: {sequences.FormatSequence(appendedSequence, AppendLink, true)}");
            Console.WriteLine($"  Link: {appendedSequence}");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates append with repeated patterns that can be optimized
        /// </summary>
        private static void DemonstrateAppendWithRepeatedPatterns(Sequences sequences, ILinks<ulong> links)
        {
            Console.WriteLine("--- Append with Repeated Patterns ---\n");

            // Create base sequence with pattern
            var basePattern = UnicodeMap.FromStringToLinkArray("abc");
            var baseSequence = sequences.Create(basePattern);

            Console.WriteLine($"Base pattern: {sequences.FormatSequence(baseSequence, AppendLink, true)}");

            // Append same pattern multiple times
            // With compression, repeated patterns should be optimized
            var repeatedPattern = UnicodeMap.FromStringToLinkArray("abcabc");
            var linksBefore = links.Count();
            var repeatedSequence = sequences.Create(repeatedPattern);
            var linksAfter = links.Count();

            Console.WriteLine($"After appending pattern: {sequences.FormatSequence(repeatedSequence, AppendLink, true)}");
            Console.WriteLine($"Links added: {linksAfter - linksBefore}");
            Console.WriteLine("Note: With compression, repeated subsequences are reused\n");
        }

        /// <summary>
        /// Demonstrates incremental sequence building - appending one element at a time
        /// </summary>
        private static void DemonstrateIncrementalBuilding(Sequences sequences, ILinks<ulong> links)
        {
            Console.WriteLine("--- Incremental Sequence Building ---\n");

            var targetWord = "Test";
            Console.WriteLine($"Building sequence incrementally for: '{targetWord}'\n");

            ulong currentSequence = 0;
            var currentChars = new System.Collections.Generic.List<ulong>();

            for (int i = 0; i < targetWord.Length; i++)
            {
                // Add one character at a time
                var charLink = UnicodeMap.FromCharToLink(targetWord[i]);
                currentChars.Add(charLink);

                currentSequence = sequences.Create(currentChars.ToArray());

                Console.WriteLine($"Step {i + 1}: {sequences.FormatSequence(currentSequence, AppendLink, true)}");
            }

            Console.WriteLine($"\nFinal sequence link: {currentSequence}\n");
        }

        /// <summary>
        /// Demonstrates optimization during append - finding longest repeated subsequences
        /// </summary>
        private static void DemonstrateAppendOptimization(Sequences sequences, ILinks<ulong> links)
        {
            Console.WriteLine("--- Append with Optimization ---\n");

            Console.WriteLine("Optimization strategy:");
            Console.WriteLine("1. When appending, search for longest repeated sequence from append point");
            Console.WriteLine("2. Only the longest repeated sequence is optimized (grouped)");
            Console.WriteLine("3. Subsequence may be moved to separate link for reuse");
            Console.WriteLine("4. This is a step toward full compression, not complete compression\n");

            // Create sequence with patterns that will benefit from optimization
            var baseText = "The quick brown fox";
            var baseChars = UnicodeMap.FromStringToLinkArray(baseText);
            var baseSequence = sequences.Create(baseChars);

            Console.WriteLine($"Base: {sequences.FormatSequence(baseSequence, AppendLink, true)}");

            // Append text with some repetition from base
            var appendText = " jumps over the lazy dog";
            var allChars = baseChars.Concat(UnicodeMap.FromStringToLinkArray(appendText)).ToArray();

            var linksBefore = links.Count();
            var optimizedSequence = sequences.Create(allChars);
            var linksAfter = links.Count();

            Console.WriteLine($"After append: {sequences.FormatSequence(optimizedSequence, AppendLink, true)}");
            Console.WriteLine($"Links added: {linksAfter - linksBefore}");
            Console.WriteLine("\nNote: 'the' appears twice and may be optimized into a shared subsequence");

            // Explain the optimization process
            Console.WriteLine("\nOptimization process during append:");
            Console.WriteLine("  1. Analyze sequence from append point");
            Console.WriteLine("  2. Find longest repeated subsequence");
            Console.WriteLine("  3. Extract and store as separate subsequence");
            Console.WriteLine("  4. Replace occurrences with reference to subsequence");
            Console.WriteLine("  5. Results in better compression and storage efficiency\n");
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
