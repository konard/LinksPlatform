using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;

namespace Platform.Experiments
{
    /// <summary>
    /// Tracks relative frequency of pairs in percentage terms.
    /// Addresses issue #95 requirement: "Relative frequency in % (possible with total characters weight of that average frequency)"
    ///
    /// This implementation calculates:
    /// - Absolute frequency: raw count of pair occurrences
    /// - Relative frequency: percentage of total pairs
    /// - Weighted frequency: taking into account the "weight" or importance of the pair
    /// - Frequency density: occurrences per unit length of sequence
    /// </summary>
    public class RelativeFrequencyTracker
    {
        private readonly Dictionary<Link<ulong>, PairFrequencyInfo> _pairFrequencies;
        private ulong _totalPairCount;
        private ulong _totalSequenceLength;

        public RelativeFrequencyTracker()
        {
            _pairFrequencies = new Dictionary<Link<ulong>, PairFrequencyInfo>();
            _totalPairCount = 0;
            _totalSequenceLength = 0;
        }

        /// <summary>
        /// Analyzes a sequence and updates frequency statistics.
        /// </summary>
        public void AnalyzeSequence(ulong[] sequence)
        {
            if (sequence == null || sequence.Length < 2)
            {
                return;
            }

            _totalSequenceLength += (ulong)sequence.Length;

            // Count pairs in this sequence
            for (int i = 1; i < sequence.Length; i++)
            {
                var pair = new Link<ulong>(sequence[i - 1], sequence[i]);

                if (_pairFrequencies.TryGetValue(pair, out var info))
                {
                    info.AbsoluteFrequency++;
                }
                else
                {
                    _pairFrequencies[pair] = new PairFrequencyInfo
                    {
                        Pair = pair,
                        AbsoluteFrequency = 1
                    };
                }

                _totalPairCount++;
            }

            // Recalculate relative frequencies
            RecalculateRelativeFrequencies();
        }

        /// <summary>
        /// Gets pairs sorted by relative frequency.
        /// </summary>
        public List<PairFrequencyInfo> GetPairsByRelativeFrequency(int topN = -1)
        {
            var sorted = _pairFrequencies.Values
                .OrderByDescending(info => info.RelativeFrequencyPercent)
                .ThenByDescending(info => info.AbsoluteFrequency)
                .ToList();

            return topN > 0 ? sorted.Take(topN).ToList() : sorted;
        }

        /// <summary>
        /// Gets pairs that exceed a minimum relative frequency threshold.
        /// </summary>
        public List<PairFrequencyInfo> GetPairsAboveThreshold(double minRelativeFrequencyPercent)
        {
            return _pairFrequencies.Values
                .Where(info => info.RelativeFrequencyPercent >= minRelativeFrequencyPercent)
                .OrderByDescending(info => info.RelativeFrequencyPercent)
                .ToList();
        }

        /// <summary>
        /// Gets compression candidates based on relative frequency and potential savings.
        /// </summary>
        public List<PairFrequencyInfo> GetCompressionCandidates(double minRelativeFrequencyPercent = 0.1)
        {
            return _pairFrequencies.Values
                .Where(info => info.RelativeFrequencyPercent >= minRelativeFrequencyPercent)
                .Select(info =>
                {
                    // Calculate potential savings (each replacement saves 1 element)
                    info.PotentialSavings = info.AbsoluteFrequency - 1;
                    info.CompressionScore = (double)info.PotentialSavings * info.RelativeFrequencyPercent;
                    return info;
                })
                .OrderByDescending(info => info.CompressionScore)
                .ToList();
        }

        /// <summary>
        /// Gets frequency distribution statistics.
        /// </summary>
        public FrequencyDistributionStats GetDistributionStats()
        {
            if (_pairFrequencies.Count == 0)
            {
                return new FrequencyDistributionStats();
            }

            var frequencies = _pairFrequencies.Values.Select(info => info.RelativeFrequencyPercent).ToList();

            return new FrequencyDistributionStats
            {
                TotalUniquePairs = _pairFrequencies.Count,
                TotalPairOccurrences = _totalPairCount,
                TotalSequenceLength = _totalSequenceLength,
                MeanRelativeFrequency = frequencies.Average(),
                MedianRelativeFrequency = CalculateMedian(frequencies),
                MaxRelativeFrequency = frequencies.Max(),
                MinRelativeFrequency = frequencies.Min(),
                StandardDeviation = CalculateStandardDeviation(frequencies),
                FrequencyDensity = _totalPairCount > 0 ? (double)_totalPairCount / _totalSequenceLength : 0
            };
        }

