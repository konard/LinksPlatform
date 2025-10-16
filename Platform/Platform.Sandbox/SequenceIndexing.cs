using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents metadata for a fragment found in a reference sequence (e.g., Pi digits).
    /// </summary>
    public struct FragmentMetadata
    {
        /// <summary>
        /// The position where the fragment was found in the reference sequence.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// The length of the fragment.
        /// </summary>
        public int Length { get; set; }

        public FragmentMetadata(long position, int length)
        {
            Position = position;
            Length = length;
        }

        public override string ToString()
        {
            return $"[Position={Position}, Length={Length}]";
        }
    }

    /// <summary>
    /// Provides functionality for indexing sequences using a reference sequence (such as Pi digits).
    /// This implements the concept from https://github.com/philipl/pifs/issues/33
    /// Instead of finding entire data in Pi, breaks data into fragments and tracks their positions.
    /// </summary>
    public class SequenceIndexer
    {
        private readonly string _referenceSequence;
        private readonly int _minFragmentLength;
        private readonly int _maxFragmentLength;

        /// <summary>
        /// Initializes a new instance of the SequenceIndexer class.
        /// </summary>
        /// <param name="referenceSequence">The reference sequence to search within (e.g., Pi digits).</param>
        /// <param name="minFragmentLength">Minimum length of fragments to search for.</param>
        /// <param name="maxFragmentLength">Maximum length of fragments to search for.</param>
        public SequenceIndexer(string referenceSequence, int minFragmentLength = 2, int maxFragmentLength = 20)
        {
            if (string.IsNullOrEmpty(referenceSequence))
            {
                throw new ArgumentException("Reference sequence cannot be null or empty", nameof(referenceSequence));
            }

            if (minFragmentLength < 1)
            {
                throw new ArgumentException("Minimum fragment length must be at least 1", nameof(minFragmentLength));
            }

            if (maxFragmentLength < minFragmentLength)
            {
                throw new ArgumentException("Maximum fragment length must be >= minimum fragment length", nameof(maxFragmentLength));
            }

            _referenceSequence = referenceSequence;
            _minFragmentLength = minFragmentLength;
            _maxFragmentLength = maxFragmentLength;
        }

        /// <summary>
        /// Encodes a source sequence by finding its fragments in the reference sequence.
        /// Uses a greedy approach: finds the longest possible fragment at each step.
        /// </summary>
        /// <param name="sourceSequence">The sequence to encode.</param>
        /// <returns>A list of fragment metadata representing positions in the reference sequence.</returns>
        public List<FragmentMetadata> Encode(string sourceSequence)
        {
            if (string.IsNullOrEmpty(sourceSequence))
            {
                return new List<FragmentMetadata>();
            }

            var metadata = new List<FragmentMetadata>();
            int currentIndex = 0;

            while (currentIndex < sourceSequence.Length)
            {
                // Try to find the longest possible fragment starting at currentIndex
                int maxLength = Math.Min(_maxFragmentLength, sourceSequence.Length - currentIndex);
                bool found = false;

                // Greedy approach: start with longest possible fragment and work down
                for (int length = maxLength; length >= _minFragmentLength; length--)
                {
                    string fragment = sourceSequence.Substring(currentIndex, length);
                    int position = _referenceSequence.IndexOf(fragment, StringComparison.Ordinal);

                    if (position >= 0)
                    {
                        metadata.Add(new FragmentMetadata(position, length));
                        currentIndex += length;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // If no fragment found, encode single character
                    string singleChar = sourceSequence.Substring(currentIndex, 1);
                    int position = _referenceSequence.IndexOf(singleChar, StringComparison.Ordinal);

                    if (position >= 0)
                    {
                        metadata.Add(new FragmentMetadata(position, 1));
                    }
                    else
                    {
                        // Character not found in reference sequence - use a special marker
                        // Negative position indicates the character itself needs to be stored
                        metadata.Add(new FragmentMetadata(-1, singleChar[0]));
                    }

                    currentIndex++;
                }
            }

            return metadata;
        }

        /// <summary>
        /// Decodes a sequence from its fragment metadata.
        /// </summary>
        /// <param name="metadata">The list of fragment metadata.</param>
        /// <returns>The decoded sequence.</returns>
        public string Decode(List<FragmentMetadata> metadata)
        {
            if (metadata == null || metadata.Count == 0)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            foreach (var fragment in metadata)
            {
                if (fragment.Position >= 0)
                {
                    // Normal case: extract from reference sequence
                    if (fragment.Position + fragment.Length <= _referenceSequence.Length)
                    {
                        result.Append(_referenceSequence.Substring((int)fragment.Position, fragment.Length));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Fragment metadata references position beyond reference sequence: Position={fragment.Position}, Length={fragment.Length}");
                    }
                }
                else
                {
                    // Special case: character not found in reference, stored directly
                    result.Append((char)fragment.Length);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Calculates the compression ratio achieved by the encoding.
        /// </summary>
        /// <param name="originalLength">Length of original sequence.</param>
        /// <param name="metadataCount">Number of fragments in metadata.</param>
        /// <returns>Compression ratio (smaller is better).</returns>
        public static double CalculateCompressionRatio(int originalLength, int metadataCount)
        {
            if (originalLength == 0)
            {
                return 0;
            }

            // Assuming metadata takes roughly 12 bytes per fragment (8 bytes for position + 4 bytes for length)
            int metadataBytes = metadataCount * 12;
            int originalBytes = originalLength; // Assuming 1 byte per character

            return (double)metadataBytes / originalBytes;
        }
    }

    /// <summary>
    /// Provides Pi digits as a reference sequence for testing.
    /// </summary>
    public static class PiSequence
    {
        /// <summary>
        /// Returns the first 1000 digits of Pi (excluding the decimal point).
        /// </summary>
        public static string GetFirst1000Digits()
        {
            // First 1000 digits of Pi after the decimal point
            return "1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679" +
                   "8214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196" +
                   "4428810975665933446128475648233786783165271201909145648566923460348610454326648213393607260249141273" +
                   "7245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094" +
                   "3305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912" +
                   "9833673362440656643086021394946395224737190702179860943702770539217176293176752384674818467669405132" +
                   "0005681271452635608277857713427577896091736371787214684409012249534301465495853710507922796892589235" +
                   "4201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859" +
                   "5024459455346908302642522308253344685035261931188171010003137838752886587533208381420617177669147303" +
                   "5982534904287554687311595628638823537875937519577818577805321712268066130019278766111959092164201989";
        }

        /// <summary>
        /// Returns the first 10000 digits of Pi (excluding the decimal point).
        /// Note: This is a simplified version. For production use, consider using a Pi generation library.
        /// </summary>
        public static string GetFirst10000Digits()
        {
            // For demonstration, we return first 1000. In a real implementation,
            // you would use a proper Pi calculation algorithm or load from a file.
            return GetFirst1000Digits();
        }

        /// <summary>
        /// Generates Pi digits using a simple algorithm (Machin's formula or similar).
        /// This is a placeholder - for production use, implement a proper Pi generation algorithm
        /// or load pre-computed digits from a file.
        /// </summary>
        public static string GenerateDigits(int count)
        {
            // For now, repeat the first 1000 digits
            string base1000 = GetFirst1000Digits();
            var sb = new StringBuilder();

            while (sb.Length < count)
            {
                sb.Append(base1000);
            }

            return sb.ToString().Substring(0, Math.Min(count, sb.Length));
        }
    }

    /// <summary>
    /// Test and demonstration class for sequence indexing.
    /// </summary>
    public static class SequenceIndexingTest
    {
        public static void Run()
        {
            Console.WriteLine("=== Sequence Indexing Experiment ===");
            Console.WriteLine();

            // Get Pi digits as reference sequence
            string piDigits = PiSequence.GetFirst1000Digits();
            Console.WriteLine($"Reference sequence (Pi): {piDigits.Length} digits");
            Console.WriteLine($"First 100 digits: {piDigits.Substring(0, 100)}...");
            Console.WriteLine();

            // Create indexer
            var indexer = new SequenceIndexer(piDigits, minFragmentLength: 2, maxFragmentLength: 10);

            // Test 1: Simple numeric sequence
            Console.WriteLine("--- Test 1: Numeric Sequence ---");
            string test1 = "14159265";
            Console.WriteLine($"Original: {test1}");

            var metadata1 = indexer.Encode(test1);
            Console.WriteLine($"Encoded: {metadata1.Count} fragments");
            foreach (var fragment in metadata1)
            {
                Console.WriteLine($"  {fragment}");
            }

            string decoded1 = indexer.Decode(metadata1);
            Console.WriteLine($"Decoded: {decoded1}");
            Console.WriteLine($"Match: {test1 == decoded1}");
            Console.WriteLine($"Compression ratio: {SequenceIndexer.CalculateCompressionRatio(test1.Length, metadata1.Count):F2}");
            Console.WriteLine();

            // Test 2: Repeated pattern
            Console.WriteLine("--- Test 2: Repeated Pattern ---");
            string test2 = "141592141592";
            Console.WriteLine($"Original: {test2}");

            var metadata2 = indexer.Encode(test2);
            Console.WriteLine($"Encoded: {metadata2.Count} fragments");
            foreach (var fragment in metadata2)
            {
                Console.WriteLine($"  {fragment}");
            }

            string decoded2 = indexer.Decode(metadata2);
            Console.WriteLine($"Decoded: {decoded2}");
            Console.WriteLine($"Match: {test2 == decoded2}");
            Console.WriteLine($"Compression ratio: {SequenceIndexer.CalculateCompressionRatio(test2.Length, metadata2.Count):F2}");
            Console.WriteLine();

            // Test 3: Mixed sequence
            Console.WriteLine("--- Test 3: Mixed Sequence ---");
            string test3 = "31415926535897";
            Console.WriteLine($"Original: {test3}");

            var metadata3 = indexer.Encode(test3);
            Console.WriteLine($"Encoded: {metadata3.Count} fragments");
            foreach (var fragment in metadata3)
            {
                Console.WriteLine($"  {fragment}");
            }

            string decoded3 = indexer.Decode(metadata3);
            Console.WriteLine($"Decoded: {decoded3}");
            Console.WriteLine($"Match: {test3 == decoded3}");
            Console.WriteLine($"Compression ratio: {SequenceIndexer.CalculateCompressionRatio(test3.Length, metadata3.Count):F2}");
            Console.WriteLine();

            // Test 4: Performance test with longer sequence
            Console.WriteLine("--- Test 4: Performance Test ---");
            string test4 = piDigits.Substring(0, 100);
            Console.WriteLine($"Original length: {test4.Length}");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var metadata4 = indexer.Encode(test4);
            sw.Stop();

            Console.WriteLine($"Encoding time: {sw.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Encoded: {metadata4.Count} fragments");
            Console.WriteLine($"Compression ratio: {SequenceIndexer.CalculateCompressionRatio(test4.Length, metadata4.Count):F2}");

            sw.Restart();
            string decoded4 = indexer.Decode(metadata4);
            sw.Stop();

            Console.WriteLine($"Decoding time: {sw.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Match: {test4 == decoded4}");
            Console.WriteLine();

            Console.WriteLine("=== All Tests Complete ===");
        }
    }
}
