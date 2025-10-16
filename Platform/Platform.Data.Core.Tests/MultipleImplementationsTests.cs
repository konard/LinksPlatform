using System;
using System.Collections.Generic;
using Xunit;
using Platform.Data.Core.Pairs;

namespace Platform.Data.Core.Tests
{
    /// <summary>
    /// Tests that demonstrate how to test multiple ILinksMemoryManager implementations
    /// in a unified way. This allows easy comparison and verification that different
    /// memory management strategies (GC, Memory-Mapped Files, etc.) all behave correctly.
    /// </summary>
    public class MultipleImplementationsTests
    {
        /// <summary>
        /// Gets a collection of memory manager factories for testing.
        /// This can be extended to include Memory-Mapped Files and other implementations.
        /// </summary>
        public static IEnumerable<object[]> GetMemoryManagerFactories()
        {
            // GC-based implementation
            yield return new object[]
            {
                "GC-based (Links2)",
                (Func<ILinksMemoryManager<ulong>>)(() => new Links2<ulong>())
            };

            // Future implementations can be added here:
            // yield return new object[]
            // {
            //     "Memory-Mapped Files",
            //     (Func<ILinksMemoryManager<ulong>>)(() => new MemoryMappedLinks<ulong>("test.db"))
            // };
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldAllocateLinks(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();

            // Act
            var link = links.Allocate();

            // Assert
            Assert.NotEqual(0UL, link);
            Assert.True(links.IsAllocated(link));
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldFreeLinks(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link = links.Allocate();

            // Act
            links.Free(link);

            // Assert
            Assert.False(links.IsAllocated(link));
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldSetAndGetSource(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link = links.Allocate();
            var source = links.Allocate();

            // Act
            links.SetSource(link, source);
            var retrievedSource = links.GetSource(link);

            // Assert
            Assert.Equal(source, retrievedSource);
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldSetAndGetTarget(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link = links.Allocate();
            var target = links.Allocate();

            // Act
            links.SetTarget(link, target);
            var retrievedTarget = links.GetTarget(link);

            // Assert
            Assert.Equal(target, retrievedTarget);
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldReuseFreedLinks(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link1 = links.Allocate();
            links.Free(link1);

            // Act
            var link2 = links.Allocate();

            // Assert
            Assert.Equal(link1, link2);
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldHandleMultipleAllocations(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            const int count = 100;
            var allocatedLinks = new List<ulong>();

            // Act
            for (int i = 0; i < count; i++)
            {
                allocatedLinks.Add(links.Allocate());
            }

            // Assert
            Assert.Equal(count, allocatedLinks.Count);
            foreach (var link in allocatedLinks)
            {
                Assert.True(links.IsAllocated(link));
            }
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldCreateValidPairs(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var source = links.Allocate();
            var target = links.Allocate();
            var pair = links.Allocate();

            // Act
            links.SetSource(pair, source);
            links.SetTarget(pair, target);

            // Assert
            Assert.Equal(source, links.GetSource(pair));
            Assert.Equal(target, links.GetTarget(pair));
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldSupportSelfReferences(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link = links.Allocate();

            // Act - Create a self-referencing link
            links.SetSource(link, link);
            links.SetTarget(link, link);

            // Assert
            Assert.Equal(link, links.GetSource(link));
            Assert.Equal(link, links.GetTarget(link));
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldHandleCapacityExpansion(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var initialCapacity = links.Capacity;

            // Act
            links.EnsureCapacity(1000);

            // Assert
            Assert.True(links.Capacity >= 1000);
            Assert.True(links.Capacity >= initialCapacity);
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldInitializeNewLinksWithNullReferences(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();

            // Act
            var link = links.Allocate();

            // Assert
            Assert.Equal(0UL, links.GetSource(link));
            Assert.Equal(0UL, links.GetTarget(link));
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldThrowOnInvalidOperations(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link = links.Allocate();
            links.Free(link);

            // Act & Assert - Should throw when trying to free an already freed link
            Assert.Throws<InvalidOperationException>(() => links.Free(link));

            // Act & Assert - Should throw when trying to access a freed link
            Assert.Throws<InvalidOperationException>(() => links.GetSource(link));
            Assert.Throws<InvalidOperationException>(() => links.GetTarget(link));
        }

        [Theory]
        [MemberData(nameof(GetMemoryManagerFactories))]
        public void AllImplementations_ShouldAllowModifyingExistingLinks(string implementationName, Func<ILinksMemoryManager<ulong>> factory)
        {
            // Arrange
            using var links = factory();
            var link = links.Allocate();
            var source1 = links.Allocate();
            var source2 = links.Allocate();
            var target1 = links.Allocate();
            var target2 = links.Allocate();

            // Act - Set initial values
            links.SetSource(link, source1);
            links.SetTarget(link, target1);

            // Verify initial values
            Assert.Equal(source1, links.GetSource(link));
            Assert.Equal(target1, links.GetTarget(link));

            // Act - Modify values
            links.SetSource(link, source2);
            links.SetTarget(link, target2);

            // Assert - Verify modified values
            Assert.Equal(source2, links.GetSource(link));
            Assert.Equal(target2, links.GetTarget(link));
        }
    }
}
