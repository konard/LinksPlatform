using System;

namespace Platform.System.Core
{
    /// <summary>
    /// Represents the memory abstraction for low-level memory management
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// Gets the total available memory in bytes
        /// </summary>
        long TotalBytes { get; }

        /// <summary>
        /// Gets the available free memory in bytes
        /// </summary>
        long FreeBytes { get; }

        /// <summary>
        /// Allocates a block of memory
        /// </summary>
        /// <param name="size">Size in bytes to allocate</param>
        /// <returns>Pointer to allocated memory</returns>
        IntPtr Allocate(long size);

        /// <summary>
        /// Frees a previously allocated block of memory
        /// </summary>
        /// <param name="pointer">Pointer to the memory block</param>
        void Free(IntPtr pointer);
    }
}
