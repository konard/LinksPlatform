using System;
using Platform.Memory;
using Platform.Memory.Experiments;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;

namespace Platform.Experiments
{
    /// <summary>
    /// <para>
    /// Demonstrates the use of PinnedArrayMemory to run Links in restricted .NET environments
    /// that don't allow unmanaged memory allocation (e.g., certain cloud platforms or sandboxed environments).
    /// </para>
    /// <para>
    /// Демонстрирует использование PinnedArrayMemory для запуска Links в ограниченных средах .NET,
    /// которые не позволяют выделение неуправляемой памяти (например, некоторые облачные платформы или изолированные среды).
    /// </para>
    /// </summary>
    public class PinnedArrayMemoryExample
    {
        /// <summary>
        /// <para>
        /// Runs a simple example demonstrating Links operations using only managed memory.
        /// This approach ensures compatibility with restricted .NET environments.
        /// </para>
        /// <para>
        /// Выполняет простой пример, демонстрирующий операции Links с использованием только управляемой памяти.
        /// Этот подход обеспечивает совместимость с ограниченными средами .NET.
        /// </para>
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== Pinned Array Memory Example ===");
            Console.WriteLine("This example demonstrates Links using ONLY managed memory (no unmanaged allocations).");
            Console.WriteLine();

            // Calculate required memory size in elements
            // Each link in UInt64 representation uses 3 UInt64 values (Source, Target, Index)
            // Let's allocate space for 1000 links as an example
            const long linksCapacity = 1000;
            const long elementsPerLink = 3; // This depends on the internal structure of UInt64Links
            const long totalElements = linksCapacity * elementsPerLink;

            try
            {
                // Use PinnedArrayMemory instead of HeapResizableDirectMemory
                // This pins a managed array and exposes it as IDirectMemory
                using (var memory = new PinnedArrayMemory<byte>(totalElements * sizeof(ulong)))
                {
                    Console.WriteLine($"✓ Allocated managed array memory: {memory.Size} bytes");
                    Console.WriteLine($"✓ Array pinned at address: 0x{memory.Pointer:X}");
                    Console.WriteLine();

                    // The PinnedArrayMemory can now be used with UInt64UnitedMemoryLinks
                    // just like HeapResizableDirectMemory, but uses only managed memory
                    using (var memoryManager = new UInt64UnitedMemoryLinks(memory))
                    using (var links = new UInt64Links(memoryManager))
                    {
                        Console.WriteLine("✓ Links initialized with managed memory");
                        Console.WriteLine();

                        // Create some test links
                        var point1 = links.CreatePoint();
                        var point2 = links.CreatePoint();
                        var link = links.CreateAndUpdate(point1, point2);

                        Console.WriteLine($"✓ Created point 1: {point1}");
                        Console.WriteLine($"✓ Created point 2: {point2}");
                        Console.WriteLine($"✓ Created link: {link} (connects {point1} → {point2})");
                        Console.WriteLine();

                        // Verify the link
                        var linkData = links.GetLink(link);
                        Console.WriteLine($"✓ Link verification:");
                        Console.WriteLine($"  - Source: {linkData[links.Constants.SourcePart]}");
                        Console.WriteLine($"  - Target: {linkData[links.Constants.TargetPart]}");
                        Console.WriteLine();

                        Console.WriteLine("=== Success! ===");
                        Console.WriteLine("Links is running with 100% managed memory - no unmanaged allocations!");
                        Console.WriteLine("This allows Links to run in any restricted .NET environment.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
