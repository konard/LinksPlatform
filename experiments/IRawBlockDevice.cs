using System;

namespace Platform.Data.Core.RawStorage
{
    /// <summary>
    /// Represents a raw block device that can be used for direct storage access without file system dependencies.
    /// This interface enables compatibility with bare-metal operating systems and provides lowest-level storage access.
    /// </summary>
    public interface IRawBlockDevice : IDisposable
    {
        /// <summary>
        /// Gets the total size of the device in bytes.
        /// </summary>
        long SizeInBytes { get; }

        /// <summary>
        /// Gets the block size (sector size) in bytes. All I/O operations should be aligned to this boundary.
        /// Common values: 512 (traditional HDD), 4096 (Advanced Format, modern SSD).
        /// </summary>
        int BlockSize { get; }

        /// <summary>
        /// Reads data from the device at the specified byte offset.
        /// </summary>
        /// <param name="offset">Byte offset from the start of the device. Should be aligned to <see cref="BlockSize"/>.</param>
        /// <param name="buffer">Buffer to read data into. Size should be a multiple of <see cref="BlockSize"/>.</param>
        /// <param name="count">Number of bytes to read. Should be a multiple of <see cref="BlockSize"/>.</param>
        /// <returns>Number of bytes actually read.</returns>
        /// <exception cref="ArgumentException">Thrown when offset or count are not properly aligned.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the device is not accessible or an I/O error occurs.</exception>
        int Read(long offset, byte[] buffer, int count);

        /// <summary>
        /// Writes data to the device at the specified byte offset.
        /// </summary>
        /// <param name="offset">Byte offset from the start of the device. Should be aligned to <see cref="BlockSize"/>.</param>
        /// <param name="buffer">Buffer containing data to write. Size should be a multiple of <see cref="BlockSize"/>.</param>
        /// <param name="count">Number of bytes to write. Should be a multiple of <see cref="BlockSize"/>.</param>
        /// <exception cref="ArgumentException">Thrown when offset or count are not properly aligned.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the device is not accessible or an I/O error occurs.</exception>
        void Write(long offset, byte[] buffer, int count);

        /// <summary>
        /// Flushes any cached writes to the physical device, ensuring data persistence.
        /// </summary>
        void Flush();
    }
}
