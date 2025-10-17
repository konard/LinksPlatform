using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.LinksCloud.Models;

namespace Platform.Data.LinksCloud.Storage
{
    /// <summary>
    /// Interface for distributed knowledge storage operations.
    /// Provides wiki-like functionality across the decentralized network.
    /// </summary>
    public interface IDistributedKnowledgeStore
    {
        /// <summary>
        /// Creates a new knowledge entry.
        /// </summary>
        Task<ulong> CreateEntryAsync(string title, string content, List<string> tags);

        /// <summary>
        /// Updates an existing knowledge entry.
        /// </summary>
        Task<bool> UpdateEntryAsync(ulong entryId, string content, Guid editorNodeId);

        /// <summary>
        /// Retrieves a knowledge entry by ID.
        /// </summary>
        Task<KnowledgeEntry?> GetEntryAsync(ulong entryId);

        /// <summary>
        /// Searches for knowledge entries by title or content.
        /// </summary>
        Task<List<KnowledgeEntry>> SearchEntriesAsync(string query);

        /// <summary>
        /// Searches for knowledge entries by tags.
        /// </summary>
        Task<List<KnowledgeEntry>> GetEntriesByTagAsync(string tag);

        /// <summary>
        /// Deletes a knowledge entry.
        /// </summary>
        Task<bool> DeleteEntryAsync(ulong entryId);

        /// <summary>
        /// Gets all knowledge entries.
        /// </summary>
        Task<List<KnowledgeEntry>> GetAllEntriesAsync();

        /// <summary>
        /// Synchronizes knowledge entries across the network.
        /// </summary>
        Task SyncWithNetworkAsync();

        /// <summary>
        /// Event triggered when a new entry is created.
        /// </summary>
        event EventHandler<KnowledgeEntry>? EntryCreated;

        /// <summary>
        /// Event triggered when an entry is updated.
        /// </summary>
        event EventHandler<KnowledgeEntry>? EntryUpdated;

        /// <summary>
        /// Event triggered when an entry is deleted.
        /// </summary>
        event EventHandler<ulong>? EntryDeleted;
    }
}
