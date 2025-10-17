using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using PatternDiscoveryExtension.Models;

namespace PatternDiscoveryExtension
{
    /// <summary>
    /// Analyzes Abstract Syntax Tree (AST) changes to detect structural modifications in code.
    /// Uses Roslyn to compare old and new syntax trees and identify meaningful changes.
    /// </summary>
    public class ASTAnalyzer
    {
        /// <summary>
        /// Analyzes changes between two versions of a document.
        /// </summary>
        public List<ASTChange> AnalyzeChanges(Document oldDocument, Document newDocument, TextChangeRange changeRange)
        {
            var changes = new List<ASTChange>();

            try
            {
                var oldRoot = oldDocument.GetSyntaxRootAsync().Result;
                var newRoot = newDocument.GetSyntaxRootAsync().Result;

                if (oldRoot == null || newRoot == null)
                    return changes;

                // Find nodes affected by the change
                var span = changeRange.Span;
                var oldNodes = oldRoot.DescendantNodes(span).ToList();
                var newNodes = newRoot.DescendantNodes(TextSpan.FromBounds(
                    span.Start,
                    Math.Min(span.Start + changeRange.NewLength, newRoot.FullSpan.End)
                )).ToList();

                // Detect additions
                foreach (var newNode in newNodes)
                {
                    if (!oldNodes.Any(o => AreSyntacticallyEquivalent(o, newNode)))
                    {
                        changes.Add(new ASTChange(
                            newDocument.FilePath ?? "Unknown",
                            newNode.Span,
                            null,
                            newNode,
                            "Added",
                            null,
                            newNode.ToString()
                        ));
                    }
                }

                // Detect removals and modifications
                foreach (var oldNode in oldNodes)
                {
                    var matchingNew = newNodes.FirstOrDefault(n => AreSyntacticallyEquivalent(n, oldNode));
                    if (matchingNew == null)
                    {
                        changes.Add(new ASTChange(
                            oldDocument.FilePath ?? "Unknown",
                            oldNode.Span,
                            oldNode,
                            null,
                            "Removed",
                            oldNode.ToString(),
                            null
                        ));
                    }
                    else if (oldNode.ToString() != matchingNew.ToString())
                    {
                        changes.Add(new ASTChange(
                            newDocument.FilePath ?? "Unknown",
                            matchingNew.Span,
                            oldNode,
                            matchingNew,
                            "Modified",
                            oldNode.ToString(),
                            matchingNew.ToString()
                        ));
                    }
                }
            }
            catch (Exception)
            {
                // Log error but don't crash the extension
            }

            return changes;
        }

        /// <summary>
        /// Determines if two syntax nodes are syntactically equivalent (same structure, possibly different content).
        /// </summary>
        private bool AreSyntacticallyEquivalent(SyntaxNode node1, SyntaxNode node2)
        {
            if (node1.Kind() != node2.Kind())
                return false;

            // For simple nodes, consider them equivalent if they have the same kind and position pattern
            if (node1 is CSharpSyntaxNode cs1 && node2 is CSharpSyntaxNode cs2)
            {
                // Compare structural properties
                return cs1.GetType() == cs2.GetType() &&
                       cs1.ChildNodes().Count() == cs2.ChildNodes().Count();
            }

            return false;
        }

        /// <summary>
        /// Extracts the semantic meaning from a syntax node for pattern matching.
        /// </summary>
        public string GetNodeSignature(SyntaxNode node)
        {
            var kind = node.Kind().ToString();
            var childCount = node.ChildNodes().Count();
            var tokenCount = node.DescendantTokens().Count();

            return $"{kind}|Children:{childCount}|Tokens:{tokenCount}";
        }
    }
}
