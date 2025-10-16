using System;

namespace Platform.Data.Core
{
    /// <summary>
    /// Represents a memory manager interface for managing Link storage.
    /// This abstraction allows different memory management strategies (GC, Memory-Mapped Files, etc.)
    /// </summary>
    /// <typeparam name="TLink">The type used to represent link addresses/identifiers.</typeparam>
    public interface ILinksMemoryManager<TLink> : IDisposable
    {
        /// <summary>
        /// Gets the total capacity of allocated memory for links.
        /// </summary>
        TLink Capacity { get; }

        /// <summary>
        /// Allocates space for a new link and returns its identifier.
        /// </summary>
        /// <returns>The identifier of the newly allocated link.</returns>
        TLink Allocate();

        /// <summary>
        /// Frees the space occupied by the link with the specified identifier.
        /// </summary>
        /// <param name="link">The identifier of the link to free.</param>
        void Free(TLink link);

        /// <summary>
        /// Gets the source of the specified link.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <returns>The source link identifier.</returns>
        TLink GetSource(TLink link);

        /// <summary>
        /// Gets the target of the specified link.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <returns>The target link identifier.</returns>
        TLink GetTarget(TLink link);

        /// <summary>
        /// Sets the source of the specified link.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <param name="source">The new source link identifier.</param>
        void SetSource(TLink link, TLink source);

        /// <summary>
        /// Sets the target of the specified link.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <param name="target">The new target link identifier.</param>
        void SetTarget(TLink link, TLink target);

        /// <summary>
        /// Checks if the specified link is allocated.
        /// </summary>
        /// <param name="link">The link identifier to check.</param>
        /// <returns>True if the link is allocated, false otherwise.</returns>
        bool IsAllocated(TLink link);

        /// <summary>
        /// Ensures the memory can accommodate at least the specified capacity.
        /// </summary>
        /// <param name="capacity">The minimum required capacity.</param>
        void EnsureCapacity(TLink capacity);
    }
}
