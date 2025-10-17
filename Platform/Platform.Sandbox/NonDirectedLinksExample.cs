using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Sandbox
{
    /// <summary>
    /// <para>
    /// Demonstrates the usage of non-directed doublets and triplets.
    /// </para>
    /// <para>
    /// This example shows how non-directed links differ from directed links:
    /// - In directed links: (A, B) != (B, A)
    /// - In non-directed links: (A, B) == (B, A)
    /// </para>
    /// </summary>
    public class NonDirectedLinksExample
    {
        /// <summary>
        /// <para>
        /// Demonstrates non-directed doublets behavior.
        /// </para>
        /// </summary>
        public void RunDoubletsExample()
        {
            Console.WriteLine("=== Non-Directed Doublets Example ===\n");

            // Create underlying storage
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);

            // Wrap with non-directed decorator
            var nonDirectedLinks = new NonDirectedDoublets<uint>(links);

            // Create some point links
            var pointA = nonDirectedLinks.Create();
            var pointB = nonDirectedLinks.Create();
            var pointC = nonDirectedLinks.Create();

            Console.WriteLine($"Created points: A={pointA}, B={pointB}, C={pointC}");

            // Create non-directed link between A and B
            var linkAB = nonDirectedLinks.Create();
            linkAB = nonDirectedLinks.Update(linkAB, pointA, pointB);

            Console.WriteLine($"\nCreated non-directed link between A and B: {linkAB}");

            // Search for (A, B) - should find the link
            var found1 = nonDirectedLinks.SearchOrDefault(pointA, pointB);
            Console.WriteLine($"Search for (A, B): {(found1 != default(uint) ? "Found" : "Not found")} - {found1}");

            // Search for (B, A) - in non-directed implementation, this finds the same link
            // because it's stored in normalized form
            var found2 = nonDirectedLinks.SearchOrDefault(pointB, pointA);
            Console.WriteLine($"Search for (B, A): {(found2 != default(uint) ? "Found" : "Not found")} - {found2}");
            Console.WriteLine($"Note: Both searches return the same link in normalized form.\n");

            // Create another non-directed link
            var linkBC = nonDirectedLinks.Create();
            linkBC = nonDirectedLinks.Update(linkBC, pointB, pointC);
            Console.WriteLine($"Created non-directed link between B and C: {linkBC}");

            // Count all links
            var totalLinks = nonDirectedLinks.Count(new uint[] { nonDirectedLinks.Constants.Any });
            Console.WriteLine($"\nTotal links in storage: {totalLinks}");

            // List all links
            Console.WriteLine("\nAll links:");
            nonDirectedLinks.Each(link =>
            {
                Console.WriteLine($"  Link {link[0]}: ({link[1]}, {link[2]})");
                return nonDirectedLinks.Constants.Continue;
            }, new uint[] { nonDirectedLinks.Constants.Any });
        }

        /// <summary>
        /// <para>
        /// Demonstrates the difference between directed and non-directed doublets.
        /// </para>
        /// </summary>
        public void CompareDirectedVsNonDirected()
        {
            Console.WriteLine("\n=== Directed vs Non-Directed Comparison ===\n");

            using var memory1 = new HeapResizableDirectMemory();
            using var directedLinks = new UnitedMemoryLinks<uint>(memory1);

            using var memory2 = new HeapResizableDirectMemory();
            using var innerLinks = new UnitedMemoryLinks<uint>(memory2);
            var nonDirectedLinks = new NonDirectedDoublets<uint>(innerLinks);

            // Create points
            var dirA = directedLinks.Create();
            var dirB = directedLinks.Create();

            var nonDirA = nonDirectedLinks.Create();
            var nonDirB = nonDirectedLinks.Create();

            Console.WriteLine("Created points A and B in both directed and non-directed storage.");

            // In directed links: create (A -> B) and (B -> A) as separate links
            var dirLinkAB = directedLinks.Create();
            dirLinkAB = directedLinks.Update(dirLinkAB, dirA, dirB);

            var dirLinkBA = directedLinks.Create();
            dirLinkBA = directedLinks.Update(dirLinkBA, dirB, dirA);

            Console.WriteLine($"\nDirected links:");
            Console.WriteLine($"  (A -> B) = {dirLinkAB}");
            Console.WriteLine($"  (B -> A) = {dirLinkBA}");
            Console.WriteLine($"  These are two separate links!");

            // In non-directed links: (A, B) and (B, A) are the same
            var nonDirLinkAB = nonDirectedLinks.Create();
            nonDirLinkAB = nonDirectedLinks.Update(nonDirLinkAB, nonDirA, nonDirB);

            Console.WriteLine($"\nNon-directed links:");
            Console.WriteLine($"  (A, B) = {nonDirLinkAB}");
            Console.WriteLine($"  (B, A) would be the same link (stored as normalized form)");
            Console.WriteLine($"  This is a single link representing an undirected connection!");

            // Count comparison
            var directedCount = directedLinks.Count(new uint[] { directedLinks.Constants.Any });
            var nonDirectedCount = nonDirectedLinks.Count(new uint[] { nonDirectedLinks.Constants.Any });

            Console.WriteLine($"\nTotal links in directed storage: {directedCount}");
            Console.WriteLine($"Total links in non-directed storage: {nonDirectedCount}");
            Console.WriteLine($"Non-directed storage is more efficient for undirected relationships!");
        }

        /// <summary>
        /// <para>
        /// Runs all examples.
        /// </para>
        /// </summary>
        public void RunAll()
        {
            RunDoubletsExample();
            CompareDirectedVsNonDirected();

            Console.WriteLine("\n=== Example completed ===");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
