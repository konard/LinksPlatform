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
    /// Universal hierarchical storage implementation for Links platform.
    /// Supports XML, JSON, FileSystem and other hierarchical data imports.
    /// </summary>
    public class LinksHierarchicalStorage<TLink> : IHierarchicalStorage<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly StringToUnicodeSequenceConverter<TLink> _stringToUnicodeSequenceConverter;
        private readonly ILinks<TLink> _links;
        private TLink _unicodeSymbolMarker;
        private TLink _unicodeSequenceMarker;
        private TLink _rootMarker;
        private TLink _nodeMarker;
        private TLink _valueMarker;
        private readonly Dictionary<string, TLink> _typeMarkers;

        private class Unindex : ISequenceIndex<TLink>
        {
            public bool Add(IList<TLink> sequence) => true;
            public bool MightContain(IList<TLink> sequence) => true;
        }

        public LinksHierarchicalStorage(ILinks<TLink> links, bool indexSequenceBeforeCreation, LinkFrequenciesCache<TLink> frequenciesCache)
        {
            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<TLink>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<TLink>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<TLink>(links, sequenceToItsLocalElementLevelsConverter);
            InitConstants(links);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(links, new AddressToRawNumberConverter<TLink>(), _unicodeSymbolMarker);
            var index = indexSequenceBeforeCreation ? new CachedFrequencyIncrementingSequenceIndex<TLink>(frequenciesCache) : (ISequenceIndex<TLink>)new Unindex();
            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<TLink>(links, charToUnicodeSymbolConverter, index, optimalVariantConverter, _unicodeSequenceMarker);
            _links = links;
            _typeMarkers = new Dictionary<string, TLink>();
        }

        private void InitConstants(ILinks<TLink> links)
        {
            var markerIndex = _one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _rootMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _nodeMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _valueMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
        }

        public TLink CreateRoot(string name) => Create(_rootMarker, name);

        public TLink CreateNode(string name, string type = null)
        {
            var marker = type != null ? GetOrCreateTypeMarker(type) : _nodeMarker;
            return Create(marker, name);
        }

        public TLink CreateValueNode(string content) => Create(_valueMarker, content);

        private TLink GetOrCreateTypeMarker(string type)
        {
            if (_typeMarkers.TryGetValue(type, out var marker))
            {
                return marker;
            }
            var typeSequence = _stringToUnicodeSequenceConverter.Convert(type);
            marker = _links.GetOrCreate(_nodeMarker, typeSequence);
            _typeMarkers[type] = marker;
            return marker;
        }

        private TLink Create(TLink marker, string content)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(content);
            return _links.GetOrCreate(marker, contentSequence);
        }

        public void AttachToParent(TLink child, TLink parent) => _links.GetOrCreate(parent, child);
    }
}
