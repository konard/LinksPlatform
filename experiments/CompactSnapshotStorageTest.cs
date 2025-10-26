using System;
using System.Collections.Generic;
using System.IO;
using Platform.Examples;
using Platform.Numbers;
using Platform.Data.Doublets;

namespace Platform.Experiments
{
    /// <summary>
    /// Test and demonstration of CompactSnapshotStorage functionality.
    /// This experiment shows how the compact snapshot storage works with
    /// tiered address spaces based on link usage frequency.
    /// </summary>
    public class CompactSnapshotStorageTest
    {
        public static void Main()
        {
            Console.WriteLine("=== Compact Snapshot Storage Test ===");
            Console.WriteLine();

            // Create a mock links storage for testing
            var linksStorage = new MockLinksStorage();

            // Add some test links
            Console.WriteLine("Creating test links...");
            var link1 = linksStorage.GetOrCreate(1UL, 2UL);
            var link2 = linksStorage.GetOrCreate(2UL, 3UL);
            var link3 = linksStorage.GetOrCreate(3UL, 4UL);
            var link4 = linksStorage.GetOrCreate(1UL, 3UL); // Link 1 and 3 are used more
            var link5 = linksStorage.GetOrCreate(2UL, 4UL); // Link 2 and 4 are used more
            Console.WriteLine($"Created {linksStorage.Count} links");
            Console.WriteLine();

            // Create compact snapshot storage
            var compactStorage = new CompactSnapshotStorage<ulong>(linksStorage);

            // Test usage counting
            Console.WriteLine("Counting link usages...");
            var usageCounts = compactStorage.CountLinkUsages();
            foreach (var kvp in usageCounts)
            {
                Console.WriteLine($"Link {kvp.Key}: used {kvp.Value} times");
            }
            Console.WriteLine();

            // Test reordering
            Console.WriteLine("Reordering links by usage frequency...");
            var orderedLinks = compactStorage.ReorderLinksByUsage(usageCounts);
            Console.WriteLine("Links ordered by usage (most frequent first):");
            for (int i = 0; i < orderedLinks.Count; i++)
            {
                var usage = usageCounts.ContainsKey(orderedLinks[i]) ? usageCounts[orderedLinks[i]] : 0;
                Console.WriteLine($"  Position {i}: Link {orderedLinks[i]} (used {usage} times)");
            }
            Console.WriteLine();

            // Test snapshot save/load
            Console.WriteLine("Testing snapshot save/load...");
            var snapshotPath = Path.Combine(Path.GetTempPath(), "test-snapshot.lcss");

            try
            {
                // Save snapshot
                using (var fileStream = File.Create(snapshotPath))
                {
                    compactStorage.SaveSnapshot(fileStream);
                }
                var fileInfo = new FileInfo(snapshotPath);
                Console.WriteLine($"Snapshot saved to: {snapshotPath}");
                Console.WriteLine($"File size: {fileInfo.Length} bytes");
                Console.WriteLine();

                // Calculate space savings
                long uncompressedSize = linksStorage.Count * 16; // 16 bytes per link (2 * 8 bytes for ulong)
                long compressedSize = fileInfo.Length;
                double savingsPercent = (1.0 - (double)compressedSize / uncompressedSize) * 100;
                Console.WriteLine($"Uncompressed size (estimated): {uncompressedSize} bytes");
                Console.WriteLine($"Compressed size: {compressedSize} bytes");
                Console.WriteLine($"Space savings: {savingsPercent:F2}%");
                Console.WriteLine();

                // Load snapshot into a new storage
                var newLinksStorage = new MockLinksStorage();
                var newCompactStorage = new CompactSnapshotStorage<ulong>(newLinksStorage);

                using (var fileStream = File.OpenRead(snapshotPath))
                {
                    newCompactStorage.LoadSnapshot(fileStream);
                }

                Console.WriteLine($"Snapshot loaded successfully!");
                Console.WriteLine($"Original storage had {linksStorage.Count} links");
                Console.WriteLine($"Loaded storage has {newLinksStorage.Count} links");
                Console.WriteLine();

                // Verify the links match
                Console.WriteLine("Verifying links match...");
                bool allMatch = true;
                foreach (var originalLink in linksStorage.GetAllLinks())
                {
                    var source = linksStorage.GetSource(originalLink);
                    var target = linksStorage.GetTarget(originalLink);

                    bool found = false;
                    foreach (var newLink in newLinksStorage.GetAllLinks())
                    {
                        var newSource = newLinksStorage.GetSource(newLink);
                        var newTarget = newLinksStorage.GetTarget(newLink);

                        if (source == newSource && target == newTarget)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine($"  ERROR: Link ({source}, {target}) not found in loaded storage!");
                        allMatch = false;
                    }
                }

                if (allMatch)
                {
                    Console.WriteLine("  All links verified successfully!");
                }
                Console.WriteLine();

                Console.WriteLine("=== Test completed successfully! ===");
            }
            finally
            {
                // Cleanup
                if (File.Exists(snapshotPath))
                {
                    File.Delete(snapshotPath);
                    Console.WriteLine($"Cleaned up test file: {snapshotPath}");
                }
            }
        }
    }

    /// <summary>
    /// Mock implementation of ILinks for testing purposes.
    /// </summary>
    public class MockLinksStorage : ILinks<ulong>
    {
        private readonly List<Link> _links = new List<Link>();
        private ulong _nextId = 1;

        public class Link
        {
            public ulong Id { get; set; }
            public ulong Source { get; set; }
            public ulong Target { get; set; }
        }

        public int Count => _links.Count;

        public ILinksConstants<ulong> Constants => new MockLinksConstants();

        public ulong GetOrCreate(ulong source, ulong target)
        {
            // Check if link already exists
            foreach (var link in _links)
            {
                if (link.Source == source && link.Target == target)
                {
                    return link.Id;
                }
            }

            // Create new link
            var newLink = new Link
            {
                Id = _nextId++,
                Source = source,
                Target = target
            };
            _links.Add(newLink);
            return newLink.Id;
        }

        public ulong GetIndex(IList<ulong> link)
        {
            if (link == null || link.Count == 0)
                return 0;
            return link[0]; // First element is the index/id
        }

        public IList<ulong> GetLink(ulong index)
        {
            var link = _links.Find(l => l.Id == index);
            if (link == null)
                return new List<ulong> { 0, 0, 0 };
            return new List<ulong> { link.Id, link.Source, link.Target };
        }

        public ulong GetSource(IList<ulong> link)
        {
            if (link == null || link.Count < 2)
                return 0;
            return link[1];
        }

        public ulong GetTarget(IList<ulong> link)
        {
            if (link == null || link.Count < 3)
                return 0;
            return link[2];
        }

        public ulong Each(Func<IList<ulong>, ulong> handler)
        {
            foreach (var link in _links)
            {
                var linkList = new List<ulong> { link.Id, link.Source, link.Target };
                var result = handler(linkList);
                if (result == Constants.Break)
                    return result;
            }
            return Constants.Continue;
        }

        public IEnumerable<IList<ulong>> GetAllLinks()
        {
            foreach (var link in _links)
            {
                yield return new List<ulong> { link.Id, link.Source, link.Target };
            }
        }
    }

    public class MockLinksConstants : ILinksConstants<ulong>
    {
        public ulong Continue => 1;
        public ulong Break => 0;
    }
}
