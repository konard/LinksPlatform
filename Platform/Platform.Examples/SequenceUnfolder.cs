using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Provides functionality to unfold links into sequences by traversing their internal structure.
    /// </summary>
    /// <typeparam name="TLink">The type of link address.</typeparam>
    public class SequenceUnfolder<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly int _maxDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceUnfolder{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="maxDepth">The maximum depth to unfold. Defaults to 1000.</param>
        public SequenceUnfolder(ILinks<TLink> links, int maxDepth = 1000)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _maxDepth = maxDepth;
        }

        /// <summary>
        /// Unfolds a link into a sequence by traversing its source and target links recursively.
        /// </summary>
        /// <param name="link">The link to unfold.</param>
        /// <returns>A list of links representing the unfolded sequence.</returns>
        public IList<TLink> Unfold(TLink link)
        {
            var sequence = new List<TLink>();
            UnfoldRecursive(link, sequence, 0);
            return sequence;
        }

        /// <summary>
        /// Recursively unfolds a link into a sequence.
        /// </summary>
        private void UnfoldRecursive(TLink link, List<TLink> sequence, int depth)
        {
            if (depth >= _maxDepth)
            {
                // Add the link itself if we've reached max depth
                sequence.Add(link);
                return;
            }

            // Check if the link exists
            if (!_links.Exists(link))
            {
                sequence.Add(link);
                return;
            }

            var linkValue = _links.GetLink(link);
            var source = linkValue[_links.Constants.SourcePart];
            var target = linkValue[_links.Constants.TargetPart];

            // If source and target are the same as the link itself, it's a self-reference
            if (EqualityComparer<TLink>.Default.Equals(source, link) &&
                EqualityComparer<TLink>.Default.Equals(target, link))
            {
                sequence.Add(link);
                return;
            }

            // If source or target point to themselves, they are leaf nodes
            var sourceIsLeaf = IsLeafNode(source);
            var targetIsLeaf = IsLeafNode(target);

            if (sourceIsLeaf && targetIsLeaf)
            {
                // Both are leaf nodes, add them to the sequence
                sequence.Add(source);
                sequence.Add(target);
            }
            else if (sourceIsLeaf)
            {
                // Source is a leaf, add it and unfold target
                sequence.Add(source);
                UnfoldRecursive(target, sequence, depth + 1);
            }
            else if (targetIsLeaf)
            {
                // Target is a leaf, unfold source and add target
                UnfoldRecursive(source, sequence, depth + 1);
                sequence.Add(target);
            }
            else
            {
                // Neither is a leaf, unfold both
                UnfoldRecursive(source, sequence, depth + 1);
                UnfoldRecursive(target, sequence, depth + 1);
            }
        }

        /// <summary>
        /// Checks if a link is a leaf node (doesn't have further links or points to itself).
        /// </summary>
        private bool IsLeafNode(TLink link)
        {
            if (!_links.Exists(link))
            {
                return true;
            }

            var linkValue = _links.GetLink(link);
            var source = linkValue[_links.Constants.SourcePart];
            var target = linkValue[_links.Constants.TargetPart];

            // A leaf node points to itself or doesn't exist
            return EqualityComparer<TLink>.Default.Equals(source, link) &&
                   EqualityComparer<TLink>.Default.Equals(target, link);
        }

        /// <summary>
        /// Unfolds a link into a flat sequence, treating each link as a pair (source, target).
        /// This is a simpler alternative that doesn't recursively unfold nested structures.
        /// </summary>
        /// <param name="link">The link to unfold.</param>
        /// <returns>A list containing [source, target] of the link.</returns>
        public IList<TLink> UnfoldSimple(TLink link)
        {
            var sequence = new List<TLink>();

            if (!_links.Exists(link))
            {
                sequence.Add(link);
                return sequence;
            }

            var linkValue = _links.GetLink(link);
            sequence.Add(linkValue[_links.Constants.SourcePart]);
            sequence.Add(linkValue[_links.Constants.TargetPart]);

            return sequence;
        }

        /// <summary>
        /// Unfolds a sequence of links by following the target chain.
        /// This treats links as a linked list where each link points to the next via its target.
        /// </summary>
        /// <param name="startLink">The starting link of the sequence.</param>
        /// <returns>A list of links in the sequence.</returns>
        public IList<TLink> UnfoldChain(TLink startLink)
        {
            var sequence = new List<TLink>();
            var visited = new HashSet<TLink>();
            var current = startLink;

            while (_links.Exists(current) && !visited.Contains(current))
            {
                visited.Add(current);
                var linkValue = _links.GetLink(current);
                var source = linkValue[_links.Constants.SourcePart];
                var target = linkValue[_links.Constants.TargetPart];

                sequence.Add(source);

                // Move to the next link in the chain
                if (EqualityComparer<TLink>.Default.Equals(target, current))
                {
                    // We've reached the end (self-reference)
                    break;
                }

                current = target;
            }

            return sequence;
        }
    }
}
