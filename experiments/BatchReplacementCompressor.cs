using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Platform.Collections;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;

namespace Platform.Experiments
{
    /// <summary>
    /// Optimized compressor that replaces multiple high-frequency pairs in a single pass.
    /// Addresses issue #95 requirement: "Update multiple pairs at a time of replacement"
    ///
    /// This implementation identifies the top-N most frequent pairs and replaces them
    /// all in a single pass through the sequence, significantly reducing the number
    /// of iterations needed for compression.
    /// </summary>
    public struct BatchReplacementCompressor
    {
        private readonly SynchronizedLinks<ulong> _links;
        private readonly int _batchSize;
        private readonly Dictionary<Link<ulong>, ulong> _doubletsFrequencies;

        /// <summary>
        /// Creates a new batch replacement compressor.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="batchSize">Number of pairs to replace in each iteration (default: 10).</param>
        public BatchReplacementCompressor(SynchronizedLinks<ulong> links, int batchSize = 10)
        {
            _links = links;
            _batchSize = batchSize;
            _doubletsFrequencies = new Dictionary<Link<ulong>, ulong>();
        }

        /// <summary>
        /// Compresses a sequence by replacing multiple high-frequency pairs simultaneously.
        /// </summary>
        /// <param name="sequence">The sequence to compress.</param>
        /// <returns>The compressed sequence.</returns>
        public ulong[] Compress(ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
            {
                return null;
            }

            if (sequence.Length == 1)
            {
                return sequence;
            }

            var copy = new ulong[sequence.Length];
            Array.Copy(sequence, copy, sequence.Length);
            int currentLength = sequence.Length;

            bool hasReplacements = true;

            while (hasReplacements)
            {
                // Count all pair frequencies in current sequence
                _doubletsFrequencies.Clear();
                for (var i = 1; i < currentLength; i++)
                {
                    var doublet = new Link<ulong>(copy[i - 1], copy[i]);
                    if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                    {
                        _doubletsFrequencies[doublet] = frequency + 1;
                    }
                    else
                    {
                        _doubletsFrequencies.Add(doublet, 1);
                    }
                }

                // Select top N most frequent pairs (with frequency > 1)
                var topPairs = _doubletsFrequencies
                    .Where(kvp => kvp.Value > 1)
                    .OrderByDescending(kvp => kvp.Value)
                    .ThenByDescending(kvp => kvp.Key.Source + kvp.Key.Target)
                    .Take(_batchSize)
                    .ToList();

                if (topPairs.Count == 0)
                {
                    hasReplacements = false;
                    break;
                }

                // Create link replacements for all top pairs
                var replacements = new Dictionary<Link<ulong>, ulong>();
                foreach (var pair in topPairs)
                {
                    var replacementLink = _links.CreateAndUpdate(pair.Key.Source, pair.Key.Target);
                    replacements[pair.Key] = replacementLink;
                }

                // Perform batch replacement in a single pass
                var tempBuffer = new ulong[currentLength];
                int writePos = 0;
                int readPos = 0;

                while (readPos < currentLength)
                {
                    if (readPos < currentLength - 1)
                    {
                        var doublet = new Link<ulong>(copy[readPos], copy[readPos + 1]);

                        // Check if this pair should be replaced
                        if (replacements.TryGetValue(doublet, out ulong replacement))
                        {
                            tempBuffer[writePos++] = replacement;
                            readPos += 2; // Skip both elements of the pair
                            continue;
                        }
                    }

                    // No replacement, copy element as-is
                    tempBuffer[writePos++] = copy[readPos++];
                }

                // Update copy with compressed sequence
                currentLength = writePos;
                Array.Copy(tempBuffer, copy, currentLength);
            }

            // Create final result array
            var final = new ulong[currentLength];
            Array.Copy(copy, final, currentLength);

            return final;
        }

        /// <summary>
        /// Compresses a sequence using a greedy approach that prioritizes pairs by a scoring function.
        /// This variant considers both frequency and the total savings (frequency * 2 - 1).
        /// </summary>
        public ulong[] CompressWithScoring(ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
            {
                return null;
            }

            if (sequence.Length == 1)
            {
                return sequence;
            }

            var copy = new ulong[sequence.Length];
            Array.Copy(sequence, copy, sequence.Length);
            int currentLength = sequence.Length;

            bool hasReplacements = true;

            while (hasReplacements)
            {
                // Count all pair frequencies in current sequence
                _doubletsFrequencies.Clear();
                for (var i = 1; i < currentLength; i++)
                {
                    var doublet = new Link<ulong>(copy[i - 1], copy[i]);
                    if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                    {
                        _doubletsFrequencies[doublet] = frequency + 1;
                    }
                    else
                    {
                        _doubletsFrequencies.Add(doublet, 1);
                    }
                }

                // Select top N pairs by scoring function (frequency * savings)
                // Savings = frequency * 2 - 1 (we replace 2 elements with 1, but add 1 link definition)
                var topPairs = _doubletsFrequencies
                    .Where(kvp => kvp.Value > 1)
                    .Select(kvp => new
                    {
                        Pair = kvp.Key,
                        Frequency = kvp.Value,
                        Score = kvp.Value * 2 - 1 // Total elements saved
                    })
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Frequency)
                    .Take(_batchSize)
                    .ToList();

                if (topPairs.Count == 0)
                {
                    hasReplacements = false;
                    break;
                }

                // Create link replacements for all top pairs
                var replacements = new Dictionary<Link<ulong>, ulong>();
                foreach (var pair in topPairs)
                {
                    var replacementLink = _links.CreateAndUpdate(pair.Pair.Source, pair.Pair.Target);
                    replacements[pair.Pair] = replacementLink;
                }

                // Perform batch replacement in a single pass
                var tempBuffer = new ulong[currentLength];
                int writePos = 0;
                int readPos = 0;

                while (readPos < currentLength)
                {
                    if (readPos < currentLength - 1)
                    {
                        var doublet = new Link<ulong>(copy[readPos], copy[readPos + 1]);

                        // Check if this pair should be replaced
                        if (replacements.TryGetValue(doublet, out ulong replacement))
                        {
                            tempBuffer[writePos++] = replacement;
                            readPos += 2; // Skip both elements of the pair
                            continue;
                        }
                    }

                    // No replacement, copy element as-is
                    tempBuffer[writePos++] = copy[readPos++];
                }

                // Update copy with compressed sequence
                currentLength = writePos;
                Array.Copy(tempBuffer, copy, currentLength);
            }

            // Create final result array
            var final = new ulong[currentLength];
            Array.Copy(copy, final, currentLength);

            return final;
        }
    }
}
