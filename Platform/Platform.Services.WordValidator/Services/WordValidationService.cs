using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.Services.WordValidator.Services
{
    public class WordValidationService : IWordValidationService
    {
        private readonly HashSet<string> _words;

        public WordValidationService()
        {
            _words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            LoadWords();
        }

        private void LoadWords()
        {
            var dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "words.txt");

            if (File.Exists(dictionaryPath))
            {
                var lines = File.ReadAllLines(dictionaryPath);
                foreach (var line in lines)
                {
                    var word = line.Trim();
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        _words.Add(word);
                    }
                }
            }
            else
            {
                // Fallback: Add some basic common words if dictionary file doesn't exist
                LoadDefaultWords();
            }
        }

        private void LoadDefaultWords()
        {
            // Basic English words for demonstration
            var basicWords = new[]
            {
                "hello", "world", "word", "test", "example", "sample",
                "the", "a", "an", "is", "are", "was", "were", "be", "been",
                "have", "has", "had", "do", "does", "did", "will", "would",
                "can", "could", "may", "might", "shall", "should", "must",
                "and", "or", "but", "not", "in", "on", "at", "to", "for",
                "of", "with", "by", "from", "about", "into", "through",
                "during", "before", "after", "above", "below", "between",
                "i", "you", "he", "she", "it", "we", "they", "me", "him",
                "her", "us", "them", "my", "your", "his", "its", "our", "their",
                "this", "that", "these", "those", "here", "there", "where",
                "when", "why", "how", "what", "which", "who", "whom", "whose",
                "one", "two", "three", "four", "five", "six", "seven", "eight",
                "nine", "ten", "first", "second", "third", "last", "next",
                "good", "bad", "big", "small", "long", "short", "high", "low",
                "new", "old", "young", "right", "left", "up", "down", "yes", "no",
                "time", "person", "year", "way", "day", "thing", "man", "woman",
                "child", "life", "work", "part", "place", "case", "point",
                "government", "company", "number", "group", "problem", "fact"
            };

            foreach (var word in basicWords)
            {
                _words.Add(word);
            }
        }

        public bool IsWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            // Normalize the word (trim and convert to lowercase for comparison)
            var normalizedWord = word.Trim();

            return _words.Contains(normalizedWord);
        }
    }
}
