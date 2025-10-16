using System;
using System.Collections.Generic;
using Platform.Data.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Provides methods to convert a link structure to an array representation.
    /// Supports different modes of recursion handling as specified in issue #77.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class LinkToArrayConverter<TLinkAddress>
    {
        private readonly Func<TLinkAddress, TLinkAddress> _getSource;
        private readonly Func<TLinkAddress, TLinkAddress> _getTarget;
        private readonly Func<TLinkAddress, bool> _isElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkToArrayConverter{TLinkAddress}"/> class.
        /// </summary>
        /// <param name="getSource">Function to get the source of a link.</param>
        /// <param name="getTarget">Function to get the target of a link.</param>
        /// <param name="isElement">Function to check if a link is an element (leaf node).</param>
        public LinkToArrayConverter(
            Func<TLinkAddress, TLinkAddress> getSource,
            Func<TLinkAddress, TLinkAddress> getTarget,
            Func<TLinkAddress, bool> isElement)
        {
            _getSource = getSource ?? throw new ArgumentNullException(nameof(getSource));
            _getTarget = getTarget ?? throw new ArgumentNullException(nameof(getTarget));
            _isElement = isElement ?? throw new ArgumentNullException(nameof(isElement));
        }

        /// <summary>
        /// Converts a link to an array without recursion checking.
        /// This is a simple wrapper around StopableSequenceWalker.WalkRight.
        /// </summary>
        /// <param name="link">The link to convert.</param>
        /// <returns>An array representation of the link structure.</returns>
        public TLinkAddress[] ToArrayWithoutRecursionCheck(TLinkAddress link)
        {
            var result = new List<TLinkAddress>();
            StopableSequenceWalker.WalkRight(
                link,
                _getSource,
                _getTarget,
                _isElement,
                element => { result.Add(element); return true; });
            return result.ToArray();
        }

        /// <summary>
        /// Converts a link to an array with recursion detection.
        /// If an element contains itself as one of its elements, it will be represented as the element itself in the array.
        /// Otherwise, the sequence will be split and flattened into the array.
        /// </summary>
        /// <param name="link">The link to convert.</param>
        /// <returns>An array representation of the link structure with recursion detection.</returns>
        public TLinkAddress[] ToArrayWithRecursionCheck(TLinkAddress link)
        {
            var result = new List<TLinkAddress>();
            var visited = new HashSet<TLinkAddress>();
            ConvertWithRecursionCheck(link, result, visited);
            return result.ToArray();
        }

        private void ConvertWithRecursionCheck(TLinkAddress link, List<TLinkAddress> result, HashSet<TLinkAddress> visited)
        {
            if (_isElement(link))
            {
                result.Add(link);
                return;
            }

            // Check if we've already visited this link (recursion detected)
            if (visited.Contains(link))
            {
                // Element has itself as one of its elements - add as single element
                result.Add(link);
                return;
            }

            // Mark as visited
            visited.Add(link);

            // Check if this link eventually references itself
            if (ContainsSelf(link, visited))
            {
                // Element contains itself - represent as single element
                result.Add(link);
            }
            else
            {
                // No self-reference - expand the sequence
                var source = _getSource(link);
                var target = _getTarget(link);

                ConvertWithRecursionCheck(source, result, visited);
                ConvertWithRecursionCheck(target, result, visited);
            }

            // Remove from visited when backtracking
            visited.Remove(link);
        }

        private bool ContainsSelf(TLinkAddress link, HashSet<TLinkAddress> visited)
        {
            var source = _getSource(link);
            var target = _getTarget(link);

            // Check if source or target directly equals the current link
            if (EqualityComparer<TLinkAddress>.Default.Equals(source, link) ||
                EqualityComparer<TLinkAddress>.Default.Equals(target, link))
            {
                return true;
            }

            // Recursively check children if they're not elements and not yet visited
            if (!_isElement(source) && !visited.Contains(source))
            {
                var tempVisited = new HashSet<TLinkAddress>(visited) { source };
                if (ContainsLink(source, link, tempVisited))
                {
                    return true;
                }
            }

            if (!_isElement(target) && !visited.Contains(target))
            {
                var tempVisited = new HashSet<TLinkAddress>(visited) { target };
                if (ContainsLink(target, link, tempVisited))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsLink(TLinkAddress current, TLinkAddress searchFor, HashSet<TLinkAddress> visited)
        {
            if (_isElement(current))
            {
                return false;
            }

            var source = _getSource(current);
            var target = _getTarget(current);

            if (EqualityComparer<TLinkAddress>.Default.Equals(source, searchFor) ||
                EqualityComparer<TLinkAddress>.Default.Equals(target, searchFor))
            {
                return true;
            }

            if (!_isElement(source) && !visited.Contains(source))
            {
                visited.Add(source);
                if (ContainsLink(source, searchFor, visited))
                {
                    return true;
                }
            }

            if (!_isElement(target) && !visited.Contains(target))
            {
                visited.Add(target);
                if (ContainsLink(target, searchFor, visited))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a link to an array with a configurable recursion depth limit.
        /// Allows exploration of recursive structures up to N steps deep.
        /// </summary>
        /// <param name="link">The link to convert.</param>
        /// <param name="maxDepth">Maximum recursion depth (1 to N).
        /// If 1, shows all surroundings until it reaches itself.
        /// If 2 to N, surroundings of recursive elements are repeated N times.</param>
        /// <returns>An array representation of the link structure with limited recursion depth.</returns>
        public TLinkAddress[] ToArrayWithRecursionDepth(TLinkAddress link, int maxDepth)
        {
            if (maxDepth < 1)
            {
                throw new ArgumentException("Maximum depth must be at least 1.", nameof(maxDepth));
            }

            var result = new List<TLinkAddress>();
            var pathStack = new Stack<(TLinkAddress link, int depth)>();
            ConvertWithDepthLimit(link, result, pathStack, maxDepth);
            return result.ToArray();
        }

        private void ConvertWithDepthLimit(
            TLinkAddress link,
            List<TLinkAddress> result,
            Stack<(TLinkAddress link, int depth)> pathStack,
            int maxDepth)
        {
            if (_isElement(link))
            {
                result.Add(link);
                return;
            }

            // Check if this link is already in the path (recursion detected)
            var currentDepth = 0;
            foreach (var (visitedLink, depth) in pathStack)
            {
                if (EqualityComparer<TLinkAddress>.Default.Equals(visitedLink, link))
                {
                    currentDepth = depth + 1;
                    break;
                }
            }

            if (currentDepth > 0 && currentDepth > maxDepth)
            {
                // Reached max depth for this recursive element
                result.Add(link);
                return;
            }

            // Add to path
            pathStack.Push((link, currentDepth));

            var source = _getSource(link);
            var target = _getTarget(link);

            ConvertWithDepthLimit(source, result, pathStack, maxDepth);
            ConvertWithDepthLimit(target, result, pathStack, maxDepth);

            // Remove from path when backtracking
            pathStack.Pop();
        }
    }
}
