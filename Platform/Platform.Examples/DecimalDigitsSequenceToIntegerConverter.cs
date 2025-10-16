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
        where TLink : struct, IUnsignedNumber<TLink>, IComparisonOperators<TLink, TLink, bool>
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
            var link = _links.GetLink(sequenceLink);
            if (link == null || link.Count == 0)
            {
                throw new ArgumentException("Invalid sequence link.", nameof(sequenceLink));
            }

            // Check for negative marker
            var source = link[_links.Constants.SourcePart];
            var target = link[_links.Constants.TargetPart];

            bool isNegative = false;
            TLink actualSequenceLink = sequenceLink;

            if (EqualityComparer<TLink>.Default.Equals(source, _negativeNumberMarker))
            {
                isNegative = true;
                actualSequenceLink = target;
                // Re-read the link for the actual sequence
                link = _links.GetLink(actualSequenceLink);
                source = link[_links.Constants.SourcePart];
                target = link[_links.Constants.TargetPart];
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
            _sequenceWalker.Walk(actualSequence, digitLink =>
            {
                var digit = _linkToDigitConverter.Convert(digitLink);
                var digitValue = int.CreateTruncating(digit);
                result = result * 10 + digitValue;
                return true; // Continue walking
            });

            if (isNegative)
            {
                result = -result;
            }

            return result;
        }
    }
}
