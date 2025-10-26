using System;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents an immutably editable bit string with infinite length.
    /// Built on top of EditableArray using byte storage for efficiency.
    /// </summary>
    public class EditableBitString
    {
        private readonly EditableArray<byte> _array;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableBitString"/> class.
        /// </summary>
        /// <param name="defaultBit">The default bit value (0 or 1) to use for undefined bits.</param>
        public EditableBitString(bool defaultBit = false)
        {
            _array = new EditableArray<byte>(defaultBit ? (byte)1 : (byte)0);
        }

        private EditableBitString(EditableArray<byte> array)
        {
            _array = array;
        }

        /// <summary>
        /// Sets a bit at the specified index.
        /// </summary>
        /// <param name="index">The bit index.</param>
        /// <param name="value">The bit value.</param>
        public void SetBit(long index, bool value)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
            }

            _array.Write(index, new[] { value ? (byte)1 : (byte)0 });
        }

        /// <summary>
        /// Gets a bit at the specified index.
        /// </summary>
        /// <param name="index">The bit index.</param>
        /// <returns>The bit value.</returns>
        public bool GetBit(long index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
            }

            return _array.Read(index) != 0;
        }

        /// <summary>
        /// Sets a range of bits.
        /// </summary>
        /// <param name="offset">The starting bit index.</param>
        /// <param name="bits">The bit values to set.</param>
        public void SetBits(long offset, bool[] bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException(nameof(bits));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");
            }

            var bytes = new byte[bits.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                bytes[i] = bits[i] ? (byte)1 : (byte)0;
            }
            _array.Write(offset, bytes);
        }

        /// <summary>
        /// Gets a range of bits.
        /// </summary>
        /// <param name="offset">The starting bit index.</param>
        /// <param name="length">The number of bits to read.</param>
        /// <returns>An array of bit values.</returns>
        public bool[] GetBits(long offset, long length)
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

            var bytes = _array.Read(offset, length);
            var bits = new bool[length];
            for (int i = 0; i < length; i++)
            {
                bits[i] = bytes[i] != 0;
            }
            return bits;
        }

        /// <summary>
        /// Optimizes the bit string by merging all ranges.
        /// </summary>
        /// <returns>A new optimized EditableBitString.</returns>
        public EditableBitString Optimize()
        {
            return new EditableBitString(_array.Optimize());
        }

        /// <summary>
        /// Saves the bit string to a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public void SaveToFile(string path)
        {
            _array.SaveToFile(path);
        }

        /// <summary>
        /// Loads a bit string from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A new EditableBitString loaded from the file.</returns>
        public static EditableBitString LoadFromFile(string path)
        {
            var array = EditableArray<byte>.LoadFromFile(path);
            return new EditableBitString(array);
        }

        /// <summary>
        /// Gets the number of ranges in the underlying array.
        /// </summary>
        public int RangeCount => _array.RangeCount;
    }
}
