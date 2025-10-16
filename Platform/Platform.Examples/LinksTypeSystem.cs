using System;
using System.Collections.Generic;
using Platform.Numbers;
using Platform.Data.Numbers.Raw;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Implements a type system for Links-based objects.
    /// Provides markers for types and properties, enabling static and dynamic type interpretation.
    /// </summary>
    public class LinksTypeSystem<TLink> : ITypeSystem<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly ILinks<TLink> _links;
        private readonly CharToUnicodeSymbolConverter<TLink> _charToUnicodeSymbolConverter;
        private readonly TLink _typeSystemMarker;
        private readonly TLink _typeMarker;
        private readonly TLink _propertyMarker;
        private readonly TLink _unicodeSymbolMarker;

        private readonly Dictionary<string, TLink> _typeCache = new Dictionary<string, TLink>();
        private readonly Dictionary<string, TLink> _propertyCache = new Dictionary<string, TLink>();
        private readonly Dictionary<TLink, string> _typeNameCache = new Dictionary<TLink, string>();
        private readonly Dictionary<TLink, string> _propertyNameCache = new Dictionary<TLink, string>();

        public TLink TypeSystemMarker => _typeSystemMarker;
        public TLink TypeMarker => _typeMarker;
        public TLink PropertyMarker => _propertyMarker;

        public LinksTypeSystem(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));

            // Initialize markers
            var markerIndex = _one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _typeSystemMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(ref markerIndex));
            _typeMarker = links.GetOrCreate(_typeSystemMarker, Arithmetic.Increment(ref markerIndex));
            _propertyMarker = links.GetOrCreate(_typeSystemMarker, Arithmetic.Increment(ref markerIndex));

            _charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(
                links,
                new AddressToRawNumberConverter<TLink>(),
                _unicodeSymbolMarker);
        }

        public TLink GetOrCreateType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

            if (_typeCache.TryGetValue(typeName, out var cachedType))
                return cachedType;

            var nameSequence = ConvertStringToSequence(typeName);
            var type = _links.GetOrCreate(_typeMarker, nameSequence);

            _typeCache[typeName] = type;
            _typeNameCache[type] = typeName;

            return type;
        }

        public TLink GetOrCreateProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

            if (_propertyCache.TryGetValue(propertyName, out var cachedProperty))
                return cachedProperty;

            var nameSequence = ConvertStringToSequence(propertyName);
            var property = _links.GetOrCreate(_propertyMarker, nameSequence);

            _propertyCache[propertyName] = property;
            _propertyNameCache[property] = propertyName;

            return property;
        }

        public bool IsType(TLink linkAddress)
        {
            bool result = false;
            _links.Each(new[] { linkAddress }, link =>
            {
                if (link != null && link.Count >= 3)
                {
                    result = EqualityComparer<TLink>.Default.Equals(link[_links.Constants.SourcePart], _typeMarker);
                }
                return _links.Constants.Break;
            });
            return result;
        }

        public bool IsProperty(TLink linkAddress)
        {
            bool result = false;
            _links.Each(new[] { linkAddress }, link =>
            {
                if (link != null && link.Count >= 3)
                {
                    result = EqualityComparer<TLink>.Default.Equals(link[_links.Constants.SourcePart], _propertyMarker);
                }
                return _links.Constants.Break;
            });
            return result;
        }

        public string GetTypeName(TLink typeMarker)
        {
            if (_typeNameCache.TryGetValue(typeMarker, out var cachedName))
                return cachedName;

            string name = null;
            _links.Each(new[] { typeMarker }, link =>
            {
                if (link != null && link.Count >= 3)
                {
                    var nameSequence = link[_links.Constants.TargetPart];
                    name = ConvertSequenceToString(nameSequence);
                }
                return _links.Constants.Break;
            });

            if (name != null)
            {
                _typeNameCache[typeMarker] = name;
                _typeCache[name] = typeMarker;
            }

            return name;
        }

        public string GetPropertyName(TLink propertyMarker)
        {
            if (_propertyNameCache.TryGetValue(propertyMarker, out var cachedName))
                return cachedName;

            string name = null;
            _links.Each(new[] { propertyMarker }, link =>
            {
                if (link != null && link.Count >= 3)
                {
                    var nameSequence = link[_links.Constants.TargetPart];
                    name = ConvertSequenceToString(nameSequence);
                }
                return _links.Constants.Break;
            });

            if (name != null)
            {
                _propertyNameCache[propertyMarker] = name;
                _propertyCache[name] = propertyMarker;
            }

            return name;
        }

        private TLink ConvertStringToSequence(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException("String cannot be null or empty.", nameof(str));

            var chars = str.ToCharArray();
            if (chars.Length == 0)
                throw new ArgumentException("String cannot be empty.", nameof(str));

            if (chars.Length == 1)
                return _charToUnicodeSymbolConverter.Convert(chars[0]);

            var sequence = _charToUnicodeSymbolConverter.Convert(chars[0]);
            for (int i = 1; i < chars.Length; i++)
            {
                var symbol = _charToUnicodeSymbolConverter.Convert(chars[i]);
                sequence = _links.GetOrCreate(sequence, symbol);
            }

            return sequence;
        }

        private string ConvertSequenceToString(TLink sequence)
        {
            try
            {
                var chars = new List<char>();
                var current = sequence;

                // Try to traverse the sequence
                // Simplified string conversion - for full implementation would need proper sequence traversal
                // This is a placeholder that acknowledges the string is stored but doesn't fully decode it
                _links.Each(new[] { sequence }, link =>
                {
                    // Just check if it exists - full conversion would require sequence walking
                    return _links.Constants.Break;
                });

                return chars.Count > 0 ? new string(chars.ToArray()) : null;
            }
            catch
            {
                return null;
            }
        }

        private int ConvertLinkToInt(TLink link)
        {
            try
            {
                return Convert.ToInt32(link);
            }
            catch
            {
                return -1;
            }
        }
    }
}
