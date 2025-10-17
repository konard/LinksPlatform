using System;
using System.Collections.Generic;

namespace LinksPlatform.Experiments
{
    /// <summary>
    /// Demonstration of two-matrix index structure for links as described in issue #526.
    ///
    /// The first matrix maps link addresses to their sources.
    /// The second matrix maps link addresses to their targets.
    ///
    /// This approach provides O(1) lookup time for finding a link's source or target
    /// given its address, compared to O(n) traversal in linked-list based implementations.
    /// </summary>
    public class MatrixIndicesDemo
    {
        /// <summary>
        /// First matrix: Maps link address to link's source
        /// Key: Link Address, Value: Source Address
        /// </summary>
        private Dictionary<ulong, ulong> AddressToSource { get; set; }

        /// <summary>
        /// Second matrix: Maps link address to link's target
        /// Key: Link Address, Value: Target Address
        /// </summary>
        private Dictionary<ulong, ulong> AddressToTarget { get; set; }

        /// <summary>
        /// Counter for generating unique link addresses
        /// </summary>
        private ulong NextAddress { get; set; }

        /// <summary>
        /// Represents a link in the associative memory
        /// </summary>
        public class Link
        {
            public ulong Address { get; set; }
            public ulong Source { get; set; }
            public ulong Target { get; set; }

            public override string ToString()
            {
                return $"Link[{Address}]: {Source} -> {Target}";
            }
        }

        public MatrixIndicesDemo()
        {
            AddressToSource = new Dictionary<ulong, ulong>();
            AddressToTarget = new Dictionary<ulong, ulong>();
            NextAddress = 1; // Start addresses from 1 (0 can represent null/empty)
        }

        /// <summary>
        /// Creates a new link with the specified source and target
        /// </summary>
        /// <param name="source">Address of the source link</param>
        /// <param name="target">Address of the target link</param>
        /// <returns>Address of the newly created link</returns>
        public ulong CreateLink(ulong source, ulong target)
        {
            ulong address = NextAddress++;

            // Store in the two matrices
            AddressToSource[address] = source;
            AddressToTarget[address] = target;

            return address;
        }

        /// <summary>
        /// Gets the source of a link given its address
        /// O(1) lookup time using the first matrix
        /// </summary>
        /// <param name="address">Link address</param>
        /// <returns>Source address, or 0 if link doesn't exist</returns>
        public ulong GetSource(ulong address)
        {
            return AddressToSource.TryGetValue(address, out ulong source) ? source : 0;
        }

        /// <summary>
        /// Gets the target of a link given its address
        /// O(1) lookup time using the second matrix
        /// </summary>
        /// <param name="address">Link address</param>
        /// <returns>Target address, or 0 if link doesn't exist</returns>
        public ulong GetTarget(ulong address)
        {
            return AddressToTarget.TryGetValue(address, out ulong target) ? target : 0;
        }

        /// <summary>
        /// Gets complete link information
        /// </summary>
        /// <param name="address">Link address</param>
        /// <returns>Link object or null if not found</returns>
        public Link GetLink(ulong address)
        {
            if (AddressToSource.ContainsKey(address))
            {
                return new Link
                {
                    Address = address,
                    Source = AddressToSource[address],
                    Target = AddressToTarget[address]
                };
            }
            return null;
        }

