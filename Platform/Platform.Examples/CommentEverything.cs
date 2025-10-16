using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.Examples
{
    /// <summary>
    /// Provides bidirectional synchronization between code and comments.
    /// Generates XML documentation comments from code structure and can scaffold code from structured comments.
    /// </summary>
    public class CommentEverything
    {
        private readonly bool _verbose;

        /// <summary>
        /// Initializes a new instance of the CommentEverything class.
        /// </summary>
        /// <param name="verbose">If true, outputs detailed processing information.</param>
        public CommentEverything(bool verbose = false)
        {
            _verbose = verbose;
        }

        /// <summary>
        /// Generates XML documentation comments for C# code in the specified file.
        /// </summary>
        /// <param name="filePath">Path to the C# source file to process.</param>
        /// <param name="outputPath">Path where the commented code will be saved. If null, overwrites the original file.</param>
        /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
        /// <returns>Number of comments generated.</returns>
        public int GenerateComments(string filePath, string outputPath = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var code = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var rewriter = new CommentGeneratorRewriter(_verbose);
            var newRoot = rewriter.Visit(root);

            var result = newRoot.ToFullString();
            var output = outputPath ?? filePath;
            File.WriteAllText(output, result, Encoding.UTF8);

            if (_verbose)
            {
                Console.WriteLine($"Generated {rewriter.CommentsAdded} comments for {filePath}");
            }

            return rewriter.CommentsAdded;
        }

        /// <summary>
        /// Generates XML documentation comments for all C# files in a directory.
        /// </summary>
        /// <param name="directoryPath">Directory to process recursively.</param>
        /// <param name="pattern">File pattern to match (default: *.cs).</param>
        /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
        /// <returns>Total number of comments generated across all files.</returns>
        public int GenerateCommentsForDirectory(string directoryPath, string pattern = "*.cs", CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            var totalComments = 0;
            var files = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    totalComments += GenerateComments(file, null, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {file}: {ex.Message}");
                }
            }

            return totalComments;
        }

        /// <summary>
        /// Generates code scaffolding from specially formatted XML comments.
        /// Comments should follow the pattern: &lt;scaffold type="..." name="..." /&gt;
        /// </summary>
        /// <param name="filePath">Path to the file containing scaffold comments.</param>
        /// <param name="outputPath">Path where generated code will be saved.</param>
        /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
        /// <returns>Number of code elements generated.</returns>
        public int GenerateCodeFromComments(string filePath, string outputPath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var code = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var generator = new CodeFromCommentsGenerator(_verbose);
            var generatedCode = generator.Generate(root);

            if (!string.IsNullOrEmpty(generatedCode))
            {
                File.WriteAllText(outputPath, generatedCode, Encoding.UTF8);
                if (_verbose)
                {
                    Console.WriteLine($"Generated {generator.ElementsGenerated} code elements to {outputPath}");
                }
            }

            return generator.ElementsGenerated;
        }

        private class CommentGeneratorRewriter : CSharpSyntaxRewriter
        {
            private readonly bool _verbose;
            public int CommentsAdded { get; private set; }

            public CommentGeneratorRewriter(bool verbose)
            {
                _verbose = verbose;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (!HasDocumentationComment(node))
                {
                    var comment = GenerateClassComment(node);
                    node = node.WithLeadingTrivia(node.GetLeadingTrivia().Insert(0, comment));
                    CommentsAdded++;
                }
                return base.VisitClassDeclaration(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (!HasDocumentationComment(node))
                {
                    var comment = GenerateMethodComment(node);
                    node = node.WithLeadingTrivia(node.GetLeadingTrivia().Insert(0, comment));
                    CommentsAdded++;
                }
                return base.VisitMethodDeclaration(node);
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (!HasDocumentationComment(node))
                {
                    var comment = GeneratePropertyComment(node);
                    node = node.WithLeadingTrivia(node.GetLeadingTrivia().Insert(0, comment));
                    CommentsAdded++;
                }
                return base.VisitPropertyDeclaration(node);
            }

            private bool HasDocumentationComment(SyntaxNode node)
            {
                return node.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                                         t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
            }

            private SyntaxTrivia GenerateClassComment(ClassDeclarationSyntax node)
            {
                var className = node.Identifier.Text;
                var comment = $@"/// <summary>
        /// Represents the {className} class.
        /// </summary>
        ";
                return SyntaxFactory.ParseLeadingTrivia(comment)[0];
            }

            private SyntaxTrivia GenerateMethodComment(MethodDeclarationSyntax node)
            {
                var methodName = node.Identifier.Text;
                var sb = new StringBuilder();
                sb.AppendLine("/// <summary>");
                sb.AppendLine($"        /// Executes the {methodName} operation.");
                sb.AppendLine("        /// </summary>");

                foreach (var param in node.ParameterList.Parameters)
                {
                    sb.AppendLine($"        /// <param name=\"{param.Identifier.Text}\">The {param.Identifier.Text} parameter.</param>");
                }

                if (!node.ReturnType.ToString().Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"        /// <returns>Returns a {node.ReturnType}.</returns>");
                }

                sb.Append("        ");
                return SyntaxFactory.ParseLeadingTrivia(sb.ToString())[0];
            }

            private SyntaxTrivia GeneratePropertyComment(PropertyDeclarationSyntax node)
            {
                var propertyName = node.Identifier.Text;
                var comment = $@"/// <summary>
        /// Gets or sets the {propertyName}.
        /// </summary>
        ";
                return SyntaxFactory.ParseLeadingTrivia(comment)[0];
            }
        }

        private class CodeFromCommentsGenerator
        {
            private readonly bool _verbose;
            public int ElementsGenerated { get; private set; }

            public CodeFromCommentsGenerator(bool verbose)
            {
                _verbose = verbose;
            }

            public string Generate(CompilationUnitSyntax root)
            {
                var sb = new StringBuilder();
                var trivia = root.DescendantTrivia()
                    .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

                foreach (var comment in trivia)
                {
                    var text = comment.ToString();
                    if (text.Contains("<scaffold"))
                    {
                        var scaffoldCode = ParseScaffoldComment(text);
                        if (!string.IsNullOrEmpty(scaffoldCode))
                        {
                            sb.AppendLine(scaffoldCode);
                            ElementsGenerated++;
                        }
                    }
                }

                return sb.ToString();
            }

            private string ParseScaffoldComment(string commentText)
            {
                // Simple scaffold parsing: <scaffold type="class" name="MyClass" />
                if (commentText.Contains("type=\"class\""))
                {
                    var nameStart = commentText.IndexOf("name=\"") + 6;
                    var nameEnd = commentText.IndexOf("\"", nameStart);
                    if (nameStart > 6 && nameEnd > nameStart)
                    {
                        var name = commentText.Substring(nameStart, nameEnd - nameStart);
                        return $@"public class {name}
{{
    // TODO: Implement {name}
}}";
                    }
                }
                else if (commentText.Contains("type=\"method\""))
                {
                    var nameStart = commentText.IndexOf("name=\"") + 6;
                    var nameEnd = commentText.IndexOf("\"", nameStart);
                    if (nameStart > 6 && nameEnd > nameStart)
                    {
                        var name = commentText.Substring(nameStart, nameEnd - nameStart);
                        return $@"public void {name}()
{{
    // TODO: Implement {name}
}}";
                    }
                }

                return null;
            }
        }
    }
}
