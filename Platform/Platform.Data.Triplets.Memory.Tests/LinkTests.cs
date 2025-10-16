using System.Linq;
using Xunit;

namespace Platform.Data.Triplets.Memory.Tests
{
    /// <summary>
    /// Tests for the Link class implementation.
    /// </summary>
    public class LinkTests
    {
        [Fact]
        public void Create_WithValidParameters_CreatesLink()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target = new Link();

            // Act
            var link = Link.Create(source, linker, target);

            // Assert
            Assert.NotNull(link);
            Assert.Equal(source, link.Source);
            Assert.Equal(linker, link.Linker);
            Assert.Equal(target, link.Target);
        }

        [Fact]
        public void Create_WithSameParameters_ReturnsSameLink()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target = new Link();

            // Act
            var link1 = Link.Create(source, linker, target);
            var link2 = Link.Create(source, linker, target);

            // Assert
            Assert.Same(link1, link2);
        }

        [Fact]
        public void CreateLinkLinkingItself_CreatesSelfReferencingLink()
        {
            // Act
            var link = Link.CreateLinkLinkingItself();

            // Assert
            Assert.Equal(link, link.Source);
            Assert.Equal(link, link.Linker);
            Assert.Equal(link, link.Target);
        }

        [Fact]
        public void CreateOutcomingSelfLinker_CreatesCorrectPattern()
        {
            // Arrange
            var target = new Link();

            // Act
            var link = Link.CreateOutcomingSelfLinker(target);

            // Assert
            Assert.Equal(link, link.Source);
            Assert.Equal(link, link.Linker);
            Assert.Equal(target, link.Target);
        }

        [Fact]
        public void CreateCycleSelfLink_CreatesCorrectPattern()
        {
            // Arrange
            var linker = new Link();

            // Act
            var link = Link.CreateCycleSelfLink(linker);

            // Assert
            Assert.Equal(link, link.Source);
            Assert.Equal(linker, link.Linker);
            Assert.Equal(link, link.Target);
        }

        [Fact]
        public void ReferersBySource_TracksReferersCorrectly()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target1 = new Link();
            var target2 = new Link();

            // Act
            var link1 = Link.Create(source, linker, target1);
            var link2 = Link.Create(source, linker, target2);

            // Assert
            var referers = source.ReferersBySource.ToList();
            Assert.Equal(2, referers.Count);
            Assert.Contains(link1, referers);
            Assert.Contains(link2, referers);
        }

        [Fact]
        public void ReferersByLinker_TracksReferersCorrectly()
        {
            // Arrange
            var source1 = new Link();
            var source2 = new Link();
            var linker = new Link();
            var target = new Link();

            // Act
            var link1 = Link.Create(source1, linker, target);
            var link2 = Link.Create(source2, linker, target);

            // Assert
            var referers = linker.ReferersByLinker.ToList();
            Assert.Equal(2, referers.Count);
            Assert.Contains(link1, referers);
            Assert.Contains(link2, referers);
        }

        [Fact]
        public void ReferersByTarget_TracksReferersCorrectly()
        {
            // Arrange
            var source = new Link();
            var linker1 = new Link();
            var linker2 = new Link();
            var target = new Link();

            // Act
            var link1 = Link.Create(source, linker1, target);
            var link2 = Link.Create(source, linker2, target);

            // Assert
            var referers = target.ReferersByTarget.ToList();
            Assert.Equal(2, referers.Count);
            Assert.Contains(link1, referers);
            Assert.Contains(link2, referers);
        }

        [Fact]
        public void Source_WhenChanged_UpdatesRefererChains()
        {
            // Arrange
            var source1 = new Link();
            var source2 = new Link();
            var linker = new Link();
            var target = new Link();
            var link = Link.Create(source1, linker, target);

            // Act
            link.Source = source2;

            // Assert
            Assert.Empty(source1.ReferersBySource);
            Assert.Single(source2.ReferersBySource);
            Assert.Contains(link, source2.ReferersBySource);
        }

        [Fact]
        public void Delete_RemovesAllReferences()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target = new Link();
            var link = Link.Create(source, linker, target);

            // Act
            link.Delete();

            // Assert
            Assert.Null(link.Source);
            Assert.Null(link.Linker);
            Assert.Null(link.Target);
            Assert.Empty(source.ReferersBySource);
            Assert.Empty(linker.ReferersByLinker);
            Assert.Empty(target.ReferersByTarget);
        }

        [Fact]
        public void Delete_RecursivelyDeletesReferers()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target = new Link();
            var link1 = Link.Create(source, linker, target);
            var link2 = Link.Create(link1, linker, target);

            // Act
            link1.Delete();

            // Assert
            Assert.Null(link1.Source);
            Assert.Null(link2.Source);
            Assert.Empty(source.ReferersBySource);
        }

        [Fact]
        public void IsDeleted_ReturnsTrueForDeletedLink()
        {
            // Arrange
            var link = new Link();

            // Assert
            Assert.True(link.IsDeleted());
        }

        [Fact]
        public void IsDeleted_ReturnsFalseForActiveLink()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target = new Link();

            // Act
            var link = Link.Create(source, linker, target);

            // Assert - the referenced links now have referers, so they are not deleted
            Assert.False(source.IsDeleted());
        }
    }
}