        /// <summary>
        /// Updates the source of an existing link
        /// </summary>
        /// <param name="address">Link address</param>
        /// <param name="newSource">New source address</param>
        /// <returns>True if successful, false if link doesn't exist</returns>
        public bool UpdateSource(ulong address, ulong newSource)
        {
            if (AddressToSource.ContainsKey(address))
            {
                AddressToSource[address] = newSource;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the target of an existing link
        /// </summary>
        /// <param name="address">Link address</param>
        /// <param name="newTarget">New target address</param>
        /// <returns>True if successful, false if link doesn't exist</returns>
        public bool UpdateTarget(ulong address, ulong newTarget)
        {
            if (AddressToTarget.ContainsKey(address))
            {
                AddressToTarget[address] = newTarget;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a link
        /// </summary>
        /// <param name="address">Link address to delete</param>
        /// <returns>True if successful, false if link doesn't exist</returns>
        public bool DeleteLink(ulong address)
        {
            bool sourceRemoved = AddressToSource.Remove(address);
            bool targetRemoved = AddressToTarget.Remove(address);
            return sourceRemoved && targetRemoved;
        }

        /// <summary>
        /// Finds all links with a specific source
        /// Note: This requires scanning the first matrix
        /// For frequent queries, consider adding a reverse index
        /// </summary>
        /// <param name="source">Source address to search for</param>
        /// <returns>List of link addresses that have this source</returns>
        public List<ulong> FindBySource(ulong source)
        {
            var result = new List<ulong>();
            foreach (var kvp in AddressToSource)
            {
                if (kvp.Value == source)
                {
                    result.Add(kvp.Key);
                }
            }
            return result;
        }

        /// <summary>
        /// Finds all links with a specific target
        /// Note: This requires scanning the second matrix
        /// For frequent queries, consider adding a reverse index
        /// </summary>
        /// <param name="target">Target address to search for</param>
        /// <returns>List of link addresses that have this target</returns>
        public List<ulong> FindByTarget(ulong target)
        {
            var result = new List<ulong>();
            foreach (var kvp in AddressToTarget)
            {
                if (kvp.Value == target)
                {
                    result.Add(kvp.Key);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the total number of links in the system
        /// </summary>
        public int Count => AddressToSource.Count;

        /// <summary>
        /// Demonstration of the matrix indices functionality
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Matrix Indices for Links - Demonstration ===\n");

            var demo = new MatrixIndicesDemo();

            // Create some self-referential points (links pointing to themselves)
            Console.WriteLine("Creating self-referential links (points):");
            ulong point1 = demo.CreateLink(0, 0); // Will be updated to point to itself
            demo.UpdateSource(point1, point1);
            demo.UpdateTarget(point1, point1);
            Console.WriteLine($"Point 1: {demo.GetLink(point1)}");

            ulong point2 = demo.CreateLink(0, 0);
            demo.UpdateSource(point2, point2);
            demo.UpdateTarget(point2, point2);
            Console.WriteLine($"Point 2: {demo.GetLink(point2)}");

            // Create links between the points
            Console.WriteLine("\nCreating links between points:");
            ulong link1 = demo.CreateLink(point1, point2);
            Console.WriteLine($"Link 1: {demo.GetLink(link1)}");

            ulong link2 = demo.CreateLink(point2, point1);
            Console.WriteLine($"Link 2: {demo.GetLink(link2)}");

            // Create a link pointing to another link
            Console.WriteLine("\nCreating a link pointing to another link:");
            ulong metalink = demo.CreateLink(link1, link2);
            Console.WriteLine($"Meta-link: {demo.GetLink(metalink)}");

            // Demonstrate O(1) lookups
            Console.WriteLine("\n=== Demonstrating O(1) lookups ===");
            Console.WriteLine($"Source of link {metalink}: {demo.GetSource(metalink)}");
            Console.WriteLine($"Target of link {metalink}: {demo.GetTarget(metalink)}");

            // Demonstrate reverse lookups (finding all links with specific source/target)
            Console.WriteLine("\n=== Reverse lookups ===");
            Console.WriteLine($"All links with source={point1}:");
            foreach (var addr in demo.FindBySource(point1))
            {
                Console.WriteLine($"  {demo.GetLink(addr)}");
            }

            Console.WriteLine($"\nAll links with target={point1}:");
            foreach (var addr in demo.FindByTarget(point1))
            {
                Console.WriteLine($"  {demo.GetLink(addr)}");
            }

            Console.WriteLine($"\nTotal links in system: {demo.Count}");

            // Demonstrate update operations
            Console.WriteLine("\n=== Update operations ===");
            Console.WriteLine($"Before update: {demo.GetLink(link1)}");
            demo.UpdateTarget(link1, metalink);
            Console.WriteLine($"After updating target: {demo.GetLink(link1)}");

            // Demonstrate deletion
            Console.WriteLine("\n=== Deletion ===");
            Console.WriteLine($"Deleting link {link2}...");
            demo.DeleteLink(link2);
            Console.WriteLine($"Link exists? {demo.GetLink(link2) != null}");
            Console.WriteLine($"Total links in system: {demo.Count}");

            Console.WriteLine("\n=== Matrix Structure Analysis ===");
            Console.WriteLine($"Matrix 1 (Address->Source) size: {demo.AddressToSource.Count} entries");
            Console.WriteLine($"Matrix 2 (Address->Target) size: {demo.AddressToTarget.Count} entries");
            Console.WriteLine("\nBoth matrices provide O(1) access time for address-based lookups.");
            Console.WriteLine("This is an improvement over linked-list traversal which requires O(n) time.");
        }
    }
}
