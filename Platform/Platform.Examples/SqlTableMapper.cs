using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Maps a substructure of Links to a SQL table structure.
    /// Each link's source and target become columns in the table.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class SqlTableMapper<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly string _tableName;
        private readonly HashSet<TLink> _substructureLinks;

        public SqlTableMapper(ILinks<TLink> links, string tableName, IEnumerable<TLink> substructureLinks)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _substructureLinks = new HashSet<TLink>(substructureLinks ?? throw new ArgumentNullException(nameof(substructureLinks)));
        }

        /// <summary>
        /// Creates a DataTable structure that represents the links substructure.
        /// Each row represents a link, with columns for Index, Source, and Target.
        /// </summary>
        public DataTable CreateTableStructure()
        {
            var table = new DataTable(_tableName);

            // Add columns based on the link structure
            table.Columns.Add("LinkIndex", typeof(TLink));
            table.Columns.Add("Source", typeof(TLink));
            table.Columns.Add("Target", typeof(TLink));

            // Set primary key
            table.PrimaryKey = new[] { table.Columns["LinkIndex"] };

            return table;
        }

        /// <summary>
        /// Populates the DataTable with data from the links substructure.
        /// </summary>
        public DataTable PopulateTable()
        {
            var table = CreateTableStructure();

            foreach (var linkIndex in _substructureLinks)
            {
                var link = _links.GetLink(linkIndex);
                if (link != null && link.Count >= 3)
                {
                    var row = table.NewRow();
                    row["LinkIndex"] = link[_links.Constants.IndexPart];
                    row["Source"] = link[_links.Constants.SourcePart];
                    row["Target"] = link[_links.Constants.TargetPart];
                    table.Rows.Add(row);
                }
            }

            return table;
        }

        /// <summary>
        /// Generates SQL CREATE TABLE statement for the links substructure.
        /// </summary>
        public string GenerateCreateTableSql(string dataType = "BIGINT")
        {
            return $@"CREATE TABLE IF NOT EXISTS {_tableName} (
    LinkIndex {dataType} PRIMARY KEY NOT NULL,
    Source {dataType} NOT NULL,
    Target {dataType} NOT NULL
);";
        }

        /// <summary>
        /// Generates SQL INSERT statements for all links in the substructure.
        /// </summary>
        public IEnumerable<string> GenerateInsertStatements()
        {
            foreach (var linkIndex in _substructureLinks)
            {
                var link = _links.GetLink(linkIndex);
                if (link != null && link.Count >= 3)
                {
                    yield return $"INSERT INTO {_tableName} (LinkIndex, Source, Target) VALUES ({link[_links.Constants.IndexPart]}, {link[_links.Constants.SourcePart]}, {link[_links.Constants.TargetPart]});";
                }
            }
        }

        /// <summary>
        /// Generates SQL UPDATE statement for a specific link.
        /// </summary>
        public string GenerateUpdateStatement(TLink linkIndex, TLink newSource, TLink newTarget)
        {
            return $"UPDATE {_tableName} SET Source = {newSource}, Target = {newTarget} WHERE LinkIndex = {linkIndex};";
        }

        /// <summary>
        /// Generates SQL DELETE statement for a specific link.
        /// </summary>
        public string GenerateDeleteStatement(TLink linkIndex)
        {
            return $"DELETE FROM {_tableName} WHERE LinkIndex = {linkIndex};";
        }

        /// <summary>
        /// Gets the table name for this mapper.
        /// </summary>
        public string TableName => _tableName;

        /// <summary>
        /// Gets the substructure links managed by this mapper.
        /// </summary>
        public HashSet<TLink> SubstructureLinks => _substructureLinks;
    }
}
