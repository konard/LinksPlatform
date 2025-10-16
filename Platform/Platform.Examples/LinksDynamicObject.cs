using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Implements dynamic object interpretation for Links-based structures.
    /// Allows treating Links as dynamic objects with properties, similar to JSON or XML objects.
    /// </summary>
    public class LinksDynamicObject<TLink> : IDynamicObject<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly ITypeSystem<TLink> _typeSystem;
        private readonly TLink _objectMarker;
        private TLink _objectLink;

        public TLink Link => _objectLink;

        public LinksDynamicObject(ILinks<TLink> links, ITypeSystem<TLink> typeSystem, TLink objectMarker)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _typeSystem = typeSystem ?? throw new ArgumentNullException(nameof(typeSystem));
            _objectMarker = objectMarker;
            _objectLink = _links.GetOrCreate(_objectMarker, _objectMarker); // Create empty object
        }

        public LinksDynamicObject(ILinks<TLink> links, ITypeSystem<TLink> typeSystem, TLink objectMarker, TLink existingLink)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _typeSystem = typeSystem ?? throw new ArgumentNullException(nameof(typeSystem));
            _objectMarker = objectMarker;
            _objectLink = existingLink;
        }

        public TLink this[string propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

                var property = _typeSystem.GetOrCreateProperty(propertyName);

                // Search for property in object's links
                TLink result = default;
                _links.Each(link =>
                {
                    var linkArray = _links.GetLink(link);
                    if (linkArray != null && linkArray.Count >= 3)
                    {
                        var source = linkArray[_links.Constants.SourcePart];
                        var linker = linkArray[_links.Constants.IndexPart];

                        // Check if this link connects our object to a value via the property
                        if (EqualityComparer<TLink>.Default.Equals(source, _objectLink) &&
                            EqualityComparer<TLink>.Default.Equals(linker, property))
                        {
                            result = linkArray[_links.Constants.TargetPart];
                            return _links.Constants.Break;
                        }
                    }
                    return _links.Constants.Continue;
                });

                return result;
            }
            set
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

                var property = _typeSystem.GetOrCreateProperty(propertyName);

                // Remove existing property value if any
                TLink existingPropertyLink = default;
                _links.Each(link =>
                {
                    var linkArray = _links.GetLink(link);
                    if (linkArray != null && linkArray.Count >= 3)
                    {
                        var source = linkArray[_links.Constants.SourcePart];
                        var linker = linkArray[_links.Constants.IndexPart];

                        if (EqualityComparer<TLink>.Default.Equals(source, _objectLink) &&
                            EqualityComparer<TLink>.Default.Equals(linker, property))
                        {
                            existingPropertyLink = linkArray[_links.Constants.IndexPart];
                            return _links.Constants.Break;
                        }
                    }
                    return _links.Constants.Continue;
                });

                if (!EqualityComparer<TLink>.Default.Equals(existingPropertyLink, default(TLink)))
                {
                    _links.Delete(existingPropertyLink);
                }

                // Create new property link: object --[property]--> value
                _links.GetOrCreate(_objectLink, property, value);
            }
        }

        public IDictionary<string, TLink> GetProperties()
        {
            var properties = new Dictionary<string, TLink>();

            _links.Each(link =>
            {
                var linkArray = _links.GetLink(link);
                if (linkArray != null && linkArray.Count >= 3)
                {
                    var source = linkArray[_links.Constants.SourcePart];
                    var linker = linkArray[_links.Constants.IndexPart];
                    var target = linkArray[_links.Constants.TargetPart];

                    if (EqualityComparer<TLink>.Default.Equals(source, _objectLink))
                    {
                        // Check if linker is a property marker
                        if (_typeSystem.IsProperty(linker))
                        {
                            var propertyName = _typeSystem.GetPropertyName(linker);
                            if (propertyName != null)
                            {
                                properties[propertyName] = target;
                            }
                        }
                    }
                }
                return _links.Constants.Continue;
            });

            return properties;
        }

        public bool HasProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return false;

            var property = _typeSystem.GetOrCreateProperty(propertyName);

            bool found = false;
            _links.Each(link =>
            {
                var linkArray = _links.GetLink(link);
                if (linkArray != null && linkArray.Count >= 3)
                {
                    var source = linkArray[_links.Constants.SourcePart];
                    var linker = linkArray[_links.Constants.IndexPart];

                    if (EqualityComparer<TLink>.Default.Equals(source, _objectLink) &&
                        EqualityComparer<TLink>.Default.Equals(linker, property))
                    {
                        found = true;
                        return _links.Constants.Break;
                    }
                }
                return _links.Constants.Continue;
            });

            return found;
        }

        public TLink GetType()
        {
            // Search for type link: object --[typeMarker]--> type
            TLink result = default;
            _links.Each(link =>
            {
                var linkArray = _links.GetLink(link);
                if (linkArray != null && linkArray.Count >= 3)
                {
                    var source = linkArray[_links.Constants.SourcePart];
                    var linker = linkArray[_links.Constants.IndexPart];
                    var target = linkArray[_links.Constants.TargetPart];

                    if (EqualityComparer<TLink>.Default.Equals(source, _objectLink))
                    {
                        // Check if the target is a type marker
                        if (_typeSystem.IsType(target))
                        {
                            result = target;
                            return _links.Constants.Break;
                        }
                    }
                }
                return _links.Constants.Continue;
            });

            return result;
        }

        public void SetType(TLink typeMarker)
        {
            if (typeMarker == null)
                throw new ArgumentNullException(nameof(typeMarker));

            if (!_typeSystem.IsType(typeMarker))
                throw new ArgumentException("The provided link is not a type marker.", nameof(typeMarker));

            // Remove existing type if any
            TLink existingTypeLink = default;
            _links.Each(link =>
            {
                var linkArray = _links.GetLink(link);
                if (linkArray != null && linkArray.Count >= 3)
                {
                    var source = linkArray[_links.Constants.SourcePart];
                    var target = linkArray[_links.Constants.TargetPart];

                    if (EqualityComparer<TLink>.Default.Equals(source, _objectLink) &&
                        _typeSystem.IsType(target))
                    {
                        existingTypeLink = linkArray[_links.Constants.IndexPart];
                        return _links.Constants.Break;
                    }
                }
                return _links.Constants.Continue;
            });

            if (!EqualityComparer<TLink>.Default.Equals(existingTypeLink, default(TLink)))
            {
                _links.Delete(existingTypeLink);
            }

            // Create type link
            var typeSystemMarker = ((LinksTypeSystem<TLink>)_typeSystem).TypeMarker;
            _links.GetOrCreate(_objectLink, typeSystemMarker, typeMarker);
        }
    }
}
