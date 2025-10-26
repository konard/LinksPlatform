using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.Data;
using Platform.Converters;

namespace Platform.Examples
{
    /// <summary>
    /// Synchronizes changes between Links storage and SQL tables bidirectionally.
    /// Tracks changes in both structures and applies them to maintain consistency.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class SqlSynchronizer<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<string, SqlTableMapper<TLink>> _tableMappers;
        private readonly Dictionary<TLink, (TLink Source, TLink Target)> _linkSnapshots;
        private readonly HashSet<TLink> _deletedLinks;

        public SqlSynchronizer(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _tableMappers = new Dictionary<string, SqlTableMapper<TLink>>();
            _linkSnapshots = new Dictionary<TLink, (TLink, TLink)>();
            _deletedLinks = new HashSet<TLink>();
        }

        /// <summary>
        /// Registers a Links substructure to be synchronized with a SQL table.
        /// </summary>
        public void RegisterTable(string tableName, IEnumerable<TLink> substructureLinks)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            if (substructureLinks == null)
                throw new ArgumentNullException(nameof(substructureLinks));

            var mapper = new SqlTableMapper<TLink>(_links, tableName, substructureLinks);
            _tableMappers[tableName] = mapper;

            // Take initial snapshot
            TakeSnapshot(substructureLinks);
        }

        /// <summary>
        /// Takes a snapshot of the current state of links for change detection.
        /// </summary>
        private void TakeSnapshot(IEnumerable<TLink> linkIndices)
        {
            foreach (var linkIndex in linkIndices)
            {
                var link = _links.GetLink(linkIndex);
                if (link != null && link.Count >= 3)
                {
                    _linkSnapshots[linkIndex] = (link[_links.Constants.SourcePart], link[_links.Constants.TargetPart]);
                }
            }
        }

        /// <summary>
        /// Detects changes in the Links structure since the last snapshot.
        /// </summary>
        public SynchronizationChanges<TLink> DetectLinksChanges()
        {
            var changes = new SynchronizationChanges<TLink>();

            // Check for updates and deletions
            foreach (var snapshot in _linkSnapshots.ToList())
            {
                var linkIndex = snapshot.Key;
                var (oldSource, oldTarget) = snapshot.Value;

                var currentLink = _links.GetLink(linkIndex);

                if (currentLink == null || currentLink.Count < 3)
                {
                    // Link was deleted
                    changes.DeletedLinks.Add(linkIndex);
                    _linkSnapshots.Remove(linkIndex);
                    _deletedLinks.Add(linkIndex);
                }
                else
                {
                    var currentSource = currentLink[_links.Constants.SourcePart];
                    var currentTarget = currentLink[_links.Constants.TargetPart];

                    if (!EqualityComparer<TLink>.Default.Equals(oldSource, currentSource) ||
                        !EqualityComparer<TLink>.Default.Equals(oldTarget, currentTarget))
                    {
                        // Link was updated
                        changes.UpdatedLinks[linkIndex] = (currentSource, currentTarget);
                        _linkSnapshots[linkIndex] = (currentSource, currentTarget);
                    }
                }
            }

            // Check for new links (this would require tracking all links in the substructure)
            // For now, we focus on tracked links

            return changes;
        }

        /// <summary>
        /// Applies changes from SQL table to Links structure.
        /// </summary>
        public void ApplyChangesFromSql(string tableName, DataTable sqlTable)
        {
            if (!_tableMappers.TryGetValue(tableName, out var mapper))
                throw new ArgumentException($"Table '{tableName}' is not registered.", nameof(tableName));

            var currentLinkIndices = new HashSet<TLink>(mapper.SubstructureLinks);

            foreach (DataRow row in sqlTable.Rows)
            {
                var linkIndex = (TLink)row["LinkIndex"];
                var source = (TLink)row["Source"];
                var target = (TLink)row["Target"];

                var existingLink = _links.GetLink(linkIndex);

                if (row.RowState == DataRowState.Added)
                {
                    // New row in SQL - create link if it doesn't exist
                    if (existingLink == null || existingLink.Count < 3)
                    {
                        _links.Update(linkIndex, source, target);
                        _linkSnapshots[linkIndex] = (source, target);
                    }
                }
                else if (row.RowState == DataRowState.Modified)
                {
                    // Modified row in SQL - update link
                    _links.Update(linkIndex, source, target);
                    _linkSnapshots[linkIndex] = (source, target);
                }
                else if (row.RowState == DataRowState.Deleted)
                {
                    // Deleted row in SQL - delete link
                    if (existingLink != null && existingLink.Count >= 3)
                    {
                        _links.Delete(linkIndex);
                        _linkSnapshots.Remove(linkIndex);
                        _deletedLinks.Add(linkIndex);
                    }
                }

                currentLinkIndices.Remove(linkIndex);
            }
        }

        /// <summary>
        /// Applies changes from Links to SQL table.
        /// </summary>
        public void ApplyChangesToSql(string tableName, DataTable sqlTable)
        {
            if (!_tableMappers.TryGetValue(tableName, out var mapper))
                throw new ArgumentException($"Table '{tableName}' is not registered.", nameof(tableName));

            var changes = DetectLinksChanges();

            // Apply updates
            foreach (var update in changes.UpdatedLinks)
            {
                var linkIndex = update.Key;
                var (newSource, newTarget) = update.Value;

                var rows = sqlTable.Select($"LinkIndex = {linkIndex}");
                if (rows.Length > 0)
                {
                    rows[0]["Source"] = newSource;
                    rows[0]["Target"] = newTarget;
                }
            }

            // Apply deletions
            foreach (var deletedLink in changes.DeletedLinks)
            {
                var rows = sqlTable.Select($"LinkIndex = {deletedLink}");
                foreach (var row in rows)
                {
                    row.Delete();
                }
            }

            // Apply insertions
            foreach (var insertedLink in changes.InsertedLinks)
            {
                var link = _links.GetLink(insertedLink);
                if (link != null && link.Count >= 3)
                {
                    var row = sqlTable.NewRow();
                    row["LinkIndex"] = link[_links.Constants.IndexPart];
                    row["Source"] = link[_links.Constants.SourcePart];
                    row["Target"] = link[_links.Constants.TargetPart];
                    sqlTable.Rows.Add(row);
                }
            }
        }

        /// <summary>
        /// Gets the mapper for a specific table.
        /// </summary>
        public SqlTableMapper<TLink> GetMapper(string tableName)
        {
            return _tableMappers.TryGetValue(tableName, out var mapper) ? mapper : null;
        }

        /// <summary>
        /// Gets all registered table names.
        /// </summary>
        public IEnumerable<string> RegisteredTables => _tableMappers.Keys;
    }

    /// <summary>
    /// Represents changes detected during synchronization.
    /// </summary>
    public class SynchronizationChanges<TLink>
    {
        public HashSet<TLink> InsertedLinks { get; } = new HashSet<TLink>();
        public Dictionary<TLink, (TLink Source, TLink Target)> UpdatedLinks { get; } = new Dictionary<TLink, (TLink, TLink)>();
        public HashSet<TLink> DeletedLinks { get; } = new HashSet<TLink>();

        public bool HasChanges => InsertedLinks.Count > 0 || UpdatedLinks.Count > 0 || DeletedLinks.Count > 0;
    }
}
