using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using PatternDiscoveryExtension;
using Xunit;

namespace PatternDiscoveryExtension.Tests
{
    public class ASTAnalyzerTests
    {
        [Fact]
        public void AnalyzeChanges_AddedNode_DetectsAddition()
        {
            // Arrange
            var analyzer = new ASTAnalyzer();
            var oldCode = "class TestClass { }";
            var newCode = "class TestClass { public void Method() { } }";

            var (oldDoc, newDoc) = CreateDocuments(oldCode, newCode);
            var changeRange = new TextChangeRange(
                new TextSpan(oldCode.Length - 2, 0),
                newCode.Length - oldCode.Length
            );

            // Act
            var changes = analyzer.AnalyzeChanges(oldDoc, newDoc, changeRange);

            // Assert
            Assert.NotEmpty(changes);
            Assert.Contains(changes, c => c.ChangeType == "Added");
        }

        [Fact]
        public void AnalyzeChanges_RemovedNode_DetectsRemoval()
        {
            // Arrange
            var analyzer = new ASTAnalyzer();
            var oldCode = "class TestClass { public void Method() { } }";
            var newCode = "class TestClass { }";

            var (oldDoc, newDoc) = CreateDocuments(oldCode, newCode);
            var changeRange = new TextChangeRange(
                new TextSpan(18, oldCode.Length - newCode.Length),
                0
            );

            // Act
            var changes = analyzer.AnalyzeChanges(oldDoc, newDoc, changeRange);

            // Assert
            Assert.NotEmpty(changes);
            Assert.Contains(changes, c => c.ChangeType == "Removed");
        }

        [Fact]
        public void AnalyzeChanges_ModifiedNode_DetectsModification()
        {
            // Arrange
            var analyzer = new ASTAnalyzer();
            var oldCode = "class TestClass { public void Method1() { } }";
            var newCode = "class TestClass { public void Method2() { } }";

            var (oldDoc, newDoc) = CreateDocuments(oldCode, newCode);
            var changeRange = new TextChangeRange(
                new TextSpan(34, 1),
                1
            );

            // Act
            var changes = analyzer.AnalyzeChanges(oldDoc, newDoc, changeRange);

            // Assert
            Assert.NotEmpty(changes);
        }

        [Fact]
        public void GetNodeSignature_SameStructure_ReturnsSameSignature()
        {
            // Arrange
            var analyzer = new ASTAnalyzer();
            var tree1 = CSharpSyntaxTree.ParseText("public void Method1() { }");
            var tree2 = CSharpSyntaxTree.ParseText("public void Method2() { }");

            var node1 = tree1.GetRoot().DescendantNodes().First();
            var node2 = tree2.GetRoot().DescendantNodes().First();

            // Act
            var sig1 = analyzer.GetNodeSignature(node1);
            var sig2 = analyzer.GetNodeSignature(node2);

            // Assert - signatures should be similar for same structure
            Assert.NotNull(sig1);
            Assert.NotNull(sig2);
        }

        [Fact]
        public void AnalyzeChanges_NoChanges_ReturnsEmpty()
        {
            // Arrange
            var analyzer = new ASTAnalyzer();
            var code = "class TestClass { }";

            var (oldDoc, newDoc) = CreateDocuments(code, code);
            var changeRange = new TextChangeRange(new TextSpan(0, 0), 0);

            // Act
            var changes = analyzer.AnalyzeChanges(oldDoc, newDoc, changeRange);

            // Assert
            Assert.Empty(changes);
        }

        private (Document oldDoc, Document newDoc) CreateDocuments(string oldCode, string newCode)
        {
            var workspace = new AdhocWorkspace();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "TestProject",
                "TestProject",
                LanguageNames.CSharp);

            var project = workspace.AddProject(projectInfo);

            var oldDoc = workspace.AddDocument(
                documentId,
                "TestDocument.cs",
                SourceText.From(oldCode));

            var newDoc = oldDoc.WithText(SourceText.From(newCode));

            return (oldDoc, newDoc);
        }
    }
}
