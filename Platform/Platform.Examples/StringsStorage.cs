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
    public class StringsStorage<TLink> : IStringsStorage<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly StringToUnicodeSequenceConverter<TLink> _stringToUnicodeSequenceConverter;
        private readonly ILinks<TLink> _links;
        private readonly TLink _stringMarker;

        private class Unindex : ISequenceIndex<TLink>
        {
            public bool Add(IList<TLink> sequence) => true;
            public bool MightContain(IList<TLink> sequence) => true;
        }

        public StringsStorage(ILinks<TLink> links, bool indexSequenceBeforeCreation, LinkFrequenciesCache<TLink> frequenciesCache)
        {
            _links = links;
            var (unicodeSymbolMarker, unicodeSequenceMarker, stringMarker) = InitConstants(links);
            _stringMarker = stringMarker;

            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<TLink>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<TLink>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<TLink>(links, sequenceToItsLocalElementLevelsConverter);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(links, new AddressToRawNumberConverter<TLink>(), unicodeSymbolMarker);
            var index = indexSequenceBeforeCreation ? new CachedFrequencyIncrementingSequenceIndex<TLink>(frequenciesCache) : (ISequenceIndex<TLink>)new Unindex();

            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<TLink>(links, charToUnicodeSymbolConverter, index, optimalVariantConverter, unicodeSequenceMarker);
        }

        private static (TLink, TLink, TLink) InitConstants(ILinks<TLink> links)
        {
            var markerIndex = _one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            var unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            var unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            var stringMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            return (unicodeSymbolMarker, unicodeSequenceMarker, stringMarker);
        }

        public TLink Store(string @string)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(@string);
            return _links.GetOrCreate(_stringMarker, contentSequence);
        }

        public string Get(TLink link)
        {
            // Note: Getting string from link requires additional implementation
            // that depends on the specific ILinks implementation being used
            // This is a simplified version that returns a placeholder
            return $"Link:{link}";
        }

        public bool Contains(string @string)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(@string);
            var link = _links.SearchOrDefault(_stringMarker, contentSequence);
            return !EqualityComparer<TLink>.Default.Equals(link, _links.Constants.Null);
        }

        public TLink GetOrCreate(string @string)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(@string);
            return _links.GetOrCreate(_stringMarker, contentSequence);
        }
    }
}
