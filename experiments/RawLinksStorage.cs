using System;
using System.Runtime.InteropServices;

namespace Platform.Data.Core.RawStorage
{
    /// <summary>
    /// Implementation of <see cref="IRawLinksStorage{TLinkAddress}"/> that stores links on a raw block device.
    /// This class provides a fixed-size link storage without file system dependencies.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type used for link addresses (must be unmanaged).</typeparam>
    public unsafe class RawLinksStorage<TLinkAddress> : IRawLinksStorage<TLinkAddress>
        where TLinkAddress : unmanaged
    {
        private const long MagicNumber = 0x534B4E494C5F5752; // "WR_LINKS" in hex
        private const long CurrentVersion = 1;

        private readonly IRawBlockDevice _device;
        private readonly int _blockSize;
        private readonly long _headerSize;
        private readonly long _linkRecordSize;
        private readonly TLinkAddress _capacity;
        private TLinkAddress _count;
        private bool _disposed;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct StorageHeader
        {
            public long MagicNumber;
            public long Version;
            public long Capacity;
            public long Count;
            public long LinkRecordSize;
            public long Reserved1;
            public long Reserved2;
            public long Checksum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct LinkRecord
        {
            public TLinkAddress Source;
            public TLinkAddress Target;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawLinksStorage{TLinkAddress}"/> class.
        /// </summary>
        /// <param name="device">The raw block device to use for storage.</param>
        /// <param name="capacity">Maximum number of links to store.</param>
        /// <param name="initializeNew">If true, initializes a new storage. If false, opens existing storage.</param>
        public RawLinksStorage(IRawBlockDevice device, TLinkAddress capacity, bool initializeNew = true)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _blockSize = device.BlockSize;
            _linkRecordSize = Marshal.SizeOf<LinkRecord>();

            // Align header to block size
            var rawHeaderSize = Marshal.SizeOf<StorageHeader>();
            _headerSize = ((rawHeaderSize + _blockSize - 1) / _blockSize) * _blockSize;

            // Calculate required device size
            var capacityValue = ConvertToLong(capacity);
            var requiredSize = _headerSize + (capacityValue * _linkRecordSize);
            if (requiredSize > device.SizeInBytes)
                throw new ArgumentException($"Device too small. Required: {requiredSize}, Available: {device.SizeInBytes}");

            if (initializeNew)
            {
                _capacity = capacity;
                _count = default;
                WriteHeader();
            }
            else
            {
                ReadHeader();
                var storedCapacity = ConvertFromLong(((StorageHeader*)null)->Capacity);
                if (!EqualityComparer.Equals(storedCapacity, capacity))
                    throw new InvalidOperationException($"Stored capacity does not match requested capacity");
                _capacity = capacity;
            }
        }

        /// <inheritdoc/>
        public TLinkAddress Capacity => _capacity;

        /// <inheritdoc/>
        public TLinkAddress Count => _count;

        /// <inheritdoc/>
        public IRawBlockDevice UnderlyingDevice => _device;

        /// <inheritdoc/>
        public void ReadLink(TLinkAddress address, out TLinkAddress source, out TLinkAddress target)
        {
            ValidateDisposed();
            ValidateAddress(address);

            var offset = GetLinkOffset(address);
            var buffer = new byte[_blockSize];
            _device.Read(offset, buffer, _blockSize);

            fixed (byte* ptr = buffer)
            {
                var record = (LinkRecord*)ptr;
                source = record->Source;
                target = record->Target;
            }
        }

        /// <inheritdoc/>
        public void WriteLink(TLinkAddress address, TLinkAddress source, TLinkAddress target)
        {
            ValidateDisposed();
            ValidateAddress(address);

            var offset = GetLinkOffset(address);
            var buffer = new byte[_blockSize];

            fixed (byte* ptr = buffer)
            {
                var record = (LinkRecord*)ptr;
                record->Source = source;
                record->Target = target;
            }

            _device.Write(offset, buffer, _blockSize);

            // Update count if this is a new link (simplified - in real implementation would check if link exists)
            var addressValue = ConvertToLong(address);
            var countValue = ConvertToLong(_count);
            if (addressValue >= countValue)
            {
                _count = ConvertFromLong(addressValue + 1);
                WriteHeader();
            }
        }

        /// <inheritdoc/>
        public void Sync()
        {
            ValidateDisposed();
            WriteHeader();
            _device.Flush();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                Sync();
                _disposed = true;
            }
        }

        private void ReadHeader()
        {
            var buffer = new byte[_blockSize];
            _device.Read(0, buffer, _blockSize);

            fixed (byte* ptr = buffer)
            {
                var header = (StorageHeader*)ptr;

                if (header->MagicNumber != MagicNumber)
                    throw new InvalidOperationException("Invalid magic number - not a valid links storage");
                if (header->Version != CurrentVersion)
                    throw new InvalidOperationException($"Unsupported version: {header->Version}");
                if (header->LinkRecordSize != _linkRecordSize)
                    throw new InvalidOperationException("Link record size mismatch");

                _count = ConvertFromLong(header->Count);
            }
        }

        private void WriteHeader()
        {
            var buffer = new byte[_blockSize];

            fixed (byte* ptr = buffer)
            {
                var header = (StorageHeader*)ptr;
                header->MagicNumber = MagicNumber;
                header->Version = CurrentVersion;
                header->Capacity = ConvertToLong(_capacity);
                header->Count = ConvertToLong(_count);
                header->LinkRecordSize = _linkRecordSize;
                header->Reserved1 = 0;
                header->Reserved2 = 0;
                header->Checksum = CalculateChecksum(header);
            }

            _device.Write(0, buffer, _blockSize);
        }

        private long GetLinkOffset(TLinkAddress address)
        {
            var addressValue = ConvertToLong(address);
            var byteOffset = _headerSize + (addressValue * _linkRecordSize);

            // Align to block boundary
            return (byteOffset / _blockSize) * _blockSize;
        }

        private long CalculateChecksum(StorageHeader* header)
        {
            // Simple XOR checksum for demonstration
            return header->MagicNumber ^ header->Version ^ header->Capacity ^ header->Count ^ header->LinkRecordSize;
        }

        private void ValidateDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RawLinksStorage<TLinkAddress>));
        }

        private void ValidateAddress(TLinkAddress address)
        {
            var addressValue = ConvertToLong(address);
            var capacityValue = ConvertToLong(_capacity);

            if (addressValue < 0 || addressValue >= capacityValue)
                throw new ArgumentOutOfRangeException(nameof(address), $"Address must be in range [0, {capacityValue})");
        }

        // Helper methods to convert between TLinkAddress and long
        private static long ConvertToLong(TLinkAddress value)
        {
            if (typeof(TLinkAddress) == typeof(ulong))
                return (long)(ulong)(object)value;
            if (typeof(TLinkAddress) == typeof(uint))
                return (uint)(object)value;
            if (typeof(TLinkAddress) == typeof(ushort))
                return (ushort)(object)value;
            if (typeof(TLinkAddress) == typeof(byte))
                return (byte)(object)value;
            throw new NotSupportedException($"Type {typeof(TLinkAddress)} not supported");
        }

        private static TLinkAddress ConvertFromLong(long value)
        {
            if (typeof(TLinkAddress) == typeof(ulong))
                return (TLinkAddress)(object)(ulong)value;
            if (typeof(TLinkAddress) == typeof(uint))
                return (TLinkAddress)(object)(uint)value;
            if (typeof(TLinkAddress) == typeof(ushort))
                return (TLinkAddress)(object)(ushort)value;
            if (typeof(TLinkAddress) == typeof(byte))
                return (TLinkAddress)(object)(byte)value;
            throw new NotSupportedException($"Type {typeof(TLinkAddress)} not supported");
        }

        private static class EqualityComparer
        {
            public static bool Equals(TLinkAddress x, TLinkAddress y)
            {
                return ConvertToLong(x) == ConvertToLong(y);
            }
        }
    }
}
