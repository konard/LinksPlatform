using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Numbers;
using Platform.Data.Doublets;

namespace Platform.Data.CMS
{
    /// <summary>
    /// Implementation of CMS content storage using LinksPlatform's Doublets.
    /// This class demonstrates how to map CMS entities (content items, properties, relationships)
    /// to the binary link data structure.
    /// </summary>
    /// <remarks>
    /// This is a simplified proof-of-concept implementation that demonstrates the core concepts
    /// of mapping CMS data structures to associative links. A production implementation would
    /// include full Unicode string support, advanced querying, and transaction handling.
    /// </remarks>
    /// <typeparam name="TLink">The type of link identifiers (e.g., ulong, uint).</typeparam>
    public class LinksBasedCMSStorage<TLink> : ICMSContentStorage<TLink, TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly ILinks<TLink> _links;

        // Semantic markers for different entity types
        private TLink _cmsMarker;
        private TLink _contentTypeMarker;
        private TLink _contentItemMarker;
        private TLink _propertyMarker;
        private TLink _relationshipMarker;

        public LinksBasedCMSStorage(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            InitializeMarkers();
        }

        private void InitializeMarkers()
        {
            // Create a semantic root for CMS-related links
            var markerIndex = _one;
            var meaningRoot = _links.GetOrCreate(markerIndex, markerIndex);

            _cmsMarker = _links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _contentTypeMarker = _links.GetOrCreate(_cmsMarker, Arithmetic.Increment(ref markerIndex));
            _contentItemMarker = _links.GetOrCreate(_cmsMarker, Arithmetic.Increment(ref markerIndex));
            _propertyMarker = _links.GetOrCreate(_cmsMarker, Arithmetic.Increment(ref markerIndex));
            _relationshipMarker = _links.GetOrCreate(_cmsMarker, Arithmetic.Increment(ref markerIndex));
        }

        public TLink CreateContent(string contentType, IDictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));

            // Convert content type name to a link using hash
            var contentTypeLink = GetOrCreateContentType(contentType);

            // Create the content item as a link from contentItemMarker to contentTypeLink
            var contentItem = _links.Create();
            _links.Update(contentItem, _contentItemMarker, contentTypeLink);

            // Store property count for this demonstration
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    AddProperty(contentItem, property.Key, property.Value);
                }
            }

            return contentItem;
        }

        public IDictionary<string, object> GetContent(TLink contentId)
        {
            // Verify the content item exists and is of correct type
            if (!IsContentItem(contentId))
                return null;

            var properties = new Dictionary<string, object>();

            // For this simplified implementation, we return basic metadata
            // A full implementation would reconstruct all properties from links
            properties["_id"] = contentId.ToString();
            properties["_type"] = "content";

            return properties;
        }

        public bool UpdateContent(TLink contentId, IDictionary<string, object> properties)
        {
            if (!IsContentItem(contentId))
                return false;

            if (properties == null)
                return false;

            foreach (var property in properties)
            {
                AddProperty(contentId, property.Key, property.Value);
            }

            return true;
        }

        public bool DeleteContent(TLink contentId)
        {
            if (!IsContentItem(contentId))
                return false;

            // Delete the content item
            _links.Delete(contentId);
            return true;
        }

        public IEnumerable<TLink> QueryContent(string contentType, IDictionary<string, object> filters = null)
        {
            var contentTypeLink = GetOrCreateContentType(contentType);
            var results = new List<TLink>();

            // Find all content items of the specified type
            _links.Each(link =>
            {
                var linkId = link[_links.Constants.IndexPart];
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                if (Comparer<TLink>.Default.Compare(source, _contentItemMarker) == 0 &&
                    Comparer<TLink>.Default.Compare(target, contentTypeLink) == 0)
                {
                    results.Add(linkId);
                }

                return _links.Constants.Continue;
            });

            return results;
        }

        public TLink CreateRelationship(TLink sourceId, TLink targetId, string relationshipType)
        {
            if (!IsContentItem(sourceId) || !IsContentItem(targetId))
                throw new ArgumentException("Both source and target must be valid content items");

            // Create: relationshipMarker -> (source -> target)
            var sourceTargetLink = _links.GetOrCreate(sourceId, targetId);
            var relationship = _links.GetOrCreate(_relationshipMarker, sourceTargetLink);

            return relationship;
        }

        public IEnumerable<TLink> GetRelatedContent(TLink contentId, string relationshipType = null)
        {
            if (!IsContentItem(contentId))
                return Enumerable.Empty<TLink>();

            var relatedItems = new List<TLink>();

            _links.Each(link =>
            {
                var linkId = link[_links.Constants.IndexPart];
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                if (Comparer<TLink>.Default.Compare(source, _relationshipMarker) == 0)
                {
                    var sourceTargetLink = target;
                    var relSource = _links.GetSource(sourceTargetLink);
                    var relTarget = _links.GetTarget(sourceTargetLink);

                    // Check if contentId is the source in this relationship
                    if (Comparer<TLink>.Default.Compare(relSource, contentId) == 0)
                    {
                        relatedItems.Add(relTarget);
                    }
                }

                return _links.Constants.Continue;
            });

            return relatedItems;
        }

        #region Helper Methods

        private TLink GetOrCreateContentType(string contentType)
        {
            // Simple hash-based approach for content type
            // In a production system, this would use Unicode string sequences
            var hash = unchecked((uint)contentType.GetHashCode());

            // Convert uint to TLink
            var two = Arithmetic.Increment(_one);
            var hashAsLink = _one;  // Start with 1

            // Simple conversion - add the hash value
            for (uint i = 0; i < (hash % 1000); i++)
            {
                hashAsLink = Arithmetic.Increment(hashAsLink);
            }

            return _links.GetOrCreate(_contentTypeMarker, hashAsLink);
        }

        private bool IsContentItem(TLink link)
        {
            var source = _links.GetSource(link);
            return Comparer<TLink>.Default.Compare(source, _contentItemMarker) == 0;
        }

        private void AddProperty(TLink contentItem, string propertyName, object propertyValue)
        {
            // Simplified property storage using hashes
            var nameHash = unchecked((uint)propertyName.GetHashCode());
            var valueHash = propertyValue != null ? unchecked((uint)propertyValue.GetHashCode()) : 0u;

            // Convert hashes to links (simplified)
            var nameLink = _one;
            for (uint i = 0; i < (nameHash % 100); i++)
            {
                nameLink = Arithmetic.Increment(nameLink);
            }

            var valueLink = _one;
            for (uint i = 0; i < (valueHash % 100); i++)
            {
                valueLink = Arithmetic.Increment(valueLink);
            }

            // Create: propertyMarker -> (contentItem -> (propertyName -> propertyValue))
            var nameValuePair = _links.GetOrCreate(nameLink, valueLink);
            var contentPropertyPair = _links.GetOrCreate(contentItem, nameValuePair);
            _links.GetOrCreate(_propertyMarker, contentPropertyPair);
        }

        #endregion
    }
}
