using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents an immutably editable array with infinite length.
    /// Each change creates a new range appended to the end.
    /// The array can be rebuilt by merging ranges from end to beginning.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    public class EditableArray<T> where T : struct
    {
        private readonly List<EditableArrayRange<T>> _ranges;
        private readonly T _defaultValue;

        /// <summary>
        /// Gets the default value used to fill undefined ranges.
        /// </summary>
        public T DefaultValue => _defaultValue;

        /// <summary>
        /// Gets the total number of ranges.
        /// </summary>
        public int RangeCount => _ranges.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableArray{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value to use for undefined elements.</param>
        public EditableArray(T defaultValue = default)
        {
            _ranges = new List<EditableArrayRange<T>>();
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Writes a range of values to the array at the specified offset.
        /// This creates a new range appended to the end.
        /// </summary>
        /// <param name="offset">The offset where to write the data.</param>
        /// <param name="data">The data to write.</param>
        public void Write(long offset, T[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");
            }
            if (data.Length == 0)
            {
                return;
            }

            var range = new EditableArrayRange<T>(offset, data);
            _ranges.Add(range);

            // Link to next range if needed (for future optimizations)
            if (_ranges.Count > 1)
            {
                _ranges[_ranges.Count - 2].Next = range;
            }
        }

        /// <summary>
        /// Reads a value at the specified index.
        /// Ranges are processed from end to beginning to get the latest value.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The value at the specified index, or the default value if undefined.</returns>
        public T Read(long index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
            }

            // Read from end to beginning to get the latest value
            for (int i = _ranges.Count - 1; i >= 0; i--)
            {
                var range = _ranges[i];
                if (range.Contains(index))
                {
                    long localIndex = index - range.Offset;
                    return range.Data[localIndex];
                }
            }

            return _defaultValue;
        }

        /// <summary>
        /// Reads a range of values from the array.
        /// </summary>
        /// <param name="offset">The starting offset.</param>
        /// <param name="length">The number of elements to read.</param>
        /// <returns>An array containing the requested values.</returns>
        public T[] Read(long offset, long length)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
            }
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds maximum supported value.");
            }

            var result = new T[length];
            for (long i = 0; i < length; i++)
            {
                result[i] = Read(offset + i);
            }
            return result;
        }

        /// <summary>
        /// Rebuilds the array by merging all ranges into a single optimized range.
        /// Ranges are processed from end to beginning to avoid repeated writes to the same parts.
        /// </summary>
        /// <param name="offset">The starting offset of the region to rebuild.</param>
        /// <param name="length">The length of the region to rebuild.</param>
        /// <returns>A new EditableArray with a single merged range.</returns>
        public EditableArray<T> Rebuild(long offset, long length)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
            }

            var rebuiltArray = new EditableArray<T>(_defaultValue);
            if (_ranges.Count == 0 || length == 0)
            {
                return rebuiltArray;
            }

            var mergedData = Read(offset, length);
            rebuiltArray.Write(offset, mergedData);

            return rebuiltArray;
        }

        /// <summary>
        /// Optimizes the array by merging all ranges.
        /// </summary>
        /// <returns>A new optimized EditableArray.</returns>
        public EditableArray<T> Optimize()
        {
            if (_ranges.Count == 0)
            {
                return new EditableArray<T>(_defaultValue);
            }

            long minOffset = _ranges.Min(r => r.Offset);
            long maxEnd = _ranges.Max(r => r.End);
            long length = maxEnd - minOffset;

            return Rebuild(minOffset, length);
        }

        /// <summary>
        /// Saves the array to a file.
        /// The file format includes metadata about offset, length, and element sizes.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public void SaveToFile(string path)
        {
            using (var stream = File.Create(path))
            using (var writer = new BinaryWriter(stream))
            {
                // Write metadata
                writer.Write(typeof(T).FullName ?? typeof(T).Name); // Type name
                writer.Write(_ranges.Count); // Number of ranges

                // Write default value
                WriteValue(writer, _defaultValue);

                // Write each range
                foreach (var range in _ranges)
                {
                    writer.Write(range.Offset);
                    writer.Write(range.Length);
                    foreach (var value in range.Data)
                    {
                        WriteValue(writer, value);
                    }
                }
            }
        }

        /// <summary>
        /// Loads an array from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A new EditableArray loaded from the file.</returns>
        public static EditableArray<T> LoadFromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream))
            {
                // Read metadata
                string typeName = reader.ReadString();
                if (typeName != typeof(T).FullName && typeName != typeof(T).Name)
                {
                    throw new InvalidDataException($"Type mismatch: expected {typeof(T).FullName}, got {typeName}");
                }

                int rangeCount = reader.ReadInt32();
                T defaultValue = ReadValue(reader);

                var array = new EditableArray<T>(defaultValue);

                // Read each range
                for (int i = 0; i < rangeCount; i++)
                {
                    long offset = reader.ReadInt64();
                    long length = reader.ReadInt64();

                    var data = new T[length];
                    for (int j = 0; j < length; j++)
                    {
                        data[j] = ReadValue(reader);
                    }

                    array.Write(offset, data);
                }

                return array;
            }
        }

        private static void WriteValue(BinaryWriter writer, T value)
        {
            if (typeof(T) == typeof(byte))
            {
                writer.Write((byte)(object)value);
            }
            else if (typeof(T) == typeof(int))
            {
                writer.Write((int)(object)value);
            }
            else if (typeof(T) == typeof(long))
            {
                writer.Write((long)(object)value);
            }
            else if (typeof(T) == typeof(float))
            {
                writer.Write((float)(object)value);
            }
            else if (typeof(T) == typeof(double))
            {
                writer.Write((double)(object)value);
            }
            else if (typeof(T) == typeof(bool))
            {
                writer.Write((bool)(object)value);
            }
            else if (typeof(T) == typeof(char))
            {
                writer.Write((char)(object)value);
            }
            else if (typeof(T) == typeof(short))
            {
                writer.Write((short)(object)value);
            }
            else if (typeof(T) == typeof(ushort))
            {
                writer.Write((ushort)(object)value);
            }
            else if (typeof(T) == typeof(uint))
            {
                writer.Write((uint)(object)value);
            }
            else if (typeof(T) == typeof(ulong))
            {
                writer.Write((ulong)(object)value);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} is not supported for file persistence.");
            }
        }

        private static T ReadValue(BinaryReader reader)
        {
            if (typeof(T) == typeof(byte))
            {
                return (T)(object)reader.ReadByte();
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)reader.ReadInt32();
            }
            else if (typeof(T) == typeof(long))
            {
                return (T)(object)reader.ReadInt64();
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)reader.ReadSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                return (T)(object)reader.ReadDouble();
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)reader.ReadBoolean();
            }
            else if (typeof(T) == typeof(char))
            {
                return (T)(object)reader.ReadChar();
            }
            else if (typeof(T) == typeof(short))
            {
                return (T)(object)reader.ReadInt16();
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (T)(object)reader.ReadUInt16();
            }
            else if (typeof(T) == typeof(uint))
            {
                return (T)(object)reader.ReadUInt32();
            }
            else if (typeof(T) == typeof(ulong))
            {
                return (T)(object)reader.ReadUInt64();
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} is not supported for file persistence.");
            }
        }

        /// <summary>
        /// Gets information about all ranges (for debugging and analysis).
        /// </summary>
        /// <returns>An enumerable of range information.</returns>
        public IEnumerable<(long Offset, long Length)> GetRangesInfo()
        {
            return _ranges.Select(r => (r.Offset, r.Length));
        }
    }
}
