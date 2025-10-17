using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.BinarySearchBugFinder
{
    /// <summary>
    /// Provides functionality to comment out code sections using AST manipulation
    /// </summary>
    public class CodeCommenter
    {
        /// <summary>
        /// Comments out statements in a source file based on indices
        /// </summary>
        /// <param name="sourceCode">The original source code</param>
        /// <param name="indicesToComment">Indices of statements to comment out</param>
        /// <returns>Modified source code with commented statements</returns>
        public string CommentOutStatements(string sourceCode, HashSet<int> indicesToComment)
        {
            var tree = CSharp​SyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();

            var statements = root.DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(s => !(s.Parent is BlockSyntax && s.Parent.Parent is MethodDeclarationSyntax))
                .ToList();

            if (statements.Count == 0)
                return sourceCode;

            var newRoot = root.ReplaceNodes(
                statements.Where((_, index) => indicesToComment.Contains(index)),
                (original, _) => CommentStatement(original)
            );

            return newRoot.ToFullString();
        }

        /// <summary>
        /// Gets all commentable statements from source code
        /// </summary>
        /// <param name="sourceCode">The source code to analyze</param>
        /// <returns>List of commentable statements</returns>
        public List<StatementSyntax> GetCommentableStatements(string sourceCode)
        {
            var tree = CSharp​SyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();

            return root.DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(s => IsCommentable(s))
                .ToList();
        }

        /// <summary>
        /// Determines if a statement can be safely commented out
        /// </summary>
        private bool IsCommentable(StatementSyntax statement)
        {
            // Don't comment namespace declarations, class declarations, etc.
            if (!(statement.Parent is BlockSyntax))
                return false;

            // Avoid commenting out closing braces
            if (statement is BlockSyntax)
                return false;

            return true;
        }

        /// <summary>
        /// Comments out a single statement
        /// </summary>
        private SyntaxNode CommentStatement(StatementSyntax statement)
        {
            var commentedText = $"/* BINARY_SEARCH_COMMENTED: {statement.ToFullString()} */";
            var commentTrivia = SyntaxFactory.Comment(commentedText);

            // Return an empty statement with the comment
            return SyntaxFactory.EmptyStatement()
                .WithLeadingTrivia(commentTrivia)
                .WithTrailingTrivia(statement.GetTrailingTrivia());
        }

        /// <summary>
        /// Uncomments all statements that were commented by this tool
        /// </summary>
        /// <param name="sourceCode">Source code with comments</param>
        /// <returns>Original source code</returns>
        public string UncommentAll(string sourceCode)
        {
            // Simple implementation - remove all BINARY_SEARCH_COMMENTED markers
            var lines = sourceCode.Split('\n');
            var result = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("/* BINARY_SEARCH_COMMENTED:"))
                {
                    var start = line.IndexOf("/* BINARY_SEARCH_COMMENTED:") + 28;
                    var end = line.LastIndexOf("*/");
                    if (end > start)
                    {
                        var uncommented = line.Substring(start, end - start).Trim();
                        result.Add(uncommented);
                    }
                }
                else
                {
                    result.Add(line);
                }
            }

            return string.Join("\n", result);
        }
    }
}
