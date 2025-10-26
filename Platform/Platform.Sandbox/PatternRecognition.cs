using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Sandbox
{
    /// <summary>
    /// Implements pattern recognition strategy based on frequency hypothesis.
    /// Hypothesis: Everything that has the same frequency of occurrence in multiple contexts is the same thing.
    /// </summary>
    public class PatternRecognition
    {
        /// <summary>
        /// Represents a pattern element (can be a single item or a sequence of items).
        /// </summary>
        public class Pattern
        {
            public List<string> Elements { get; set; }
            public int Occurrences { get; set; }
            public int TotalContexts { get; set; }

            public Pattern(List<string> elements)
            {
                Elements = new List<string>(elements);
                Occurrences = 0;
                TotalContexts = 0;
            }

            public double Frequency => TotalContexts > 0 ? (double)Occurrences / TotalContexts : 0;

            public string AsString() => string.Join(" ", Elements);

            public override string ToString() => $"{AsString()} (frequency: {Occurrences}/{TotalContexts} = {Frequency:F2})";
        }

        /// <summary>
        /// Analyzes contexts (lines) to find patterns and their frequencies.
        /// </summary>
        public static Dictionary<string, Pattern> AnalyzePatterns(List<List<string>> contexts)
        {
            var patterns = new Dictionary<string, Pattern>();
            var totalContexts = contexts.Count;

            // Analyze all possible subsequences in all contexts
            foreach (var context in contexts)
            {
                var seenInThisContext = new HashSet<string>();

                // Check all possible subsequences
                for (int length = 1; length <= context.Count; length++)
                {
                    for (int start = 0; start <= context.Count - length; start++)
                    {
                        var subsequence = context.GetRange(start, length);
                        var key = string.Join(" ", subsequence);

                        // Track unique occurrences per context
                        if (!seenInThisContext.Contains(key))
                        {
                            if (!patterns.ContainsKey(key))
                            {
                                patterns[key] = new Pattern(subsequence);
                            }
                            patterns[key].Occurrences++;
                            seenInThisContext.Add(key);
                        }
                    }
                }
            }

            // Set total contexts for all patterns
            foreach (var pattern in patterns.Values)
            {
                pattern.TotalContexts = totalContexts;
            }

            return patterns;
        }

        /// <summary>
        /// Finds patterns that occur in all contexts (100% frequency).
        /// </summary>
        public static List<Pattern> FindUniversalPatterns(Dictionary<string, Pattern> patterns)
        {
            return patterns.Values
                .Where(p => Math.Abs(p.Frequency - 1.0) < 0.0001) // Frequency = 1.0 (100%)
                .OrderByDescending(p => p.Elements.Count)
                .ToList();
        }

        /// <summary>
        /// Groups patterns by their frequency.
        /// Patterns with the same frequency could represent the same thing (variables/placeholders).
        /// </summary>
        public static Dictionary<double, List<Pattern>> GroupByFrequency(Dictionary<string, Pattern> patterns)
        {
            var grouped = new Dictionary<double, List<Pattern>>();

            foreach (var pattern in patterns.Values)
            {
                var freq = Math.Round(pattern.Frequency, 4); // Round to avoid floating-point issues
                if (!grouped.ContainsKey(freq))
                {
                    grouped[freq] = new List<Pattern>();
                }
                grouped[freq].Add(pattern);
            }

            return grouped;
        }

        /// <summary>
        /// Generates the common pattern by replacing variable parts with placeholders.
        /// </summary>
        public static List<string> GenerateCommonPattern(List<List<string>> contexts, Dictionary<string, Pattern> patterns)
        {
            if (contexts.Count == 0) return new List<string>();

            var universalPatterns = FindUniversalPatterns(patterns);
            var longestUniversal = universalPatterns.FirstOrDefault();

            if (longestUniversal != null && longestUniversal.Elements.Count == contexts[0].Count)
            {
                // All elements are the same across contexts
                return longestUniversal.Elements;
            }

            // Build pattern with placeholders for variable parts
            var result = new List<string>();
            var contextLength = contexts[0].Count;

            for (int i = 0; i < contextLength; i++)
            {
                var element = contexts[0][i];
                var allSame = contexts.All(c => c.Count > i && c[i] == element);

                if (allSame)
                {
                    result.Add(element);
                }
                else
                {
                    result.Add("*");
                }
            }

            return result;
        }

        /// <summary>
        /// Analyzes and prints detailed pattern recognition results.
        /// </summary>
        public static void AnalyzeAndPrint(string title, List<List<string>> contexts)
        {
            Console.WriteLine($"\n=== {title} ===");
            Console.WriteLine("Contexts:");
            for (int i = 0; i < contexts.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {string.Join(" ", contexts[i])}");
            }

            var patterns = AnalyzePatterns(contexts);
            var universalPatterns = FindUniversalPatterns(patterns);

            Console.WriteLine("\nPatterns with 100% frequency (occur in all contexts):");
            foreach (var pattern in universalPatterns)
            {
                Console.WriteLine($"  {pattern}");
            }

            var commonPattern = GenerateCommonPattern(contexts, patterns);
            Console.WriteLine($"\nCommon pattern: {string.Join(" ", commonPattern)}");

            // Find single-element patterns to identify variables
            var singleElementPatterns = patterns.Values
                .Where(p => p.Elements.Count == 1)
                .OrderBy(p => p.Frequency)
                .ThenBy(p => p.Elements[0])
                .ToList();

            Console.WriteLine("\nSingle element frequencies:");
            foreach (var pattern in singleElementPatterns)
            {
                Console.WriteLine($"  {pattern}");
            }

            // Group by frequency to find potential equivalences
            var groupedByFreq = GroupByFrequency(patterns.Values
                .Where(p => p.Elements.Count == 1 && p.Frequency < 1.0)
                .ToDictionary(p => p.AsString(), p => p));

            if (groupedByFreq.Count > 0)
            {
                Console.WriteLine("\nElements grouped by frequency (potential equivalences):");
                foreach (var group in groupedByFreq.OrderBy(g => g.Key))
                {
                    if (group.Value.Count > 1)
                    {
                        var elements = string.Join(", ", group.Value.Select(p => p.AsString()));
                        Console.WriteLine($"  Frequency {group.Key:F2}: {elements}");
                        Console.WriteLine($"    Interpretation: These could represent the same variable/placeholder");
                    }
                }
            }
        }
    }
}
