using System;
using System.Linq;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating the SortedSequenceConverter functionality.
    /// Shows how to create unique signatures from sequences for anagram detection and error correction.
    /// </summary>
    public class SortedSequenceConverterCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links database file", args);

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile, UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();
                UnicodeMap.InitNew(syncLinks);

                var converter = new SortedSequenceConverter(syncLinks);

                Console.WriteLine("=== Sorted Sequence Converter Demo ===\n");
                DemoBasicSequences(converter);
                DemoWordAnagrams(converter, syncLinks);
                DemoErrorCorrection(converter);
                Console.WriteLine("\n=== Demo Complete ===");
            }
        }

        private void DemoBasicSequences(SortedSequenceConverter converter)
        {
            Console.WriteLine("1. Basic Sequence Signatures:");
            var seq1 = new ulong[] { 1, 2, 3, 4, 5 };
            var seq2 = new ulong[] { 5, 4, 3, 2, 1 };
            var seq3 = new ulong[] { 1, 2, 2, 3, 3, 4, 5 };

            var sig1 = converter.GetSortedUniqueSequence(seq1);
            var sig2 = converter.GetSortedUniqueSequence(seq2);
            var sig3 = converter.GetSortedUniqueSequence(seq3);

            Console.WriteLine($"   [1,2,3,4,5] signature: {sig1}");
            Console.WriteLine($"   [5,4,3,2,1] signature: {sig2}");
            Console.WriteLine($"   [1,2,2,3,3,4,5] signature: {sig3}");
            Console.WriteLine($"   All equal: {sig1 == sig2 && sig2 == sig3}\n");
        }

        private void DemoWordAnagrams(SortedSequenceConverter converter, SynchronizedLinks<ulong> links)
        {
            Console.WriteLine("2. Word Anagram Detection:");

            // Create sequences for "listen" and "silent"
            var listen = new[] {
                UnicodeMap.FromCharToLink('l'),
                UnicodeMap.FromCharToLink('i'),
                UnicodeMap.FromCharToLink('s'),
                UnicodeMap.FromCharToLink('t'),
                UnicodeMap.FromCharToLink('e'),
                UnicodeMap.FromCharToLink('n')
            };

            var silent = new[] {
                UnicodeMap.FromCharToLink('s'),
                UnicodeMap.FromCharToLink('i'),
                UnicodeMap.FromCharToLink('l'),
                UnicodeMap.FromCharToLink('e'),
                UnicodeMap.FromCharToLink('n'),
                UnicodeMap.FromCharToLink('t')
            };

            var listenSig = converter.GetSortedUniqueSequence(listen);
            var silentSig = converter.GetSortedUniqueSequence(silent);

            Console.WriteLine($"   'listen' signature: {listenSig}");
            Console.WriteLine($"   'silent' signature: {silentSig}");
            Console.WriteLine($"   Anagrams detected: {listenSig == silentSig}\n");
        }

        private void DemoErrorCorrection(SortedSequenceConverter converter)
        {
            Console.WriteLine("3. Error Correction Use Case:");

            var correctWord = new ulong[] { 10, 20, 30, 40 };
            var typoWord = new ulong[] { 10, 30, 20, 40 }; // Swapped letters
            var differentWord = new ulong[] { 10, 20, 30, 50 };

            var sig1 = converter.GetSortedUniqueSequence(correctWord);
            var sig2 = converter.GetSortedUniqueSequence(typoWord);
            var sig3 = converter.GetSortedUniqueSequence(differentWord);

            Console.WriteLine($"   Correct word signature: {sig1}");
            Console.WriteLine($"   Typo word signature: {sig2}");
            Console.WriteLine($"   Different word signature: {sig3}");
            Console.WriteLine($"   Typo matches correct (potential fix): {sig1 == sig2}");
            Console.WriteLine($"   Different word is distinct: {sig1 != sig3}");
        }
    }
}
