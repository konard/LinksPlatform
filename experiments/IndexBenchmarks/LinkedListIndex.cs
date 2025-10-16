using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexBenchmarks
{
    /// <summary>
    /// Linked-list based index implementation
    /// Simple linear search implementation for comparison
    /// </summary>
    public class LinkedListIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>
    {
        private class LinkNode
        {
            public TLink LinkAddress;
            public TLink Source;
            public TLink Target;
        }

        private readonly List<LinkNode> _links;

        public LinkedListIndex(int capacity = 1000)
        {
            _links = new List<LinkNode>(capacity);
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            _links.Add(new LinkNode
            {
                LinkAddress = linkAddress,
                Source = source,
                Target = target
            });
        }

        public bool Remove(TLink linkAddress, TLink source, TLink target)
        {
            for (int i = 0; i < _links.Count; i++)
            {
                var node = _links[i];
                if (node.LinkAddress.Equals(linkAddress) &&
                    node.Source.Equals(source) &&
                    node.Target.Equals(target))
                {
                    _links.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public TLink Search(TLink source, TLink target)
        {
            foreach (var node in _links)
            {
                if (node.Source.Equals(source) && node.Target.Equals(target))
                    return node.LinkAddress;
            }
            return default;
        }

        public IEnumerable<(TLink linkAddress, TLink target)> GetBySource(TLink source)
        {
            return _links
                .Where(n => n.Source.Equals(source))
                .Select(n => (n.LinkAddress, n.Target));
        }

        public IEnumerable<(TLink linkAddress, TLink source)> GetByTarget(TLink target)
        {
            return _links
                .Where(n => n.Target.Equals(target))
                .Select(n => (n.LinkAddress, n.Source));
        }

        public int Count => _links.Count;

        public void Clear()
        {
            _links.Clear();
        }
    }
}
