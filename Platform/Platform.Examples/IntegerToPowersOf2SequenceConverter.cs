using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Converters;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Converts an integer number to a sequence representing it as a sum of powers of 2 (binary representation).
    /// Each element in the sequence represents a power of 2 that is present in the binary representation.
    /// For example, 13 (binary: 1101) = 2^3 + 2^2 + 2^0 = [3, 2, 0]
    /// </summary>
    /// <typeparam name="TLink">The type of link addresses.</typeparam>
    public class IntegerToPowersOf2SequenceConverter<TLink> : LinksOperatorBase<TLink>, IConverter<BigInteger, TLink>
        where TLink : struct, IUnsignedNumber<TLink>, IComparisonOperators<TLink, TLink, bool>
    {
        private readonly IConverter<TLink> _powerToLinkConverter;
        private readonly IConverter<IList<TLink>, TLink> _listToSequenceConverter;
        private readonly TLink _powersOf2SequenceMarker;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerToPowersOf2SequenceConverter{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="powerToLinkConverter">Converter from power index to link.</param>
        /// <param name="listToSequenceConverter">Converter from list of links to sequence link.</param>
        /// <param name="powersOf2SequenceMarker">Marker link to identify powers-of-2 sequences.</param>
        public IntegerToPowersOf2SequenceConverter(
            ILinks<TLink> links,
            IConverter<TLink> powerToLinkConverter,
            IConverter<IList<TLink>, TLink> listToSequenceConverter,
            TLink powersOf2SequenceMarker)
            : base(links)
        {
            _powerToLinkConverter = powerToLinkConverter;
            _listToSequenceConverter = listToSequenceConverter;
            _powersOf2SequenceMarker = powersOf2SequenceMarker;
        }

        /// <summary>
        /// Converts an integer to a sequence of powers of 2.
        /// </summary>
        /// <param name="number">The integer to convert.</param>
        /// <returns>A link representing the sequence of powers of 2.</returns>
        public TLink Convert(BigInteger number)
        {
            if (number < 0)
            {
                throw new ArgumentException("Only non-negative integers are supported.", nameof(number));
            }

            if (number == 0)
            {
                // Zero has no powers of 2, return empty sequence or special marker
                return _powersOf2SequenceMarker;
            }

            var powers = new List<TLink>();
            int bitPosition = 0;

            // Extract each set bit position (power of 2)
            while (number > 0)
            {
                if ((number & 1) == 1)
                {
                    // Convert bit position to link
                    var powerLink = _powerToLinkConverter.Convert(TLink.CreateTruncating(bitPosition));
                    powers.Add(powerLink);
                }
                number >>= 1;
                bitPosition++;
            }

            // Convert the list of powers to a sequence
            var sequence = _listToSequenceConverter.Convert(powers);

            // Mark this sequence as a powers-of-2 representation
            return _links.GetOrCreate(_powersOf2SequenceMarker, sequence);
        }
    }
}
