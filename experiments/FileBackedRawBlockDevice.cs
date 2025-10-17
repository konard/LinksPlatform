using System;
using System.IO;

namespace Platform.Data.Core.RawStorage
{
    /// <summary>
    /// A file-backed implementation of <see cref="IRawBlockDevice"/> for testing and development.
    /// This implementation simulates a raw block device using a regular file, which is useful for:
    /// - Testing without requiring elevated privileges
    /// - Development on systems without raw device access
    /// - Creating portable storage that can be moved between systems
    /// </summary>
    public class FileBackedRawBlockDevice : IRawBlockDevice
    {
        private readonly FileStream _file;
        private readonly int _blockSize;
        private readonly long _sizeInBytes;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBackedRawBlockDevice"/> class.
        /// </summary>
        /// <param name="filePath">Path to the file that will act as the block device.</param>
        /// <param name="sizeInBytes">Total size of the device in bytes. Must be a multiple of <paramref name="blockSize"/>.</param>
        /// <param name="blockSize">Block size in bytes (default: 4096 for modern storage).</param>
        /// <param name="createNew">If true, creates a new file. If false, opens existing file.</param>
        public FileBackedRawBlockDevice(string filePath, long sizeInBytes, int blockSize = 4096, bool createNew = true)
        {
            if (sizeInBytes <= 0)
                throw new ArgumentException("Size must be positive", nameof(sizeInBytes));
            if (blockSize <= 0 || (blockSize & (blockSize - 1)) != 0)
                throw new ArgumentException("Block size must be a positive power of 2", nameof(blockSize));
            if (sizeInBytes % blockSize != 0)
                throw new ArgumentException($"Size must be a multiple of block size ({blockSize})", nameof(sizeInBytes));

            _blockSize = blockSize;
            _sizeInBytes = sizeInBytes;

            if (createNew)
            {
                // Create new file with the specified size
                _file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, _blockSize, FileOptions.WriteThrough);
                _file.SetLength(sizeInBytes);
            }
            else
            {
                // Open existing file
                _file = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, _blockSize, FileOptions.WriteThrough);
                if (_file.Length != sizeInBytes)
                    throw new InvalidOperationException($"Existing file size ({_file.Length}) does not match expected size ({sizeInBytes})");
            }
        }

        /// <inheritdoc/>
        public long SizeInBytes => _sizeInBytes;

        /// <inheritdoc/>
        public int BlockSize => _blockSize;

        /// <inheritdoc/>
        public int Read(long offset, byte[] buffer, int count)
        {
            ValidateDisposed();
            ValidateAlignment(offset, count);
            ValidateRange(offset, count);

            _file.Seek(offset, SeekOrigin.Begin);
            return _file.Read(buffer, 0, count);
        }

        /// <inheritdoc/>
        public void Write(long offset, byte[] buffer, int count)
        {
            ValidateDisposed();
            ValidateAlignment(offset, count);
            ValidateRange(offset, count);

            _file.Seek(offset, SeekOrigin.Begin);
            _file.Write(buffer, 0, count);
        }

        /// <inheritdoc/>
        public void Flush()
        {
            ValidateDisposed();
            _file.Flush(true);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _file?.Dispose();
                _disposed = true;
            }
        }

        private void ValidateDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileBackedRawBlockDevice));
        }

        private void ValidateAlignment(long offset, int count)
        {
            if (offset % _blockSize != 0)
                throw new ArgumentException($"Offset must be aligned to block size ({_blockSize})", nameof(offset));
            if (count % _blockSize != 0)
                throw new ArgumentException($"Count must be a multiple of block size ({_blockSize})", nameof(count));
        }

        private void ValidateRange(long offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");
            if (offset + count > _sizeInBytes)
                throw new ArgumentException("Read/write operation would exceed device size");
        }
    }
}
