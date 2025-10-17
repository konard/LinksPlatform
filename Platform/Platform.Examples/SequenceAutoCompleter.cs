using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Implements auto-complete algorithm based on stored sequences in Links doublets structure.
    /// Searches for sequences that start with a given prefix and returns possible completions.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class SequenceAutoCompleter<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly TLink _any;

        /// <summary>
        /// Initializes a new instance of SequenceAutoCompleter.
        /// </summary>
        /// <param name="links">The links storage to search sequences in.</param>
        public SequenceAutoCompleter(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _any = _links.Constants.Any;
        }

        /// <summary>
        /// Finds all sequences that start with the given prefix sequence.
        /// </summary>
        /// <param name="prefix">The prefix sequence to match.</param>
        /// <returns>List of link indices representing sequences that start with the prefix.</returns>
        public List<TLink> FindCompletions(IList<TLink> prefix)
        {
            if (prefix == null || prefix.Count == 0)
            {
                return new List<TLink>();
            }

            var results = new List<TLink>();
            var prefixLength = prefix.Count;

            // Search for all links and check if they represent sequences starting with the prefix
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];

                // Extract sequence from this link
                var sequence = ExtractSequence(linkIndex);

                // Check if this sequence starts with the prefix
                if (sequence.Count >= prefixLength && SequenceStartsWith(sequence, prefix))
                {
                    results.Add(linkIndex);
                }

                return _links.Constants.Continue;
            }, _any, _any);

            return results;
        }

        /// <summary>
        /// Finds all possible next elements after a given prefix sequence.
        /// </summary>
        /// <param name="prefix">The prefix sequence.</param>
        /// <returns>List of unique next elements that can follow the prefix.</returns>
        public List<TLink> FindNextElements(IList<TLink> prefix)
        {
            if (prefix == null || prefix.Count == 0)
            {
                return new List<TLink>();
            }

            var nextElements = new HashSet<TLink>();
            var prefixLength = prefix.Count;

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                var sequence = ExtractSequence(linkIndex);

                // If sequence starts with prefix and has at least one more element
                if (sequence.Count > prefixLength && SequenceStartsWith(sequence, prefix))
                {
                    // Add the next element after the prefix
                    nextElements.Add(sequence[prefixLength]);
                }

                return _links.Constants.Continue;
            }, _any, _any);

            return nextElements.ToList();
        }

        /// <summary>
        /// Extracts a flat sequence from a link that represents a tree structure.
        /// Traverses the doublets structure to collect all leaf elements.
        /// </summary>
        /// <param name="linkIndex">The link index to extract sequence from.</param>
        /// <returns>List of elements in the sequence.</returns>
        private List<TLink> ExtractSequence(TLink linkIndex)
        {
            var result = new List<TLink>();
            var visited = new HashSet<TLink>();
            ExtractSequenceRecursive(linkIndex, result, visited);
            return result;
        }

        /// <summary>
        /// Recursively extracts sequence elements from a doublet structure.
        /// Follows the doublet tree structure (source and target) to collect all elements.
        /// </summary>
        private void ExtractSequenceRecursive(TLink linkIndex, List<TLink> result, HashSet<TLink> visited)
        {
            // Prevent infinite loops in case of cyclic structures
            if (visited.Contains(linkIndex))
            {
                return;
            }
            visited.Add(linkIndex);

            var link = _links.GetLink(linkIndex);
            if (link == null)
            {
                return;
            }

            var source = link[_links.Constants.SourcePart];
            var target = link[_links.Constants.TargetPart];

            // If this is a point (self-reference), it's a leaf element
            if (Equals(source, linkIndex) && Equals(target, linkIndex))
            {
                result.Add(linkIndex);
                return;
            }

            // If source or target are different from the link itself, traverse them
            if (!Equals(source, linkIndex))
            {
                var sourceLink = _links.GetLink(source);
                if (sourceLink != null)
                {
                    var sourceSource = sourceLink[_links.Constants.SourcePart];
                    var sourceTarget = sourceLink[_links.Constants.TargetPart];

                    // If source is a point, add it directly
                    if (Equals(sourceSource, source) && Equals(sourceTarget, source))
                    {
                        result.Add(source);
                    }
                    else
                    {
                        // Otherwise recurse into source
                        ExtractSequenceRecursive(source, result, visited);
                    }
                }
            }

            if (!Equals(target, linkIndex))
            {
                var targetLink = _links.GetLink(target);
                if (targetLink != null)
                {
                    var targetSource = targetLink[_links.Constants.SourcePart];
                    var targetTarget = targetLink[_links.Constants.TargetPart];

                    // If target is a point, add it directly
                    if (Equals(targetSource, target) && Equals(targetTarget, target))
                    {
                        result.Add(target);
                    }
                    else
                    {
                        // Otherwise recurse into target
                        ExtractSequenceRecursive(target, result, visited);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a sequence starts with the given prefix.
        /// </summary>
        private bool SequenceStartsWith(List<TLink> sequence, IList<TLink> prefix)
        {
            if (sequence.Count < prefix.Count)
            {
                return false;
            }

            for (int i = 0; i < prefix.Count; i++)
            {
                if (!Equals(sequence[i], prefix[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
