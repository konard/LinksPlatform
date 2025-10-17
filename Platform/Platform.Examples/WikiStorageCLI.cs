using System;
using Platform.IO;
using Platform.Memory;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating WikiStorage capabilities.
    /// Shows how Links Platform can replace traditional databases in Wikipedia-like applications.
    /// </summary>
    public class WikiStorageCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                Console.WriteLine("=== Wikipedia-like Storage Example using Links Platform ===");
                Console.WriteLine();
                Console.WriteLine("This example demonstrates how Links Platform can be used as a database");
                Console.WriteLine("replacement for Wikipedia-like wiki applications, storing:");
                Console.WriteLine("  - Pages with titles");
                Console.WriteLine("  - Revisions with content, authors, and timestamps");
                Console.WriteLine("  - Links between pages");
                Console.WriteLine("  - Categories and page categorization");
                Console.WriteLine();

                // Initialize storage
                using (var memory = new HeapResizableDirectMemory())
                using (var links = new UnitedMemoryLinks<ulong>(memory))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var unicodeMap = new UnicodeMap(syncLinks);
                    unicodeMap.Init();
                    var sequences = new Sequences(syncLinks, new SequencesOptions<ulong> { UseSequenceMarker = true, UseCompression = false });
                    var wiki = new WikiStorage(syncLinks, sequences);

                    // Create sample pages
                    Console.WriteLine("Creating sample wiki pages...");
                    var linksPlatformPage = wiki.CreatePage("Links Platform");
                    var associativeModelPage = wiki.CreatePage("Associative Model");
                    var databasesPage = wiki.CreatePage("Databases");

                    Console.WriteLine($"  - Created page: Links Platform (ID: {linksPlatformPage})");
                    Console.WriteLine($"  - Created page: Associative Model (ID: {associativeModelPage})");
                    Console.WriteLine($"  - Created page: Databases (ID: {databasesPage})");
                    Console.WriteLine();

                    // Create revisions
                    Console.WriteLine("Creating page revisions...");
                    var revision1 = wiki.CreateRevision(
                        linksPlatformPage,
                        "Links Platform is an associative data storage system.",
                        "Alice",
                        DateTime.Now.AddDays(-10)
                    );
                    var revision2 = wiki.CreateRevision(
                        linksPlatformPage,
                        "Links Platform is an associative data storage system that uses doublets to store information.",
                        "Bob",
                        DateTime.Now.AddDays(-5)
                    );
                    var revision3 = wiki.CreateRevision(
                        linksPlatformPage,
                        "Links Platform is an associative data storage system that uses doublets to store information efficiently.",
                        "Alice",
                        DateTime.Now
                    );

                    Console.WriteLine($"  - Created 3 revisions for 'Links Platform' page");
                    Console.WriteLine();

                    // Create links between pages
                    Console.WriteLine("Creating inter-page links...");
                    wiki.CreatePageLink(linksPlatformPage, associativeModelPage);
                    wiki.CreatePageLink(linksPlatformPage, databasesPage);
                    wiki.CreatePageLink(associativeModelPage, databasesPage);

                    Console.WriteLine("  - Links Platform -> Associative Model");
                    Console.WriteLine("  - Links Platform -> Databases");
                    Console.WriteLine("  - Associative Model -> Databases");
                    Console.WriteLine();

                    // Create categories
                    Console.WriteLine("Creating categories...");
                    var computerScienceCategory = wiki.CreateCategory("Computer Science");
                    var dataStorageCategory = wiki.CreateCategory("Data Storage");

                    wiki.AddPageToCategory(linksPlatformPage, computerScienceCategory);
                    wiki.AddPageToCategory(linksPlatformPage, dataStorageCategory);
                    wiki.AddPageToCategory(associativeModelPage, computerScienceCategory);
                    wiki.AddPageToCategory(databasesPage, dataStorageCategory);

                    Console.WriteLine("  - Category: Computer Science");
                    Console.WriteLine("  - Category: Data Storage");
                    Console.WriteLine();

                    // Demonstrate queries
                    Console.WriteLine("=== Query Examples ===");
                    Console.WriteLine();

                    Console.WriteLine("1. Retrieving page title:");
                    var title = wiki.GetPageTitle(linksPlatformPage);
                    Console.WriteLine($"   Page {linksPlatformPage} title: {title}");
                    Console.WriteLine();

                    Console.WriteLine("2. Getting revision history:");
                    var revisions = wiki.GetPageRevisions(linksPlatformPage);
                    Console.WriteLine($"   Found {revisions.Count} revisions for 'Links Platform'");
                    Console.WriteLine();

                    Console.WriteLine("3. Finding backlinks:");
                    var backlinks = wiki.GetBacklinks(databasesPage);
                    Console.WriteLine($"   Pages linking to 'Databases': {backlinks.Count}");
                    Console.WriteLine();

                    Console.WriteLine("4. Listing pages in category:");
                    var csPages = wiki.GetPagesInCategory(computerScienceCategory);
                    Console.WriteLine($"   Pages in 'Computer Science': {csPages.Count}");
                    Console.WriteLine();

                    // Print statistics
                    Console.WriteLine("=== Storage Statistics ===");
                    wiki.PrintStatistics();
                    Console.WriteLine();

                    Console.WriteLine("=== Benefits of Links Platform for Wiki Storage ===");
                    Console.WriteLine("  ✓ Flexible schema: Add new properties without migrations");
                    Console.WriteLine("  ✓ Natural graph structure: Pages and links are native entities");
                    Console.WriteLine("  ✓ Efficient relationships: Direct link references instead of JOINs");
                    Console.WriteLine("  ✓ Version history: All revisions stored as linked structures");
                    Console.WriteLine("  ✓ Fast traversal: Follow links directly without index lookups");
                    Console.WriteLine();

                    Console.WriteLine("Example completed successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
