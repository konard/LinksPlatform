using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a pair of links (source and target).
    /// </summary>
    public struct DoubletLink<TLink>
    {
        public TLink Source { get; set; }
        public TLink Target { get; set; }

        public DoubletLink(TLink source, TLink target)
        {
            Source = source;
            Target = target;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DoubletLink<TLink>)) return false;
            var other = (DoubletLink<TLink>)obj;
            return EqualityComparer<TLink>.Default.Equals(Source, other.Source) &&
                   EqualityComparer<TLink>.Default.Equals(Target, other.Target);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<TLink>.Default.GetHashCode(Source) * 397) ^
                       EqualityComparer<TLink>.Default.GetHashCode(Target);
            }
        }
    }

    /// <summary>
    /// Represents a server-side frequency dictionary that stores and updates symbol frequencies.
    /// The dictionary is persisted to disk and can be loaded on server restart.
    /// </summary>
    public class ServerSideFrequencyDictionary<TLink>
    {
        private readonly Dictionary<TLink, ulong> _symbolFrequencies;
        private readonly Dictionary<DoubletLink<TLink>, ulong> _doubletFrequencies;
        private readonly string _storagePath;
        private readonly object _lock = new object();

        public ServerSideFrequencyDictionary(string storagePath)
        {
            _storagePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
            _symbolFrequencies = new Dictionary<TLink, ulong>();
            _doubletFrequencies = new Dictionary<DoubletLink<TLink>, ulong>();

            LoadFromDisk();
        }

        /// <summary>
        /// Gets the frequency of a specific symbol.
        /// </summary>
        public ulong GetSymbolFrequency(TLink symbol)
        {
            lock (_lock)
            {
                return _symbolFrequencies.TryGetValue(symbol, out var frequency) ? frequency : 0;
            }
        }

        /// <summary>
        /// Gets the frequency of a specific doublet (pair of symbols).
        /// </summary>
        public ulong GetDoubletFrequency(DoubletLink<TLink> doublet)
        {
            lock (_lock)
            {
                return _doubletFrequencies.TryGetValue(doublet, out var frequency) ? frequency : 0;
            }
        }

        /// <summary>
        /// Increments the frequency of a symbol by the specified amount.
        /// </summary>
        public void IncrementSymbol(TLink symbol, ulong amount = 1)
        {
            lock (_lock)
            {
                if (_symbolFrequencies.TryGetValue(symbol, out var frequency))
                {
                    _symbolFrequencies[symbol] = frequency + amount;
                }
                else
                {
                    _symbolFrequencies[symbol] = amount;
                }
            }
        }

        /// <summary>
        /// Increments the frequency of a doublet by the specified amount.
        /// </summary>
        public void IncrementDoublet(DoubletLink<TLink> doublet, ulong amount = 1)
        {
            lock (_lock)
            {
                if (_doubletFrequencies.TryGetValue(doublet, out var frequency))
                {
                    _doubletFrequencies[doublet] = frequency + amount;
                }
                else
                {
                    _doubletFrequencies[doublet] = amount;
                }
            }
        }

        /// <summary>
        /// Updates frequencies based on a sequence of symbols.
        /// This is called each time new text is received.
        /// </summary>
        public void UpdateFromSequence(TLink[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return;
            }

            lock (_lock)
            {
                // Update symbol frequencies
                foreach (var symbol in sequence)
                {
                    IncrementSymbol(symbol);
                }

                // Update doublet frequencies
                for (var i = 1; i < sequence.Length; i++)
                {
                    var doublet = new DoubletLink<TLink>(sequence[i - 1], sequence[i]);
                    IncrementDoublet(doublet);
                }
            }
        }

        /// <summary>
        /// Gets all doublets sorted by frequency in descending order.
        /// </summary>
        public IEnumerable<KeyValuePair<DoubletLink<TLink>, ulong>> GetDoubletsSortedByFrequency()
        {
            lock (_lock)
            {
                return _doubletFrequencies.OrderByDescending(kvp => kvp.Value).ToList();
            }
        }

        /// <summary>
        /// Gets all symbols sorted by frequency in descending order.
        /// </summary>
        public IEnumerable<KeyValuePair<TLink, ulong>> GetSymbolsSortedByFrequency()
        {
            lock (_lock)
            {
                return _symbolFrequencies.OrderByDescending(kvp => kvp.Value).ToList();
            }
        }

        /// <summary>
        /// Saves the frequency dictionary to disk.
        /// </summary>
        public void SaveToDisk()
        {
            lock (_lock)
            {
                try
                {
                    using (var writer = new BinaryWriter(File.Open(_storagePath, FileMode.Create)))
                    {
                        // Write symbol frequencies
                        writer.Write(_symbolFrequencies.Count);
                        foreach (var kvp in _symbolFrequencies)
                        {
                            WriteLink(writer, kvp.Key);
                            writer.Write(kvp.Value);
                        }

                        // Write doublet frequencies
                        writer.Write(_doubletFrequencies.Count);
                        foreach (var kvp in _doubletFrequencies)
                        {
                            WriteLink(writer, kvp.Key.Source);
                            WriteLink(writer, kvp.Key.Target);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving frequency dictionary: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads the frequency dictionary from disk.
        /// </summary>
        private void LoadFromDisk()
        {
            if (!File.Exists(_storagePath))
            {
                return;
            }

            try
            {
                using (var reader = new BinaryReader(File.Open(_storagePath, FileMode.Open)))
                {
                    // Read symbol frequencies
                    var symbolCount = reader.ReadInt32();
                    for (var i = 0; i < symbolCount; i++)
                    {
                        var symbol = ReadLink(reader);
                        var frequency = reader.ReadUInt64();
                        _symbolFrequencies[symbol] = frequency;
                    }

                    // Read doublet frequencies
                    var doubletCount = reader.ReadInt32();
                    for (var i = 0; i < doubletCount; i++)
                    {
                        var source = ReadLink(reader);
                        var target = ReadLink(reader);
                        var frequency = reader.ReadUInt64();
                        _doubletFrequencies[new DoubletLink<TLink>(source, target)] = frequency;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading frequency dictionary: {ex.Message}");
            }
        }

        private void WriteLink(BinaryWriter writer, TLink link)
        {
            if (typeof(TLink) == typeof(ulong))
            {
                writer.Write((ulong)(object)link);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(TLink)} is not supported for serialization");
            }
        }

        private TLink ReadLink(BinaryReader reader)
        {
            if (typeof(TLink) == typeof(ulong))
            {
                return (TLink)(object)reader.ReadUInt64();
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(TLink)} is not supported for deserialization");
            }
        }

        /// <summary>
        /// Gets the total number of symbols processed.
        /// </summary>
        public ulong GetTotalSymbolCount()
        {
            lock (_lock)
            {
                return (ulong)_symbolFrequencies.Values.Sum(f => (long)f);
            }
        }

        /// <summary>
        /// Gets the total number of doublets processed.
        /// </summary>
        public ulong GetTotalDoubletCount()
        {
            lock (_lock)
            {
                return (ulong)_doubletFrequencies.Values.Sum(f => (long)f);
            }
        }
    }
}
