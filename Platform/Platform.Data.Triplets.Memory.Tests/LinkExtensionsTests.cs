using System.Linq;
using Xunit;

namespace Platform.Data.Triplets.Memory.Tests
{
    /// <summary>
    /// Tests for the LinkExtensions class.
    /// </summary>
    public class LinkExtensionsTests
    {
        [Fact]
        public void GetAllReferers_ReturnsAllReferers()
        {
            // Arrange
            var link = new Link();
            var linker = new Link();
            var target = new Link();

            var referer1 = Link.Create(link, linker, target);
            var referer2 = Link.Create(target, link, target);
            var referer3 = Link.Create(target, linker, link);

            // Act
            var allReferers = link.GetAllReferers().ToList();

            // Assert
            Assert.Equal(3, allReferers.Count);
            Assert.Contains(referer1, allReferers);
            Assert.Contains(referer2, allReferers);
            Assert.Contains(referer3, allReferers);
        }

        [Fact]
        public void CountReferers_ReturnsCorrectCount()
        {
            // Arrange
            var link = new Link();
            var linker = new Link();
            var target = new Link();

            Link.Create(link, linker, target);
            Link.Create(target, link, target);
            Link.Create(target, linker, link);

            // Act
            var count = link.CountReferers();

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public void HasReferers_ReturnsTrueWhenHasReferers()
        {
            // Arrange
            var link = new Link();
            var linker = new Link();
            var target = new Link();

            Link.Create(link, linker, target);

            // Act & Assert
            Assert.True(link.HasReferers());
        }

        [Fact]
        public void HasReferers_ReturnsFalseWhenNoReferers()
        {
            // Arrange
            var link = new Link();

            // Act & Assert
            Assert.False(link.HasReferers());
        }

        [Fact]
        public void IsSelfReference_ReturnsTrueForSelfReferencingLink()
        {
            // Act
            var link = Link.CreateLinkLinkingItself();

            // Assert
            Assert.True(link.IsSelfReference());
        }

        [Fact]
        public void IsSelfReference_ReturnsFalseForNonSelfReferencingLink()
        {
            // Arrange
            var source = new Link();
            var linker = new Link();
            var target = new Link();

            // Act
            var link = Link.Create(source, linker, target);

            // Assert
            Assert.False(link.IsSelfReference());
        }

        [Fact]
        public void IsCompleteSelfLoop_ReturnsTrueForCompleteSelfLoop()
        {
            // Act
            var link = Link.CreateLinkLinkingItself();

            // Assert
            Assert.True(link.IsCompleteSelfLoop());
        }

        [Fact]
        public void IsCompleteSelfLoop_ReturnsFalseForPartialSelfLoop()
        {
            // Arrange
            var target = new Link();

            // Act
            var link = Link.CreateOutcomingSelfLinker(target);

            // Assert
            Assert.False(link.IsCompleteSelfLoop());
        }

        [Fact]
        public void FindReferer_FindsMatchingReferer()
        {
            // Arrange
            var link = new Link();
            var linker = new Link();
            var target = new Link();

            var referer = Link.Create(link, linker, target);

            // Act
            var found = link.FindReferer(link, linker, target);

            // Assert
            Assert.Same(referer, found);
        }

        [Fact]
        public void FindReferer_ReturnsNullWhenNotFound()
        {
            // Arrange
            var link = new Link();
            var other = new Link();

            // Act
            var found = link.FindReferer(other, other, other);

            // Assert
            Assert.Null(found);
        }

        [Fact]
        public void FindReferers_FindsAllMatchingReferers()
        {
            // Arrange
            var link = new Link();
            var linker = new Link();
            var target1 = new Link();
            var target2 = new Link();

            var referer1 = Link.Create(link, linker, target1);
            var referer2 = Link.Create(link, linker, target2);
            Link.Create(target1, linker, target2); // Non-matching

            // Act
            var found = link.FindReferers(source: link, linker: linker).ToList();

            // Assert
            Assert.Equal(2, found.Count);
            Assert.Contains(referer1, found);
            Assert.Contains(referer2, found);
        }

        [Fact]
        public void FindReferers_WithNullParameters_MatchesAny()
        {
            // Arrange
            var link = new Link();
            var linker = new Link();
            var target = new Link();

            var referer1 = Link.Create(link, linker, target);
            var referer2 = Link.Create(link, target, linker);

            // Act
            var found = link.FindReferers(source: link).ToList();

            // Assert
            Assert.Equal(2, found.Count);
            Assert.Contains(referer1, found);
            Assert.Contains(referer2, found);
        }
    }
}
