using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    /// <summary>
    /// Algorithm to classify words from an explanatory dictionary as basic or compound.
    ///
    /// A basic word is defined as a word that cannot be explained by other basic words
    /// without referencing itself. This means its definition is inherently self-referential.
    ///
    /// Algorithm:
    /// 1. Parse dictionary definitions for all words
    /// 2. Build a directed graph where edges represent "word A is used in definition of word B"
    /// 3. Identify strongly connected components (cycles) in the graph using Tarjan's algorithm
    /// 4. Words in cycles (especially self-loops) are candidates for basic words
    /// 5. Words that can be fully explained without cycles are compound words
    /// </summary>
    public class BasicWordsFinder
    {
        private readonly Dictionary<string, string> _dictionary;
        private readonly Dictionary<string, HashSet<string>> _wordReferences;
        private readonly HashSet<string> _stopwords;

        public BasicWordsFinder()
        {
            _dictionary = new Dictionary<string, string>();
            _wordReferences = new Dictionary<string, HashSet<string>>();

            // Common stopwords that don't carry meaning
            // Note: We keep words like "be", "exist", "have" etc. as they might be basic words
            _stopwords = new HashSet<string>
            {
                "a", "an", "the", "was", "were", "been",
                "do", "does", "did", "will", "would", "could",
                "should", "may", "might", "must", "can", "to", "of", "in", "for",
                "on", "at", "by", "with", "from", "as", "that", "which", "who",
                "or", "and", "but", "if", "than", "when", "where", "how", "what",
                "there", "here", "this", "these", "those", "it", "its", "itself",
                "one", "ones", "any", "some", "such", "no", "not", "only", "own",
                "same", "so", "too", "very"
            };
        }

        /// <summary>
        /// Add a word and its definition to the dictionary
        /// </summary>
        public void AddDefinition(string word, string definition)
        {
            var normalizedWord = word.ToLower().Trim();
            _dictionary[normalizedWord] = definition.ToLower();
        }

        /// <summary>
        /// Extract meaningful words from a definition (excluding common stopwords)
        /// </summary>
        private HashSet<string> ExtractWordsFromDefinition(string definition)
        {
            // Simple word extraction - match alphabetic words
            var matches = Regex.Matches(definition.ToLower(), @"\b[a-z]+\b");
            var words = new HashSet<string>();

            foreach (Match match in matches)
            {
                var word = match.Value;
                if (!_stopwords.Contains(word) && _dictionary.ContainsKey(word))
                {
                    words.Add(word);
                }
            }

            return words;
        }

        /// <summary>
        /// Build a directed graph of word references in definitions
        /// </summary>
        private void BuildReferenceGraph()
        {
            _wordReferences.Clear();
            foreach (var kvp in _dictionary)
            {
                var word = kvp.Key;
                var definition = kvp.Value;
                _wordReferences[word] = ExtractWordsFromDefinition(definition);
            }
        }

        /// <summary>
        /// Find words that reference themselves in their definitions
        /// </summary>
        private HashSet<string> FindSelfReferentialWords()
        {
            var selfReferential = new HashSet<string>();
            foreach (var kvp in _wordReferences)
            {
                if (kvp.Value.Contains(kvp.Key))
                {
                    selfReferential.Add(kvp.Key);
                }
            }
            return selfReferential;
        }

        /// <summary>
        /// Find strongly connected components using Tarjan's algorithm
        /// </summary>
        private List<HashSet<string>> FindStronglyConnectedComponents()
        {
            var index = 0;
            var stack = new Stack<string>();
            var indices = new Dictionary<string, int>();
            var lowlinks = new Dictionary<string, int>();
            var onStack = new HashSet<string>();
            var sccs = new List<HashSet<string>>();

            void StrongConnect(string node)
            {
                indices[node] = index;
                lowlinks[node] = index;
                index++;
                stack.Push(node);
                onStack.Add(node);

                // Consider successors of node
                if (_wordReferences.TryGetValue(node, out var successors))
                {
                    foreach (var successor in successors)
                    {
                        if (!indices.ContainsKey(successor))
                        {
                            StrongConnect(successor);
                            lowlinks[node] = Math.Min(lowlinks[node], lowlinks[successor]);
                        }
                        else if (onStack.Contains(successor))
                        {
                            lowlinks[node] = Math.Min(lowlinks[node], indices[successor]);
                        }
                    }
                }

                // If node is a root node, pop the stack and generate an SCC
                if (lowlinks[node] == indices[node])
                {
                    var component = new HashSet<string>();
                    string w;
                    do
                    {
                        w = stack.Pop();
                        onStack.Remove(w);
                        component.Add(w);
                    } while (w != node);

                    sccs.Add(component);
                }
            }

            foreach (var node in _dictionary.Keys)
            {
                if (!indices.ContainsKey(node))
                {
                    StrongConnect(node);
                }
            }

            return sccs;
        }

        /// <summary>
        /// Classify words as basic or compound
        /// </summary>
        /// <returns>Tuple of (basicWords, compoundWords)</returns>
        public (HashSet<string> basicWords, HashSet<string> compoundWords) ClassifyWords()
        {
            BuildReferenceGraph();

            // Find self-referential words
            var selfReferential = FindSelfReferentialWords();

            // Find strongly connected components (cyclic dependencies)
            var sccs = FindStronglyConnectedComponents();

            // Words in cycles (SCCs with size > 1 or self-loops) are basic
            var basicWords = new HashSet<string>(selfReferential);

            // Add words from non-trivial strongly connected components
            foreach (var component in sccs)
            {
                if (component.Count > 1)
                {
                    basicWords.UnionWith(component);
                }
            }

            // Words that can be defined without cycles are compound
            var compoundWords = new HashSet<string>(_dictionary.Keys);
            compoundWords.ExceptWith(basicWords);

            return (basicWords, compoundWords);
        }

        /// <summary>
        /// Get all word references for analysis
        /// </summary>
        public Dictionary<string, HashSet<string>> GetWordReferences()
        {
            if (_wordReferences.Count == 0)
            {
                BuildReferenceGraph();
            }
            return new Dictionary<string, HashSet<string>>(_wordReferences);
        }

        /// <summary>
        /// Get the total number of words in the dictionary
        /// </summary>
        public int WordCount => _dictionary.Count;
    }
}
