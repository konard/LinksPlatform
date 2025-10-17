using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using PatternDiscoveryExtension;
using PatternDiscoveryExtension.Models;
using Xunit;

namespace PatternDiscoveryExtension.Tests
{
    public class PatternDetectorTests
    {
        [Fact]
        public void AddChangeAndDetectPatterns_SingleChange_ReturnsEmpty()
        {
            // Arrange
            var detector = new PatternDetector();
            var change = CreateTestChange("Added", "MethodDeclaration");

            // Act
            var patterns = detector.AddChangeAndDetectPatterns(change);

            // Assert
            Assert.Empty(patterns);
        }

        [Fact]
        public void AddChangeAndDetectPatterns_TwoSimilarChanges_DetectsPattern()
        {
            // Arrange
            var detector = new PatternDetector();

            // Act - Add two similar changes
            var change1 = CreateTestChange("Added", "MethodDeclaration");
            var patterns1 = detector.AddChangeAndDetectPatterns(change1);

            var change2 = CreateTestChange("Added", "MethodDeclaration");
            var patterns2 = detector.AddChangeAndDetectPatterns(change2);

            // Assert
            Assert.Empty(patterns1); // First occurrence doesn't create pattern
            Assert.NotEmpty(patterns2); // Second occurrence creates pattern
            Assert.True(patterns2.First().IsWellFormed);
        }

        [Fact]
        public void AddChangeAndDetectPatterns_DifferentChanges_NoPattern()
        {
            // Arrange
            var detector = new PatternDetector();

            // Act
            var change1 = CreateTestChange("Added", "MethodDeclaration");
            detector.AddChangeAndDetectPatterns(change1);

            var change2 = CreateTestChange("Removed", "PropertyDeclaration");
            var patterns = detector.AddChangeAndDetectPatterns(change2);

            // Assert
            Assert.Empty(patterns);
        }

        [Fact]
        public void AddChangeAndDetectPatterns_RepeatedPattern_IncreasesOccurrences()
        {
            // Arrange
            var detector = new PatternDetector();

            // Act - Add the same pattern three times
            for (int i = 0; i < 6; i++)
            {
                var change = CreateTestChange("Modified", "VariableDeclarator");
                detector.AddChangeAndDetectPatterns(change);
            }

            var allPatterns = detector.GetAllPatterns();

            // Assert
            Assert.NotEmpty(allPatterns);
            var pattern = allPatterns.First();
            Assert.True(pattern.Occurrences >= 2);
        }

        [Fact]
        public void GetAllPatterns_AfterClear_ReturnsEmpty()
        {
            // Arrange
            var detector = new PatternDetector();
            detector.AddChangeAndDetectPatterns(CreateTestChange("Added", "MethodDeclaration"));
            detector.AddChangeAndDetectPatterns(CreateTestChange("Added", "MethodDeclaration"));

            // Act
            detector.Clear();

            // Assert
            Assert.Empty(detector.GetAllPatterns());
        }

        [Fact]
        public void Pattern_IsWellFormed_RequiresMultipleOccurrencesAndHighConfidence()
        {
            // Arrange
            var changes = new System.Collections.Generic.List<ASTChange>
            {
                CreateTestChange("Added", "MethodDeclaration")
            };

            // Act
            var pattern = new Pattern(changes)
            {
                Occurrences = 1,
                Confidence = 0.5
            };

            // Assert
            Assert.False(pattern.IsWellFormed);

            // Increase occurrences and confidence
            pattern.Occurrences = 2;
            pattern.Confidence = 0.8;
            Assert.True(pattern.IsWellFormed);
        }

        private ASTChange CreateTestChange(string changeType, string nodeKind)
        {
            var code = nodeKind switch
            {
                "MethodDeclaration" => "public void TestMethod() { }",
                "PropertyDeclaration" => "public int TestProperty { get; set; }",
                "VariableDeclarator" => "int x = 5;",
                _ => "var test = 1;"
            };

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var node = root.DescendantNodes().FirstOrDefault();

            return new ASTChange(
                "TestDocument.cs",
                TextSpan.FromBounds(0, code.Length),
                null,
                node,
                changeType,
                null,
                code
            );
        }
    }
}
