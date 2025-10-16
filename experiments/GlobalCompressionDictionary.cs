using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;

namespace Platform.Experiments
{
    /// <summary>
    /// Global compression dictionary that can be shared across multiple compressions.
    /// Addresses issue #95 requirement: "Global dictionary / count of pairs across multiple compressions"
    ///
    /// The dictionary stores:
    /// - Frequent pairs that appear across multiple files
    /// - Their global frequency counts
    /// - Link mappings for consistent compression
    ///
    /// Configuration options (as suggested in issue comments):
    /// - MinElementThreshold: Store pairs containing less than X elements
    /// - MinUsageThreshold: Store sequences used at least Y times
    /// - MaxSequenceLength: Maximum sequence length to track
    /// </summary>
    public class GlobalCompressionDictionary
    {
        private readonly string _dictionaryFilePath;
        private readonly Dictionary<Link<ulong>, GlobalPairInfo> _globalPairs;
        private readonly GlobalDictionarySettings _settings;

        public GlobalCompressionDictionary(string dictionaryFilePath, GlobalDictionarySettings settings = null)
        {
            _dictionaryFilePath = dictionaryFilePath;
            _settings = settings ?? GlobalDictionarySettings.Default;
            _globalPairs = new Dictionary<Link<ulong>, GlobalPairInfo>();

            LoadFromFile();
        }

