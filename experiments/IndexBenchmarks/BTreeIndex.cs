using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexBenchmarks
{
    /// <summary>
    /// Simplified B-Tree based index implementation
    /// For simplicity, uses SortedDictionary which is internally a Red-Black tree (similar to B-Tree properties)
    /// A full B-Tree implementation would be significantly more complex
    /// </summary>
    public class BTreeIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>, IComparable<TLink>
    {
        private struct LinkKey : IComparable<LinkKey>, IEquatable<LinkKey>
        {
            public TLink Source;
            public TLink Target;

            public LinkKey(TLink source, TLink target)
            {
                Source = source;
                Target = target;
            }

            public int CompareTo(LinkKey other)
            {
                int sourceCmp = Source.CompareTo(other.Source);
                if (sourceCmp != 0) return sourceCmp;
                return Target.CompareTo(other.Target);
            }

            public bool Equals(LinkKey other) => Source.Equals(other.Source) && Target.Equals(other.Target);
            public override bool Equals(object obj) => obj is LinkKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Source, Target);
        }

        // Using SortedDictionary which provides B-Tree-like balanced tree behavior
        private readonly SortedDictionary<LinkKey, TLink> _tree;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink target)>> _sourceIndex;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink source)>> _targetIndex;

        public BTreeIndex()
        {
            _tree = new SortedDictionary<LinkKey, TLink>();
            _sourceIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
            _targetIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            var key = new LinkKey(source, target);
            _tree[key] = linkAddress;

            if (!_sourceIndex.TryGetValue(source, out var sourceList))
            {
                sourceList = new List<(TLink, TLink)>();
                _sourceIndex[source] = sourceList;
            }
            int existingIndex = sourceList.FindIndex(x => x.target.Equals(target));
            if (existingIndex >= 0)
                sourceList[existingIndex] = (linkAddress, target);
            else
                sourceList.Add((linkAddress, target));

            if (!_targetIndex.TryGetValue(target, out var targetList))
            {
                targetList = new List<(TLink, TLink)>();
                _targetIndex[target] = targetList;
            }
            existingIndex = targetList.FindIndex(x => x.source.Equals(source));
            if (existingIndex >= 0)
                targetList[existingIndex] = (linkAddress, source);
            else
                targetList.Add((linkAddress, source));
        }

        public bool Remove(TLink linkAddress, TLink source, TLink target)
        {
            var key = new LinkKey(source, target);
            if (!_tree.Remove(key))
                return false;

            if (_sourceIndex.TryGetValue(source, out var sourceList))
            {
                sourceList.RemoveAll(x => x.linkAddress.Equals(linkAddress));
                if (sourceList.Count == 0)
                    _sourceIndex.Remove(source);
            }

            if (_targetIndex.TryGetValue(target, out var targetList))
            {
                targetList.RemoveAll(x => x.linkAddress.Equals(linkAddress));
                if (targetList.Count == 0)
                    _targetIndex.Remove(target);
            }

            return true;
        }

        public TLink Search(TLink source, TLink target)
        {
            var key = new LinkKey(source, target);
            return _tree.TryGetValue(key, out var linkAddress) ? linkAddress : default;
        }

        public IEnumerable<(TLink linkAddress, TLink target)> GetBySource(TLink source)
        {
            if (_sourceIndex.TryGetValue(source, out var list))
                return list;
            return Enumerable.Empty<(TLink, TLink)>();
        }

        public IEnumerable<(TLink linkAddress, TLink source)> GetByTarget(TLink target)
        {
            if (_targetIndex.TryGetValue(target, out var list))
                return list;
            return Enumerable.Empty<(TLink, TLink)>();
        }

        public int Count => _tree.Count;

        public void Clear()
        {
            _tree.Clear();
            _sourceIndex.Clear();
            _targetIndex.Clear();
        }
    }
}
