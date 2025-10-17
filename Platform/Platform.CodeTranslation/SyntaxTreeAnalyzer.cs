using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.CodeTranslation
{
    /// <summary>
    /// Analyzes syntax trees to extract structural information.
    /// </summary>
    public class SyntaxTreeAnalyzer
    {
        /// <summary>
        /// Extracts all class declarations from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to analyze.</param>
        /// <returns>A collection of class declaration syntax nodes.</returns>
        public IEnumerable<ClassDeclarationSyntax> GetClassDeclarations(SyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            var root = syntaxTree.GetRoot();
            return root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        }

        /// <summary>
        /// Extracts all method declarations from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to analyze.</param>
        /// <returns>A collection of method declaration syntax nodes.</returns>
        public IEnumerable<MethodDeclarationSyntax> GetMethodDeclarations(SyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            var root = syntaxTree.GetRoot();
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        /// <summary>
        /// Extracts all using directives from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to analyze.</param>
        /// <returns>A collection of using directive syntax nodes.</returns>
        public IEnumerable<UsingDirectiveSyntax> GetUsingDirectives(SyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            var root = syntaxTree.GetRoot();
            return root.DescendantNodes().OfType<UsingDirectiveSyntax>();
        }

        /// <summary>
        /// Gets a summary of the syntax tree structure.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to analyze.</param>
        /// <returns>A string describing the structure of the syntax tree.</returns>
        public string GetStructureSummary(SyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            var classes = GetClassDeclarations(syntaxTree).Count();
            var methods = GetMethodDeclarations(syntaxTree).Count();
            var usings = GetUsingDirectives(syntaxTree).Count();

            return $"Usings: {usings}, Classes: {classes}, Methods: {methods}";
        }
    }
}
