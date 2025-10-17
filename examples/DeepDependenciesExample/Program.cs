using System;
using System.Collections.Generic;
using Platform.Data.Doublets.DeepDependencies;

namespace DeepDependenciesExample
{
    /// <summary>
    /// Example demonstrating deep dependencies tracking in LinksPlatform.
    /// This example shows how to track all links used by a link and all links referencing a link.
    /// </summary>
    class Program
    {
        // Simple mock implementation for demonstration
        class MockLinks : Platform.Data.ILinks<ulong>
        {
            private readonly Dictionary<ulong, ulong[]> _links = new Dictionary<ulong, ulong[]>();
            private ulong _nextId = 1;

            public Platform.Data.LinksConstants<ulong> Constants { get; } = new MockLinksConstants();

            public ulong Create()
            {
                var id = _nextId++;
                _links[id] = new ulong[] { id, id, id }; // Index, Source, Target (point to self)
                return id;
            }

            public ulong Create(ulong source, ulong target)
            {
                var id = _nextId++;
                _links[id] = new ulong[] { id, source, target };
                return id;
            }

            public ulong[] GetLink(ulong link)
            {
                return _links.TryGetValue(link, out var value) ? value : null;
            }

            public void Each(Func<ulong[], ulong> handler)
            {
                foreach (var link in _links.Values)
                {
                    if (handler(link) == Constants.Break)
                    {
                        break;
                    }
                }
            }

            public ulong Each(Func<Platform.Data.IList<ulong>, ulong> handler, Platform.Data.IList<ulong> restrictions)
            {
                throw new NotImplementedException();
            }

            public ulong Count(Platform.Data.IList<ulong> restrictions)
            {
                return (ulong)_links.Count;
            }

            public ulong Update(ulong link, ulong newSource, ulong newTarget)
            {
                if (_links.ContainsKey(link))
                {
                    _links[link] = new ulong[] { link, newSource, newTarget };
                }
                return link;
            }

            public void Delete(ulong link)
            {
                _links.Remove(link);
            }
        }

        class MockLinksConstants : Platform.Data.LinksConstants<ulong>
        {
            public override ulong Continue => 1;
            public override ulong Break => 0;
            public override ulong Skip => 2;
            public override int IndexPart => 0;
            public override int SourcePart => 1;
            public override int TargetPart => 2;
            public override ulong Null => 0;
            public override ulong Any => ulong.MaxValue;
            public override bool IsFullPoint(ulong link) => false;
            public override bool IsPartialPoint(ulong link) => false;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== Deep Dependencies Example ===\n");

            // Create a simple links structure
            var links = new MockLinks();

            // Create a chain of dependencies: link1 -> link2 -> link3 -> point
            var point = links.Create(); // Self-referencing point
            Console.WriteLine($"Created point: {point}");

            var link3 = links.Create(point, point);
            Console.WriteLine($"Created link3: {link3} (references point {point})");

            var link2 = links.Create(link3, point);
            Console.WriteLine($"Created link2: {link2} (references link3 {link3} and point {point})");

            var link1 = links.Create(link2, link3);
            Console.WriteLine($"Created link1: {link1} (references link2 {link2} and link3 {link3})");

            // Create an index to track deep dependencies
            var index = new DeepDependenciesIndex<ulong>(links);

            Console.WriteLine("\n=== Computing Deep Dependencies ===\n");

            // Get all links used by link1 (should include link2, link3, and point)
            var usedByLink1 = index.GetUsedByLink(link1);
            Console.WriteLine($"Links used by link1 ({link1}):");
            foreach (var usedLink in usedByLink1)
            {
                Console.WriteLine($"  - Link {usedLink}");
            }

            // Get all links referencing point (should include link3, link2, link1)
            var referencingPoint = index.GetReferencingLink(point);
            Console.WriteLine($"\nLinks referencing point ({point}):");
            foreach (var refLink in referencingPoint)
            {
                Console.WriteLine($"  - Link {refLink}");
            }

            Console.WriteLine("\n=== Using BitString Index ===\n");

            // Demonstrate BitString-based implementation
            var bitIndex = new DeepDependenciesBitStringIndex<ulong>(links, maxLinkIndex: 1000);

            var usedBitArray = bitIndex.GetUsedByLink(link1);
            var usedSet = bitIndex.BitArrayToSet(usedBitArray);
            Console.WriteLine($"Links used by link1 ({link1}) [BitString version]:");
            foreach (var usedLink in usedSet)
            {
                Console.WriteLine($"  - Link {usedLink}");
            }

            Console.WriteLine("\n=== Practical Use Case: Partial Sequence Search ===\n");
            Console.WriteLine("Deep dependencies can be useful for:");
            Console.WriteLine("1. Finding all elements that contribute to a sequence");
            Console.WriteLine("2. Identifying impact of changes (what depends on this link?)");
            Console.WriteLine("3. Efficient graph traversal and caching");
            Console.WriteLine("4. Building dependency graphs for analysis");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
