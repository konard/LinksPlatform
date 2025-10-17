using System;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents a bitmap for optimizing bit string operations.
    /// Each bit in the bitmap corresponds to N bits in the original bit string,
    /// where 1 indicates at least one bit is set in that segment, and 0 indicates all bits are zero.
    /// This allows for faster searching and navigation within large bit strings.
    /// </summary>
    public class BitStringBitmap
    {
        private readonly byte[] _bitString;
        private readonly byte[] _bitmap;
        private readonly int _granularity;

        /// <summary>
        /// Initializes a new instance of the BitStringBitmap class.
        /// </summary>
        /// <param name="bitStringSize">Size of the bit string in bits.</param>
        /// <param name="granularity">Number of bits in the original bit string represented by each bitmap bit.
        /// Default is 8 bits (1 byte). Can be set to match cache line size (64 bytes = 512 bits) or page size (4096 bytes = 32768 bits).</param>
        public BitStringBitmap(int bitStringSize, int granularity = 8)
        {
            if (bitStringSize <= 0)
                throw new ArgumentException("Bit string size must be positive", nameof(bitStringSize));
            if (granularity <= 0)
                throw new ArgumentException("Granularity must be positive", nameof(granularity));

            _granularity = granularity;

            // Calculate size in bytes for bit string
            int bitStringBytes = (bitStringSize + 7) / 8;
            _bitString = new byte[bitStringBytes];

            // Calculate size in bytes for bitmap
            int bitmapBits = (bitStringSize + granularity - 1) / granularity;
            int bitmapBytes = (bitmapBits + 7) / 8;
            _bitmap = new byte[bitmapBytes];
        }

        /// <summary>
        /// Gets the granularity (number of bits per bitmap entry).
        /// </summary>
        public int Granularity => _granularity;

        /// <summary>
        /// Gets the size of the bit string in bits.
        /// </summary>
        public int BitStringSize => _bitString.Length * 8;

        /// <summary>
        /// Gets the size of the bitmap in bits.
        /// </summary>
        public int BitmapSize => _bitmap.Length * 8;

        /// <summary>
        /// Sets a bit in the bit string and updates the corresponding bitmap entry.
        /// </summary>
        /// <param name="bitIndex">Index of the bit to set.</param>
        public void SetBit(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= BitStringSize)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            // Set bit in bit string
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;
            _bitString[byteIndex] |= (byte)(1 << bitOffset);

            // Update bitmap
            UpdateBitmapForSegment(bitIndex);
        }

        /// <summary>
        /// Clears a bit in the bit string and updates the corresponding bitmap entry.
        /// </summary>
        /// <param name="bitIndex">Index of the bit to clear.</param>
        public void ClearBit(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= BitStringSize)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            // Clear bit in bit string
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;
            _bitString[byteIndex] &= (byte)~(1 << bitOffset);

            // Update bitmap
            UpdateBitmapForSegment(bitIndex);
        }

        /// <summary>
        /// Gets the value of a bit in the bit string.
        /// </summary>
        /// <param name="bitIndex">Index of the bit to get.</param>
        /// <returns>True if the bit is set, false otherwise.</returns>
        public bool GetBit(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= BitStringSize)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;
            return (_bitString[byteIndex] & (1 << bitOffset)) != 0;
        }

        /// <summary>
        /// Gets the value of a bitmap bit.
        /// </summary>
        /// <param name="bitmapIndex">Index in the bitmap.</param>
        /// <returns>True if at least one bit is set in the corresponding segment, false otherwise.</returns>
        public bool GetBitmapBit(int bitmapIndex)
        {
            if (bitmapIndex < 0 || bitmapIndex >= BitmapSize)
                throw new ArgumentOutOfRangeException(nameof(bitmapIndex));

            int byteIndex = bitmapIndex / 8;
            int bitOffset = bitmapIndex % 8;
            return (_bitmap[byteIndex] & (1 << bitOffset)) != 0;
        }

        /// <summary>
        /// Updates the bitmap for the segment containing the specified bit.
        /// </summary>
        private void UpdateBitmapForSegment(int bitIndex)
        {
            int segmentIndex = bitIndex / _granularity;
            int segmentStartBit = segmentIndex * _granularity;
            int segmentEndBit = Math.Min(segmentStartBit + _granularity, BitStringSize);

            // Check if any bit in the segment is set
            bool anyBitSet = false;
            for (int i = segmentStartBit; i < segmentEndBit; i++)
            {
                int byteIndex = i / 8;
                int bitOffset = i % 8;
                if ((_bitString[byteIndex] & (1 << bitOffset)) != 0)
                {
                    anyBitSet = true;
                    break;
                }
            }

            // Update bitmap
            int bitmapByteIndex = segmentIndex / 8;
            int bitmapBitOffset = segmentIndex % 8;

            if (anyBitSet)
            {
                _bitmap[bitmapByteIndex] |= (byte)(1 << bitmapBitOffset);
            }
            else
            {
                _bitmap[bitmapByteIndex] &= (byte)~(1 << bitmapBitOffset);
            }
        }

        /// <summary>
        /// Finds the index of the next set bit starting from the specified position.
        /// Uses bitmap for faster searching by skipping empty segments.
        /// </summary>
        /// <param name="startBitIndex">Starting position for the search.</param>
        /// <returns>Index of the next set bit, or -1 if no set bit is found.</returns>
        public int FindNextSetBit(int startBitIndex)
        {
            if (startBitIndex < 0)
                startBitIndex = 0;

            if (startBitIndex >= BitStringSize)
                return -1;

            int currentSegment = startBitIndex / _granularity;
            int currentBitInSegment = startBitIndex % _granularity;

            // Search through segments using bitmap
            while (currentSegment < (BitStringSize + _granularity - 1) / _granularity)
            {
                // Check if bitmap indicates this segment has any set bits
                if (GetBitmapBit(currentSegment))
                {
                    // Search within this segment
                    int segmentStart = currentSegment * _granularity;
                    int segmentEnd = Math.Min(segmentStart + _granularity, BitStringSize);

                    for (int i = segmentStart + currentBitInSegment; i < segmentEnd; i++)
                    {
                        if (GetBit(i))
                            return i;
                    }
                }

                currentSegment++;
                currentBitInSegment = 0;
            }

            return -1;
        }

        /// <summary>
        /// Counts the number of set bits in the bit string.
        /// Uses bitmap to skip empty segments for better performance.
        /// </summary>
        /// <returns>The number of set bits.</returns>
        public int CountSetBits()
        {
            int count = 0;
            int totalSegments = (BitStringSize + _granularity - 1) / _granularity;

            for (int segment = 0; segment < totalSegments; segment++)
            {
                // Skip segments with no set bits
                if (!GetBitmapBit(segment))
                    continue;

                int segmentStart = segment * _granularity;
                int segmentEnd = Math.Min(segmentStart + _granularity, BitStringSize);

                for (int i = segmentStart; i < segmentEnd; i++)
                {
                    if (GetBit(i))
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Rebuilds the entire bitmap by scanning the bit string.
        /// Useful after bulk operations on the bit string.
        /// </summary>
        public void RebuildBitmap()
        {
            Array.Clear(_bitmap, 0, _bitmap.Length);

            int totalSegments = (BitStringSize + _granularity - 1) / _granularity;

            for (int segment = 0; segment < totalSegments; segment++)
            {
                int segmentStart = segment * _granularity;
                int segmentEnd = Math.Min(segmentStart + _granularity, BitStringSize);

                bool anyBitSet = false;
                for (int i = segmentStart; i < segmentEnd; i++)
                {
                    if (GetBit(i))
                    {
                        anyBitSet = true;
                        break;
                    }
                }

                if (anyBitSet)
                {
                    int bitmapByteIndex = segment / 8;
                    int bitmapBitOffset = segment % 8;
                    _bitmap[bitmapByteIndex] |= (byte)(1 << bitmapBitOffset);
                }
            }
        }
    }
}
