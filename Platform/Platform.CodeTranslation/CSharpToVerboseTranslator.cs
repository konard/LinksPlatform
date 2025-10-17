using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.CodeTranslation
{
    /// <summary>
    /// Translates C# code to a more verbose version with additional comments and documentation.
    /// This serves as a demonstration of AST-based code transformation.
    /// </summary>
    public class CSharpToVerboseTranslator : ICodeTranslator
    {
        private readonly CSharpParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpToVerboseTranslator"/> class.
        /// </summary>
        public CSharpToVerboseTranslator()
        {
            _parser = new CSharpParser();
        }

        /// <summary>
        /// Gets the source language name.
        /// </summary>
        public string SourceLanguage => "C#";

        /// <summary>
        /// Gets the target language name.
        /// </summary>
        public string TargetLanguage => "C# (Verbose)";

        /// <summary>
        /// Translates C# source code to a more verbose version.
        /// </summary>
        /// <param name="sourceCode">The C# source code to translate.</param>
        /// <returns>The verbose version of the source code.</returns>
        public string Translate(string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));
            }

            var syntaxTree = _parser.Parse(sourceCode);
            var root = syntaxTree.GetRoot();

            // Add comments to classes
            var rewriter = new VerboseRewriter();
            var newRoot = rewriter.Visit(root);

            return newRoot.ToFullString();
        }

        private class VerboseRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // Add a comment before the class if it doesn't have one
                var trivia = node.GetLeadingTrivia();
                if (!trivia.Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                                     t.IsKind(SyntaxKind.MultiLineCommentTrivia)))
                {
                    var comment = SyntaxFactory.Comment($"// Class: {node.Identifier.Text}\n");
                    node = node.WithLeadingTrivia(trivia.Add(comment));
                }

                return base.VisitClassDeclaration(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                // Add a comment before the method if it doesn't have one
                var trivia = node.GetLeadingTrivia();
                if (!trivia.Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                                     t.IsKind(SyntaxKind.MultiLineCommentTrivia)))
                {
                    var comment = SyntaxFactory.Comment($"    // Method: {node.Identifier.Text}\n");
                    node = node.WithLeadingTrivia(trivia.Add(comment));
                }

                return base.VisitMethodDeclaration(node);
            }
        }
    }
}
