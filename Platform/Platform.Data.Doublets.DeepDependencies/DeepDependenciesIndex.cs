using System;
using System.Collections.Generic;
using Platform.Data;

namespace Platform.Data.Doublets.DeepDependencies
{
    /// <summary>
    /// Tracks deep dependencies for links in an associative data store.
    /// Maintains sets of all links that are used by each link (dependencies)
    /// and all links that reference each link (referrers).
    /// </summary>
    /// <typeparam name="TLink">The type used for link addresses.</typeparam>
    public class DeepDependenciesIndex<TLink>
        where TLink : struct
    {
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<TLink, HashSet<TLink>> _usedByLink;
        private readonly Dictionary<TLink, HashSet<TLink>> _referencingLink;

        /// <summary>
        /// Initializes a new instance of the DeepDependenciesIndex class.
        /// </summary>
        /// <param name="links">The links storage to track dependencies for.</param>
        public DeepDependenciesIndex(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _usedByLink = new Dictionary<TLink, HashSet<TLink>>();
            _referencingLink = new Dictionary<TLink, HashSet<TLink>>();
        }

        /// <summary>
        /// Gets all links that are used by the specified link (deep dependencies).
        /// This includes the link's Source, Target, and recursively all links they use.
        /// </summary>
        /// <param name="link">The link to get dependencies for.</param>
        /// <returns>A set of all links that are used by the specified link.</returns>
        public HashSet<TLink> GetUsedByLink(TLink link)
        {
            if (_usedByLink.TryGetValue(link, out var cached))
            {
                return new HashSet<TLink>(cached);
            }

            var used = new HashSet<TLink>();
            ComputeUsedByLink(link, used);
            _usedByLink[link] = used;
            return new HashSet<TLink>(used);
        }

        /// <summary>
        /// Gets all links that reference the specified link (deep referrers).
        /// This includes all links that have this link as Source or Target,
        /// and recursively all links that reference those links.
        /// </summary>
        /// <param name="link">The link to get referrers for.</param>
        /// <returns>A set of all links that reference the specified link.</returns>
        public HashSet<TLink> GetReferencingLink(TLink link)
        {
            if (_referencingLink.TryGetValue(link, out var cached))
            {
                return new HashSet<TLink>(cached);
            }

            var referencing = new HashSet<TLink>();
            ComputeReferencingLink(link, referencing);
            _referencingLink[link] = referencing;
            return new HashSet<TLink>(referencing);
        }

        /// <summary>
        /// Clears the cached dependency information for the specified link.
        /// </summary>
        /// <param name="link">The link to clear cache for.</param>
        public void InvalidateCache(TLink link)
        {
            _usedByLink.Remove(link);
            _referencingLink.Remove(link);
        }

        /// <summary>
        /// Clears all cached dependency information.
        /// </summary>
        public void ClearCache()
        {
            _usedByLink.Clear();
            _referencingLink.Clear();
        }

        private void ComputeUsedByLink(TLink link, HashSet<TLink> result)
        {
            if (result.Contains(link))
            {
                return; // Avoid infinite recursion in cycles
            }

            result.Add(link);

            var linkContents = _links.GetLink(link);
            if (linkContents == null)
            {
                return;
            }

            var source = linkContents[_links.Constants.SourcePart];
            var target = linkContents[_links.Constants.TargetPart];

            // Recursively add dependencies from source and target
            if (!EqualityComparer<TLink>.Default.Equals(source, default(TLink)) &&
                !EqualityComparer<TLink>.Default.Equals(source, link))
            {
                ComputeUsedByLink(source, result);
            }

            if (!EqualityComparer<TLink>.Default.Equals(target, default(TLink)) &&
                !EqualityComparer<TLink>.Default.Equals(target, link))
            {
                ComputeUsedByLink(target, result);
            }
        }

        private void ComputeReferencingLink(TLink link, HashSet<TLink> result)
        {
            if (result.Contains(link))
            {
                return; // Avoid infinite recursion in cycles
            }

            result.Add(link);

            // Find all links that reference this link
            _links.Each(referrer =>
            {
                var linkContents = _links.GetLink(referrer);
                if (linkContents != null)
                {
                    var source = linkContents[_links.Constants.SourcePart];
                    var target = linkContents[_links.Constants.TargetPart];

                    if (EqualityComparer<TLink>.Default.Equals(source, link) ||
                        EqualityComparer<TLink>.Default.Equals(target, link))
                    {
                        if (!EqualityComparer<TLink>.Default.Equals(referrer, link))
                        {
                            ComputeReferencingLink(referrer, result);
                        }
                    }
                }
                return _links.Constants.Continue;
            });
        }
    }
}
