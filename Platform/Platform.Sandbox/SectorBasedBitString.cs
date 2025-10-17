using System;
using System.Collections.Generic;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents a sparse bit string using sector-based storage.
    /// Uses an array of sector indices where each entry can be:
    /// - 0: sector is empty (all bits are 0)
    /// - non-zero: index/address of the bit string sector in storage
    /// This allows:
    /// 1. Optimized search (skip empty sectors)
    /// 2. Space savings on disk (only non-empty sectors are stored)
    /// 3. Random order filling (sectors can be allocated on demand)
    /// </summary>
    public class SectorBasedBitString
    {
        private readonly int _sectorSize;
        private readonly Dictionary<int, byte[]> _sectors;
        private readonly int _totalSectors;

        /// <summary>
        /// Typical system page size in bytes (4KB).
        /// </summary>
        public const int DefaultSectorSize = 4096;

        /// <summary>
        /// Processor cache line size in bytes (64 bytes).
        /// </summary>
        public const int CacheLineSectorSize = 64;

        /// <summary>
        /// Initializes a new instance of the SectorBasedBitString class.
        /// </summary>
        /// <param name="totalBits">Total size of the bit string in bits.</param>
        /// <param name="sectorSizeBytes">Size of each sector in bytes.
        /// Default is system page size (4096 bytes).
        /// Can be set to cache line size (64 bytes) for better cache performance.</param>
        public SectorBasedBitString(long totalBits, int sectorSizeBytes = DefaultSectorSize)
        {
            if (totalBits <= 0)
                throw new ArgumentException("Total bits must be positive", nameof(totalBits));
            if (sectorSizeBytes <= 0)
                throw new ArgumentException("Sector size must be positive", nameof(sectorSizeBytes));

            _sectorSize = sectorSizeBytes;
            _sectors = new Dictionary<int, byte[]>();

            // Calculate total number of sectors needed
            int bitsPerSector = sectorSizeBytes * 8;
            _totalSectors = (int)((totalBits + bitsPerSector - 1) / bitsPerSector);

            TotalBits = totalBits;
        }

        /// <summary>
        /// Gets the total size of the bit string in bits.
        /// </summary>
        public long TotalBits { get; }

        /// <summary>
        /// Gets the sector size in bytes.
        /// </summary>
        public int SectorSize => _sectorSize;

        /// <summary>
        /// Gets the number of bits per sector.
        /// </summary>
        public int BitsPerSector => _sectorSize * 8;

        /// <summary>
        /// Gets the total number of sectors.
        /// </summary>
        public int TotalSectors => _totalSectors;

        /// <summary>
        /// Gets the number of allocated (non-empty) sectors.
        /// </summary>
        public int AllocatedSectorsCount => _sectors.Count;

        /// <summary>
        /// Gets the memory usage efficiency as a percentage.
        /// </summary>
        public double MemoryEfficiency => _totalSectors > 0
            ? (1.0 - (double)AllocatedSectorsCount / _totalSectors) * 100
            : 0;

        /// <summary>
        /// Sets a bit in the bit string, allocating the sector if necessary.
        /// </summary>
        /// <param name="bitIndex">Index of the bit to set.</param>
        public void SetBit(long bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= TotalBits)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            int sectorIndex = (int)(bitIndex / BitsPerSector);
            int bitInSector = (int)(bitIndex % BitsPerSector);

            // Allocate sector if it doesn't exist
            if (!_sectors.ContainsKey(sectorIndex))
            {
                _sectors[sectorIndex] = new byte[_sectorSize];
            }

            byte[] sector = _sectors[sectorIndex];
            int byteIndex = bitInSector / 8;
            int bitOffset = bitInSector % 8;

            sector[byteIndex] |= (byte)(1 << bitOffset);
        }

        /// <summary>
        /// Clears a bit in the bit string.
        /// If the sector becomes empty, it can be deallocated to save memory.
        /// </summary>
        /// <param name="bitIndex">Index of the bit to clear.</param>
        /// <param name="deallocateIfEmpty">If true, deallocates the sector if it becomes empty.</param>
        public void ClearBit(long bitIndex, bool deallocateIfEmpty = true)
        {
            if (bitIndex < 0 || bitIndex >= TotalBits)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            int sectorIndex = (int)(bitIndex / BitsPerSector);

            // If sector doesn't exist, nothing to clear
            if (!_sectors.ContainsKey(sectorIndex))
                return;

            byte[] sector = _sectors[sectorIndex];
            int bitInSector = (int)(bitIndex % BitsPerSector);
            int byteIndex = bitInSector / 8;
            int bitOffset = bitInSector % 8;

            sector[byteIndex] &= (byte)~(1 << bitOffset);

            // Check if sector is empty and deallocate if requested
            if (deallocateIfEmpty && IsSectorEmpty(sector))
            {
                _sectors.Remove(sectorIndex);
            }
        }

        /// <summary>
        /// Gets the value of a bit in the bit string.
        /// </summary>
        /// <param name="bitIndex">Index of the bit to get.</param>
        /// <returns>True if the bit is set, false otherwise.</returns>
        public bool GetBit(long bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= TotalBits)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            int sectorIndex = (int)(bitIndex / BitsPerSector);

            // If sector doesn't exist, bit is 0
            if (!_sectors.ContainsKey(sectorIndex))
                return false;

            byte[] sector = _sectors[sectorIndex];
            int bitInSector = (int)(bitIndex % BitsPerSector);
            int byteIndex = bitInSector / 8;
            int bitOffset = bitInSector % 8;

            return (sector[byteIndex] & (1 << bitOffset)) != 0;
        }

        /// <summary>
        /// Checks if a sector exists (is allocated).
        /// </summary>
        /// <param name="sectorIndex">Index of the sector.</param>
        /// <returns>True if the sector is allocated, false otherwise.</returns>
        public bool IsSectorAllocated(int sectorIndex)
        {
            if (sectorIndex < 0 || sectorIndex >= _totalSectors)
                throw new ArgumentOutOfRangeException(nameof(sectorIndex));

            return _sectors.ContainsKey(sectorIndex);
        }

        /// <summary>
        /// Checks if a sector contains only zeros.
        /// </summary>
        private bool IsSectorEmpty(byte[] sector)
        {
            foreach (byte b in sector)
            {
                if (b != 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Finds the index of the next set bit starting from the specified position.
        /// Optimized by skipping non-allocated sectors.
        /// </summary>
        /// <param name="startBitIndex">Starting position for the search.</param>
        /// <returns>Index of the next set bit, or -1 if no set bit is found.</returns>
        public long FindNextSetBit(long startBitIndex)
        {
            if (startBitIndex < 0)
                startBitIndex = 0;

            if (startBitIndex >= TotalBits)
                return -1;

            int currentSector = (int)(startBitIndex / BitsPerSector);
            int currentBitInSector = (int)(startBitIndex % BitsPerSector);

            while (currentSector < _totalSectors)
            {
                // Skip non-allocated sectors
                if (_sectors.ContainsKey(currentSector))
                {
                    byte[] sector = _sectors[currentSector];
                    long sectorStartBit = (long)currentSector * BitsPerSector;
                    long sectorEndBit = Math.Min(sectorStartBit + BitsPerSector, TotalBits);

                    // Search within sector
                    for (long i = sectorStartBit + currentBitInSector; i < sectorEndBit; i++)
                    {
                        if (GetBit(i))
                            return i;
                    }
                }

                currentSector++;
                currentBitInSector = 0;
            }

            return -1;
        }

        /// <summary>
        /// Counts the number of set bits in the bit string.
        /// Optimized by only scanning allocated sectors.
        /// </summary>
        /// <returns>The number of set bits.</returns>
        public long CountSetBits()
        {
            long count = 0;

            foreach (var kvp in _sectors)
            {
                byte[] sector = kvp.Value;

                foreach (byte b in sector)
                {
                    // Use Brian Kernighan's algorithm to count bits
                    byte temp = b;
                    while (temp != 0)
                    {
                        temp &= (byte)(temp - 1);
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Clears all bits and deallocates all sectors.
        /// </summary>
        public void Clear()
        {
            _sectors.Clear();
        }

        /// <summary>
        /// Gets the indices of all allocated sectors.
        /// </summary>
        /// <returns>Collection of allocated sector indices.</returns>
        public IEnumerable<int> GetAllocatedSectorIndices()
        {
            return _sectors.Keys;
        }

        /// <summary>
        /// Compacts the storage by removing empty sectors.
        /// Useful after bulk clear operations.
        /// </summary>
        /// <returns>Number of sectors deallocated.</returns>
        public int Compact()
        {
            int removed = 0;
            var sectorsToRemove = new List<int>();

            foreach (var kvp in _sectors)
            {
                if (IsSectorEmpty(kvp.Value))
                {
                    sectorsToRemove.Add(kvp.Key);
                }
            }

            foreach (int sectorIndex in sectorsToRemove)
            {
                _sectors.Remove(sectorIndex);
                removed++;
            }

            return removed;
        }

        /// <summary>
        /// Gets statistics about the bit string storage.
        /// </summary>
        /// <returns>String containing storage statistics.</returns>
        public string GetStatistics()
        {
            return $"Total Bits: {TotalBits}\n" +
                   $"Total Sectors: {_totalSectors}\n" +
                   $"Allocated Sectors: {AllocatedSectorsCount}\n" +
                   $"Sector Size: {_sectorSize} bytes ({BitsPerSector} bits)\n" +
                   $"Memory Saved: {MemoryEfficiency:F2}%\n" +
                   $"Set Bits: {CountSetBits()}";
        }
    }
}
