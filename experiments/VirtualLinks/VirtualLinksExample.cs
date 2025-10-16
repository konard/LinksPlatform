using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Example demonstrating the usage of virtual links.
    ///
    /// This example shows how virtual links can be used to access computed information
    /// about links without physically storing that information in the database.
    /// </summary>
    public class VirtualLinksExample
    {
        /// <summary>
        /// Demonstrates basic virtual link usage.
        /// </summary>
        public static void BasicExample()
        {
            Console.WriteLine("=== Virtual Links Basic Example ===\n");

            // Create a simple in-memory links storage
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);

            // Create some test links
            var link1 = links.Create();
            var link2 = links.Create();
            var link3 = links.CreateAndUpdate(link1, link2);

            Console.WriteLine($"Created links: {link1}, {link2}, {link3}");
            Console.WriteLine();

            // Create virtual link factory
            var factory = new VirtualLinkFactory<ulong>(links);

            // Example 1: Get link index (ID) via virtual link
            var indexVirtualLink = factory.CreateIndexLink(link3);
            Console.WriteLine($"Link 3 Index (via virtual link): {indexVirtualLink.GetValue()}");
            Console.WriteLine($"  Can resolve: {indexVirtualLink.CanResolve()}");
            Console.WriteLine();

            // Example 2: Get link source via virtual link
            var sourceVirtualLink = factory.CreateSourceLink(link3);
            Console.WriteLine($"Link 3 Source (via virtual link): {sourceVirtualLink.GetValue()}");
            Console.WriteLine($"  Expected: {link1}");
            Console.WriteLine();

            // Example 3: Get link target via virtual link
            var targetVirtualLink = factory.CreateTargetLink(link3);
            Console.WriteLine($"Link 3 Target (via virtual link): {targetVirtualLink.GetValue()}");
            Console.WriteLine($"  Expected: {link2}");
            Console.WriteLine();

            // Example 4: Count references to a link
            var refCountVirtualLink = factory.CreateReferenceCountLink(link1);
            Console.WriteLine($"Link 1 Reference Count (via virtual link): {refCountVirtualLink.GetValue()}");
            Console.WriteLine($"  (Links that reference link1 as source or target)");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates virtual link address space usage.
        /// This shows how virtual addresses can be reserved and automatically resolved.
        /// </summary>
        public static void AddressSpaceExample()
        {
            Console.WriteLine("=== Virtual Links Address Space Example ===\n");

            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);

            // Create test links
            var link1 = links.Create();
            var link2 = links.Create();
            var link3 = links.CreateAndUpdate(link1, link2);

            Console.WriteLine($"Real links: {link1}, {link2}, {link3}");
            Console.WriteLine();

            // Create virtual link address space starting at a high address
            // to avoid conflicts with real links
            var factory = new VirtualLinkFactory<ulong>(links);
            var virtualSpace = new VirtualLinkAddressSpace<ulong>(
                factory,
                virtualBase: 1_000_000_000UL,
                virtualLinksPerRealLink: 16
            );

            Console.WriteLine($"Virtual address space base: {virtualSpace.VirtualBase}");
            Console.WriteLine($"Virtual links per real link: {virtualSpace.VirtualLinksPerRealLink}");
            Console.WriteLine();

            // Get virtual addresses for different aspects of link3
            var indexVirtualAddr = virtualSpace.GetVirtualAddress(link3, VirtualLinkType.Index);
            var sourceVirtualAddr = virtualSpace.GetVirtualAddress(link3, VirtualLinkType.Source);
            var targetVirtualAddr = virtualSpace.GetVirtualAddress(link3, VirtualLinkType.Target);

            Console.WriteLine($"Virtual address for Index of link {link3}: {indexVirtualAddr}");
            Console.WriteLine($"Virtual address for Source of link {link3}: {sourceVirtualAddr}");
            Console.WriteLine($"Virtual address for Target of link {link3}: {targetVirtualAddr}");
            Console.WriteLine();

            // Resolve virtual addresses back to virtual links
            Console.WriteLine("Resolving virtual addresses:");

            var resolvedIndex = virtualSpace.ResolveVirtualAddress(indexVirtualAddr);
            if (resolvedIndex.HasValue)
            {
                var (realLink, type) = resolvedIndex.Value;
                Console.WriteLine($"  {indexVirtualAddr} -> Real Link: {realLink}, Type: {type}");

                var virtualLink = virtualSpace.CreateFromAddress(indexVirtualAddr);
                Console.WriteLine($"    Value: {virtualLink.GetValue()}");
            }

            var resolvedSource = virtualSpace.ResolveVirtualAddress(sourceVirtualAddr);
            if (resolvedSource.HasValue)
            {
                var (realLink, type) = resolvedSource.Value;
                Console.WriteLine($"  {sourceVirtualAddr} -> Real Link: {realLink}, Type: {type}");

                var virtualLink = virtualSpace.CreateFromAddress(sourceVirtualAddr);
                Console.WriteLine($"    Value: {virtualLink.GetValue()} (expected: {link1})");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates how virtual links can be useful for triggers.
        /// Virtual links provide a way to access metadata without modifying the core storage.
        /// </summary>
        public static void TriggerExample()
        {
            Console.WriteLine("=== Virtual Links with Triggers Example ===\n");

            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);

            var factory = new VirtualLinkFactory<ulong>(links);

            // Create a simple link structure
            var entityType = links.Create();
            var nameProperty = links.Create();

            var person = links.CreateAndUpdate(entityType, nameProperty);

            Console.WriteLine($"Created entity: {person}");
            Console.WriteLine();

            // Simulate a trigger that needs to access link metadata
            Console.WriteLine("Trigger: When a link is created, log its metadata");
            Console.WriteLine($"  Link ID: {factory.CreateIndexLink(person).GetValue()}");
            Console.WriteLine($"  Link Source: {factory.CreateSourceLink(person).GetValue()}");
            Console.WriteLine($"  Link Target: {factory.CreateTargetLink(person).GetValue()}");
            Console.WriteLine($"  Reference Count: {factory.CreateReferenceCountLink(person).GetValue()}");
            Console.WriteLine();

            // Virtual links enable triggers to access computed information
            // without that information being stored in the database
            Console.WriteLine("Advantage: All this metadata is computed on-demand,");
            Console.WriteLine("           not stored in the database!");
            Console.WriteLine();
        }

        /// <summary>
        /// Runs all examples.
        /// </summary>
        public static void RunAllExamples()
        {
            try
            {
                BasicExample();
                Console.WriteLine(new string('=', 60));
                Console.WriteLine();

                AddressSpaceExample();
                Console.WriteLine(new string('=', 60));
                Console.WriteLine();

                TriggerExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running examples: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
