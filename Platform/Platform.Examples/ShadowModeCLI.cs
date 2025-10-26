using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// Command-line interface for demonstrating shadow mode functionality.
    /// This example shows how to run a Links-based storage in shadow mode,
    /// where it replicates all operations from a primary storage without
    /// being used for reads.
    /// </para>
    /// <para>
    /// Интерфейс командной строки для демонстрации функциональности теневого режима.
    /// Этот пример показывает, как запустить хранилище на основе Links в теневом режиме,
    /// где оно реплицирует все операции из основного хранилища без
    /// использования для чтения.
    /// </para>
    /// </summary>
    public class ShadowModeCLI : ICommandLineInterface
    {
        private const string PrimaryDatabaseFilename = "primary.links";
        private const string ShadowDatabaseFilename = "shadow.links";

        public void Run(params string[] args)
        {
            try
            {
#if DEBUG
                // Clean up any existing databases in debug mode
                File.Delete(PrimaryDatabaseFilename);
                File.Delete(ShadowDatabaseFilename);
#endif

                Console.WriteLine("=== Links Shadow Mode Demo ===");
                Console.WriteLine("This demo runs a Links storage in shadow mode (read-only replica).");
                Console.WriteLine();

                using (var primaryMemoryAdapter = new UInt64UnitedMemoryLinks(PrimaryDatabaseFilename, 1024 * 1024))
                using (var primaryLinks = new UInt64Links(primaryMemoryAdapter))
                using (var shadowMemoryAdapter = new UInt64UnitedMemoryLinks(ShadowDatabaseFilename, 1024 * 1024))
                using (var shadowLinks = new UInt64Links(shadowMemoryAdapter))
                {
                    // Wrap primary and shadow in the shadow mode decorator
                    var shadowModeStorage = new ShadowLinksDecorator<ulong>(
                        primaryLinks,
                        shadowLinks,
                        logger: message => Console.WriteLine($"[Shadow Mode] {message}"),
                        validateConsistency: true
                    );

                    Console.WriteLine("Storages initialized.");
                    Console.WriteLine($"Primary storage: {PrimaryDatabaseFilename}");
                    Console.WriteLine($"Shadow storage: {ShadowDatabaseFilename}");
                    Console.WriteLine();

                    // Demonstrate shadow mode operations
                    DemonstrateOperations(shadowModeStorage, primaryLinks, shadowLinks);
                }

                Console.WriteLine();
                Console.WriteLine("Demo completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void DemonstrateOperations(
            ShadowLinksDecorator<ulong> shadowModeStorage,
            ILinks<ulong> primaryLinks,
            ILinks<ulong> shadowLinks)
        {
            Console.WriteLine("--- Creating Links ---");

            // Create some links through the shadow mode decorator
            var link1 = shadowModeStorage.Create(null);
            Console.WriteLine($"Created link1: {link1}");

            var link2 = shadowModeStorage.Create(null);
            Console.WriteLine($"Created link2: {link2}");

            var link3 = shadowModeStorage.Create(new[] { link1, link2 });
            Console.WriteLine($"Created link3 pointing to link1 and link2: {link3}");

            Console.WriteLine();
            Console.WriteLine("--- Verifying Replication ---");

            // Verify that both storages have the same number of links
            var primaryCount = primaryLinks.Count(null);
            var shadowCount = shadowLinks.Count(null);

            Console.WriteLine($"Primary storage count: {primaryCount}");
            Console.WriteLine($"Shadow storage count: {shadowCount}");

            if (primaryCount == shadowCount)
            {
                Console.WriteLine("SUCCESS: Shadow storage is in sync with primary!");
            }
            else
            {
                Console.WriteLine("WARNING: Shadow storage is out of sync!");
            }

            Console.WriteLine();
            Console.WriteLine("--- Updating Links ---");

            // Update a link
            shadowModeStorage.Update(new[] { link3 }, new[] { link3, link2, link1 });
            Console.WriteLine($"Updated link3 to point to link2 and link1 (reversed)");

            Console.WriteLine();
            Console.WriteLine("--- Verifying Update Replication ---");

            primaryCount = primaryLinks.Count(null);
            shadowCount = shadowLinks.Count(null);

            Console.WriteLine($"Primary storage count: {primaryCount}");
            Console.WriteLine($"Shadow storage count: {shadowCount}");

            Console.WriteLine();
            Console.WriteLine("--- Reading from Storage ---");
            Console.WriteLine("Note: Reads are served from PRIMARY storage only (shadow is replica)");

            // Read operations only use primary storage
            var readCount = shadowModeStorage.Count(null);
            Console.WriteLine($"Total links (read from primary): {readCount}");

            Console.WriteLine();
            Console.WriteLine("--- Deleting Links ---");

            // Delete a link
            shadowModeStorage.Delete(new[] { link1 });
            Console.WriteLine($"Deleted link1: {link1}");

            Console.WriteLine();
            Console.WriteLine("--- Final Verification ---");

            primaryCount = primaryLinks.Count(null);
            shadowCount = shadowLinks.Count(null);

            Console.WriteLine($"Primary storage count: {primaryCount}");
            Console.WriteLine($"Shadow storage count: {shadowCount}");

            if (primaryCount == shadowCount)
            {
                Console.WriteLine("SUCCESS: Shadow storage remained in sync throughout all operations!");
            }
            else
            {
                Console.WriteLine("WARNING: Shadow storage is out of sync!");
            }
        }
    }
}
