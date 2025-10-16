using System;
using System.Collections.Generic;

namespace IndexBenchmarks
{
    /// <summary>
    /// Common interface for all link index implementations
    /// Represents an index that maps (source, target) pairs to link addresses
    /// </summary>
    /// <typeparam name="TLink">The type used for link addresses</typeparam>
    public interface ILinksIndex<TLink> where TLink : struct
    {
        /// <summary>
        /// Adds a link to the index
        /// </summary>
        /// <param name="linkAddress">The address of the link</param>
        /// <param name="source">The source of the link</param>
        /// <param name="target">The target of the link</param>
        void Add(TLink linkAddress, TLink source, TLink target);

        /// <summary>
        /// Removes a link from the index
        /// </summary>
        /// <param name="linkAddress">The address of the link to remove</param>
        /// <param name="source">The source of the link</param>
        /// <param name="target">The target of the link</param>
        /// <returns>True if the link was removed, false if it wasn't found</returns>
        bool Remove(TLink linkAddress, TLink source, TLink target);

        /// <summary>
        /// Searches for a link with the given source and target
        /// </summary>
        /// <param name="source">The source to search for</param>
        /// <param name="target">The target to search for</param>
        /// <returns>The link address if found, default(TLink) otherwise</returns>
        TLink Search(TLink source, TLink target);

        /// <summary>
        /// Enumerates all links with the given source
        /// </summary>
        /// <param name="source">The source to search for</param>
        /// <returns>Enumerable of (linkAddress, target) pairs</returns>
        IEnumerable<(TLink linkAddress, TLink target)> GetBySource(TLink source);

        /// <summary>
        /// Enumerates all links with the given target
        /// </summary>
        /// <param name="target">The target to search for</param>
        /// <returns>Enumerable of (linkAddress, source) pairs</returns>
        IEnumerable<(TLink linkAddress, TLink source)> GetByTarget(TLink target);

        /// <summary>
        /// Gets the total number of links in the index
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Clears all links from the index
        /// </summary>
        void Clear();
    }
}
