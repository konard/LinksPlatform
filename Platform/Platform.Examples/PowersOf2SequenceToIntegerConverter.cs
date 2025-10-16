using System;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets;
using Platform.Data.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Converts a sequence of powers of 2 back to an integer.
    /// Each element in the sequence represents a power of 2 that should be summed.
    /// For example, sequence [3, 2, 0] = 2^3 + 2^2 + 2^0 = 8 + 4 + 1 = 13
    /// </summary>
    /// <typeparam name="TLink">The type of link addresses.</typeparam>
    public class PowersOf2SequenceToIntegerConverter<TLink> : LinksOperatorBase<TLink>, IConverter<TLink, BigInteger>
    {
        private readonly IConverter<TLink> _linkToPowerConverter;
        private readonly ISequenceWalker<TLink> _sequenceWalker;
        private readonly TLink _powersOf2SequenceMarker;

        /// <summary>
        /// Initializes a new instance of the <see cref="PowersOf2SequenceToIntegerConverter{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="linkToPowerConverter">Converter from link to power index.</param>
        /// <param name="sequenceWalker">Walker to traverse the sequence.</param>
        /// <param name="powersOf2SequenceMarker">Marker link to identify powers-of-2 sequences.</param>
        public PowersOf2SequenceToIntegerConverter(
            ILinks<TLink> links,
            IConverter<TLink> linkToPowerConverter,
            ISequenceWalker<TLink> sequenceWalker,
            TLink powersOf2SequenceMarker)
            : base(links)
        {
            _linkToPowerConverter = linkToPowerConverter;
            _sequenceWalker = sequenceWalker;
            _powersOf2SequenceMarker = powersOf2SequenceMarker;
        }

        /// <summary>
        /// Converts a powers-of-2 sequence to an integer.
        /// </summary>
        /// <param name="sequenceLink">The link representing the powers-of-2 sequence.</param>
        /// <returns>The reconstructed integer.</returns>
        public BigInteger Convert(TLink sequenceLink)
        {
            TLink source = default(TLink);
            TLink target = default(TLink);

            // Read the link to get source and target
            _links.Each(link =>
            {
                if (EqualityComparer<TLink>.Default.Equals(link[_links.Constants.IndexPart], sequenceLink))
                {
                    source = link[_links.Constants.SourcePart];
                    target = link[_links.Constants.TargetPart];
                    return _links.Constants.Break;
                }
                return _links.Constants.Continue;
            }, _links.Constants.Any, sequenceLink);

            // Check if this is a marked powers-of-2 sequence
            TLink actualSequence;
            if (EqualityComparer<TLink>.Default.Equals(source, _powersOf2SequenceMarker))
            {
                // It's a marked sequence, get the actual sequence
                actualSequence = target;
            }
            else
            {
                // Assume the link itself is the sequence
                actualSequence = sequenceLink;
            }

            // Special case: if actualSequence equals the marker, it represents zero
            if (EqualityComparer<TLink>.Default.Equals(actualSequence, _powersOf2SequenceMarker))
            {
                return BigInteger.Zero;
            }

            BigInteger result = BigInteger.Zero;

            // Walk through the sequence and sum the powers of 2
            foreach (var step in _sequenceWalker.Walk(actualSequence))
            {
                if (step != null && step.Count > 0)
                {
                    var powerLink = step[0]; // Get the first element
                    var power = _linkToPowerConverter.Convert(powerLink);
                    var powerValue = (int)(object)power;
                    result += BigInteger.Pow(2, powerValue);
                }
            }

            return result;
        }

        private static class EqualityComparer<T>
        {
            public static bool Equals(T a, T b) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(a, b);
            public static System.Collections.Generic.IEqualityComparer<T> Default => System.Collections.Generic.EqualityComparer<T>.Default;
        }
    }
}
