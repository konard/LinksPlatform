using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Platform.Benchmarks
{
    /// <summary>
    /// Performance benchmarks comparing methods with and without [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// to validate the actual impact of this attribute on performance.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net80, baseline: true)]
    [MemoryDiagnoser]
    [MarkdownExporter]
    public class AggressiveInliningBenchmarks
    {
        // Test data
        private char[] _testChars;
        private Dictionary<Link<ulong>, ulong> _frequenciesWithInlining;
        private Dictionary<Link<ulong>, ulong> _frequenciesWithoutInlining;
        private Link<ulong>[] _testDoublets;
        private ulong _maxFrequency;
        private Link<ulong> _maxDoublet;

        [Params(100, 1000, 10000)]
        public int OperationCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Setup test data for IsEscape benchmarks
            _testChars = new char[OperationCount];
            var random = new Random(42);
            for (int i = 0; i < OperationCount; i++)
            {
                // Mix of escape and non-escape characters
                _testChars[i] = random.Next(10) < 2 ? (random.Next(2) == 0 ? '\\' : '~') : (char)('a' + random.Next(26));
            }

            // Setup test data for frequency benchmarks
            _frequenciesWithInlining = new Dictionary<Link<ulong>, ulong>();
            _frequenciesWithoutInlining = new Dictionary<Link<ulong>, ulong>();
            _testDoublets = new Link<ulong>[OperationCount];
            for (int i = 0; i < OperationCount; i++)
            {
                _testDoublets[i] = new Link<ulong>((ulong)(random.Next(100) + 1), (ulong)(random.Next(100) + 1));
            }

            _maxFrequency = 1;
            _maxDoublet = new Link<ulong>(1, 1);
        }

        #region IsEscape Benchmarks

        [Benchmark(Description = "IsEscape WITH AggressiveInlining")]
        public int IsEscape_WithInlining()
        {
            int count = 0;
            for (int i = 0; i < _testChars.Length; i++)
            {
                if (IsEscape_Inlined(_testChars[i]))
                    count++;
            }
            return count;
        }

        [Benchmark(Description = "IsEscape WITHOUT AggressiveInlining")]
        public int IsEscape_WithoutInlining()
        {
            int count = 0;
            for (int i = 0; i < _testChars.Length; i++)
            {
                if (IsEscape_NotInlined(_testChars[i]))
                    count++;
            }
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEscape_Inlined(char c) => c == '\\' || c == '~';

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsEscape_NotInlined(char c) => c == '\\' || c == '~';

        #endregion

        #region IncrementFrequency Benchmarks

        [Benchmark(Description = "IncrementFrequency WITH AggressiveInlining")]
        public ulong IncrementFrequency_WithInlining()
        {
            _frequenciesWithInlining.Clear();
            ulong sum = 0;
            for (int i = 0; i < _testDoublets.Length; i++)
            {
                sum += IncrementFrequency_Inlined(_testDoublets[i], _frequenciesWithInlining);
            }
            return sum;
        }

        [Benchmark(Description = "IncrementFrequency WITHOUT AggressiveInlining")]
        public ulong IncrementFrequency_WithoutInlining()
        {
            _frequenciesWithoutInlining.Clear();
            ulong sum = 0;
            for (int i = 0; i < _testDoublets.Length; i++)
            {
                sum += IncrementFrequency_NotInlined(_testDoublets[i], _frequenciesWithoutInlining);
            }
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong IncrementFrequency_Inlined(Link<ulong> doublet, Dictionary<Link<ulong>, ulong> frequencies)
        {
            if (frequencies.TryGetValue(doublet, out ulong frequency))
            {
                frequency++;
                frequencies[doublet] = frequency;
            }
            else
            {
                frequency = 1;
                frequencies.Add(doublet, frequency);
            }
            return frequency;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ulong IncrementFrequency_NotInlined(Link<ulong> doublet, Dictionary<Link<ulong>, ulong> frequencies)
        {
            if (frequencies.TryGetValue(doublet, out ulong frequency))
            {
                frequency++;
                frequencies[doublet] = frequency;
            }
            else
            {
                frequency = 1;
                frequencies.Add(doublet, frequency);
            }
            return frequency;
        }

        #endregion

        #region DecrementFrequency Benchmarks

        [Benchmark(Description = "DecrementFrequency WITH AggressiveInlining")]
        public int DecrementFrequency_WithInlining()
        {
            // Pre-populate the dictionary
            var frequencies = new Dictionary<Link<ulong>, ulong>();
            foreach (var doublet in _testDoublets)
            {
                frequencies[doublet] = 5; // Start with frequency of 5
            }

            int removeCount = 0;
            for (int i = 0; i < _testDoublets.Length; i++)
            {
                removeCount += DecrementFrequency_Inlined(_testDoublets[i], frequencies);
            }
            return removeCount;
        }

        [Benchmark(Description = "DecrementFrequency WITHOUT AggressiveInlining")]
        public int DecrementFrequency_WithoutInlining()
        {
            // Pre-populate the dictionary
            var frequencies = new Dictionary<Link<ulong>, ulong>();
            foreach (var doublet in _testDoublets)
            {
                frequencies[doublet] = 5; // Start with frequency of 5
            }

            int removeCount = 0;
            for (int i = 0; i < _testDoublets.Length; i++)
            {
                removeCount += DecrementFrequency_NotInlined(_testDoublets[i], frequencies);
            }
            return removeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int DecrementFrequency_Inlined(Link<ulong> doublet, Dictionary<Link<ulong>, ulong> frequencies)
        {
            if (frequencies.TryGetValue(doublet, out ulong frequency))
            {
                frequency--;
                if (frequency == 0)
                {
                    frequencies.Remove(doublet);
                    return 1;
                }
                else
                {
                    frequencies[doublet] = frequency;
                }
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int DecrementFrequency_NotInlined(Link<ulong> doublet, Dictionary<Link<ulong>, ulong> frequencies)
        {
            if (frequencies.TryGetValue(doublet, out ulong frequency))
            {
                frequency--;
                if (frequency == 0)
                {
                    frequencies.Remove(doublet);
                    return 1;
                }
                else
                {
                    frequencies[doublet] = frequency;
                }
            }
            return 0;
        }

        #endregion

        #region UpdateMaxDoublet Benchmarks

        [Benchmark(Description = "UpdateMaxDoublet WITH AggressiveInlining")]
        public Link<ulong> UpdateMaxDoublet_WithInlining()
        {
            Link<ulong> maxDoublet = new Link<ulong>(1, 1);
            ulong maxFrequency = 1;

            for (int i = 0; i < _testDoublets.Length; i++)
            {
                ulong frequency = (ulong)(i % 10 + 1); // Varying frequencies
                UpdateMaxDoublet_Inlined(_testDoublets[i], frequency, ref maxDoublet, ref maxFrequency);
            }
            return maxDoublet;
        }

        [Benchmark(Description = "UpdateMaxDoublet WITHOUT AggressiveInlining")]
        public Link<ulong> UpdateMaxDoublet_WithoutInlining()
        {
            Link<ulong> maxDoublet = new Link<ulong>(1, 1);
            ulong maxFrequency = 1;

            for (int i = 0; i < _testDoublets.Length; i++)
            {
                ulong frequency = (ulong)(i % 10 + 1); // Varying frequencies
                UpdateMaxDoublet_NotInlined(_testDoublets[i], frequency, ref maxDoublet, ref maxFrequency);
            }
            return maxDoublet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxDoublet_Inlined(Link<ulong> doublet, ulong frequency, ref Link<ulong> maxDoublet, ref ulong maxFrequency)
        {
            if (frequency > 1)
            {
                if (maxFrequency < frequency)
                {
                    maxFrequency = frequency;
                    maxDoublet = doublet;
                }
                else if (maxFrequency == frequency &&
                    (doublet.Source + doublet.Target) > (maxDoublet.Source + maxDoublet.Target))
                {
                    maxDoublet = doublet;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void UpdateMaxDoublet_NotInlined(Link<ulong> doublet, ulong frequency, ref Link<ulong> maxDoublet, ref ulong maxFrequency)
        {
            if (frequency > 1)
            {
                if (maxFrequency < frequency)
                {
                    maxFrequency = frequency;
                    maxDoublet = doublet;
                }
                else if (maxFrequency == frequency &&
                    (doublet.Source + doublet.Target) > (maxDoublet.Source + maxDoublet.Target))
                {
                    maxDoublet = doublet;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Simple Link structure to match the usage in the codebase
    /// </summary>
    public struct Link<T> : IEquatable<Link<T>> where T : IEquatable<T>
    {
        public T Source { get; set; }
        public T Target { get; set; }

        public Link(T source, T target)
        {
            Source = source;
            Target = target;
        }

        public bool Equals(Link<T> other)
        {
            return Source.Equals(other.Source) && Target.Equals(other.Target);
        }

        public override bool Equals(object obj)
        {
            return obj is Link<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, Target);
        }
    }
}
