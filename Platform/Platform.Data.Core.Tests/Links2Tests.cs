using System;
using Xunit;
using Platform.Data.Core.Pairs;

namespace Platform.Data.Core.Tests
{
    /// <summary>
    /// Tests for the Links2 garbage-collected implementation.
    /// These tests verify the basic functionality of the GC-based memory manager.
    /// </summary>
    public class Links2Tests
    {
        [Fact]
        public void Constructor_CreatesInstance_WithDefaultCapacity()
        {
            // Arrange & Act
            using var links = new Links2<ulong>();

            // Assert
            Assert.NotNull(links);
            Assert.True(links.Capacity > 0);
        }

        [Fact]
        public void Constructor_CreatesInstance_WithSpecifiedCapacity()
        {
            // Arrange & Act
            using var links = new Links2<ulong>(initialCapacity: 100);

            // Assert
            Assert.NotNull(links);
            Assert.True(links.Capacity >= 1); // At least the null link
        }

        [Fact]
        public void Allocate_CreatesNewLink_ReturnsValidIdentifier()
        {
            // Arrange
            using var links = new Links2<ulong>();

            // Act
            var link = links.Allocate();

            // Assert
            Assert.NotEqual(0UL, link);
            Assert.True(links.IsAllocated(link));
        }

        [Fact]
        public void Allocate_MultipleLinks_ReturnsUniqueIdentifiers()
        {
            // Arrange
            using var links = new Links2<ulong>();

            // Act
            var link1 = links.Allocate();
            var link2 = links.Allocate();
            var link3 = links.Allocate();

            // Assert
            Assert.NotEqual(link1, link2);
            Assert.NotEqual(link2, link3);
            Assert.NotEqual(link1, link3);
            Assert.True(links.IsAllocated(link1));
            Assert.True(links.IsAllocated(link2));
            Assert.True(links.IsAllocated(link3));
        }

        [Fact]
        public void Free_DeallocatesLink_LinkNoLongerAllocated()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();

            // Act
            links.Free(link);

            // Assert
            Assert.False(links.IsAllocated(link));
        }

        [Fact]
        public void Allocate_AfterFree_ReusesFreedLink()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link1 = links.Allocate();
            links.Free(link1);

            // Act
            var link2 = links.Allocate();

            // Assert
            Assert.Equal(link1, link2);
            Assert.True(links.IsAllocated(link2));
        }

        [Fact]
        public void SetSource_SetsSourceCorrectly()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();
            var source = links.Allocate();

            // Act
            links.SetSource(link, source);

            // Assert
            Assert.Equal(source, links.GetSource(link));
        }

        [Fact]
        public void SetTarget_SetsTargetCorrectly()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();
            var target = links.Allocate();

            // Act
            links.SetTarget(link, target);

            // Assert
            Assert.Equal(target, links.GetTarget(link));
        }

        [Fact]
        public void GetSource_NewlyAllocatedLink_ReturnsNullLink()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();

            // Act
            var source = links.GetSource(link);

            // Assert
            Assert.Equal(0UL, source);
        }

        [Fact]
        public void GetTarget_NewlyAllocatedLink_ReturnsNullLink()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();

            // Act
            var target = links.GetTarget(link);

            // Assert
            Assert.Equal(0UL, target);
        }

        [Fact]
        public void CreatePair_CanLinkTwoLinks()
        {
            // Arrange
            using var links = new Links2<ulong>();
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

        [Fact]
        public void Count_ReturnsCorrectNumberOfAllocatedLinks()
        {
            // Arrange
            using var links = new Links2<ulong>();

            // Act
            var link1 = links.Allocate();
            var link2 = links.Allocate();
            var link3 = links.Allocate();

            // Assert
            Assert.Equal(3, links.Count);
        }

        [Fact]
        public void Count_AfterFree_ReturnsCorrectCount()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link1 = links.Allocate();
            var link2 = links.Allocate();
            var link3 = links.Allocate();

            // Act
            links.Free(link2);

            // Assert
            Assert.Equal(2, links.Count);
        }

        [Fact]
        public void EnsureCapacity_IncreasesCapacity()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var initialCapacity = links.Capacity;

            // Act
            links.EnsureCapacity(1000);

            // Assert
            Assert.True(links.Capacity >= 1000);
        }

        [Fact]
        public void Free_ThrowsException_WhenLinkNotAllocated()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();
            links.Free(link);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => links.Free(link));
        }

        [Fact]
        public void GetSource_ThrowsException_WhenLinkNotAllocated()
        {
            // Arrange
            using var links = new Links2<ulong>();
            var link = links.Allocate();
            links.Free(link);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => links.GetSource(link));
        }

        [Fact]
        public void WorksWithDifferentNumericTypes()
        {
            // Test with uint
            using (var linksUint = new Links2<uint>())
            {
                var link = linksUint.Allocate();
                Assert.True(linksUint.IsAllocated(link));
            }

            // Test with int
            using (var linksInt = new Links2<int>())
            {
                var link = linksInt.Allocate();
                Assert.True(linksInt.IsAllocated(link));
            }

            // Test with ushort
            using (var linksUshort = new Links2<ushort>())
            {
                var link = linksUshort.Allocate();
                Assert.True(linksUshort.IsAllocated(link));
            }
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var links = new Links2<ulong>();

            // Act & Assert (should not throw)
            links.Dispose();
            links.Dispose();
        }

        [Fact]
        public void Allocate_ThrowsException_AfterDispose()
        {
            // Arrange
            var links = new Links2<ulong>();
            links.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => links.Allocate());
        }
    }
}
