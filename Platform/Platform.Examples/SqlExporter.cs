using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Exports Links structures to SQL format (SQLite compatible).
    /// Similar to CSVExporter but generates SQL statements.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class SqlExporter<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly string _tableName;

        public SqlExporter(ILinks<TLink> links, string tableName = "Links")
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _tableName = tableName ?? "Links";
        }

        /// <summary>
        /// Exports all links to a SQL file.
        /// </summary>
        public void ExportToFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                WriteCreateTable(writer);
                WriteInserts(writer);
            }
        }

        /// <summary>
        /// Exports specific links to a SQL file.
        /// </summary>
        public void ExportToFile(string filePath, IEnumerable<TLink> linkIndices)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                WriteCreateTable(writer);
                WriteInserts(writer, linkIndices);
            }
        }

        /// <summary>
        /// Generates SQL statements as a string.
        /// </summary>
        public string GenerateSql()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                WriteCreateTable(writer);
                WriteInserts(writer);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates SQL statements for specific links.
        /// </summary>
        public string GenerateSql(IEnumerable<TLink> linkIndices)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                WriteCreateTable(writer);
                WriteInserts(writer, linkIndices);
            }
            return sb.ToString();
        }

        private void WriteCreateTable(TextWriter writer)
        {
            writer.WriteLine($"-- Links table structure");
            writer.WriteLine($"CREATE TABLE IF NOT EXISTS {_tableName} (");
            writer.WriteLine("    LinkIndex INTEGER PRIMARY KEY NOT NULL,");
            writer.WriteLine("    Source INTEGER NOT NULL,");
            writer.WriteLine("    Target INTEGER NOT NULL");
            writer.WriteLine(");");
            writer.WriteLine();
            writer.WriteLine($"-- Clear existing data");
            writer.WriteLine($"DELETE FROM {_tableName};");
            writer.WriteLine();
        }

        private void WriteInserts(TextWriter writer)
        {
            writer.WriteLine($"-- Insert links data");
            writer.WriteLine("BEGIN TRANSACTION;");

            _links.Each(link =>
            {
                var index = link[_links.Constants.IndexPart];
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                writer.WriteLine($"INSERT INTO {_tableName} (LinkIndex, Source, Target) VALUES ({index}, {source}, {target});");

                return _links.Constants.Continue;
            });

            writer.WriteLine("COMMIT;");
        }

        private void WriteInserts(TextWriter writer, IEnumerable<TLink> linkIndices)
        {
            writer.WriteLine($"-- Insert links data");
            writer.WriteLine("BEGIN TRANSACTION;");

            foreach (var linkIndex in linkIndices)
            {
                var link = _links.GetLink(linkIndex);
                if (link != null && link.Count >= 3)
                {
                    var index = link[_links.Constants.IndexPart];
                    var source = link[_links.Constants.SourcePart];
                    var target = link[_links.Constants.TargetPart];

                    writer.WriteLine($"INSERT INTO {_tableName} (LinkIndex, Source, Target) VALUES ({index}, {source}, {target});");
                }
            }

            writer.WriteLine("COMMIT;");
        }
    }
}
