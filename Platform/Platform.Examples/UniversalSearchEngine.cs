using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Numbers;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Infinitely precise universal search engine that can work with any objects
    /// that have characteristics, parameters, options, settings, descriptions,
    /// or answers to questions about the object or link.
    ///
    /// This engine uses bitstrings or direct links between criteria values
    /// and the actual objects of potential interest.
    /// </summary>
    public class UniversalSearchEngine<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly ILinks<TLink> _links;
        private readonly SequenceIndex<TLink> _index;
        private readonly CharToUnicodeSymbolConverter<TLink> _charToUnicodeSymbolConverter;

        private readonly TLink _objectMarker;
        private readonly TLink _criteriaMarker;
        private readonly TLink _valueMarker;

        public UniversalSearchEngine(ILinks<TLink> links, SequenceIndex<TLink> index)
        {
            _links = links;
            _index = index;

            InitializeMarkers(out _objectMarker, out _criteriaMarker, out _valueMarker);

            var addressToRawNumberConverter = new Platform.Data.Numbers.Raw.AddressToRawNumberConverter<TLink>();
            _charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(
                links,
                addressToRawNumberConverter,
                GetOrCreateUnicodeSymbolMarker());
        }

        private void InitializeMarkers(out TLink objectMarker, out TLink criteriaMarker, out TLink valueMarker)
        {
            var meaningRoot = _links.GetOrCreate(_one, _one);
            objectMarker = _links.GetOrCreate(meaningRoot, Arithmetic.Increment(_one));
            criteriaMarker = _links.GetOrCreate(meaningRoot, Arithmetic.Add(Arithmetic.Increment(_one), _one));
            valueMarker = _links.GetOrCreate(meaningRoot, Arithmetic.Add(Arithmetic.Add(_one, _one), _one));
        }

        private TLink GetOrCreateUnicodeSymbolMarker()
        {
            var meaningRoot = _links.GetOrCreate(_one, _one);
            return _links.GetOrCreate(meaningRoot, Arithmetic.Add(Arithmetic.Add(Arithmetic.Add(Arithmetic.Add(_one, _one), _one), _one), _one));
        }

        /// <summary>
        /// Registers an object with the given name into the search engine.
        /// </summary>
        public TLink RegisterObject(string objectName)
        {
            var nameSequence = ConvertStringToSequence(objectName);
            var objectLink = _links.GetOrCreate(_objectMarker, nameSequence);
            return objectLink;
        }

        /// <summary>
        /// Adds a characteristic (criteria-value pair) to an object.
        /// </summary>
        public void AddCharacteristic(TLink objectLink, string criteriaName, string value)
        {
            var criteriaSequence = ConvertStringToSequence(criteriaName);
            var valueSequence = ConvertStringToSequence(value);

            var criteria = _links.GetOrCreate(_criteriaMarker, criteriaSequence);
            var valueLink = _links.GetOrCreate(_valueMarker, valueSequence);

            // Create a link: object -> (criteria -> value)
            var criteriaValuePair = _links.GetOrCreate(criteria, valueLink);
            _links.GetOrCreate(objectLink, criteriaValuePair);
        }

        /// <summary>
        /// Searches for objects that match the given criteria and value.
        /// </summary>
        public IEnumerable<TLink> Search(string criteriaName, string value)
        {
            var criteriaSequence = ConvertStringToSequence(criteriaName);
            var valueSequence = ConvertStringToSequence(value);

            var criteria = _links.SearchOrDefault(_criteriaMarker, criteriaSequence);
            if (Comparer<TLink>.Default.Compare(criteria, _links.Constants.Null) == 0)
            {
                return Enumerable.Empty<TLink>();
            }

            var valueLink = _links.SearchOrDefault(_valueMarker, valueSequence);
            if (Comparer<TLink>.Default.Compare(valueLink, _links.Constants.Null) == 0)
            {
                return Enumerable.Empty<TLink>();
            }

            var criteriaValuePair = _links.SearchOrDefault(criteria, valueLink);
            if (Comparer<TLink>.Default.Compare(criteriaValuePair, _links.Constants.Null) == 0)
            {
                return Enumerable.Empty<TLink>();
            }

            // Find all objects that have this criteria-value pair
            var results = new List<TLink>();
            var query = new Link<TLink>(_links.Constants.Any, criteriaValuePair);

            _links.Each(link =>
            {
                var target = _links.GetTarget(link);
                if (Comparer<TLink>.Default.Compare(target, criteriaValuePair) == 0)
                {
                    var source = _links.GetSource(link);
                    results.Add(source);
                }
                return _links.Constants.Continue;
            }, query);

            return results;
        }

        /// <summary>
        /// Searches for objects matching multiple criteria (AND logic).
        /// </summary>
        public IEnumerable<TLink> SearchMultipleCriteria(params (string criteriaName, string value)[] criteriaValuePairs)
        {
            if (criteriaValuePairs.Length == 0)
            {
                return Enumerable.Empty<TLink>();
            }

            var results = new HashSet<TLink>(Search(criteriaValuePairs[0].criteriaName, criteriaValuePairs[0].value));

            for (int i = 1; i < criteriaValuePairs.Length; i++)
            {
                var nextResults = new HashSet<TLink>(Search(criteriaValuePairs[i].criteriaName, criteriaValuePairs[i].value));
                results.IntersectWith(nextResults);
            }

            return results;
        }

        /// <summary>
        /// Gets the name of an object.
        /// </summary>
        public string GetObjectName(TLink objectLink)
        {
            var source = _links.GetSource(objectLink);
            if (Comparer<TLink>.Default.Compare(source, _objectMarker) == 0)
            {
                var nameSequence = _links.GetTarget(objectLink);
                return ConvertSequenceToString(nameSequence);
            }
            return null;
        }

        /// <summary>
        /// Gets all characteristics of an object.
        /// </summary>
        public IEnumerable<(string criteria, string value)> GetCharacteristics(TLink objectLink)
        {
            var characteristics = new List<(string, string)>();
            var query = new Link<TLink>(objectLink, _links.Constants.Any);

            _links.Each(link =>
            {
                var criteriaValuePair = _links.GetTarget(link);

                var criteria = _links.GetSource(criteriaValuePair);
                var value = _links.GetTarget(criteriaValuePair);

                var criteriaSource = _links.GetSource(criteria);
                var valueSource = _links.GetSource(value);

                if (Comparer<TLink>.Default.Compare(criteriaSource, _criteriaMarker) == 0 &&
                    Comparer<TLink>.Default.Compare(valueSource, _valueMarker) == 0)
                {
                    var criteriaName = ConvertSequenceToString(_links.GetTarget(criteria));
                    var valueName = ConvertSequenceToString(_links.GetTarget(value));
                    characteristics.Add((criteriaName, valueName));
                }

                return _links.Constants.Continue;
            }, query);

            return characteristics;
        }

        private TLink ConvertStringToSequence(string str)
        {
            var elements = new TLink[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                elements[i] = _charToUnicodeSymbolConverter.Convert(str[i]);
            }
            _index.Add(elements);
            return _links.GetOrCreate(elements[0], elements[elements.Length - 1]);
        }

        private string ConvertSequenceToString(TLink sequence)
        {
            // Simple approach: just return a placeholder for now
            // In a real implementation, you would traverse the sequence structure
            return sequence.ToString();
        }
    }
}
