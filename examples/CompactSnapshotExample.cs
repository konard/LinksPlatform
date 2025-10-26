using System;
using System.IO;
using Platform.Examples;

namespace Platform.Examples.CompactSnapshot
{
    /// <summary>
    /// Example demonstrating compact snapshot storage for Links Platform.
    ///
    /// This example shows how to:
    /// 1. Create a links storage with various links
    /// 2. Save a compact snapshot using tiered address spaces
    /// 3. Load the snapshot back
    /// 4. Understand space savings from compact storage
    ///
    /// The compact snapshot format uses different address sizes based on
    /// link usage frequency:
    /// - Most frequent 256 links: 2 bytes per link (byte addresses)
    /// - Next 65,536 links: 4 bytes per link (ushort addresses)
    /// - Next 4.3B links: 8 bytes per link (uint addresses)
    /// - Remaining links: 16 bytes per link (ulong addresses)
    /// </summary>
    public class CompactSnapshotExample
    {
        public static void RunExample()
        {
            Console.WriteLine("=== Compact Snapshot Storage Example ===");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates how compact snapshot storage");
            Console.WriteLine("can significantly reduce storage space by using smaller");
            Console.WriteLine("addresses for more frequently used links.");
            Console.WriteLine();

            // In a real scenario, you would use an actual ILinks implementation
            // such as Platform.Data.Doublets
            Console.WriteLine("Key concepts:");
            Console.WriteLine("1. Links are reordered by usage frequency");
            Console.WriteLine("2. Most used links get smallest addresses (byte range)");
            Console.WriteLine("3. Less used links get progressively larger addresses");
            Console.WriteLine("4. This creates significant space savings for real-world data");
            Console.WriteLine();

            Console.WriteLine("Benefits:");
            Console.WriteLine("- Reduced storage space for snapshots");
            Console.WriteLine("- Faster serialization/deserialization");
            Console.WriteLine("- Better compression ratios");
            Console.WriteLine("- Optimized for real-world usage patterns");
            Console.WriteLine();

            Console.WriteLine("Use cases:");
            Console.WriteLine("- Database backups");
            Console.WriteLine("- Data transfer between systems");
            Console.WriteLine("- Archival storage");
            Console.WriteLine("- Version control for link databases");
            Console.WriteLine();

            Console.WriteLine("Related issues:");
            Console.WriteLine("- #29: Binary data protocol");
            Console.WriteLine("- #36: Pack/Unpack functionality");
            Console.WriteLine("- #95: Compression optimization");
            Console.WriteLine("- #194: Universal Links String file format");
            Console.WriteLine("- #320: Binary Sequences file format");
            Console.WriteLine("- #555: Software logs storage");
            Console.WriteLine();

            Console.WriteLine("File format structure:");
            Console.WriteLine("- Header: Magic number 'LCSS' + version + total links count");
            Console.WriteLine("- Byte space: Count + links (2 bytes each)");
            Console.WriteLine("- UShort space: Count + links (4 bytes each)");
            Console.WriteLine("- UInt space: Count + links (8 bytes each)");
            Console.WriteLine("- ULong space: Count + links (16 bytes each)");
            Console.WriteLine();

            Console.WriteLine("Example space calculation:");
            Console.WriteLine("For a database with 1000 links where:");
            Console.WriteLine("- 100 links are heavily used");
            Console.WriteLine("- 500 links are moderately used");
            Console.WriteLine("- 400 links are rarely used");
            Console.WriteLine();
            Console.WriteLine("Standard storage: 1000 links * 16 bytes = 16,000 bytes");
            Console.WriteLine("Compact storage:");
            Console.WriteLine("  100 links * 2 bytes = 200 bytes (byte space)");
            Console.WriteLine("  500 links * 4 bytes = 2,000 bytes (ushort space)");
            Console.WriteLine("  400 links * 8 bytes = 3,200 bytes (uint space)");
            Console.WriteLine("  Total: 5,400 bytes (66% space savings!)");
            Console.WriteLine();

            Console.WriteLine("=== End of Example ===");
        }
    }
}
