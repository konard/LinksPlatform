using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PatternDiscoveryExtension.Models
{
    /// <summary>
    /// Represents a single change to the Abstract Syntax Tree (AST).
    /// Captures the essential information about what changed in the code structure.
    /// </summary>
    public class ASTChange
    {
        public DateTime Timestamp { get; }
        public string DocumentPath { get; }
        public TextSpan Span { get; }
        public SyntaxNode? OldNode { get; }
        public SyntaxNode? NewNode { get; }
        public string ChangeType { get; }
        public string? OldText { get; }
        public string? NewText { get; }

        public ASTChange(
            string documentPath,
            TextSpan span,
            SyntaxNode? oldNode,
            SyntaxNode? newNode,
            string changeType,
            string? oldText = null,
            string? newText = null)
        {
            Timestamp = DateTime.UtcNow;
            DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
            Span = span;
            OldNode = oldNode;
            NewNode = newNode;
            ChangeType = changeType ?? throw new ArgumentNullException(nameof(changeType));
            OldText = oldText;
            NewText = newText;
        }

        /// <summary>
        /// Gets the kind of syntax node involved in this change.
        /// </summary>
        public string NodeKind => NewNode?.Kind().ToString() ?? OldNode?.Kind().ToString() ?? "Unknown";

        public override string ToString()
        {
            return $"{ChangeType} at {DocumentPath}:{Span.Start} ({NodeKind})";
        }
    }
}
