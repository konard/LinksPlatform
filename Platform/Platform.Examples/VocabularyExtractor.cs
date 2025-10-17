using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    /// <summary>
    /// Extracts and analyzes vocabulary from conversation messages
    /// </summary>
    public class VocabularyExtractor
    {
        private readonly HashSet<string> _commonWords;

        public VocabularyExtractor()
        {
            // Common English words to filter out (can be expanded)
            _commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for",
                "of", "with", "by", "from", "as", "is", "was", "are", "were", "be",
                "been", "being", "have", "has", "had", "do", "does", "did", "will",
                "would", "should", "could", "may", "might", "must", "can", "i", "you",
                "he", "she", "it", "we", "they", "them", "this", "that", "these",
                "those", "am", "my", "your", "his", "her", "its", "our", "their"
            };
        }

        /// <summary>
        /// Extracts vocabulary from messages sent by a specific person
        /// </summary>
        /// <param name="messages">All conversation messages</param>
        /// <param name="person">The person whose vocabulary to extract</param>
        /// <returns>Dictionary of words with their frequency counts</returns>
        public Dictionary<string, int> ExtractVocabulary(List<Message> messages, string person)
        {
            var vocabulary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var message in messages)
            {
                if (message.Sender.Equals(person, StringComparison.OrdinalIgnoreCase))
                {
                    var words = ExtractWords(message.Content);

                    foreach (var word in words)
                    {
                        if (!_commonWords.Contains(word) && word.Length > 2)
                        {
                            if (vocabulary.ContainsKey(word))
                            {
                                vocabulary[word]++;
                            }
                            else
                            {
                                vocabulary[word] = 1;
                            }
                        }
                    }
                }
            }

            return vocabulary;
        }

        /// <summary>
        /// Extracts words from text
        /// </summary>
        private List<string> ExtractWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // Remove punctuation and split by whitespace
            var cleanText = Regex.Replace(text, @"[^\w\s]", " ");
            var words = cleanText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Select(w => w.Trim().ToLowerInvariant()).Where(w => !string.IsNullOrEmpty(w)).ToList();
        }

        /// <summary>
        /// Gets the most frequently used words by a person
        /// </summary>
        /// <param name="messages">All conversation messages</param>
        /// <param name="person">The person whose vocabulary to analyze</param>
        /// <param name="topCount">Number of top words to return</param>
        /// <returns>List of words sorted by frequency (descending)</returns>
        public List<KeyValuePair<string, int>> GetTopWords(List<Message> messages, string person, int topCount = 20)
        {
            var vocabulary = ExtractVocabulary(messages, person);
            return vocabulary.OrderByDescending(kvp => kvp.Value)
                .Take(topCount)
                .ToList();
        }

        /// <summary>
        /// Gets unique words used by a person that are not commonly used by others in the conversation
        /// </summary>
        /// <param name="messages">All conversation messages</param>
        /// <param name="person">The person whose unique vocabulary to extract</param>
        /// <returns>Dictionary of unique words with their frequency</returns>
        public Dictionary<string, int> GetUniqueVocabulary(List<Message> messages, string person)
        {
            var personVocabulary = ExtractVocabulary(messages, person);
            var othersVocabulary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var message in messages)
            {
                if (!message.Sender.Equals(person, StringComparison.OrdinalIgnoreCase))
                {
                    var words = ExtractWords(message.Content);

                    foreach (var word in words)
                    {
                        if (!_commonWords.Contains(word) && word.Length > 2)
                        {
                            if (othersVocabulary.ContainsKey(word))
                            {
                                othersVocabulary[word]++;
                            }
                            else
                            {
                                othersVocabulary[word] = 1;
                            }
                        }
                    }
                }
            }

            // Find words unique to the person or rarely used by others
            var uniqueWords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in personVocabulary)
            {
                if (!othersVocabulary.ContainsKey(kvp.Key) ||
                    (othersVocabulary[kvp.Key] < kvp.Value / 2)) // Person uses it significantly more
                {
                    uniqueWords[kvp.Key] = kvp.Value;
                }
            }

            return uniqueWords;
        }

        /// <summary>
        /// Gets vocabulary suggestions for autocomplete, prioritizing words the other person uses
        /// </summary>
        /// <param name="messages">All conversation messages</param>
        /// <param name="person">The person whose vocabulary to prioritize</param>
        /// <param name="prefix">The text prefix to match</param>
        /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
        /// <returns>List of word suggestions sorted by frequency</returns>
        public List<string> GetAutocompleteSuggestions(List<Message> messages, string person, string prefix, int maxSuggestions = 10)
        {
            var vocabulary = ExtractVocabulary(messages, person);
            var lowerPrefix = prefix.ToLowerInvariant();

            return vocabulary
                .Where(kvp => kvp.Key.StartsWith(lowerPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxSuggestions)
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }
}
