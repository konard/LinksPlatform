using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.Triplets.Memory
{
    /// <summary>
    /// Provides extension methods for Link operations.
    /// </summary>
    public static class LinkExtensions
    {
        /// <summary>
        /// Gets all links that directly or indirectly reference this link.
        /// </summary>
        /// <param name="link">The link to get referers for.</param>
        /// <returns>All referers by source, linker, and target.</returns>
        public static IEnumerable<Link> GetAllReferers(this Link link)
        {
            if (link == null)
                yield break;

            foreach (var referer in link.ReferersBySource)
                yield return referer;

            foreach (var referer in link.ReferersByLinker)
                yield return referer;

            foreach (var referer in link.ReferersByTarget)
                yield return referer;
        }

        /// <summary>
        /// Counts all referers of this link.
        /// </summary>
        /// <param name="link">The link to count referers for.</param>
        /// <returns>The total number of referers.</returns>
        public static int CountReferers(this Link link)
        {
            if (link == null)
                return 0;

            return link.ReferersBySource.Count() +
                   link.ReferersByLinker.Count() +
                   link.ReferersByTarget.Count();
        }

        /// <summary>
        /// Checks if this link has any referers.
        /// </summary>
        /// <param name="link">The link to check.</param>
        /// <returns>True if the link has referers, false otherwise.</returns>
        public static bool HasReferers(this Link link)
        {
            if (link == null)
                return false;

            return link.ReferersBySource.Any() ||
                   link.ReferersByLinker.Any() ||
                   link.ReferersByTarget.Any();
        }

        /// <summary>
        /// Checks if this link is a self-reference (references itself in any position).
        /// </summary>
        /// <param name="link">The link to check.</param>
        /// <returns>True if the link references itself, false otherwise.</returns>
        public static bool IsSelfReference(this Link link)
        {
            if (link == null)
                return false;

            return link.Source == link || link.Linker == link || link.Target == link;
        }

        /// <summary>
        /// Checks if this link forms a complete self-loop (references itself in all positions).
        /// </summary>
        /// <param name="link">The link to check.</param>
        /// <returns>True if the link references itself in all three positions, false otherwise.</returns>
        public static bool IsCompleteSelfLoop(this Link link)
        {
            if (link == null)
                return false;

            return link.Source == link && link.Linker == link && link.Target == link;
        }

        /// <summary>
        /// Finds a link with the specified components starting from this link's referers.
        /// </summary>
        /// <param name="link">The starting link.</param>
        /// <param name="source">The source to search for.</param>
        /// <param name="linker">The linker to search for.</param>
        /// <param name="target">The target to search for.</param>
        /// <returns>A matching link if found, otherwise null.</returns>
        public static Link? FindReferer(this Link link, Link? source, Link? linker, Link? target)
        {
            if (link == null)
                return null;

            foreach (var referer in link.GetAllReferers())
            {
                if ((source == null || referer.Source == source) &&
                    (linker == null || referer.Linker == linker) &&
                    (target == null || referer.Target == target))
                {
                    return referer;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all links that match a specific pattern from this link's referers.
        /// </summary>
        /// <param name="link">The starting link.</param>
        /// <param name="source">The source to match (null matches any).</param>
        /// <param name="linker">The linker to match (null matches any).</param>
        /// <param name="target">The target to match (null matches any).</param>
        /// <returns>All matching links.</returns>
        public static IEnumerable<Link> FindReferers(this Link link, Link? source = null, Link? linker = null, Link? target = null)
        {
            if (link == null)
                yield break;

            foreach (var referer in link.GetAllReferers())
            {
                if ((source == null || referer.Source == source) &&
                    (linker == null || referer.Linker == linker) &&
                    (target == null || referer.Target == target))
                {
                    yield return referer;
                }
            }
        }
    }
}
