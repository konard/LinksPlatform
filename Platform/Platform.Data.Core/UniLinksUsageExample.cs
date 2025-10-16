using System;
using System.Collections.Generic;

namespace Platform.Data.Core
{
    /// <summary>
    /// Example demonstrating the simplicity of the unified In/Out operations
    /// compared to traditional CRUD operations.
    ///
    /// Key insight: All data manipulation can be reduced to just two operations:
    /// - In: Write/modify data (covers Create, Update, Delete)
    /// - Out: Read/query data (covers Read, Search, Query)
    ///
    /// This unification simplifies the API and makes it more consistent.
    /// </summary>
    public class UniLinksUsageExample
    {
        /// <summary>
        /// Demonstrates how the wrapper brings back traditional CRUD operations
        /// while internally using the unified In/Out approach.
        /// </summary>
        public static void DemonstrateCRUDWrapper()
        {
            // Create a mock implementation for demonstration
            var mockLinks = new MockUniLinksIO();
            var wrapper = new UniLinksWrapper<ulong>(mockLinks);

            // Traditional CRUD operations now available through wrapper

            // Create: wrapper around In(null, parts)
            var linkId = wrapper.Create(new ulong[] { 0, 1, 2 });
            Console.WriteLine($"Created link: {linkId}");

            // Read: wrapper around Out(handler, pattern)
            var source = wrapper.Read(1, linkId);
            Console.WriteLine($"Read source: {source}");

            // Update: wrapper around In(before, after)
            var updatedId = wrapper.Update(new ulong[] { linkId }, new ulong[] { linkId, 5, 6 });
            Console.WriteLine($"Updated link: {updatedId}");

            // Delete: wrapper around In(parts, null)
            wrapper.Delete(new ulong[] { linkId });
            Console.WriteLine($"Deleted link: {linkId}");
        }

        /// <summary>
        /// Demonstrates direct usage of In/Out operations,
        /// showing the simplicity of the unified approach.
        /// </summary>
        public static void DemonstrateUnifiedOperations()
        {
            var links = new MockUniLinksIO();

            // Create: In(null, parts)
            var linkId = links.In(null, new ulong[] { 0, 1, 2 });

            // Read: Out(handler, pattern)
            links.Out(parts =>
            {
                Console.WriteLine($"Link: [{parts[0]}] {parts[1]} -> {parts[2]}");
                return true; // continue
            }, linkId);

            // Update: In(before, after)
            links.In(new ulong[] { linkId }, new ulong[] { linkId, 3, 4 });

            // Delete: In(parts, null)
            links.In(new ulong[] { linkId }, null);
        }

        /// <summary>
        /// Shows how extension methods provide semantic sugar
        /// while maintaining the simplicity of In/Out.
        /// </summary>
        public static void DemonstrateExtensionMethods()
        {
            var links = new MockUniLinksIO();

            // Semantic operations as extension methods with explicit type specification
            var linkId = links.CreateLink<ulong>(1, 2);
            var source = links.GetSource<ulong>(linkId);
            var target = links.GetTarget<ulong>(linkId);
            links.UpdateLink<ulong>(linkId, 3, 4);
            links.DeleteLink<ulong>(linkId);
        }

        /// <summary>
        /// Compares the code complexity between traditional and unified approaches.
        /// </summary>
        public static void ShowSimplicityComparison()
        {
            Console.WriteLine("Traditional approach requires 4+ operations:");
            Console.WriteLine("  - Create(parts)");
            Console.WriteLine("  - Read(id)");
            Console.WriteLine("  - Update(id, parts)");
            Console.WriteLine("  - Delete(id)");
            Console.WriteLine();
            Console.WriteLine("Unified approach requires only 2 operations:");
            Console.WriteLine("  - In(before, after)  // Handles Create, Update, Delete");
            Console.WriteLine("  - Out(handler, pattern)  // Handles Read, Query, Search");
            Console.WriteLine();
            Console.WriteLine("This reduction from 4+ to 2 operations demonstrates");
            Console.WriteLine("how Links simplicity influences the code.");
        }
    }

    /// <summary>
    /// Mock implementation for demonstration purposes.
    /// In a real system, this would be implemented by the actual links storage.
    /// </summary>
    internal class MockUniLinksIO : IUniLinksIO<ulong>
    {
        private readonly Dictionary<ulong, ulong[]> _storage = new Dictionary<ulong, ulong[]>();
        private ulong _nextId = 1;

        public bool Out(Func<ulong[], bool> handler, params ulong[] pattern)
        {
            if (pattern == null || pattern.Length == 0)
            {
                // Query all links
                foreach (var link in _storage.Values)
                {
                    if (!handler(link))
                        return false;
                }
                return true;
            }
            else if (pattern.Length == 1 && _storage.TryGetValue(pattern[0], out var link))
            {
                // Query specific link
                return handler(link);
            }
            return true;
        }

        public ulong In(ulong[] before, ulong[] after)
        {
            if (before == null && after != null)
            {
                // Create
                var id = _nextId++;
                after[0] = id; // Set the id
                _storage[id] = after;
                return id;
            }
            else if (before != null && after != null)
            {
                // Update
                var id = before[0];
                if (_storage.ContainsKey(id))
                {
                    _storage[id] = after;
                }
                return id;
            }
            else if (before != null && after == null)
            {
                // Delete
                var id = before[0];
                _storage.Remove(id);
                return default;
            }
            return default;
        }
    }
}
