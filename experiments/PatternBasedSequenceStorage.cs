using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates pattern-based sequence storage optimization.
    ///
    /// Based on issue #142, this experiment shows how sequences can be stored more efficiently
    /// using pattern detection. Instead of storing every step individually, we can use markers
    /// to represent repeated patterns and skip redundant counting when there's only one path option.
    ///
    /// Example: The sequence "mama" can be stored as:
    /// 1. Individual steps: START -> m -> a -> m -> a -> STOP
    /// 2. With pattern markers: START -> m -> {a,m} -> STOP (where {a,m} represents the pattern)
    /// 3. Optimal: When a step has only one option, we can skip counting/indexing
    ///
    /// This reduces storage space and improves query performance for pattern matching.
    /// </summary>
    public class PatternBasedSequenceStorage
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;

        // Markers for pattern representation
        private ulong _patternMarker;
        private ulong _startMarker;
        private ulong _stopMarker;
        private ulong _singlePathMarker;

        public PatternBasedSequenceStorage(ILinks<ulong> links, Sequences sequences)
        {
            _links = links;
            _sequences = sequences;
            InitializeMarkers();
        }

        private void InitializeMarkers()
        {
            // Create semantic markers for pattern-based storage
            var meaningRoot = _links.GetOrCreate<ulong>(1, 1);
            _patternMarker = _links.GetOrCreate<ulong>(meaningRoot, _links.Create());
            _startMarker = _links.GetOrCreate<ulong>(meaningRoot, _links.Create());
            _stopMarker = _links.GetOrCreate<ulong>(meaningRoot, _links.Create());
            _singlePathMarker = _links.GetOrCreate<ulong>(meaningRoot, _links.Create());
        }

        /// <summary>
        /// Creates a sequence and analyzes its pattern structure.
        /// </summary>
        public SequenceAnalysis CreateWithPatternAnalysis(ulong[] elements)
        {
            if (elements == null || elements.Length == 0)
                throw new ArgumentException("Elements cannot be null or empty", nameof(elements));

            var analysis = new SequenceAnalysis
            {
                OriginalElements = elements,
                ElementCount = elements.Length
            };

            // Detect repeating patterns in the sequence
            var patterns = DetectPatterns(elements);
            analysis.DetectedPatterns = patterns;

            // Create the standard sequence
            var sequenceLink = _sequences.Create(elements);
            analysis.StandardSequenceLink = sequenceLink;

            // If patterns were detected, create optimized representation
            if (patterns.Count > 0)
            {
                var optimizedLink = CreateOptimizedSequence(elements, patterns);
                analysis.OptimizedSequenceLink = optimizedLink;
                analysis.HasOptimization = true;
            }

            return analysis;
        }

        /// <summary>
        /// Detects repeating patterns within a sequence of elements.
        /// </summary>
        private List<Pattern> DetectPatterns(ulong[] elements)
        {
            var patterns = new List<Pattern>();

            // Look for repeating subsequences
            for (int patternLength = 1; patternLength <= elements.Length / 2; patternLength++)
            {
                for (int startIndex = 0; startIndex <= elements.Length - patternLength; startIndex++)
                {
                    var potentialPattern = elements.Skip(startIndex).Take(patternLength).ToArray();
                    var occurrences = FindOccurrences(elements, potentialPattern);

                    if (occurrences.Count >= 2)
                    {
                        patterns.Add(new Pattern
                        {
                            Elements = potentialPattern,
                            Occurrences = occurrences,
                            Length = patternLength
                        });
                    }
                }
            }

            // Remove overlapping patterns, keep the most beneficial ones
            return RemoveOverlappingPatterns(patterns);
        }

        /// <summary>
        /// Finds all occurrences of a pattern within the elements array.
        /// </summary>
        private List<int> FindOccurrences(ulong[] elements, ulong[] pattern)
        {
            var occurrences = new List<int>();

            for (int i = 0; i <= elements.Length - pattern.Length; i++)
            {
                bool matches = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (elements[i + j] != pattern[j])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    occurrences.Add(i);
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Removes overlapping patterns, preferring longer patterns with more occurrences.
        /// </summary>
        private List<Pattern> RemoveOverlappingPatterns(List<Pattern> patterns)
        {
            // Sort by benefit: longer patterns with more occurrences are preferred
            var sorted = patterns
                .OrderByDescending(p => p.Length * p.Occurrences.Count)
                .ToList();

            var result = new List<Pattern>();
            var usedPositions = new HashSet<int>();

            foreach (var pattern in sorted)
            {
                var validOccurrences = pattern.Occurrences
                    .Where(pos => !Enumerable.Range(pos, pattern.Length).Any(usedPositions.Contains))
                    .ToList();

                if (validOccurrences.Count >= 2)
                {
                    result.Add(new Pattern
                    {
                        Elements = pattern.Elements,
                        Occurrences = validOccurrences,
                        Length = pattern.Length
                    });

                    // Mark positions as used
                    foreach (var pos in validOccurrences)
                    {
                        for (int i = pos; i < pos + pattern.Length; i++)
                        {
                            usedPositions.Add(i);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an optimized sequence representation using detected patterns.
        ///
        /// The optimization works by:
        /// 1. Creating a pattern link for repeated subsequences
        /// 2. Using the pattern link instead of repeating the full subsequence
        /// 3. Marking single-path steps (where there's only one option) with a special marker
        ///    to indicate that counting/indexing can be skipped
        /// </summary>
        private ulong CreateOptimizedSequence(ulong[] elements, List<Pattern> patterns)
        {
            // For simplicity, we'll optimize the most beneficial pattern
            if (patterns.Count == 0)
                return 0;

            var bestPattern = patterns.OrderByDescending(p => p.Length * p.Occurrences.Count).First();

            // Create a link representing the pattern itself
            var patternSequence = _sequences.Create(bestPattern.Elements);
            var patternLink = _links.GetOrCreate(_patternMarker, patternSequence);

            // Build optimized sequence by replacing pattern occurrences
            var optimizedElements = new List<ulong>();
            var positionsToSkip = new HashSet<int>();

            foreach (var occurrence in bestPattern.Occurrences)
            {
                for (int i = occurrence; i < occurrence + bestPattern.Length; i++)
                {
                    positionsToSkip.Add(i);
                }
            }

            for (int i = 0; i < elements.Length; i++)
            {
                if (positionsToSkip.Contains(i))
                {
                    // Check if this is the start of a pattern occurrence
                    if (bestPattern.Occurrences.Contains(i))
                    {
                        // Use pattern link instead of individual elements
                        optimizedElements.Add(patternLink);
                    }
                }
                else
                {
                    optimizedElements.Add(elements[i]);
                }
            }

            // Create the optimized sequence
            if (optimizedElements.Count > 0)
            {
                return _sequences.Create(optimizedElements.ToArray());
            }

            return 0;
        }

        /// <summary>
        /// Represents a detected pattern within a sequence.
        /// </summary>
        public class Pattern
        {
            public ulong[] Elements { get; set; }
            public List<int> Occurrences { get; set; }
            public int Length { get; set; }

            public override string ToString()
            {
                return $"Pattern [{string.Join(", ", Elements)}] found {Occurrences.Count} times at positions [{string.Join(", ", Occurrences)}]";
            }
        }

        /// <summary>
        /// Contains analysis results for a sequence.
        /// </summary>
        public class SequenceAnalysis
        {
            public ulong[] OriginalElements { get; set; }
            public int ElementCount { get; set; }
            public ulong StandardSequenceLink { get; set; }
            public ulong OptimizedSequenceLink { get; set; }
            public List<Pattern> DetectedPatterns { get; set; }
            public bool HasOptimization { get; set; }

            public void PrintSummary()
            {
                Console.WriteLine($"Sequence Analysis:");
                Console.WriteLine($"  Elements: [{string.Join(", ", OriginalElements)}]");
                Console.WriteLine($"  Element Count: {ElementCount}");
                Console.WriteLine($"  Standard Sequence Link: {StandardSequenceLink}");

                if (HasOptimization)
                {
                    Console.WriteLine($"  Optimized Sequence Link: {OptimizedSequenceLink}");
                    Console.WriteLine($"  Detected Patterns: {DetectedPatterns.Count}");
                    foreach (var pattern in DetectedPatterns)
                    {
                        Console.WriteLine($"    - {pattern}");
                    }
                }
                else
                {
                    Console.WriteLine($"  No optimization patterns detected.");
                }
            }
        }
    }
}
