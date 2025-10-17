using System;
using System.Collections.Generic;
using System.Linq;

namespace PatternDiscoveryExtension.Models
{
    /// <summary>
    /// Represents a detected pattern of repetitive code changes.
    /// A pattern consists of a sequence of AST changes that repeat in a similar way.
    /// </summary>
    public class Pattern
    {
        public string Id { get; }
        public List<ASTChange> Changes { get; }
        public int Occurrences { get; set; }
        public DateTime FirstOccurrence { get; }
        public DateTime LastOccurrence { get; set; }
        public double Confidence { get; set; }

        public Pattern(List<ASTChange> changes)
        {
            Id = Guid.NewGuid().ToString();
            Changes = changes ?? throw new ArgumentNullException(nameof(changes));
            Occurrences = 1;
            FirstOccurrence = DateTime.UtcNow;
            LastOccurrence = DateTime.UtcNow;
            Confidence = 0.0;
        }

        /// <summary>
        /// Gets a human-readable description of the pattern.
        /// </summary>
        public string Description
        {
            get
            {
                if (Changes.Count == 0)
                    return "Empty pattern";

                if (Changes.Count == 1)
                {
                    var change = Changes[0];
                    return $"{change.ChangeType} {change.NodeKind}";
                }

                var changeTypes = Changes.Select(c => c.ChangeType).Distinct().ToList();
                var nodeKinds = Changes.Select(c => c.NodeKind).Distinct().ToList();

                if (changeTypes.Count == 1)
                    return $"{changeTypes[0]} {Changes.Count} {(nodeKinds.Count == 1 ? nodeKinds[0] : "nodes")}";

                return $"{Changes.Count} changes: {string.Join(", ", changeTypes.Take(3))}";
            }
        }

        /// <summary>
        /// Determines if this pattern is well-formed (i.e., suitable for repetition suggestion).
        /// A pattern is well-formed if it has occurred multiple times with high confidence.
        /// </summary>
        public bool IsWellFormed => Occurrences >= 2 && Confidence >= 0.7;

        public override string ToString()
        {
            return $"Pattern: {Description} (Occurrences: {Occurrences}, Confidence: {Confidence:P0})";
        }
    }
}
