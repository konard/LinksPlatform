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
    /// Storage for internationalization (i18n) data in a links file.
    /// Allows storing translations for multiple languages in a single file.
    /// </summary>
    /// <remarks>
    /// Structure:
    /// - languageMarker -> languageCode (e.g., "en", "ru")
    /// - translationKeyMarker -> keyString
    /// - translation: languageLink -> keyLink -> valueString
    /// </remarks>
    public class I18nStorage<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly StringToUnicodeSequenceConverter<TLink> _stringToUnicodeSequenceConverter;
        private readonly ILinks<TLink> _links;
        private TLink _unicodeSymbolMarker;
        private TLink _unicodeSequenceMarker;
        private TLink _languageMarker;
        private TLink _translationKeyMarker;
        private TLink _translationMarker;
        private readonly Dictionary<string, TLink> _languageCache;
        private readonly Dictionary<string, TLink> _keyCache;

        private class Unindex : ISequenceIndex<TLink>
        {
            public bool Add(IList<TLink> sequence) => true;
            public bool MightContain(IList<TLink> sequence) => true;
        }

        public I18nStorage(ILinks<TLink> links, bool indexSequenceBeforeCreation, LinkFrequenciesCache<TLink> frequenciesCache)
        {
            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<TLink>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<TLink>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<TLink>(links, sequenceToItsLocalElementLevelsConverter);
            InitConstants(links);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(links, new AddressToRawNumberConverter<TLink>(), _unicodeSymbolMarker);
            var index = indexSequenceBeforeCreation ? new CachedFrequencyIncrementingSequenceIndex<TLink>(frequenciesCache) : (ISequenceIndex<TLink>)new Unindex();
            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<TLink>(links, charToUnicodeSymbolConverter, index, optimalVariantConverter, _unicodeSequenceMarker);
            _links = links;
            _languageCache = new Dictionary<string, TLink>();
            _keyCache = new Dictionary<string, TLink>();
        }

        private void InitConstants(ILinks<TLink> links)
        {
            var markerIndex = _one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _languageMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _translationKeyMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _translationMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
        }

        /// <summary>
        /// Creates or gets a language identifier link.
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en", "ru", "es").</param>
        /// <returns>Link representing the language.</returns>
        public TLink GetOrCreateLanguage(string languageCode)
        {
            if (_languageCache.TryGetValue(languageCode, out var cached))
            {
                return cached;
            }
            var languageSequence = _stringToUnicodeSequenceConverter.Convert(languageCode);
            var languageLink = _links.GetOrCreate(_languageMarker, languageSequence);
            _languageCache[languageCode] = languageLink;
            return languageLink;
        }

        /// <summary>
        /// Creates or gets a translation key link.
        /// </summary>
        /// <param name="key">Translation key (e.g., "greeting.hello", "button.submit").</param>
        /// <returns>Link representing the key.</returns>
        public TLink GetOrCreateKey(string key)
        {
            if (_keyCache.TryGetValue(key, out var cached))
            {
                return cached;
            }
            var keySequence = _stringToUnicodeSequenceConverter.Convert(key);
            var keyLink = _links.GetOrCreate(_translationKeyMarker, keySequence);
            _keyCache[key] = keyLink;
            return keyLink;
        }

        /// <summary>
        /// Stores a translation for a given language and key.
        /// </summary>
        /// <param name="languageCode">Language code.</param>
        /// <param name="key">Translation key.</param>
        /// <param name="value">Translated value.</param>
        /// <returns>Link representing the translation.</returns>
        public TLink SetTranslation(string languageCode, string key, string value)
        {
            var languageLink = GetOrCreateLanguage(languageCode);
            var keyLink = GetOrCreateKey(key);
            var valueSequence = _stringToUnicodeSequenceConverter.Convert(value);

            // Create structure: translationMarker -> (languageLink -> keyLink) -> valueSequence
            var languageKeyPair = _links.GetOrCreate(languageLink, keyLink);
            var translation = _links.GetOrCreate(languageKeyPair, valueSequence);
            return _links.GetOrCreate(_translationMarker, translation);
        }

        /// <summary>
        /// Retrieves a translation for a given language and key.
        /// </summary>
        /// <param name="languageCode">Language code.</param>
        /// <param name="key">Translation key.</param>
        /// <returns>Link representing the translation value, or default if not found.</returns>
        public TLink GetTranslation(string languageCode, string key)
        {
            var languageLink = GetOrCreateLanguage(languageCode);
            var keyLink = GetOrCreateKey(key);
            var languageKeyPair = _links.SearchOrDefault(languageLink, keyLink);

            if (Comparer<TLink>.Default.Compare(languageKeyPair, _zero) == 0)
            {
                return _zero;
            }

            // Search for translation with this language-key pair
            TLink result = _zero;
            _links.Each(link =>
            {
                var source = _links.GetSource(link);
                var target = _links.GetTarget(link);

                if (Comparer<TLink>.Default.Compare(source, languageKeyPair) == 0)
                {
                    result = target;
                    return _links.Constants.Break;
                }
                return _links.Constants.Continue;
            }, _links.Constants.Any, _links.Constants.Any);

            return result;
        }
    }
}
