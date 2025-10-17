using System.Collections.Generic;

namespace Platform.Data.CMS
{
    /// <summary>
    /// Represents a generic CMS content storage interface that can be implemented
    /// using LinksPlatform's Doublets as the underlying data layer.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    /// <typeparam name="TContentId">The type of content identifiers.</typeparam>
    public interface ICMSContentStorage<TLink, TContentId>
    {
        /// <summary>
        /// Creates a new content item with the specified type and properties.
        /// </summary>
        /// <param name="contentType">The type of content (e.g., "Page", "Article", "Media").</param>
        /// <param name="properties">Dictionary of property names and values.</param>
        /// <returns>The identifier of the created content.</returns>
        TContentId CreateContent(string contentType, IDictionary<string, object> properties);

        /// <summary>
        /// Retrieves a content item by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>Dictionary of property names and values, or null if not found.</returns>
        IDictionary<string, object> GetContent(TContentId contentId);

        /// <summary>
        /// Updates an existing content item with new property values.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="properties">Dictionary of property names and values to update.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool UpdateContent(TContentId contentId, IDictionary<string, object> properties);

        /// <summary>
        /// Deletes a content item by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool DeleteContent(TContentId contentId);

        /// <summary>
        /// Queries content items by type and optional property filters.
        /// </summary>
        /// <param name="contentType">The type of content to query.</param>
        /// <param name="filters">Optional dictionary of property names and values to filter by.</param>
        /// <returns>Collection of content identifiers matching the criteria.</returns>
        IEnumerable<TContentId> QueryContent(string contentType, IDictionary<string, object> filters = null);

        /// <summary>
        /// Creates a relationship between two content items.
        /// </summary>
        /// <param name="sourceId">The source content identifier.</param>
        /// <param name="targetId">The target content identifier.</param>
        /// <param name="relationshipType">The type of relationship (e.g., "parent-child", "reference").</param>
        /// <returns>The identifier of the relationship.</returns>
        TLink CreateRelationship(TContentId sourceId, TContentId targetId, string relationshipType);

        /// <summary>
        /// Gets all related content items for a given content identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="relationshipType">Optional filter by relationship type.</param>
        /// <returns>Collection of related content identifiers.</returns>
        IEnumerable<TContentId> GetRelatedContent(TContentId contentId, string relationshipType = null);
    }
}
