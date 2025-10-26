using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Converters;

namespace Platform.Examples
{
    /// <summary>
    /// Imports Links structures from SQL DataTable.
    /// Complements SqlExporter for bidirectional synchronization.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class SqlImporter<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly UncheckedConverter<long, TLink> _longToLinkConverter = UncheckedConverter<long, TLink>.Default;

        public SqlImporter(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// Imports links from a DataTable.
        /// Creates or updates links based on the table data.
        /// </summary>
        public ImportResult<TLink> Import(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new ImportResult<TLink>();

            foreach (DataRow row in table.Rows)
            {
                try
                {
                    var linkIndex = ConvertToLink(row["LinkIndex"]);
                    var source = ConvertToLink(row["Source"]);
                    var target = ConvertToLink(row["Target"]);

                    var existingLink = _links.GetLink(linkIndex);

                    if (existingLink == null || existingLink.Count < 3)
                    {
                        // Create new link
                        _links.Update(linkIndex, source, target);
                        result.CreatedLinks.Add(linkIndex);
                    }
                    else
                    {
                        // Update existing link if different
                        var currentSource = existingLink[_links.Constants.SourcePart];
                        var currentTarget = existingLink[_links.Constants.TargetPart];

                        if (!EqualityComparer<TLink>.Default.Equals(currentSource, source) ||
                            !EqualityComparer<TLink>.Default.Equals(currentTarget, target))
                        {
                            _links.Update(linkIndex, source, target);
                            result.UpdatedLinks.Add(linkIndex);
                        }
                        else
                        {
                            result.UnchangedLinks.Add(linkIndex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error importing row: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Imports links from a DataTable with merge strategy.
        /// </summary>
        public ImportResult<TLink> ImportWithMerge(DataTable table, bool deleteUnmatchedLinks = false)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = Import(table);

            if (deleteUnmatchedLinks)
            {
                // Get all link indices from the table
                var tableIndices = new HashSet<TLink>();
                foreach (DataRow row in table.Rows)
                {
                    var linkIndex = ConvertToLink(row["LinkIndex"]);
                    tableIndices.Add(linkIndex);
                }

                // Find and delete links not in the table
                var linksToDelete = new List<TLink>();
                _links.Each(link =>
                {
                    var index = link[_links.Constants.IndexPart];
                    if (!tableIndices.Contains(index))
                    {
                        linksToDelete.Add(index);
                    }
                    return _links.Constants.Continue;
                });

                foreach (var linkToDelete in linksToDelete)
                {
                    try
                    {
                        _links.Delete(linkToDelete);
                        result.DeletedLinks.Add(linkToDelete);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error deleting link {linkToDelete}: {ex.Message}");
                    }
                }
            }

            return result;
        }

        private TLink ConvertToLink(object value)
        {
            if (value is TLink link)
                return link;

            if (value is long longValue)
                return _longToLinkConverter.Convert(longValue);

            if (value is int intValue)
                return _longToLinkConverter.Convert(intValue);

            if (value is ulong ulongValue)
                return _longToLinkConverter.Convert((long)ulongValue);

            if (value is uint uintValue)
                return _longToLinkConverter.Convert(uintValue);

            throw new InvalidCastException($"Cannot convert {value?.GetType().Name ?? "null"} to {typeof(TLink).Name}");
        }
    }

    /// <summary>
    /// Result of an import operation.
    /// </summary>
    public class ImportResult<TLink>
    {
        public HashSet<TLink> CreatedLinks { get; } = new HashSet<TLink>();
        public HashSet<TLink> UpdatedLinks { get; } = new HashSet<TLink>();
        public HashSet<TLink> DeletedLinks { get; } = new HashSet<TLink>();
        public HashSet<TLink> UnchangedLinks { get; } = new HashSet<TLink>();
        public List<string> Errors { get; } = new List<string>();

        public int TotalProcessed => CreatedLinks.Count + UpdatedLinks.Count + DeletedLinks.Count + UnchangedLinks.Count;
        public bool HasErrors => Errors.Count > 0;
        public bool Success => !HasErrors;
    }
}
