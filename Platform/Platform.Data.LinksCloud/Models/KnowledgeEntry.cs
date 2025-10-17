using System;
using System.Collections.Generic;

namespace Platform.Data.LinksCloud.Models
{
    /// <summary>
    /// Represents a knowledge entry in the distributed encyclopedia.
    /// Similar to a wiki page that can be edited by anyone in the network.
    /// </summary>
    public class KnowledgeEntry
    {
        /// <summary>
        /// Unique identifier for this knowledge entry.
        /// </summary>
        public ulong EntryId { get; set; }

        /// <summary>
        /// Title of the knowledge entry.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Content of the knowledge entry.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// List of tags/categories for this entry.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Author's node ID.
        /// </summary>
        public Guid AuthorNodeId { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last modification timestamp.
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Version number for conflict resolution.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Hash of the content for integrity verification.
        /// </summary>
        public string ContentHash { get; set; } = string.Empty;

        /// <summary>
        /// Related knowledge entries.
        /// </summary>
        public List<ulong> RelatedEntries { get; set; } = new();

        public KnowledgeEntry()
        {
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
            Version = 1;
        }
    }
}
