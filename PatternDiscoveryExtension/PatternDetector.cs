using System;
using System.Collections.Generic;
using System.Linq;
using PatternDiscoveryExtension.Models;

namespace PatternDiscoveryExtension
{
    /// <summary>
    /// Detects repetitive patterns in a sequence of AST changes.
    /// Uses sequence matching algorithms to identify when similar changes repeat.
    /// </summary>
    public class PatternDetector
    {
        private readonly List<ASTChange> _changeHistory = new();
        private readonly List<Pattern> _detectedPatterns = new();
        private readonly int _maxHistorySize = 100;
        private readonly int _minPatternLength = 1;
        private readonly int _maxPatternLength = 10;

        /// <summary>
        /// Adds a new change to the history and checks for patterns.
        /// </summary>
        public List<Pattern> AddChangeAndDetectPatterns(ASTChange change)
        {
            _changeHistory.Add(change);

            // Trim history if it gets too large
            if (_changeHistory.Count > _maxHistorySize)
            {
                _changeHistory.RemoveAt(0);
            }

            // Detect new patterns
            var newPatterns = DetectPatterns();

            // Update existing patterns or add new ones
            foreach (var newPattern in newPatterns)
            {
                var existing = _detectedPatterns.FirstOrDefault(p => ArePatternsEquivalent(p, newPattern));
                if (existing != null)
                {
                    existing.Occurrences++;
                    existing.LastOccurrence = DateTime.UtcNow;
                    existing.Confidence = CalculateConfidence(existing);
                }
                else
                {
                    _detectedPatterns.Add(newPattern);
                }
            }

            // Return well-formed patterns that should be suggested to the user
            return _detectedPatterns.Where(p => p.IsWellFormed).ToList();
        }

        /// <summary>
        /// Detects patterns in the recent change history.
        /// </summary>
        private List<Pattern> DetectPatterns()
        {
            var patterns = new List<Pattern>();

            if (_changeHistory.Count < _minPatternLength * 2)
                return patterns;

            // Look for repeating sequences of various lengths
            for (int length = _minPatternLength; length <= Math.Min(_maxPatternLength, _changeHistory.Count / 2); length++)
            {
                var recentChanges = _changeHistory.TakeLast(length * 2).ToList();
                if (recentChanges.Count < length * 2)
                    continue;

                var firstSequence = recentChanges.Take(length).ToList();
                var secondSequence = recentChanges.Skip(length).Take(length).ToList();

                if (AreSequencesSimilar(firstSequence, secondSequence))
                {
                    var pattern = new Pattern(firstSequence)
                    {
                        Occurrences = 2,
                        Confidence = CalculateSequenceSimilarity(firstSequence, secondSequence)
                    };
                    patterns.Add(pattern);
                }
            }

            return patterns;
        }

        /// <summary>
        /// Determines if two patterns are equivalent.
        /// </summary>
        private bool ArePatternsEquivalent(Pattern p1, Pattern p2)
        {
            if (p1.Changes.Count != p2.Changes.Count)
                return false;

            for (int i = 0; i < p1.Changes.Count; i++)
            {
                if (!AreChangesSimilar(p1.Changes[i], p2.Changes[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if two sequences of changes are similar.
        /// </summary>
        private bool AreSequencesSimilar(List<ASTChange> seq1, List<ASTChange> seq2)
        {
            if (seq1.Count != seq2.Count)
                return false;

            int similarCount = 0;
            for (int i = 0; i < seq1.Count; i++)
            {
                if (AreChangesSimilar(seq1[i], seq2[i]))
                    similarCount++;
            }

            // Consider sequences similar if at least 80% of changes match
            return (double)similarCount / seq1.Count >= 0.8;
        }

        /// <summary>
        /// Calculates similarity score between two sequences.
        /// </summary>
        private double CalculateSequenceSimilarity(List<ASTChange> seq1, List<ASTChange> seq2)
        {
            if (seq1.Count != seq2.Count)
                return 0.0;

            if (seq1.Count == 0)
                return 1.0;

            int similarCount = 0;
            for (int i = 0; i < seq1.Count; i++)
            {
                if (AreChangesSimilar(seq1[i], seq2[i]))
                    similarCount++;
            }

            return (double)similarCount / seq1.Count;
        }

        /// <summary>
        /// Determines if two AST changes are similar (same type of change, same node kind).
        /// </summary>
        private bool AreChangesSimilar(ASTChange c1, ASTChange c2)
        {
            return c1.ChangeType == c2.ChangeType &&
                   c1.NodeKind == c2.NodeKind;
        }

        /// <summary>
        /// Calculates confidence score for a pattern based on occurrences and consistency.
        /// </summary>
        private double CalculateConfidence(Pattern pattern)
        {
            // Base confidence from occurrences (more occurrences = higher confidence)
            double baseConfidence = Math.Min(pattern.Occurrences / 5.0, 1.0);

            // Time factor (recent patterns are more relevant)
            var timeSinceFirst = (DateTime.UtcNow - pattern.FirstOccurrence).TotalMinutes;
            double timeFactor = timeSinceFirst < 5 ? 1.0 : 0.8;

            return baseConfidence * timeFactor;
        }

        /// <summary>
        /// Gets all detected patterns.
        /// </summary>
        public List<Pattern> GetAllPatterns() => _detectedPatterns.ToList();

        /// <summary>
        /// Clears the change history and detected patterns.
        /// </summary>
        public void Clear()
        {
            _changeHistory.Clear();
            _detectedPatterns.Clear();
        }
    }
}
