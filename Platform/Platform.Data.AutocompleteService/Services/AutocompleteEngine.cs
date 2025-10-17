using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Data.AutocompleteService.Services
{
    public class AutocompleteEngine
    {
        private readonly ILinks<ulong> _links;
        private readonly SequenceIndex<ulong> _index;
        private readonly Dictionary<string, int> _sequenceFrequency;

        public AutocompleteEngine(ILinks<ulong> links, SequenceIndex<ulong> index)
        {
            _links = links;
            _index = index;
            _sequenceFrequency = new Dictionary<string, int>();
        }

        public void IndexText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                IndexLine(line);
            }
        }

        private void IndexLine(string line)
        {
            // Index the entire line as a sequence
            var linkArray = UnicodeMap.FromStringToLinkArray(line);
            _index.Add(linkArray);

            // Track frequency for ranking suggestions
            var normalizedLine = line.Trim();
            if (!string.IsNullOrEmpty(normalizedLine))
            {
                if (_sequenceFrequency.ContainsKey(normalizedLine))
                {
                    _sequenceFrequency[normalizedLine]++;
                }
                else
                {
                    _sequenceFrequency[normalizedLine] = 1;
                }
            }

            // Index word-level sequences for better autocomplete
            var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                // Index individual words
                if (!string.IsNullOrWhiteSpace(words[i]))
                {
                    var wordLinkArray = UnicodeMap.FromStringToLinkArray(words[i]);
                    _index.Add(wordLinkArray);

                    // Track word frequency
                    var normalizedWord = words[i].Trim();
                    if (_sequenceFrequency.ContainsKey(normalizedWord))
                    {
                        _sequenceFrequency[normalizedWord]++;
                    }
                    else
                    {
                        _sequenceFrequency[normalizedWord] = 1;
                    }
                }

                // Index word pairs for context-aware suggestions
                if (i < words.Length - 1 && !string.IsNullOrWhiteSpace(words[i]) && !string.IsNullOrWhiteSpace(words[i + 1]))
                {
                    var wordPair = words[i] + " " + words[i + 1];
                    var pairLinkArray = UnicodeMap.FromStringToLinkArray(wordPair);
                    _index.Add(pairLinkArray);

                    // Track word pair frequency
                    if (_sequenceFrequency.ContainsKey(wordPair))
                    {
                        _sequenceFrequency[wordPair]++;
                    }
                    else
                    {
                        _sequenceFrequency[wordPair] = 1;
                    }
                }
            }
        }

        public List<string> GetSuggestions(string prefix, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return new List<string>();
            }

            // Use frequency dictionary for prefix matching and ranking
            var suggestions = _sequenceFrequency
                .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key)
                .Take(maxResults)
                .Select(kvp => kvp.Key)
                .ToList();

            return suggestions;
        }

        public ulong GetIndexedSequencesCount()
        {
            return _links.Count() - UnicodeMap.MapSize;
        }

        public int GetUniqueSequencesCount()
        {
            return _sequenceFrequency.Count;
        }
    }
}
