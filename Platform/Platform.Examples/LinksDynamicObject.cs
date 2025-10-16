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

                // Search for property in object's links: object -> property -> value
                TLink result = default;
                var propertyLink = _links.SearchOrDefault(_objectLink, property);
                if (!EqualityComparer<TLink>.Default.Equals(propertyLink, default(TLink)))
                {
                    // Found a link from object to property, now get its value
                    _links.Each(link =>
                    {
                        if (link != null && link.Count >= 3)
                        {
                            result = link[_links.Constants.TargetPart];
                        }
                        return _links.Constants.Break;
                    }, propertyLink);
                }

                return result;
            }
            set
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

                var property = _typeSystem.GetOrCreateProperty(propertyName);

                // Create link: object -> property as intermediate, then property -> value
                // Simplified: just create object -> value with property as "type"
                var propertyLink = _links.GetOrCreate(_objectLink, property);
                // Store the actual value as target of the property link
                // For simplicity: property points to value
                _links.GetOrCreate(propertyLink, value);
            }
        }

        public IDictionary<string, TLink> GetProperties()
        {
            var properties = new Dictionary<string, TLink>();

            // Find all links where source is our object
            _links.Each(link =>
            {
                if (link != null && link.Count >= 3)
                {
                    var source = link[_links.Constants.SourcePart];
                    var target = link[_links.Constants.TargetPart];

                    if (EqualityComparer<TLink>.Default.Equals(source, _objectLink))
                    {
                        // Check if target is a property marker
                        if (_typeSystem.IsProperty(target))
                        {
                            var propertyName = _typeSystem.GetPropertyName(target);
                            if (propertyName != null)
                            {
                                // Get the value - simplified
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
            var propertyLink = _links.SearchOrDefault(_objectLink, property);
            return !EqualityComparer<TLink>.Default.Equals(propertyLink, default(TLink));
        }

        public TLink GetType()
        {
            // Search for type link where object points to a type marker
            TLink result = default;
            _links.Each(link =>
            {
                if (link != null && link.Count >= 3)
                {
                    var source = link[_links.Constants.SourcePart];
                    var target = link[_links.Constants.TargetPart];

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
            if (EqualityComparer<TLink>.Default.Equals(typeMarker, default(TLink)))
                throw new ArgumentNullException(nameof(typeMarker));

            if (!_typeSystem.IsType(typeMarker))
                throw new ArgumentException("The provided link is not a type marker.", nameof(typeMarker));

            // Create type link: object -> typeMarker
            _links.GetOrCreate(_objectLink, typeMarker);
        }
    }
}
