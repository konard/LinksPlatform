using System;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents a range in an editable array with offset, length, and data.
    /// Each range can reference the next range in the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the range.</typeparam>
    public class EditableArrayRange<T>
    {
        /// <summary>
        /// Gets the offset of this range in the virtual array.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Gets the length of this range.
        /// </summary>
        public long Length => Data.Length;

        /// <summary>
        /// Gets the data stored in this range.
        /// </summary>
        public T[] Data { get; }

        /// <summary>
        /// Gets or sets the reference to the next range.
        /// This is the only mutable field in the range.
        /// </summary>
        public EditableArrayRange<T> Next { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableArrayRange{T}"/> class.
        /// </summary>
        /// <param name="offset">The offset of this range in the virtual array.</param>
        /// <param name="data">The data to store in this range.</param>
        public EditableArrayRange(long offset, T[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");
            }

            Offset = offset;
            Data = data;
            Next = null;
        }

        /// <summary>
        /// Gets the end position (exclusive) of this range.
        /// </summary>
        public long End => Offset + Length;

        /// <summary>
        /// Checks if this range overlaps with the specified offset range.
        /// </summary>
        /// <param name="offset">The starting offset.</param>
        /// <param name="length">The length of the range.</param>
        /// <returns>True if ranges overlap, false otherwise.</returns>
        public bool Overlaps(long offset, long length)
        {
            long end = offset + length;
            return Offset < end && offset < End;
        }

        /// <summary>
        /// Checks if this range contains the specified index.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the index is within this range, false otherwise.</returns>
        public bool Contains(long index)
        {
            return index >= Offset && index < End;
        }
    }
}
