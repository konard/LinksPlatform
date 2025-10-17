using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Indexes;

namespace Platform.Examples
{
    /// <summary>
    /// Implements a lazy file-based index for sequences (link unfolds).
    /// This index caches long sequences to files to avoid multiple memory jumps when reading.
    /// </summary>
    /// <typeparam name="TLink">The type of link address.</typeparam>
    public class SequenceUnfoldIndex<TLink> : ISequenceIndex<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly string _indexDirectory;
        private readonly int _minSequenceLength;
        private readonly Dictionary<TLink, string> _sequenceFiles;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceUnfoldIndex{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="indexDirectory">The directory where sequence files will be stored. Defaults to ".links-sequences".</param>
        /// <param name="minSequenceLength">The minimum sequence length to warrant file indexing. Defaults to 10.</param>
        public SequenceUnfoldIndex(ILinks<TLink> links, string indexDirectory = ".links-sequences", int minSequenceLength = 10)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _indexDirectory = indexDirectory ?? throw new ArgumentNullException(nameof(indexDirectory));
            _minSequenceLength = minSequenceLength;
            _sequenceFiles = new Dictionary<TLink, string>();

            // Create the index directory if it doesn't exist
            if (!Directory.Exists(_indexDirectory))
            {
                Directory.CreateDirectory(_indexDirectory);
            }

            // Load existing index files
            LoadExistingIndexFiles();
        }

        /// <summary>
        /// Loads existing sequence index files from the index directory.
        /// </summary>
        private void LoadExistingIndexFiles()
        {
            if (!Directory.Exists(_indexDirectory))
            {
                return;
            }

            var files = Directory.GetFiles(_indexDirectory, "seq-*.bin");
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.StartsWith("seq-") && fileName.Length > 4)
                {
                    var linkAddressStr = fileName.Substring(4);
                    if (TryParseLinkAddress(linkAddressStr, out TLink linkAddress))
                    {
                        lock (_lock)
                        {
                            _sequenceFiles[linkAddress] = file;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to parse a link address from a string.
        /// </summary>
        private bool TryParseLinkAddress(string str, out TLink linkAddress)
        {
            linkAddress = default;
            try
            {
                if (typeof(TLink) == typeof(ulong))
                {
                    if (ulong.TryParse(str, out var value))
                    {
                        linkAddress = (TLink)(object)value;
                        return true;
                    }
                }
                else if (typeof(TLink) == typeof(uint))
                {
                    if (uint.TryParse(str, out var value))
                    {
                        linkAddress = (TLink)(object)value;
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a sequence to the index. If the sequence is long enough, it will be cached to a file.
        /// </summary>
        /// <param name="sequence">The sequence to index.</param>
        /// <returns>True if the sequence was newly indexed, false if it was already indexed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(IList<TLink> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return false;
            }

            // Only index sequences that are long enough
            if (sequence.Count < _minSequenceLength)
            {
                return false;
            }

            // Use the first element as the sequence identifier
            // In a real scenario, this might be the root link of the sequence
            var sequenceId = sequence[0];

            lock (_lock)
            {
                // Check if already indexed
                if (_sequenceFiles.ContainsKey(sequenceId))
                {
                    return false;
                }

                // Create a file for this sequence
                var fileName = Path.Combine(_indexDirectory, $"seq-{sequenceId}.bin");
                WriteSequenceToFile(fileName, sequence);
                _sequenceFiles[sequenceId] = fileName;
                return true;
            }
        }

        /// <summary>
        /// Checks if a sequence might be contained in the index.
        /// </summary>
        /// <param name="sequence">The sequence to check.</param>
        /// <returns>True if the sequence might be in the index, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MightContain(IList<TLink> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return false;
            }

            var sequenceId = sequence[0];
            lock (_lock)
            {
                return _sequenceFiles.ContainsKey(sequenceId);
            }
        }

        /// <summary>
        /// Reads a sequence from the index file.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <returns>The sequence elements, or null if not found.</returns>
        public IList<TLink> ReadSequence(TLink sequenceId)
        {
            string fileName;
            lock (_lock)
            {
                if (!_sequenceFiles.TryGetValue(sequenceId, out fileName))
                {
                    return null;
                }
            }

            if (!File.Exists(fileName))
            {
                return null;
            }

            return ReadSequenceFromFile(fileName);
        }

        /// <summary>
        /// Writes a sequence to a binary file.
        /// </summary>
        private void WriteSequenceToFile(string fileName, IList<TLink> sequence)
        {
            using (var writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                // Write the sequence length
                writer.Write(sequence.Count);

                // Write each element
                if (typeof(TLink) == typeof(ulong))
                {
                    foreach (var element in sequence)
                    {
                        writer.Write((ulong)(object)element);
                    }
                }
                else if (typeof(TLink) == typeof(uint))
                {
                    foreach (var element in sequence)
                    {
                        writer.Write((uint)(object)element);
                    }
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(TLink)} is not supported for binary serialization.");
                }
            }
        }

        /// <summary>
        /// Reads a sequence from a binary file.
        /// </summary>
        private IList<TLink> ReadSequenceFromFile(string fileName)
        {
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
            {
                // Read the sequence length
                var count = reader.ReadInt32();
                var sequence = new List<TLink>(count);

                // Read each element
                if (typeof(TLink) == typeof(ulong))
                {
                    for (int i = 0; i < count; i++)
                    {
                        sequence.Add((TLink)(object)reader.ReadUInt64());
                    }
                }
                else if (typeof(TLink) == typeof(uint))
                {
                    for (int i = 0; i < count; i++)
                    {
                        sequence.Add((TLink)(object)reader.ReadUInt32());
                    }
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(TLink)} is not supported for binary deserialization.");
                }

                return sequence;
            }
        }

        /// <summary>
        /// Gets the number of indexed sequences.
        /// </summary>
        public int IndexedSequenceCount
        {
            get
            {
                lock (_lock)
                {
                    return _sequenceFiles.Count;
                }
            }
        }

        /// <summary>
        /// Clears the entire index, removing all files.
        /// </summary>
        public void ClearIndex()
        {
            lock (_lock)
            {
                foreach (var file in _sequenceFiles.Values)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                _sequenceFiles.Clear();
            }
        }
    }
}