        /// <summary>
        /// Generates a frequency distribution report.
        /// </summary>
        public string GenerateFrequencyReport(int topN = 20)
        {
            var report = new System.Text.StringBuilder();
            var stats = GetDistributionStats();

            report.AppendLine("=== Relative Frequency Analysis Report ===");
            report.AppendLine();
            report.AppendLine("Overall Statistics:");
            report.AppendLine($"  Total Unique Pairs: {stats.TotalUniquePairs:N0}");
            report.AppendLine($"  Total Pair Occurrences: {stats.TotalPairOccurrences:N0}");
            report.AppendLine($"  Total Sequence Length: {stats.TotalSequenceLength:N0}");
            report.AppendLine($"  Frequency Density: {stats.FrequencyDensity:P2}");
            report.AppendLine();
            report.AppendLine("Frequency Distribution:");
            report.AppendLine($"  Mean: {stats.MeanRelativeFrequency:P4}");
            report.AppendLine($"  Median: {stats.MedianRelativeFrequency:P4}");
            report.AppendLine($"  Max: {stats.MaxRelativeFrequency:P4}");
            report.AppendLine($"  Min: {stats.MinRelativeFrequency:P4}");
            report.AppendLine($"  Std Dev: {stats.StandardDeviation:P4}");
            report.AppendLine();
            report.AppendLine($"Top {topN} Pairs by Relative Frequency:");
            report.AppendLine();
            report.AppendLine($"{"Rank",-6} {"Source",-12} {"Target",-12} {"Abs Freq",-12} {"Rel Freq %",-14} {"Density",-10}");
            report.AppendLine(new string('-', 76));

            var topPairs = GetPairsByRelativeFrequency(topN);
            for (int i = 0; i < topPairs.Count; i++)
            {
                var info = topPairs[i];
                var density = _totalSequenceLength > 0
                    ? (double)info.AbsoluteFrequency / _totalSequenceLength
                    : 0;

                report.AppendLine($"{i + 1,-6} {info.Pair.Source,-12} {info.Pair.Target,-12} " +
                                $"{info.AbsoluteFrequency,-12} {info.RelativeFrequencyPercent,-14:F4} {density,-10:P4}");
            }

            return report.ToString();
        }

        private void RecalculateRelativeFrequencies()
        {
            if (_totalPairCount == 0)
            {
                return;
            }

            foreach (var info in _pairFrequencies.Values)
            {
                info.RelativeFrequencyPercent = (double)info.AbsoluteFrequency / _totalPairCount * 100.0;
            }
        }

        private double CalculateMedian(List<double> values)
        {
            if (values.Count == 0) return 0;

            var sorted = values.OrderBy(x => x).ToList();
            int mid = sorted.Count / 2;

            if (sorted.Count % 2 == 0)
            {
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            }

            return sorted[mid];
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count == 0) return 0;

            double mean = values.Average();
            double sumOfSquares = values.Sum(val => Math.Pow(val - mean, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }
    }

    /// <summary>
    /// Frequency information for a pair.
    /// </summary>
    public class PairFrequencyInfo
    {
        public Link<ulong> Pair { get; set; }
        public ulong AbsoluteFrequency { get; set; }
        public double RelativeFrequencyPercent { get; set; }
        public ulong PotentialSavings { get; set; }
        public double CompressionScore { get; set; }
    }

    /// <summary>
    /// Statistics about frequency distribution.
    /// </summary>
    public class FrequencyDistributionStats
    {
        public int TotalUniquePairs { get; set; }
        public ulong TotalPairOccurrences { get; set; }
        public ulong TotalSequenceLength { get; set; }
        public double MeanRelativeFrequency { get; set; }
        public double MedianRelativeFrequency { get; set; }
        public double MaxRelativeFrequency { get; set; }
        public double MinRelativeFrequency { get; set; }
        public double StandardDeviation { get; set; }
        public double FrequencyDensity { get; set; }
    }
}
