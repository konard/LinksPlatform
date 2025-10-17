#nullable enable

using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Data.Triplets.Doublets
{
    /// <summary>
    /// Provides extension methods for working with doublet links to support triplet operations.
    /// </summary>
    public static class TripletLinksExtensions
    {
        /// <summary>
        /// Gets a link as an array [Index, Source, Target].
        /// </summary>
        /// <typeparam name="TLinkAddress">The type used for link addresses.</typeparam>
        /// <param name="links">The doublets links instance.</param>
        /// <param name="link">The link to retrieve.</param>
        /// <returns>An array containing [Index, Source, Target], or null if not found.</returns>
        public static IList<TLinkAddress>? GetLink<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress link)
        {
            IList<TLinkAddress>? result = null;

            links.Each(l =>
            {
                result = l;
                return links.Constants.Break;
            }, new TLinkAddress[] { link });

            return result;
        }

        /// <summary>
        /// Gets or creates a link with the specified source and target.
        /// </summary>
        /// <typeparam name="TLinkAddress">The type used for link addresses.</typeparam>
        /// <param name="links">The doublets links instance.</param>
        /// <param name="source">The source link.</param>
        /// <param name="target">The target link.</param>
        /// <returns>The address of the existing or newly created link.</returns>
        public static TLinkAddress GetOrCreate<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target)
        {
            // Try to find existing link
            TLinkAddress existingLink = default!;
            bool found = false;

            links.Each(link =>
            {
                existingLink = link[links.Constants.IndexPart];
                found = true;
                return links.Constants.Break;
            }, new TLinkAddress[] { links.Constants.Any, source, target });

            if (found)
            {
                return existingLink;
            }

            // Create new link
            return links.Create(new TLinkAddress[] { source, target });
        }
    }
}
