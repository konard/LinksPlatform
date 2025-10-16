using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Minimal engine that can generate definitions/descriptions of patterns based on any data.
    /// Can be used for science. Can be used to understand and recreate structural definition of any language.
    /// </summary>
    /// <typeparam name="TLink">The type of the link.</typeparam>
    public class AutopatternsEngine<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<string, PatternDefinition> _discoveredPatterns;

        public AutopatternsEngine(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _discoveredPatterns = new Dictionary<string, PatternDefinition>();
        }

        /// <summary>
        /// Analyzes data and discovers repeating patterns.
        /// </summary>
        /// <param name="data">The data to analyze (can be string, bytes, or any sequence).</param>
        /// <returns>Collection of discovered patterns with their definitions.</returns>
        public IEnumerable<PatternDefinition> DiscoverPatterns(string data)
        {
            if (string.IsNullOrEmpty(data))
                return Enumerable.Empty<PatternDefinition>();

            var patterns = new List<PatternDefinition>();

            // Discover repeating substring patterns
            patterns.AddRange(FindRepeatingSubstrings(data));

            // Discover structural patterns (sequences, repetitions)
            patterns.AddRange(FindStructuralPatterns(data));

            // Discover character class patterns
            patterns.AddRange(FindCharacterClassPatterns(data));

            // Store discovered patterns
            foreach (var pattern in patterns)
            {
                var key = pattern.PatternId;
                if (!_discoveredPatterns.ContainsKey(key))
                {
                    _discoveredPatterns[key] = pattern;
                }
            }

            return patterns;
        }

        /// <summary>
        /// Generates a human-readable definition for a discovered pattern.
        /// </summary>
        /// <param name="pattern">The pattern to describe.</param>
        /// <returns>A text description of the pattern.</returns>
        public string GeneratePatternDefinition(PatternDefinition pattern)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Pattern: {pattern.PatternId}");
            sb.AppendLine($"Type: {pattern.Type}");
            sb.AppendLine($"Frequency: {pattern.Frequency}");
            sb.AppendLine($"Description: {pattern.Description}");

            if (!string.IsNullOrEmpty(pattern.Example))
            {
                sb.AppendLine($"Example: {pattern.Example}");
            }

            if (pattern.Properties.Count > 0)
            {
                sb.AppendLine("Properties:");
                foreach (var prop in pattern.Properties)
                {
                    sb.AppendLine($"  {prop.Key}: {prop.Value}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Analyzes structural relationships in the data using the Links structure.
        /// </summary>
        /// <returns>Patterns representing structural relationships.</returns>
        public IEnumerable<PatternDefinition> AnalyzeStructuralRelationships()
        {
            var patterns = new List<PatternDefinition>();
            var linkCounts = new Dictionary<string, int>();

            // Analyze link patterns in the doublets structure
            _links.Each(link =>
            {
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                var patternKey = $"{source}->{target}";
                linkCounts[patternKey] = linkCounts.ContainsKey(patternKey)
                    ? linkCounts[patternKey] + 1
                    : 1;

                return _links.Constants.Continue;
            });

            // Generate patterns from link analysis
            foreach (var linkPattern in linkCounts.Where(kv => kv.Value > 1))
            {
                patterns.Add(new PatternDefinition
                {
                    PatternId = $"link_{linkPattern.Key}",
                    Type = PatternType.Structural,
                    Frequency = linkPattern.Value,
                    Description = $"Link pattern: {linkPattern.Key}",
                    Example = linkPattern.Key
                });
            }

            return patterns;
        }

        private IEnumerable<PatternDefinition> FindRepeatingSubstrings(string data)
        {
            var patterns = new List<PatternDefinition>();
            var substringCounts = new Dictionary<string, int>();
            var minLength = 2;
            var maxLength = Math.Min(20, data.Length / 2);

            // Find all repeating substrings
            for (int length = minLength; length <= maxLength; length++)
            {
                for (int i = 0; i <= data.Length - length; i++)
                {
                    var substring = data.Substring(i, length);

                    // Skip if only whitespace or single character repeated
                    if (string.IsNullOrWhiteSpace(substring))
                        continue;

                    substringCounts[substring] = substringCounts.ContainsKey(substring)
                        ? substringCounts[substring] + 1
                        : 1;
                }
            }

            // Create patterns for substrings that appear more than once
            foreach (var item in substringCounts.Where(kv => kv.Value > 1).OrderByDescending(kv => kv.Value))
            {
                patterns.Add(new PatternDefinition
                {
                    PatternId = $"substring_{item.Key.GetHashCode()}",
                    Type = PatternType.Repeating,
                    Frequency = item.Value,
                    Description = $"Repeating substring of length {item.Key.Length}",
                    Example = item.Key,
                    Properties = new Dictionary<string, string>
                    {
                        { "length", item.Key.Length.ToString() },
                        { "content", item.Key }
                    }
                });
            }

            return patterns.Take(100); // Limit to top 100 patterns
        }

        private IEnumerable<PatternDefinition> FindStructuralPatterns(string data)
        {
            var patterns = new List<PatternDefinition>();

            // Detect sequences (e.g., "abc", "123")
            var sequences = DetectSequences(data);
            patterns.AddRange(sequences);

            // Detect alternating patterns (e.g., "ababab")
            var alternating = DetectAlternatingPatterns(data);
            patterns.AddRange(alternating);

            return patterns;
        }

        private IEnumerable<PatternDefinition> FindCharacterClassPatterns(string data)
        {
            var patterns = new List<PatternDefinition>();

            var digitCount = data.Count(char.IsDigit);
            var letterCount = data.Count(char.IsLetter);
            var whitespaceCount = data.Count(char.IsWhiteSpace);
            var punctuationCount = data.Count(char.IsPunctuation);

            if (digitCount > 0)
            {
                patterns.Add(new PatternDefinition
                {
                    PatternId = "char_class_digit",
                    Type = PatternType.CharacterClass,
                    Frequency = digitCount,
                    Description = "Digit characters",
                    Example = string.Join("", data.Where(char.IsDigit).Distinct())
                });
            }

            if (letterCount > 0)
            {
                patterns.Add(new PatternDefinition
                {
                    PatternId = "char_class_letter",
                    Type = PatternType.CharacterClass,
                    Frequency = letterCount,
                    Description = "Letter characters",
                    Example = $"Sample: {string.Join("", data.Where(char.IsLetter).Distinct().Take(10))}"
                });
            }

            return patterns;
        }

        private IEnumerable<PatternDefinition> DetectSequences(string data)
        {
            var patterns = new List<PatternDefinition>();
            var minSequenceLength = 3;

            for (int i = 0; i < data.Length - minSequenceLength + 1; i++)
            {
                int seqLength = 1;
                while (i + seqLength < data.Length &&
                       data[i + seqLength] == data[i + seqLength - 1] + 1)
                {
                    seqLength++;
                }

                if (seqLength >= minSequenceLength)
                {
                    var sequence = data.Substring(i, seqLength);
                    patterns.Add(new PatternDefinition
                    {
                        PatternId = $"sequence_{sequence}",
                        Type = PatternType.Sequence,
                        Frequency = 1,
                        Description = $"Sequential pattern of length {seqLength}",
                        Example = sequence
                    });
                }
            }

            return patterns;
        }

        private IEnumerable<PatternDefinition> DetectAlternatingPatterns(string data)
        {
            var patterns = new List<PatternDefinition>();

            // Detect simple alternating patterns (e.g., "ababab")
            for (int i = 0; i < data.Length - 4; i++)
            {
                if (data[i] == data[i + 2] && data[i + 1] == data[i + 3] && data[i] != data[i + 1])
                {
                    int length = 2;
                    while (i + length * 2 < data.Length &&
                           data[i] == data[i + length * 2] &&
                           data[i + 1] == data[i + length * 2 + 1])
                    {
                        length++;
                    }

                    if (length >= 2)
                    {
                        var pattern = $"{data[i]}{data[i + 1]}";
                        patterns.Add(new PatternDefinition
                        {
                            PatternId = $"alternating_{pattern}_{i}",
                            Type = PatternType.Alternating,
                            Frequency = length,
                            Description = $"Alternating pattern repeated {length} times",
                            Example = string.Concat(Enumerable.Repeat(pattern, Math.Min(length, 3)))
                        });
                    }
                }
            }

            return patterns;
        }

        /// <summary>
        /// Gets all discovered patterns.
        /// </summary>
        public IEnumerable<PatternDefinition> GetAllDiscoveredPatterns()
        {
            return _discoveredPatterns.Values;
        }

        /// <summary>
        /// Clears all discovered patterns.
        /// </summary>
        public void ClearPatterns()
        {
            _discoveredPatterns.Clear();
        }
    }

    /// <summary>
    /// Represents a discovered pattern with its definition and properties.
    /// </summary>
    public class PatternDefinition
    {
        public string PatternId { get; set; }
        public PatternType Type { get; set; }
        public int Frequency { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Types of patterns that can be discovered.
    /// </summary>
    public enum PatternType
    {
        Repeating,
        Sequence,
        Alternating,
        Structural,
        CharacterClass,
        Custom
    }
}
