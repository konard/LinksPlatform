using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets;
using Platform.Data.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Converts a sequence of decimal digits back to an integer.
    /// Each element in the sequence represents a single decimal digit (0-9).
    /// For example, sequence [1, 2, 3, 4, 5] = 12345
    /// </summary>
    /// <typeparam name="TLink">The type of link addresses.</typeparam>
    public class DecimalDigitsSequenceToIntegerConverter<TLink> : LinksOperatorBase<TLink>, IConverter<TLink, BigInteger>
    {
        private readonly IConverter<TLink> _linkToDigitConverter;
        private readonly ISequenceWalker<TLink> _sequenceWalker;
        private readonly TLink _decimalDigitsSequenceMarker;
        private readonly TLink _negativeNumberMarker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalDigitsSequenceToIntegerConverter{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="linkToDigitConverter">Converter from link to digit (0-9).</param>
        /// <param name="sequenceWalker">Walker to traverse the sequence.</param>
        /// <param name="decimalDigitsSequenceMarker">Marker link to identify decimal digit sequences.</param>
        /// <param name="negativeNumberMarker">Marker link to identify negative numbers.</param>
        public DecimalDigitsSequenceToIntegerConverter(
            ILinks<TLink> links,
            IConverter<TLink> linkToDigitConverter,
            ISequenceWalker<TLink> sequenceWalker,
            TLink decimalDigitsSequenceMarker,
            TLink negativeNumberMarker)
            : base(links)
        {
            _linkToDigitConverter = linkToDigitConverter;
            _sequenceWalker = sequenceWalker;
            _decimalDigitsSequenceMarker = decimalDigitsSequenceMarker;
            _negativeNumberMarker = negativeNumberMarker;
        }

        /// <summary>
        /// Converts a decimal digits sequence to an integer.
        /// </summary>
        /// <param name="sequenceLink">The link representing the decimal digits sequence.</param>
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

            bool isNegative = false;
            TLink actualSequenceLink = sequenceLink;

            if (EqualityComparer<TLink>.Default.Equals(source, _negativeNumberMarker))
            {
                isNegative = true;
                actualSequenceLink = target;
                // Re-read the link for the actual sequence
                _links.Each(link =>
                {
                    if (EqualityComparer<TLink>.Default.Equals(link[_links.Constants.IndexPart], actualSequenceLink))
                    {
                        source = link[_links.Constants.SourcePart];
                        target = link[_links.Constants.TargetPart];
                        return _links.Constants.Break;
                    }
                    return _links.Constants.Continue;
                }, _links.Constants.Any, actualSequenceLink);
            }

            // Check if this is a marked decimal digits sequence
            TLink actualSequence;
            if (EqualityComparer<TLink>.Default.Equals(source, _decimalDigitsSequenceMarker))
            {
                // It's a marked sequence, get the actual sequence
                actualSequence = target;
            }
            else
            {
                // Assume the link itself is the sequence
                actualSequence = actualSequenceLink;
            }

            BigInteger result = BigInteger.Zero;

            // Walk through the sequence and reconstruct the number
            foreach (var step in _sequenceWalker.Walk(actualSequence))
            {
                if (step != null && step.Count > 0)
                {
                    var digitLink = step[0]; // Get the first element
                    var digit = _linkToDigitConverter.Convert(digitLink);
                    var digitValue = (int)(object)digit;
                    result = result * 10 + digitValue;
                }
            }

            if (isNegative)
            {
                result = -result;
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
