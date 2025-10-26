using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a chain of links stored in associative storage.
    /// This implementation provides blockchain-like functionality where each link
    /// in the chain references the previous link, creating an immutable sequence.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class LinksChain<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;
        private readonly ILinks<TLink> _links;
        private readonly TLink _prevMarker;
        private readonly TLink _nextMarker;
        private readonly TLink _chainMarker;

        /// <summary>
        /// Gets the link that marks a "Previous" relationship in the chain.
        /// </summary>
        public TLink PrevMarker => _prevMarker;

        /// <summary>
        /// Gets the link that marks a "Next" relationship in the chain.
        /// </summary>
        public TLink NextMarker => _nextMarker;

        /// <summary>
        /// Gets the link that marks a chain structure.
        /// </summary>
        public TLink ChainMarker => _chainMarker;

        /// <summary>
        /// Initializes a new instance of the LinksChain class.
        /// </summary>
        /// <param name="links">The links storage to use.</param>
        /// <param name="prevMarker">Optional marker for "Previous" relationships. If not provided, will be created.</param>
        /// <param name="nextMarker">Optional marker for "Next" relationships. If not provided, will be created.</param>
        /// <param name="chainMarker">Optional marker for chain structures. If not provided, will be created.</param>
        public LinksChain(ILinks<TLink> links, TLink prevMarker = default, TLink nextMarker = default, TLink chainMarker = default)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));

            // Initialize or use provided markers
            if (_equalityComparer.Equals(prevMarker, default))
            {
                _prevMarker = _links.Create();
            }
            else
            {
                _prevMarker = prevMarker;
            }

            if (_equalityComparer.Equals(nextMarker, default))
            {
                _nextMarker = _links.Create();
            }
            else
            {
                _nextMarker = nextMarker;
            }

            if (_equalityComparer.Equals(chainMarker, default))
            {
                _chainMarker = _links.Create();
            }
            else
            {
                _chainMarker = chainMarker;
            }
        }

        /// <summary>
        /// Creates a new chain with a single element.
        /// </summary>
        /// <param name="data">The data link to store in the chain.</param>
        /// <returns>The link representing the chain element.</returns>
        public TLink CreateChain(TLink data)
        {
            // Create a chain element that references the data
            // ChainElement: ChainMarker -> Data
            var chainElement = _links.Update(_links.Create(), _chainMarker, data);
            return chainElement;
        }

        /// <summary>
        /// Appends a new element to the end of the chain.
        /// </summary>
        /// <param name="chainHead">The current head of the chain.</param>
        /// <param name="data">The data to append.</param>
        /// <returns>The new chain element.</returns>
        public TLink Append(TLink chainHead, TLink data)
        {
            if (_equalityComparer.Equals(chainHead, default))
            {
                throw new ArgumentException("Chain head cannot be default value", nameof(chainHead));
            }

            // Find the last element in the chain
            var lastElement = GetLastElement(chainHead);

            // Create new chain element
            var newElement = _links.Update(_links.Create(), _chainMarker, data);

            // Create the "next" relationship: NextMarker -> (lastElement -> newElement)
            var nextLink = _links.Update(_links.Create(), lastElement, newElement);
            _links.Update(_links.Create(), _nextMarker, nextLink);

            // Create the "prev" relationship: PrevMarker -> (newElement -> lastElement)
            var prevLink = _links.Update(_links.Create(), newElement, lastElement);
            _links.Update(_links.Create(), _prevMarker, prevLink);

            return newElement;
        }

        /// <summary>
        /// Gets the previous element in the chain.
        /// </summary>
        /// <param name="element">The current chain element.</param>
        /// <returns>The previous element, or default if this is the first element.</returns>
        public TLink GetPrevious(TLink element)
        {
            // Search for (PrevMarker -> (element -> ?)) pattern
            TLink previous = default;

            _links.Each(link =>
            {
                if (link[_links.Constants.SourcePart].Equals(_prevMarker))
                {
                    var innerLinkIndex = link[_links.Constants.TargetPart];
                    _links.Each(innerLink =>
                    {
                        if (innerLink[_links.Constants.IndexPart].Equals(innerLinkIndex) &&
                            innerLink[_links.Constants.SourcePart].Equals(element))
                        {
                            previous = innerLink[_links.Constants.TargetPart];
                            return _links.Constants.Break;
                        }
                        return _links.Constants.Continue;
                    });

                    if (!_equalityComparer.Equals(previous, default))
                    {
                        return _links.Constants.Break;
                    }
                }
                return _links.Constants.Continue;
            });

            return previous;
        }

        /// <summary>
        /// Gets the next element in the chain.
        /// </summary>
        /// <param name="element">The current chain element.</param>
        /// <returns>The next element, or default if this is the last element.</returns>
        public TLink GetNext(TLink element)
        {
            // Search for (NextMarker -> (element -> ?)) pattern
            TLink next = default;

            _links.Each(link =>
            {
                if (link[_links.Constants.SourcePart].Equals(_prevMarker))
                {
                    var innerLinkIndex = link[_links.Constants.TargetPart];
                    _links.Each(innerLink =>
                    {
                        if (innerLink[_links.Constants.IndexPart].Equals(innerLinkIndex) &&
                            innerLink[_links.Constants.SourcePart].Equals(element))
                        {
                            next = innerLink[_links.Constants.TargetPart];
                            return _links.Constants.Break;
                        }
                        return _links.Constants.Continue;
                    });

                    if (!_equalityComparer.Equals(next, default))
                    {
                        return _links.Constants.Break;
                    }
                }
                return _links.Constants.Continue;
            });

            return next;
        }

        /// <summary>
        /// Gets the data stored in a chain element.
        /// </summary>
        /// <param name="element">The chain element.</param>
        /// <returns>The data link.</returns>
        public TLink GetData(TLink element)
        {
            TLink data = default;
            _links.Each(link =>
            {
                if (link[_links.Constants.IndexPart].Equals(element))
                {
                    data = link[_links.Constants.TargetPart];
                    return _links.Constants.Break;
                }
                return _links.Constants.Continue;
            });
            return data;
        }

        /// <summary>
        /// Gets the first element in the chain.
        /// </summary>
        /// <param name="anyElement">Any element in the chain.</param>
        /// <returns>The first element.</returns>
        public TLink GetFirstElement(TLink anyElement)
        {
            var current = anyElement;
            var prev = GetPrevious(current);

            while (!_equalityComparer.Equals(prev, default))
            {
                current = prev;
                prev = GetPrevious(current);
            }

            return current;
        }

        /// <summary>
        /// Gets the last element in the chain.
        /// </summary>
        /// <param name="anyElement">Any element in the chain.</param>
        /// <returns>The last element.</returns>
        public TLink GetLastElement(TLink anyElement)
        {
            var current = anyElement;
            var next = GetNext(current);

            while (!_equalityComparer.Equals(next, default))
            {
                current = next;
                next = GetNext(current);
            }

            return current;
        }

        /// <summary>
        /// Gets all elements in the chain in forward order.
        /// </summary>
        /// <param name="chainHead">The first element of the chain.</param>
        /// <returns>An enumerable of all chain elements.</returns>
        public IEnumerable<TLink> GetAllElements(TLink chainHead)
        {
            var current = GetFirstElement(chainHead);

            while (!_equalityComparer.Equals(current, default))
            {
                yield return current;
                current = GetNext(current);
            }
        }

        /// <summary>
        /// Gets the length of the chain.
        /// </summary>
        /// <param name="anyElement">Any element in the chain.</param>
        /// <returns>The number of elements in the chain.</returns>
        public int GetChainLength(TLink anyElement)
        {
            return GetAllElements(anyElement).Count();
        }

        /// <summary>
        /// Validates that the chain is well-formed (no cycles, consistent prev/next relationships).
        /// </summary>
        /// <param name="chainHead">The first element of the chain.</param>
        /// <returns>True if the chain is valid, false otherwise.</returns>
        public bool ValidateChain(TLink chainHead)
        {
            var visited = new HashSet<TLink>();
            var current = GetFirstElement(chainHead);

            while (!_equalityComparer.Equals(current, default))
            {
                // Check for cycles
                if (visited.Contains(current))
                {
                    return false;
                }

                visited.Add(current);

                // Verify bidirectional consistency
                var next = GetNext(current);
                if (!_equalityComparer.Equals(next, default))
                {
                    var prevOfNext = GetPrevious(next);
                    if (!_equalityComparer.Equals(prevOfNext, current))
                    {
                        return false; // Inconsistent prev/next relationship
                    }
                }

                current = next;
            }

            return true;
        }

        /// <summary>
        /// Creates a chain from a collection of data links.
        /// </summary>
        /// <param name="dataLinks">The data links to chain together.</param>
        /// <returns>The first element of the created chain.</returns>
        public TLink CreateChainFromSequence(IEnumerable<TLink> dataLinks)
        {
            if (dataLinks == null || !dataLinks.Any())
            {
                throw new ArgumentException("Data links cannot be null or empty", nameof(dataLinks));
            }

            var first = dataLinks.First();
            var chainHead = CreateChain(first);
            var current = chainHead;

            foreach (var data in dataLinks.Skip(1))
            {
                current = Append(current, data);
            }

            return GetFirstElement(current);
        }
    }
}
