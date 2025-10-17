using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Sandbox
{
    /// <summary>
    /// Implements frequency-based pattern recognition as described in issue #652.
    ///
    /// Key concepts:
    /// 1. Frequency of occurrence serves as a hash function
    /// 2. Elements with equal frequency may have equal/similar meaning or share common context
    /// 3. Fragments with same frequency in source text may correspond in translations
    /// 4. When fragments with equal frequency surround a text part, that part is a variable in a template
    /// </summary>
    public class FrequencyBasedPatternRecognition
    {
        /// <summary>
        /// Represents a text fragment with its frequency count
        /// </summary>
        public class Fragment
        {
            public string Text { get; set; }
            public int Frequency { get; set; }
            public List<int> Positions { get; set; }

            public Fragment()
            {
                Positions = new List<int>();
            }

            public override string ToString() => $"'{Text}' (freq: {Frequency})";
        }

        /// <summary>
        /// Represents a detected pattern/template in text
        /// </summary>
        public class Pattern
        {
            public Fragment LeftConstant { get; set; }
            public Fragment RightConstant { get; set; }
            public List<string> Variables { get; set; }
            public int Frequency { get; set; }

            public Pattern()
            {
                Variables = new List<string>();
            }

            public override string ToString()
            {
                var vars = string.Join(", ", Variables.Select(v => $"'{v}'"));
                return $"Pattern (freq: {Frequency}): [{LeftConstant?.Text}] <variable: {vars}> [{RightConstant?.Text}]";
            }
        }

        private readonly int _minFragmentLength;
        private readonly int _maxFragmentLength;

        public FrequencyBasedPatternRecognition(int minFragmentLength = 2, int maxFragmentLength = 50)
        {
            _minFragmentLength = minFragmentLength;
            _maxFragmentLength = maxFragmentLength;
        }

        /// <summary>
        /// Calculates frequency of all text fragments
        /// </summary>
        public Dictionary<string, Fragment> CalculateFragmentFrequencies(string text)
        {
            var fragments = new Dictionary<string, Fragment>();

            // Iterate through all possible fragment lengths
            for (int length = _minFragmentLength; length <= Math.Min(_maxFragmentLength, text.Length); length++)
            {
                // Iterate through all positions in text
                for (int pos = 0; pos <= text.Length - length; pos++)
                {
                    string fragmentText = text.Substring(pos, length);

                    if (!fragments.ContainsKey(fragmentText))
                    {
                        fragments[fragmentText] = new Fragment
                        {
                            Text = fragmentText,
                            Frequency = 0
                        };
                    }

                    fragments[fragmentText].Frequency++;
                    fragments[fragmentText].Positions.Add(pos);
                }
            }

            return fragments;
        }

        /// <summary>
        /// Groups fragments by their frequency (frequency as hash function)
        /// </summary>
        public Dictionary<int, List<Fragment>> GroupByFrequency(Dictionary<string, Fragment> fragments)
        {
            var frequencyGroups = new Dictionary<int, List<Fragment>>();

            foreach (var fragment in fragments.Values)
            {
                if (!frequencyGroups.ContainsKey(fragment.Frequency))
                {
                    frequencyGroups[fragment.Frequency] = new List<Fragment>();
                }
                frequencyGroups[fragment.Frequency].Add(fragment);
            }

            return frequencyGroups;
        }

        /// <summary>
        /// Detects patterns where variable parts are surrounded by constant parts with equal frequency
        /// </summary>
        public List<Pattern> DetectPatterns(string text, Dictionary<string, Fragment> fragments, int minPatternFrequency = 2)
        {
            var patterns = new List<Pattern>();
            var frequencyGroups = GroupByFrequency(fragments);

            // For each frequency group (potential constants)
            foreach (var freq in frequencyGroups.Keys.Where(f => f >= minPatternFrequency).OrderByDescending(f => f))
            {
                var constantCandidates = frequencyGroups[freq];

                // Try to find patterns where these fragments act as constants
                foreach (var leftConstant in constantCandidates)
                {
                    foreach (var rightConstant in constantCandidates)
                    {
                        if (leftConstant == rightConstant) continue;

                        // Check if these constants appear together with variable parts in between
                        var pattern = FindPatternInstances(text, leftConstant, rightConstant);

                        if (pattern != null && pattern.Frequency >= minPatternFrequency)
                        {
                            patterns.Add(pattern);
                        }
                    }
                }
            }

            return patterns;
        }

        /// <summary>
        /// Finds instances where left and right constants surround variable parts
        /// </summary>
        private Pattern FindPatternInstances(string text, Fragment leftConstant, Fragment rightConstant)
        {
            var pattern = new Pattern
            {
                LeftConstant = leftConstant,
                RightConstant = rightConstant,
                Frequency = 0
            };

            // For each occurrence of left constant
            foreach (var leftPos in leftConstant.Positions)
            {
                int searchStartPos = leftPos + leftConstant.Text.Length;

                // Look for right constant after left constant
                foreach (var rightPos in rightConstant.Positions)
                {
                    if (rightPos > searchStartPos)
                    {
                        // Extract variable part between constants
                        int variableStart = searchStartPos;
                        int variableLength = rightPos - searchStartPos;

                        if (variableLength > 0 && variableLength < text.Length / 2) // Reasonable variable length
                        {
                            string variable = text.Substring(variableStart, variableLength);
                            pattern.Variables.Add(variable);
                            pattern.Frequency++;
                        }

                        break; // Use first matching right constant
                    }
                }
            }

            return pattern.Frequency > 0 ? pattern : null;
        }

        /// <summary>
        /// Analyzes text and prints frequency statistics
        /// </summary>
        public void AnalyzeText(string text)
        {
            Console.WriteLine($"=== Frequency-Based Pattern Recognition Analysis ===");
            Console.WriteLine($"Text length: {text.Length} characters");
            Console.WriteLine();

            // Calculate frequencies
            Console.WriteLine("Calculating fragment frequencies...");
            var fragments = CalculateFragmentFrequencies(text);
            Console.WriteLine($"Total unique fragments: {fragments.Count}");
            Console.WriteLine();

            // Show top fragments by frequency
            Console.WriteLine("Top 20 most frequent fragments:");
            var topFragments = fragments.Values
                .OrderByDescending(f => f.Frequency)
                .ThenBy(f => f.Text.Length)
                .Take(20)
                .ToList();

            foreach (var fragment in topFragments)
            {
                Console.WriteLine($"  {fragment} - Positions: {string.Join(", ", fragment.Positions.Take(5))}{(fragment.Positions.Count > 5 ? "..." : "")}");
            }
            Console.WriteLine();

            // Group by frequency
            var frequencyGroups = GroupByFrequency(fragments);
            Console.WriteLine($"Frequency distribution: {frequencyGroups.Count} different frequency values");
            Console.WriteLine("Top 10 frequency groups:");
            foreach (var group in frequencyGroups.OrderByDescending(g => g.Key).Take(10))
            {
                Console.WriteLine($"  Frequency {group.Key}: {group.Value.Count} fragments");
            }
            Console.WriteLine();

            // Detect patterns
            Console.WriteLine("Detecting patterns...");
            var patterns = DetectPatterns(text, fragments, minPatternFrequency: 2);
            Console.WriteLine($"Found {patterns.Count} potential patterns");
            Console.WriteLine();

            // Show top patterns
            Console.WriteLine("Top 10 patterns:");
            foreach (var pattern in patterns.OrderByDescending(p => p.Frequency).Take(10))
            {
                Console.WriteLine($"  {pattern}");
                if (pattern.Variables.Count > 0)
                {
                    Console.WriteLine($"    Examples: {string.Join(" | ", pattern.Variables.Take(3))}");
                }
            }
        }

        /// <summary>
        /// Finds potential translations by comparing frequency patterns
        /// </summary>
        public Dictionary<string, string> FindTranslationCandidates(string sourceText, string targetText, int topN = 10)
        {
            Console.WriteLine("=== Translation Candidate Detection ===");

            var sourceFragments = CalculateFragmentFrequencies(sourceText);
            var targetFragments = CalculateFragmentFrequencies(targetText);

            var sourceByFreq = GroupByFrequency(sourceFragments);
            var targetByFreq = GroupByFrequency(targetFragments);

            var candidates = new Dictionary<string, string>();

            // Match fragments with same frequency
            foreach (var freq in sourceByFreq.Keys)
            {
                if (targetByFreq.ContainsKey(freq))
                {
                    var sourceFrags = sourceByFreq[freq];
                    var targetFrags = targetByFreq[freq];

                    // If frequencies match and counts are similar, these might be translations
                    if (sourceFrags.Count == targetFrags.Count)
                    {
                        for (int i = 0; i < Math.Min(sourceFrags.Count, targetFrags.Count); i++)
                        {
                            if (candidates.Count >= topN) break;
                            candidates[sourceFrags[i].Text] = targetFrags[i].Text;
                        }
                    }
                }
            }

            Console.WriteLine($"Found {candidates.Count} translation candidates based on frequency matching");
            foreach (var pair in candidates.Take(topN))
            {
                Console.WriteLine($"  '{pair.Key}' <-> '{pair.Value}'");
            }

            return candidates;
        }

        /// <summary>
        /// Example demonstrating the frequency-based pattern recognition
        /// </summary>
        public static void RunExample()
        {
            var recognizer = new FrequencyBasedPatternRecognition(minFragmentLength: 2, maxFragmentLength: 20);

            // Example 1: Russian text from the issue
            string exampleText = @"То,что интересуется музыкальные коллапсы раз означает.
То,что интересуется более справки,то.е. и и более раз,можем определить шаблон.
харе Кришна харе Кришна Кришна Кришна харе харе
харе Рама харе Рама Рама Рама харе харе";

            recognizer.AnalyzeText(exampleText);
            Console.WriteLine();
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine();

            // Example 2: Pattern with variables
            string patternText = @"The cat sat on the mat. The dog sat on the rug. The bird sat on the branch.";
            recognizer.AnalyzeText(patternText);
            Console.WriteLine();
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine();

            // Example 3: Translation detection (hypothetical)
            string english = "the cat sat on the mat and the dog sat on the rug";
            string russian = "кошка сидела на коврике и собака сидела на ковре";
            recognizer.FindTranslationCandidates(english, russian);
        }
    }
}
