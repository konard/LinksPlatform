using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Converters;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Provides autocomplete functionality for sequences stored in doublets links storage.
    /// Supports word-level, sentence-level, and fuzzy (typo-tolerant) completion.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    public class SequenceAutocomplete<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly SequenceIndex<TLink> _index;
        private readonly IConverter<string, TLink> _stringToLinkConverter;
        private readonly IConverter<TLink, string> _linkToStringConverter;

        public SequenceAutocomplete(
            ILinks<TLink> links,
            SequenceIndex<TLink> index,
            IConverter<string, TLink> stringToLinkConverter,
            IConverter<TLink, string> linkToStringConverter)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _index = index ?? throw new ArgumentNullException(nameof(index));
            _stringToLinkConverter = stringToLinkConverter ?? throw new ArgumentNullException(nameof(stringToLinkConverter));
            _linkToStringConverter = linkToStringConverter ?? throw new ArgumentNullException(nameof(linkToStringConverter));
        }

        /// <summary>
        /// Completes the input prefix up to the end of the current word.
        /// </summary>
        /// <param name="prefix">The input prefix to complete.</param>
        /// <param name="maxResults">Maximum number of completion suggestions to return.</param>
        /// <returns>List of word completions.</returns>
        public List<string> CompleteWord(string prefix, int maxResults = 10)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return new List<string>();
            }

            var prefixLinks = ConvertStringToLinkArray(prefix);
            var completions = new List<string>();

            // Search for sequences that start with the prefix
            var matches = FindSequencesStartingWith(prefixLinks);

            foreach (var match in matches.Take(maxResults))
            {
                try
                {
                    var completionText = _linkToStringConverter.Convert(match);

                    // Extract just the word (up to first whitespace after prefix)
                    var wordEnd = completionText.IndexOfAny(new[] { ' ', '\n', '\t', '\r' }, prefix.Length);
                    var word = wordEnd >= 0 ? completionText.Substring(0, wordEnd) : completionText;

                    if (!completions.Contains(word) && word.Length > prefix.Length)
                    {
                        completions.Add(word);
                    }
                }
                catch
                {
                    // Skip sequences that cannot be converted to strings
                    continue;
                }
            }

            return completions;
        }

        /// <summary>
        /// Completes the input prefix up to the end of the sentence.
        /// </summary>
        /// <param name="prefix">The input prefix to complete.</param>
        /// <param name="maxResults">Maximum number of completion suggestions to return.</param>
        /// <returns>List of sentence completions.</returns>
        public List<string> CompleteSentence(string prefix, int maxResults = 10)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return new List<string>();
            }

            var prefixLinks = ConvertStringToLinkArray(prefix);
            var completions = new List<string>();

            // Search for sequences that start with the prefix
            var matches = FindSequencesStartingWith(prefixLinks);

            foreach (var match in matches.Take(maxResults))
            {
                try
                {
                    var completionText = _linkToStringConverter.Convert(match);

                    // Extract up to sentence ending punctuation
                    var sentenceEnd = completionText.IndexOfAny(new[] { '.', '!', '?', '\n' }, prefix.Length);
                    var sentence = sentenceEnd >= 0 ? completionText.Substring(0, sentenceEnd + 1) : completionText;

                    if (!completions.Contains(sentence) && sentence.Length > prefix.Length)
                    {
                        completions.Add(sentence);
                    }
                }
                catch
                {
                    // Skip sequences that cannot be converted to strings
                    continue;
                }
            }

            return completions;
        }

        /// <summary>
        /// Performs fuzzy autocomplete to correct typos and find similar sequences.
        /// Uses approximate string matching based on edit distance.
        /// </summary>
        /// <param name="input">The input text that may contain typos.</param>
        /// <param name="maxResults">Maximum number of completion suggestions to return.</param>
        /// <param name="maxEditDistance">Maximum allowed edit distance (default: 2).</param>
        /// <returns>List of fuzzy-matched completions.</returns>
        public List<string> FuzzyComplete(string input, int maxResults = 10, int maxEditDistance = 2)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            var candidates = new Dictionary<string, int>(); // completion -> edit distance

            // Try to find sequences with similar prefixes
            // For fuzzy matching, we'll generate variations of the input
            var variations = GenerateFuzzyVariations(input, maxEditDistance);

            foreach (var variation in variations)
            {
                try
                {
                    var variationLinks = ConvertStringToLinkArray(variation);
                    var matches = FindSequencesStartingWith(variationLinks);

                    foreach (var match in matches)
                    {
                        try
                        {
                            var completionText = _linkToStringConverter.Convert(match);

                            // Calculate edit distance between input and the found sequence
                            var distance = CalculateEditDistance(input, completionText.Substring(0, Math.Min(input.Length, completionText.Length)));

                            if (distance <= maxEditDistance)
                            {
                                if (!candidates.ContainsKey(completionText) || candidates[completionText] > distance)
                                {
                                    candidates[completionText] = distance;
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            // Return results sorted by edit distance (closest matches first)
            return candidates
                .OrderBy(kvp => kvp.Value)
                .Take(maxResults)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Finds sequences in the index that start with the given prefix.
        /// Note: This is a conceptual implementation. A full implementation would require
        /// using the SequenceIndex API to search for partial matches.
        /// The actual search would involve traversing the links structure to find
        /// all sequences that begin with the prefix pattern.
        /// </summary>
        private List<TLink> FindSequencesStartingWith(TLink[] prefixLinks)
        {
            var results = new List<TLink>();

            // Conceptual implementation:
            // 1. Use SequenceIndex to find the first link in the prefix
            // 2. Walk the links structure to find sequences starting with that pattern
            // 3. Filter sequences that match the full prefix
            // 4. Return matching sequence link identifiers

            // For a complete implementation, integrate with Platform.Data.Doublets.Sequences
            // search capabilities once the appropriate API methods are available

            return results;
        }

        /// <summary>
        /// Collects sequences that contain the given prefix sequence.
        /// </summary>
        private void CollectSequencesContainingPrefix(TLink prefixSequence, List<TLink> results, int maxDepth = 100)
        {
            if (results.Count >= maxDepth)
            {
                return;
            }

            // Walk through links that have the prefix sequence as source or target
            _links.Each(link =>
            {
                var linkIndex = _links.GetIndex(link);
                var source = _links.GetSource(link);
                var target = _links.GetTarget(link);

                // Check if this link extends the prefix sequence
                if (EqualityComparer<TLink>.Default.Equals(source, prefixSequence))
                {
                    results.Add(linkIndex);
                }

                return _links.Constants.Continue;
            }, _links.Constants.Any, prefixSequence);
        }

        /// <summary>
        /// Converts a string to an array of link identifiers.
        /// </summary>
        private TLink[] ConvertStringToLinkArray(string text)
        {
            var result = _stringToLinkConverter.Convert(text);

            // If the converter returns a single link representing the sequence,
            // we need to decompose it into individual character links
            // This depends on the converter implementation
            return new TLink[] { result };
        }

        /// <summary>
        /// Generates fuzzy variations of the input string by simulating common typos.
        /// </summary>
        private List<string> GenerateFuzzyVariations(string input, int maxEditDistance)
        {
            var variations = new HashSet<string> { input };

            if (maxEditDistance <= 0 || string.IsNullOrEmpty(input))
            {
                return variations.ToList();
            }

            // Generate single-character variations (deletion, substitution, insertion)
            for (int i = 0; i < input.Length; i++)
            {
                // Deletion
                if (input.Length > 1)
                {
                    variations.Add(input.Remove(i, 1));
                }

                // Substitution with adjacent keyboard keys (simplified)
                foreach (var replacement in GetAdjacentKeys(input[i]))
                {
                    variations.Add(input.Substring(0, i) + replacement + input.Substring(i + 1));
                }

                // Transposition (swap with next character)
                if (i < input.Length - 1)
                {
                    var chars = input.ToCharArray();
                    var temp = chars[i];
                    chars[i] = chars[i + 1];
                    chars[i + 1] = temp;
                    variations.Add(new string(chars));
                }
            }

            // For maxEditDistance > 1, recursively generate variations
            // (limited to avoid combinatorial explosion)
            if (maxEditDistance > 1 && variations.Count < 50)
            {
                var secondLevelVariations = new List<string>();
                foreach (var variation in variations.ToList())
                {
                    if (variation != input)
                    {
                        secondLevelVariations.AddRange(GenerateFuzzyVariations(variation, maxEditDistance - 1));
                    }
                }
                foreach (var v in secondLevelVariations)
                {
                    variations.Add(v);
                }
            }

            return variations.Take(50).ToList(); // Limit to prevent excessive computation
        }

        /// <summary>
        /// Returns characters adjacent to the given character on a QWERTY keyboard.
        /// </summary>
        private List<char> GetAdjacentKeys(char c)
        {
            // Simplified keyboard adjacency map
            var adjacencyMap = new Dictionary<char, string>
            {
                {'a', "qwsz"}, {'b', "vghn"}, {'c', "xdfv"}, {'d', "erfcxs"}, {'e', "wrsd"},
                {'f', "rtgvcd"}, {'g', "tyhbvf"}, {'h', "yujnbg"}, {'i', "ujko"}, {'j', "uikmnh"},
                {'k', "ijolm"}, {'l', "kop"}, {'m', "njk"}, {'n', "bhjm"}, {'o', "iklp"},
                {'p', "ol"}, {'q', "wa"}, {'r', "etfd"}, {'s', "wedxza"}, {'t', "ryfg"},
                {'u', "yihj"}, {'v', "cfgb"}, {'w', "qesa"}, {'x', "zsdc"}, {'y', "tuhi"},
                {'z', "asx"}
            };

            var lower = char.ToLower(c);
            if (adjacencyMap.ContainsKey(lower))
            {
                return adjacencyMap[lower].ToList();
            }

            return new List<char>();
        }

        /// <summary>
        /// Calculates the Levenshtein edit distance between two strings.
        /// </summary>
        private int CalculateEditDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            }

            if (string.IsNullOrEmpty(s2))
            {
                return s1.Length;
            }

            var len1 = s1.Length;
            var len2 = s2.Length;
            var matrix = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= len2; j++)
            {
                matrix[0, j] = j;
            }

            for (int i = 1; i <= len1; i++)
            {
                for (int j = 1; j <= len2; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[len1, len2];
        }
    }
}
