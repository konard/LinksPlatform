using System;
using System.Collections.Generic;
using Platform.Numbers;
using Platform.Data.Numbers.Raw;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Implements storage for Lino (Links Notation) data using the Links platform.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class LinksLinoStorage<TLink> : ILinoStorage<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly StringToUnicodeSequenceConverter<TLink> _stringToUnicodeSequenceConverter;
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<string, TLink> _identifierMap;
        private TLink _unicodeSymbolMarker;
        private TLink _unicodeSequenceMarker;
        private TLink _linoLinkMarker;
        private TLink _linoReferenceMarker;

        private class Unindex : ISequenceIndex<TLink>
        {
            public bool Add(IList<TLink> sequence) => true;
            public bool MightContain(IList<TLink> sequence) => true;
        }

        public LinksLinoStorage(ILinks<TLink> links, bool indexSequenceBeforeCreation, LinkFrequenciesCache<TLink> frequenciesCache)
        {
            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<TLink>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<TLink>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<TLink>(links, sequenceToItsLocalElementLevelsConverter);
            InitConstants(links);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(links, new AddressToRawNumberConverter<TLink>(), _unicodeSymbolMarker);
            var index = indexSequenceBeforeCreation ? new CachedFrequencyIncrementingSequenceIndex<TLink>(frequenciesCache) : (ISequenceIndex<TLink>)new Unindex();
            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<TLink>(links, charToUnicodeSymbolConverter, index, optimalVariantConverter, _unicodeSequenceMarker);
            _links = links;
            _identifierMap = new Dictionary<string, TLink>();
        }

        private void InitConstants(ILinks<TLink> links)
        {
            var markerIndex = _one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _linoLinkMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _linoReferenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
        }

        public TLink CreateLink(string id, params string[] values)
        {
            // Convert string values to unicode sequences
            var linkParts = new List<TLink>();
            foreach (var value in values)
            {
                var valueSequence = _stringToUnicodeSequenceConverter.Convert(value);
                linkParts.Add(valueSequence);
            }

            // Create the link based on number of parts
            TLink resultLink;
            if (linkParts.Count == 1)
            {
                // Single value: marker -> value
                resultLink = _links.GetOrCreate(_linoLinkMarker, linkParts[0]);
            }
            else if (linkParts.Count == 2)
            {
                // Doublet: source -> target
                resultLink = _links.GetOrCreate(linkParts[0], linkParts[1]);
            }
            else
            {
                // N-tuple: create sequence of all parts
                TLink sequence = linkParts[0];
                for (int i = 1; i < linkParts.Count; i++)
                {
                    sequence = _links.GetOrCreate(sequence, linkParts[i]);
                }
                resultLink = _links.GetOrCreate(_linoLinkMarker, sequence);
            }

            // Store identifier mapping if provided
            if (!string.IsNullOrEmpty(id))
            {
                _identifierMap[id] = resultLink;
            }

            return resultLink;
        }

        public TLink CreateReference(string id, TLink target)
        {
            TLink resultLink;
            if (!string.IsNullOrEmpty(id))
            {
                var idSequence = _stringToUnicodeSequenceConverter.Convert(id);
                resultLink = _links.GetOrCreate(idSequence, target);
                _identifierMap[id] = resultLink;
            }
            else
            {
                resultLink = _links.GetOrCreate(_linoReferenceMarker, target);
            }
            return resultLink;
        }

        public TLink GetLink(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return _zero;
            }
            return _identifierMap.TryGetValue(id, out var link) ? link : _zero;
        }
    }
}
