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

        public bool IsType(TLink link)
        {
            var linkArray = _links.GetLink(link);
            if (linkArray == null || linkArray.Count < 3)
                return false;

            return EqualityComparer<TLink>.Default.Equals(linkArray[_links.Constants.SourcePart], _typeMarker);
        }

        public bool IsProperty(TLink link)
        {
            var linkArray = _links.GetLink(link);
            if (linkArray == null || linkArray.Count < 3)
                return false;

            return EqualityComparer<TLink>.Default.Equals(linkArray[_links.Constants.SourcePart], _propertyMarker);
        }

        public string GetTypeName(TLink typeMarker)
        {
            if (_typeNameCache.TryGetValue(typeMarker, out var cachedName))
                return cachedName;

            var linkArray = _links.GetLink(typeMarker);
            if (linkArray == null || linkArray.Count < 3)
                return null;

            var nameSequence = linkArray[_links.Constants.TargetPart];
            var name = ConvertSequenceToString(nameSequence);

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

            var linkArray = _links.GetLink(propertyMarker);
            if (linkArray == null || linkArray.Count < 3)
                return null;

            var nameSequence = linkArray[_links.Constants.TargetPart];
            var name = ConvertSequenceToString(nameSequence);

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
                while (!EqualityComparer<TLink>.Default.Equals(current, _zero))
                {
                    var linkArray = _links.GetLink(current);
                    if (linkArray == null || linkArray.Count < 3)
                        break;

                    var source = linkArray[_links.Constants.SourcePart];
                    var target = linkArray[_links.Constants.TargetPart];

                    // Check if target is a unicode symbol
                    var targetArray = _links.GetLink(target);
                    if (targetArray != null && targetArray.Count >= 3)
                    {
                        var targetSource = targetArray[_links.Constants.SourcePart];
                        if (EqualityComparer<TLink>.Default.Equals(targetSource, _unicodeSymbolMarker))
                        {
                            // Extract character code
                            var charCode = targetArray[_links.Constants.TargetPart];
                            var charValue = ConvertLinkToInt(charCode);
                            if (charValue >= 0 && charValue <= 0x10FFFF)
                            {
                                chars.Add((char)charValue);
                            }
                        }
                    }

                    // Check if this is a single symbol
                    if (EqualityComparer<TLink>.Default.Equals(source, _unicodeSymbolMarker))
                    {
                        var charCode = target;
                        var charValue = ConvertLinkToInt(charCode);
                        if (charValue >= 0 && charValue <= 0x10FFFF)
                        {
                            chars.Insert(0, (char)charValue);
                        }
                        break;
                    }

                    current = source;
                }

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
