using System;

namespace Platform.Examples.ReadAccessMetrics
{
    /// <summary>
    /// Interface for tracking read access metrics of links.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public interface IReadAccessMetrics<TLinkAddress>
    {
        /// <summary>
        /// Gets the average read access time for a specific link.
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The average time between read accesses, or null if no reads have occurred.</returns>
        TimeSpan? GetAverageReadAccessTime(TLinkAddress linkAddress);

        /// <summary>
        /// Gets the last read access time for a specific link.
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The timestamp of the last read access, or null if no reads have occurred.</returns>
        DateTime? GetLastReadAccessTime(TLinkAddress linkAddress);

        /// <summary>
        /// Gets the creation date and time for a specific link.
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The timestamp when the link was created, or null if unknown.</returns>
        DateTime? GetCreationDateTime(TLinkAddress linkAddress);

        /// <summary>
        /// Gets the total count of read accesses for a specific link.
        /// </summary>
        /// <param name="linkAddress">The address of the link.</param>
        /// <returns>The total number of times the link has been read.</returns>
        long GetTotalReadAccessCount(TLinkAddress linkAddress);
    }
}
