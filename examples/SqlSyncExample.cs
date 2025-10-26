using System;
using System.Data;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace LinksPlatform.Examples
{
    /// <summary>
    /// Demonstrates bidirectional synchronization between Links and SQL.
    /// This example shows how to:
    /// 1. Export Links to SQL format
    /// 2. Import Links from SQL DataTable
    /// 3. Synchronize changes bidirectionally
    /// </summary>
    public class SqlSyncExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Links and SQL Synchronization Example ===\n");

            // Use in-memory links storage for demonstration
            using (var memory = new HeapResizableDirectMemory())
            using (var links = new UInt64Links(memory))
            {
                // Step 1: Create some sample links
                Console.WriteLine("Step 1: Creating sample links...");
                var link1 = links.Create();
                var link2 = links.Create();
                var link3 = links.Create();

                link1 = links.Update(link1, link1, link2);
                link2 = links.Update(link2, link1, link3);
                link3 = links.Update(link3, link2, link1);

                Console.WriteLine($"Created {links.Count()} links");
                Console.WriteLine($"  Link {link1}: {links.GetSource(link1)} -> {links.GetTarget(link1)}");
                Console.WriteLine($"  Link {link2}: {links.GetSource(link2)} -> {links.GetTarget(link2)}");
                Console.WriteLine($"  Link {link3}: {links.GetSource(link3)} -> {links.GetTarget(link3)}");
                Console.WriteLine();

                // Step 2: Export links to SQL file
                Console.WriteLine("Step 2: Exporting links to SQL...");
                var exporter = new SqlExporter<ulong>(links, "ExampleLinks");
                exporter.ExportToFile("links_export.sql");
                Console.WriteLine("Exported to: links_export.sql");
                Console.WriteLine();

                // Step 3: Create a SQL table mapper for a substructure
                Console.WriteLine("Step 3: Creating SQL table mapper...");
                var substructureLinks = new[] { link1, link2 };
                var mapper = new SqlTableMapper<ulong>(links, "LinksSubstructure", substructureLinks);

                var dataTable = mapper.PopulateTable();
                Console.WriteLine($"Created DataTable '{mapper.TableName}' with {dataTable.Rows.Count} rows");
                Console.WriteLine();

                // Step 4: Set up synchronization
                Console.WriteLine("Step 4: Setting up synchronization...");
                var synchronizer = new SqlSynchronizer<ulong>(links);
                synchronizer.RegisterTable("LinksSubstructure", substructureLinks);
                Console.WriteLine("Registered table for synchronization");
                Console.WriteLine();

                // Step 5: Modify a link and detect changes
                Console.WriteLine("Step 5: Modifying link and detecting changes...");
                links.Update(link1, link2, link3);  // Change link1's references

                var changes = synchronizer.DetectLinksChanges();
                Console.WriteLine($"Detected {changes.UpdatedLinks.Count} updated links");
                foreach (var update in changes.UpdatedLinks)
                {
                    Console.WriteLine($"  Link {update.Key}: Source={update.Value.Source}, Target={update.Value.Target}");
                }
                Console.WriteLine();

                // Step 6: Apply changes to SQL DataTable
                Console.WriteLine("Step 6: Applying changes to SQL DataTable...");
                synchronizer.ApplyChangesToSql("LinksSubstructure", dataTable);
                Console.WriteLine($"DataTable now has {dataTable.Rows.Count} rows");
                foreach (DataRow row in dataTable.Rows)
                {
                    Console.WriteLine($"  LinkIndex={row["LinkIndex"]}, Source={row["Source"]}, Target={row["Target"]}");
                }
                Console.WriteLine();

                // Step 7: Import from SQL DataTable
                Console.WriteLine("Step 7: Demonstrating import from SQL...");
                var importer = new SqlImporter<ulong>(links);

                // Create a new DataTable to import
                var importTable = mapper.CreateTableStructure();
                var newRow = importTable.NewRow();
                newRow["LinkIndex"] = link3;
                newRow["Source"] = link1;
                newRow["Target"] = link2;
                importTable.Rows.Add(newRow);

                var importResult = importer.Import(importTable);
                Console.WriteLine($"Import result:");
                Console.WriteLine($"  Created: {importResult.CreatedLinks.Count}");
                Console.WriteLine($"  Updated: {importResult.UpdatedLinks.Count}");
                Console.WriteLine($"  Unchanged: {importResult.UnchangedLinks.Count}");
                Console.WriteLine($"  Errors: {importResult.Errors.Count}");
                Console.WriteLine();

                // Step 8: Demonstrate SQL generation
                Console.WriteLine("Step 8: Generating SQL statements...");
                Console.WriteLine("CREATE TABLE:");
                Console.WriteLine(mapper.GenerateCreateTableSql());
                Console.WriteLine("\nINSERT STATEMENTS:");
                foreach (var insert in mapper.GenerateInsertStatements())
                {
                    Console.WriteLine(insert);
                }
                Console.WriteLine();

                Console.WriteLine("=== Example completed successfully! ===");
            }
        }
    }
}
