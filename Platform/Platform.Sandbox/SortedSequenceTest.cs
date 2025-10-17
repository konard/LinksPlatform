using System;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Unicode;
using Platform.Examples;

namespace Platform.Sandbox
{
    /// <summary>
    /// Demonstrates the SortedSequenceConverter functionality for creating unique signatures
    /// from sequences, useful for anagram detection and error correction.
    /// </summary>
    public static class SortedSequenceTest
    {
        public static void TestWordSignatures()
        {
            Console.WriteLine("=== Sorted Sequence Signatures Test ===\n");

            using (var memoryManager = new UInt64UnitedMemoryLinks("sorted_sequence_test.db", 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryManager))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                var converter = new SortedSequenceConverter(syncLinks);

                // Test with simple numeric sequences
                Console.WriteLine("Test 1: Simple numeric sequences");
                var seq1 = new ulong[] { 1, 2, 3, 4, 5 };
                var seq2 = new ulong[] { 5, 4, 3, 2, 1 }; // Same elements, different order
                var seq3 = new ulong[] { 1, 2, 2, 3, 3, 4, 5 }; // With duplicates

                var sig1 = converter.GetSortedUniqueSequence(seq1);
                var sig2 = converter.GetSortedUniqueSequence(seq2);
                var sig3 = converter.GetSortedUniqueSequence(seq3);

                Console.WriteLine($"Sequence [1,2,3,4,5] signature: {sig1}");
                Console.WriteLine($"Sequence [5,4,3,2,1] signature: {sig2}");
                Console.WriteLine($"Sequence [1,2,2,3,3,4,5] signature: {sig3}");
                Console.WriteLine($"All three signatures are equal: {sig1 == sig2 && sig2 == sig3}\n");

                // Test with mapping creation
                Console.WriteLine("Test 2: Creating sequence-to-signature mappings");
                var word1 = new ulong[] { 10, 20, 30, 40 };
                var word2 = new ulong[] { 40, 30, 20, 10 };

                var mapping1 = converter.CreateSequenceToSignatureMapping(word1);
                var mapping2 = converter.CreateSequenceToSignatureMapping(word2);

                Console.WriteLine($"Mapping 1 created: {mapping1}");
                Console.WriteLine($"Mapping 2 created: {mapping2}");

                // Verify they point to the same signature
                var sig1Link = syncLinks.GetLink(mapping1);
                var sig2Link = syncLinks.GetLink(mapping2);
                Console.WriteLine($"Both mappings point to same signature: {sig1Link[syncLinks.Constants.TargetPart] == sig2Link[syncLinks.Constants.TargetPart]}\n");

                // Test finding sequences by signature
                Console.WriteLine("Test 3: Finding sequences by signature");
                var sharedSignature = sig1Link[syncLinks.Constants.TargetPart];
                var matchingSequences = converter.FindSequencesBySignature(sharedSignature).ToArray();
                Console.WriteLine($"Found {matchingSequences.Length} sequences with the same signature");

                // Test with character sequences (simulating words)
                Console.WriteLine("\nTest 4: Character-based sequences (word anagrams)");
                var charL = UnicodeMap.FromCharToLink('l');
                var charI = UnicodeMap.FromCharToLink('i');
                var charS = UnicodeMap.FromCharToLink('s');
                var charT = UnicodeMap.FromCharToLink('t');
                var charE = UnicodeMap.FromCharToLink('e');
                var charN = UnicodeMap.FromCharToLink('n');

                var listen = new[] { charL, charI, charS, charT, charE, charN };
                var silent = new[] { charS, charI, charL, charE, charN, charT };

                var listenSig = converter.GetSortedUniqueSequence(listen);
                var silentSig = converter.GetSortedUniqueSequence(silent);

                Console.WriteLine($"'listen' signature: {listenSig}");
                Console.WriteLine($"'silent' signature: {silentSig}");
                Console.WriteLine($"Signatures match (anagrams detected): {listenSig == silentSig}");

                Console.WriteLine("\n=== Test Complete ===");
            }
        }

        public static void TestErrorCorrection()
        {
            Console.WriteLine("\n=== Error Correction Example ===\n");

            using (var memoryManager = new UInt64UnitedMemoryLinks("error_correction_test.db", 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryManager))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                var converter = new SortedSequenceConverter(syncLinks);

                // Simulate a dictionary of words with their signatures
                var word1 = new ulong[] { 1, 2, 3, 4 }; // "word"
                var word2 = new ulong[] { 1, 3, 2, 4 }; // "wodr" (typo)
                var word3 = new ulong[] { 1, 2, 3, 5 }; // "wore" (different word)

                converter.CreateSequenceToSignatureMapping(word1);
                converter.CreateSequenceToSignatureMapping(word2);
                converter.CreateSequenceToSignatureMapping(word3);

                var sig1 = converter.GetSortedUniqueSequence(word1);
                var sig2 = converter.GetSortedUniqueSequence(word2);
                var sig3 = converter.GetSortedUniqueSequence(word3);

                Console.WriteLine("Word signatures created for error correction:");
                Console.WriteLine($"Word [1,2,3,4] signature: {sig1}");
                Console.WriteLine($"Word [1,3,2,4] signature: {sig2}");
                Console.WriteLine($"Word [1,2,3,5] signature: {sig3}");

                Console.WriteLine($"\nFirst two words share signature (potential typo): {sig1 == sig2}");
                Console.WriteLine($"Third word has different signature: {sig1 != sig3}");

                // Find all words with the same signature as the potential typo
                var relatedWords = converter.FindSequencesBySignature(sig2).ToArray();
                Console.WriteLine($"\nWords with same signature as typo: {relatedWords.Length}");
                Console.WriteLine("These could be suggested as corrections.");

                Console.WriteLine("\n=== Error Correction Example Complete ===");
            }
        }
    }
}
