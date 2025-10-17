using System;

namespace Platform.Data.Core.RawStorage
{
    /// <summary>
    /// Provides direct access to a raw storage device for links storage.
    /// This interface abstracts the raw block device to provide link-specific operations.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type used for link addresses (e.g., ulong, uint).</typeparam>
    public interface IRawLinksStorage<TLinkAddress> : IDisposable
    {
        /// <summary>
        /// Gets the maximum number of links that can be stored in the allocated device space.
        /// </summary>
        TLinkAddress Capacity { get; }

        /// <summary>
        /// Gets the current number of links stored. This count is persisted in the device header.
        /// </summary>
        TLinkAddress Count { get; }

        /// <summary>
        /// Reads a link at the specified address from the raw device.
        /// </summary>
        /// <param name="address">The address of the link to read (1-based indexing).</param>
        /// <param name="source">Output parameter for the source link address.</param>
        /// <param name="target">Output parameter for the target link address.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address is out of valid range.</exception>
        void ReadLink(TLinkAddress address, out TLinkAddress source, out TLinkAddress target);

        /// <summary>
        /// Writes a link at the specified address to the raw device.
        /// </summary>
        /// <param name="address">The address where the link should be written (1-based indexing).</param>
        /// <param name="source">The source link address.</param>
        /// <param name="target">The target link address.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address is out of valid range.</exception>
        void WriteLink(TLinkAddress address, TLinkAddress source, TLinkAddress target);

        /// <summary>
        /// Ensures all pending writes are persisted to the device.
        /// </summary>
        void Sync();

        /// <summary>
        /// Gets the underlying raw block device for direct access if needed.
        /// </summary>
        IRawBlockDevice UnderlyingDevice { get; }
    }
}