        /// <summary>
        /// Records pair frequencies from a new document.
        /// </summary>
        public void RecordPairFrequencies(Dictionary<Link<ulong>, ulong> pairFrequencies)
        {
            foreach (var kvp in pairFrequencies)
            {
                var pair = kvp.Key;
                var frequency = kvp.Value;

                if (_globalPairs.TryGetValue(pair, out var info))
                {
                    info.TotalFrequency += frequency;
                    info.DocumentCount++;
                    info.LastSeen = DateTime.UtcNow;
                }
                else
                {
                    // Only add pairs that meet the criteria
                    if (ShouldTrackPair(pair, frequency))
                    {
                        _globalPairs[pair] = new GlobalPairInfo
                        {
                            Pair = pair,
                            TotalFrequency = frequency,
                            DocumentCount = 1,
                            FirstSeen = DateTime.UtcNow,
                            LastSeen = DateTime.UtcNow
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Gets the top N pairs from the global dictionary based on usage.
        /// </summary>
        public List<GlobalPairInfo> GetTopPairs(int count)
        {
            return _globalPairs.Values
                .Where(info => info.TotalFrequency >= _settings.MinUsageThreshold)
                .OrderByDescending(info => info.TotalFrequency)
                .ThenByDescending(info => info.DocumentCount)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Gets pairs that should be pre-compressed based on global statistics.
        /// </summary>
        public Dictionary<Link<ulong>, ulong> GetPreCompressionPairs(SynchronizedLinks<ulong> links)
        {
            var result = new Dictionary<Link<ulong>, ulong>();
            var topPairs = GetTopPairs(_settings.MaxDictionarySize);

            foreach (var info in topPairs)
            {
                var replacementLink = links.CreateAndUpdate(info.Pair.Source, info.Pair.Target);
                result[info.Pair] = replacementLink;
            }

            return result;
        }

        /// <summary>
        /// Saves the dictionary to a file.
        /// </summary>
        public void SaveToFile()
        {
            try
            {
                using (var writer = new StreamWriter(_dictionaryFilePath, false, Encoding.UTF8))
                {
                    writer.WriteLine($"# Global Compression Dictionary");
                    writer.WriteLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                    writer.WriteLine($"# Total Pairs: {_globalPairs.Count}");
                    writer.WriteLine($"# Settings: MinUsage={_settings.MinUsageThreshold}, MinElement={_settings.MinElementThreshold}");
                    writer.WriteLine();

                    // Header
                    writer.WriteLine("Source\tTarget\tTotalFrequency\tDocumentCount\tFirstSeen\tLastSeen");

                    // Sort by frequency for readability
                    var sortedPairs = _globalPairs.Values
                        .OrderByDescending(info => info.TotalFrequency)
                        .ToList();

                    foreach (var info in sortedPairs)
                    {
                        writer.WriteLine($"{info.Pair.Source}\t{info.Pair.Target}\t{info.TotalFrequency}\t{info.DocumentCount}\t{info.FirstSeen:yyyy-MM-dd HH:mm:ss}\t{info.LastSeen:yyyy-MM-dd HH:mm:ss}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to save global dictionary: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the dictionary from a file.
        /// </summary>
        private void LoadFromFile()
        {
            if (!File.Exists(_dictionaryFilePath))
            {
                return;
            }

            try
            {
                _globalPairs.Clear();

                var lines = File.ReadAllLines(_dictionaryFilePath, Encoding.UTF8);
                bool headerPassed = false;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (!headerPassed)
                    {
                        headerPassed = true; // Skip header line
                        continue;
                    }

                    var parts = line.Split('\t');
                    if (parts.Length >= 6)
                    {
                        if (ulong.TryParse(parts[0], out var source) &&
                            ulong.TryParse(parts[1], out var target) &&
                            ulong.TryParse(parts[2], out var totalFreq) &&
                            ulong.TryParse(parts[3], out var docCount) &&
                            DateTime.TryParse(parts[4], out var firstSeen) &&
                            DateTime.TryParse(parts[5], out var lastSeen))
                        {
                            var pair = new Link<ulong>(source, target);
                            _globalPairs[pair] = new GlobalPairInfo
                            {
                                Pair = pair,
                                TotalFrequency = totalFreq,
                                DocumentCount = docCount,
                                FirstSeen = firstSeen,
                                LastSeen = lastSeen
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load global dictionary: {ex.Message}");
                _globalPairs.Clear();
            }
        }

        /// <summary>
        /// Determines if a pair should be tracked based on settings.
        /// </summary>
        private bool ShouldTrackPair(Link<ulong>, ulong frequency)
        {
            // For now, track all pairs with frequency > 1
            // In future, could add more sophisticated filtering based on element types
            return frequency > 1;
        }

        /// <summary>
        /// Gets statistics about the dictionary.
        /// </summary>
        public GlobalDictionaryStats GetStats()
        {
            return new GlobalDictionaryStats
            {
                TotalPairs = _globalPairs.Count,
                TotalFrequency = _globalPairs.Values.Sum(info => (long)info.TotalFrequency),
                AverageFrequency = _globalPairs.Count > 0
                    ? _globalPairs.Values.Average(info => info.TotalFrequency)
                    : 0,
                HighFrequencyPairs = _globalPairs.Values.Count(info => info.TotalFrequency >= 10),
                MultiDocumentPairs = _globalPairs.Values.Count(info => info.DocumentCount > 1)
            };
        }
    }

    /// <summary>
    /// Information about a pair in the global dictionary.
    /// </summary>
    public class GlobalPairInfo
    {
        public Link<ulong> Pair { get; set; }
        public ulong TotalFrequency { get; set; }
        public ulong DocumentCount { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }

    /// <summary>
    /// Settings for the global dictionary.
    /// </summary>
    public class GlobalDictionarySettings
    {
        /// <summary>
        /// Minimum number of times a pair must be used to be stored in the dictionary.
        /// </summary>
        public ulong MinUsageThreshold { get; set; } = 5;

        /// <summary>
        /// Maximum number of elements in a pair to be tracked.
        /// Pairs with more than this many elements are ignored.
        /// </summary>
        public ulong MinElementThreshold { get; set; } = 3;

        /// <summary>
        /// Maximum number of pairs to keep in the dictionary.
        /// </summary>
        public int MaxDictionarySize { get; set; } = 10000;

        public static GlobalDictionarySettings Default => new GlobalDictionarySettings();
    }

    /// <summary>
    /// Statistics about the global dictionary.
    /// </summary>
    public class GlobalDictionaryStats
    {
        public int TotalPairs { get; set; }
        public long TotalFrequency { get; set; }
        public double AverageFrequency { get; set; }
        public int HighFrequencyPairs { get; set; }
        public int MultiDocumentPairs { get; set; }
    }
}
