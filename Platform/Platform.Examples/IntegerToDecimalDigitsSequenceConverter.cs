using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Converts an integer number to a sequence of decimal digits.
    /// Each element in the sequence represents a single decimal digit (0-9).
    /// For example, 12345 = [1, 2, 3, 4, 5]
    /// </summary>
    /// <typeparam name="TLink">The type of link addresses.</typeparam>
    public class IntegerToDecimalDigitsSequenceConverter<TLink> : LinksOperatorBase<TLink>, IConverter<BigInteger, TLink>
        where TLink : struct, IUnsignedNumber<TLink>, IComparisonOperators<TLink, TLink, bool>
    {
        private readonly IConverter<TLink> _digitToLinkConverter;
        private readonly IConverter<IList<TLink>, TLink> _listToSequenceConverter;
        private readonly TLink _decimalDigitsSequenceMarker;
        private readonly TLink _negativeNumberMarker;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerToDecimalDigitsSequenceConverter{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="digitToLinkConverter">Converter from digit (0-9) to link.</param>
        /// <param name="listToSequenceConverter">Converter from list of links to sequence link.</param>
        /// <param name="decimalDigitsSequenceMarker">Marker link to identify decimal digit sequences.</param>
        /// <param name="negativeNumberMarker">Marker link to identify negative numbers.</param>
        public IntegerToDecimalDigitsSequenceConverter(
            ILinks<TLink> links,
            IConverter<TLink> digitToLinkConverter,
            IConverter<IList<TLink>, TLink> listToSequenceConverter,
            TLink decimalDigitsSequenceMarker,
            TLink negativeNumberMarker)
            : base(links)
        {
            _digitToLinkConverter = digitToLinkConverter;
            _listToSequenceConverter = listToSequenceConverter;
            _decimalDigitsSequenceMarker = decimalDigitsSequenceMarker;
            _negativeNumberMarker = negativeNumberMarker;
        }

        /// <summary>
        /// Converts an integer to a sequence of decimal digits.
        /// </summary>
        /// <param name="number">The integer to convert.</param>
        /// <returns>A link representing the sequence of decimal digits.</returns>
        public TLink Convert(BigInteger number)
        {
            bool isNegative = number < 0;
            if (isNegative)
            {
                number = BigInteger.Abs(number);
            }

            if (number == 0)
            {
                // Zero is represented as a single digit
                var zeroLink = _digitToLinkConverter.Convert(TLink.Zero);
                var zeroSequence = _listToSequenceConverter.Convert(new List<TLink> { zeroLink });
                return _links.GetOrCreate(_decimalDigitsSequenceMarker, zeroSequence);
            }

            var digits = new List<TLink>();

            // Extract decimal digits from most significant to least significant
            var numberString = number.ToString();
            foreach (char digitChar in numberString)
            {
                int digitValue = digitChar - '0';
                var digitLink = _digitToLinkConverter.Convert(TLink.CreateTruncating(digitValue));
                digits.Add(digitLink);
            }

            // Convert the list of digits to a sequence
            var sequence = _listToSequenceConverter.Convert(digits);

            // Mark this sequence as a decimal digits representation
            var markedSequence = _links.GetOrCreate(_decimalDigitsSequenceMarker, sequence);

            // If negative, wrap with negative marker
            if (isNegative)
            {
                return _links.GetOrCreate(_negativeNumberMarker, markedSequence);
            }

            return markedSequence;
        }
    }
}
