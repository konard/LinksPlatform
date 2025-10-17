using System;
using System.Collections;
using System.Collections.Generic;
using Platform.Data;
using Platform.Numbers;

namespace Platform.Data.Doublets.DeepDependencies
{
    /// <summary>
    /// Tracks deep dependencies for links using BitArray for efficient storage.
    /// This implementation is more memory-efficient for dense link address spaces.
    /// </summary>
    /// <typeparam name="TLink">The type used for link addresses.</typeparam>
    public class DeepDependenciesBitStringIndex<TLink>
        where TLink : struct, IComparable<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<TLink, BitArray> _usedByLink;
        private readonly Dictionary<TLink, BitArray> _referencingLink;
        private readonly int _maxLinkIndex;

        /// <summary>
        /// Initializes a new instance of the DeepDependenciesBitStringIndex class.
        /// </summary>
        /// <param name="links">The links storage to track dependencies for.</param>
        /// <param name="maxLinkIndex">The maximum link index to support.</param>
        public DeepDependenciesBitStringIndex(ILinks<TLink> links, int maxLinkIndex = 65536)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _maxLinkIndex = maxLinkIndex;
            _usedByLink = new Dictionary<TLink, BitArray>();
            _referencingLink = new Dictionary<TLink, BitArray>();
        }

        /// <summary>
        /// Gets a BitArray representing all links that are used by the specified link.
        /// Each bit index corresponds to a link address, with bit set to 1 if the link is used.
        /// </summary>
        /// <param name="link">The link to get dependencies for.</param>
        /// <returns>A BitArray where set bits indicate used links.</returns>
        public BitArray GetUsedByLink(TLink link)
        {
            if (_usedByLink.TryGetValue(link, out var cached))
            {
                return new BitArray(cached);
            }

            var used = new BitArray(_maxLinkIndex);
            var visited = new HashSet<TLink>();
            ComputeUsedByLink(link, used, visited);
            _usedByLink[link] = used;
            return new BitArray(used);
        }

        /// <summary>
        /// Gets a BitArray representing all links that reference the specified link.
        /// Each bit index corresponds to a link address, with bit set to 1 if the link references the target.
        /// </summary>
        /// <param name="link">The link to get referrers for.</param>
        /// <returns>A BitArray where set bits indicate referencing links.</returns>
        public BitArray GetReferencingLink(TLink link)
        {
            if (_referencingLink.TryGetValue(link, out var cached))
            {
                return new BitArray(cached);
            }

            var referencing = new BitArray(_maxLinkIndex);
            var visited = new HashSet<TLink>();
            ComputeReferencingLink(link, referencing, visited);
            _referencingLink[link] = referencing;
            return new BitArray(referencing);
        }

        /// <summary>
        /// Gets the set of link addresses from a BitArray.
        /// </summary>
        /// <param name="bitArray">The BitArray to convert.</param>
        /// <returns>A set of link addresses.</returns>
        public HashSet<TLink> BitArrayToSet(BitArray bitArray)
        {
            var result = new HashSet<TLink>();
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i])
                {
                    result.Add(ConvertToLink(i));
                }
            }
            return result;
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

        private void ComputeUsedByLink(TLink link, BitArray result, HashSet<TLink> visited)
        {
            if (visited.Contains(link))
            {
                return; // Avoid infinite recursion in cycles
            }

            visited.Add(link);

            int linkIndex = ConvertToInt(link);
            if (linkIndex >= 0 && linkIndex < _maxLinkIndex)
            {
                result[linkIndex] = true;
            }

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
                ComputeUsedByLink(source, result, visited);
            }

            if (!EqualityComparer<TLink>.Default.Equals(target, default(TLink)) &&
                !EqualityComparer<TLink>.Default.Equals(target, link))
            {
                ComputeUsedByLink(target, result, visited);
            }
        }

        private void ComputeReferencingLink(TLink link, BitArray result, HashSet<TLink> visited)
        {
            if (visited.Contains(link))
            {
                return; // Avoid infinite recursion in cycles
            }

            visited.Add(link);

            int linkIndex = ConvertToInt(link);
            if (linkIndex >= 0 && linkIndex < _maxLinkIndex)
            {
                result[linkIndex] = true;
            }

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
                            ComputeReferencingLink(referrer, result, visited);
                        }
                    }
                }
                return _links.Constants.Continue;
            });
        }

        private int ConvertToInt(TLink link)
        {
            return Convert.ToInt32(link);
        }

        private TLink ConvertToLink(int index)
        {
            return (TLink)Convert.ChangeType(index, typeof(TLink));
        }
    }
}
