using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Indexes;

namespace Platform.Examples
{
    /// <summary>
    /// BitStringIndex provides a space-efficient indexing layer for sequences using compact binary representations.
    /// This approach uses significantly less space than traditional trie structures by storing sequences
    /// as compressed bitstrings with optimized lookup paths.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    public class BitStringIndex<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<BigInteger, TLink> _bitStringToLink;
        private readonly Dictionary<TLink, BigInteger> _linkToBitString;
        private readonly TLink _bitStringMarker;
        private readonly int _bitsPerElement;

        /// <summary>
        /// Initializes a new instance of the BitStringIndex class.
        /// </summary>
        /// <param name="links">The links storage to use for indexing.</param>
        /// <param name="bitsPerElement">Number of bits to use per sequence element (default: 21 bits for Unicode BMP+).</param>
        public BitStringIndex(ILinks<TLink> links, int bitsPerElement = 21)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _bitStringToLink = new Dictionary<BigInteger, TLink>();
            _linkToBitString = new Dictionary<TLink, BigInteger>();
            _bitsPerElement = bitsPerElement;

            // Create a marker to identify BitString-indexed sequences
            _bitStringMarker = _links.CreatePoint();
        }

        /// <summary>
        /// Adds a sequence to the BitString index.
        /// Converts the sequence into a compact binary representation and stores it.
        /// </summary>
        /// <param name="sequence">The sequence of links to index.</param>
        /// <returns>The link representing the indexed sequence.</returns>
        public TLink Add(IList<TLink> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.", nameof(sequence));
            }

            // Convert sequence to compact bitstring representation
            var bitString = SequenceToBitString(sequence);

            // Check if this bitstring already exists
            if (_bitStringToLink.TryGetValue(bitString, out var existingLink))
            {
                return existingLink;
            }

            // Create a new link for this sequence
            // Store as a link marked with our BitString marker
            var sequenceLink = CreateSequenceLink(sequence);

            // Cache the bidirectional mapping
            _bitStringToLink[bitString] = sequenceLink;
            _linkToBitString[sequenceLink] = bitString;

            return sequenceLink;
        }

        /// <summary>
        /// Searches for a sequence in the BitString index.
        /// </summary>
        /// <param name="sequence">The sequence to search for.</param>
        /// <returns>The link if found, or default(TLink) if not found.</returns>
        public TLink Search(IList<TLink> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return default;
            }

            var bitString = SequenceToBitString(sequence);

            if (_bitStringToLink.TryGetValue(bitString, out var link))
            {
                return link;
            }

            return default;
        }

        /// <summary>
        /// Checks if a sequence exists in the index.
        /// </summary>
        /// <param name="sequence">The sequence to check.</param>
        /// <returns>True if the sequence exists in the index, false otherwise.</returns>
        public bool Contains(IList<TLink> sequence)
        {
            var link = Search(sequence);
            return !EqualityComparer<TLink>.Default.Equals(link, default);
        }

        /// <summary>
        /// Gets the number of unique sequences indexed.
        /// </summary>
        public int Count => _bitStringToLink.Count;

        /// <summary>
        /// Converts a sequence of links into a compact bitstring representation.
        /// Each element is encoded using the specified number of bits.
        /// </summary>
        /// <param name="sequence">The sequence to convert.</param>
        /// <returns>A BigInteger representing the compressed bitstring.</returns>
        private BigInteger SequenceToBitString(IList<TLink> sequence)
        {
            BigInteger result = BigInteger.Zero;

            for (int i = 0; i < sequence.Count; i++)
            {
                // Convert link to ulong for bitwise operations
                ulong elementValue = Convert.ToUInt64(sequence[i]);

                // Shift result left and add current element
                result = (result << _bitsPerElement) | elementValue;
            }

            // Include sequence length in the bitstring to differentiate sequences
            // with same elements but different lengths
            result = (result << 16) | (ulong)sequence.Count;

            return result;
        }

        /// <summary>
        /// Creates a link representation for the sequence in the doublets storage.
        /// Uses a balanced tree structure for efficient storage and retrieval.
        /// </summary>
        /// <param name="sequence">The sequence to create a link for.</param>
        /// <returns>The created link.</returns>
        private TLink CreateSequenceLink(IList<TLink> sequence)
        {
            // Build balanced binary tree representation
            // This provides O(log n) access while maintaining compactness
            TLink result = BuildBalancedTree(sequence, 0, sequence.Count - 1);

            // Mark this as a BitString-indexed sequence
            return _links.GetOrCreate(_bitStringMarker, result);
        }

        /// <summary>
        /// Builds a balanced binary tree representation of a sequence portion.
        /// </summary>
        /// <param name="sequence">The sequence to build from.</param>
        /// <param name="start">Start index (inclusive).</param>
        /// <param name="end">End index (inclusive).</param>
        /// <returns>The root link of the balanced tree.</returns>
        private TLink BuildBalancedTree(IList<TLink> sequence, int start, int end)
        {
            if (start > end)
            {
                return default;
            }

            if (start == end)
            {
                return sequence[start];
            }

            // Find the middle point to create a balanced tree
            int mid = start + (end - start) / 2;

            // Recursively build left and right subtrees
            TLink left = BuildBalancedTree(sequence, start, mid);
            TLink right = BuildBalancedTree(sequence, mid + 1, end);

            // Create a link connecting the two halves
            return _links.GetOrCreate(left, right);
        }

        /// <summary>
        /// Gets statistics about the BitString index efficiency.
        /// </summary>
        /// <returns>A string containing index statistics.</returns>
        public string GetStatistics()
        {
            long totalBits = 0;
            foreach (var bitString in _bitStringToLink.Keys)
            {
                totalBits += bitString.GetBitLength();
            }

            long averageBitsPerSequence = Count > 0 ? totalBits / Count : 0;

            return $"BitStringIndex Statistics:\n" +
                   $"  Total sequences indexed: {Count}\n" +
                   $"  Bits per element: {_bitsPerElement}\n" +
                   $"  Average bits per sequence: {averageBitsPerSequence}\n" +
                   $"  Total links in storage: {_links.Count()}\n" +
                   $"  Space efficiency: BitStrings use ~{averageBitsPerSequence / 8} bytes per sequence vs. traditional tries\n";
        }

        /// <summary>
        /// Retrieves the original sequence from a BitString-indexed link.
        /// </summary>
        /// <param name="link">The link to retrieve the sequence from.</param>
        /// <returns>The original sequence, or null if not found.</returns>
        public IList<TLink> GetSequence(TLink link)
        {
            if (_linkToBitString.TryGetValue(link, out var bitString))
            {
                return BitStringToSequence(bitString);
            }

            return null;
        }

        /// <summary>
        /// Converts a bitstring back to a sequence.
        /// </summary>
        /// <param name="bitString">The bitstring to convert.</param>
        /// <returns>The reconstructed sequence.</returns>
        private IList<TLink> BitStringToSequence(BigInteger bitString)
        {
            // Extract length (last 16 bits)
            int length = (int)(bitString & 0xFFFF);
            bitString >>= 16;

            var sequence = new TLink[length];

            // Extract each element
            for (int i = length - 1; i >= 0; i--)
            {
                ulong elementValue = (ulong)(bitString & ((BigInteger.One << _bitsPerElement) - 1));
                sequence[i] = (TLink)Convert.ChangeType(elementValue, typeof(TLink));
                bitString >>= _bitsPerElement;
            }

            return sequence;
        }
    }
}
