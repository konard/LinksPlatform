using System;
using System.Linq;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Converts sequences into sorted unique sets of values that can be used as signatures.
    /// For example, words "listen" and "silent" will have the same sorted sequence signature.
    /// This can be used for error correction and anagram detection.
    /// </summary>
    public class SortedSequenceConverter
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;

        public SortedSequenceConverter(ILinks<ulong> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _sequences = new Sequences(links);
        }

        /// <summary>
        /// Converts a sequence into a sorted unique set.
        /// </summary>
        /// <param name="sequenceLink">The link representing the sequence to convert.</param>
        /// <returns>A link representing the sorted unique sequence.</returns>
        public ulong GetSortedUniqueSequence(ulong sequenceLink)
        {
            var elements = CollectSequenceElements(sequenceLink);
            var uniqueSorted = elements.Distinct().OrderBy(x => x).ToArray();
            return _sequences.Create(uniqueSorted);
        }

        /// <summary>
        /// Converts a sequence array into a sorted unique set.
        /// </summary>
        /// <param name="sequence">The array of elements to convert.</param>
        /// <returns>A link representing the sorted unique sequence.</returns>
        public ulong GetSortedUniqueSequence(ulong[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.", nameof(sequence));
            }
            var uniqueSorted = sequence.Distinct().OrderBy(x => x).ToArray();
            return _sequences.Create(uniqueSorted);
        }

        /// <summary>
        /// Creates a mapping between a source sequence and its sorted unique signature.
        /// </summary>
        /// <param name="sourceSequenceLink">The source sequence link.</param>
        /// <returns>A link connecting the source sequence to its sorted unique signature.</returns>
        public ulong CreateSequenceToSignatureMapping(ulong sourceSequenceLink)
        {
            var signatureLink = GetSortedUniqueSequence(sourceSequenceLink);
            return _links.GetOrCreate(sourceSequenceLink, signatureLink);
        }

        /// <summary>
        /// Creates a mapping between a source sequence array and its sorted unique signature.
        /// </summary>
        /// <param name="sourceSequence">The source sequence array.</param>
        /// <returns>A link connecting the source sequence to its sorted unique signature.</returns>
        public ulong CreateSequenceToSignatureMapping(ulong[] sourceSequence)
        {
            if (sourceSequence == null || sourceSequence.Length == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.", nameof(sourceSequence));
            }
            var sourceSequenceLink = _sequences.Create(sourceSequence);
            var signatureLink = GetSortedUniqueSequence(sourceSequence);
            return _links.GetOrCreate(sourceSequenceLink, signatureLink);
        }

        /// <summary>
        /// Finds all sequences that have the same sorted unique signature.
        /// This can be used for anagram detection and error correction.
        /// </summary>
        /// <param name="signatureLink">The signature link to search for.</param>
        /// <returns>An enumerable of sequence links that match the signature.</returns>
        public IEnumerable<ulong> FindSequencesBySignature(ulong signatureLink)
        {
            var results = new List<ulong>();
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                var target = link[_links.Constants.TargetPart];
                if (target == signatureLink)
                {
                    results.Add(link[_links.Constants.SourcePart]);
                }
                return _links.Constants.Continue;
            }, _links.Constants.Any, signatureLink);
            return results;
        }

        private List<ulong> CollectSequenceElements(ulong sequenceLink)
        {
            var elements = new List<ulong>();
            CollectElementsRecursively(sequenceLink, elements);
            return elements;
        }

        private void CollectElementsRecursively(ulong link, List<ulong> elements)
        {
            var linkData = _links.GetLink(link);
            if (linkData == null)
            {
                elements.Add(link);
                return;
            }

            var source = linkData[_links.Constants.SourcePart];
            var target = linkData[_links.Constants.TargetPart];

            // If both source and target point to other links, recursively collect
            if (source != link && _links.Exists(source))
            {
                CollectElementsRecursively(source, elements);
            }
            else if (source != 0)
            {
                elements.Add(source);
            }

            if (target != link && _links.Exists(target))
            {
                CollectElementsRecursively(target, elements);
            }
            else if (target != 0)
            {
                elements.Add(target);
            }
        }
    }
}
