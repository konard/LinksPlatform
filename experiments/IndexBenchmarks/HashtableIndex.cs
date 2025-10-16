using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexBenchmarks
{
    /// <summary>
    /// Hashtable-based index implementation
    /// Uses Dictionary for O(1) average-case lookups
    /// </summary>
    public class HashtableIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>
    {
        private struct LinkKey : IEquatable<LinkKey>
        {
            public TLink Source;
            public TLink Target;

            public LinkKey(TLink source, TLink target)
            {
                Source = source;
                Target = target;
            }

            public bool Equals(LinkKey other) => Source.Equals(other.Source) && Target.Equals(other.Target);
            public override bool Equals(object obj) => obj is LinkKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Source, Target);
        }

        private readonly Dictionary<LinkKey, TLink> _mainIndex;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink target)>> _sourceIndex;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink source)>> _targetIndex;

        public HashtableIndex(int capacity = 1000)
        {
            _mainIndex = new Dictionary<LinkKey, TLink>(capacity);
            _sourceIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
            _targetIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            var key = new LinkKey(source, target);
            _mainIndex[key] = linkAddress;

            if (!_sourceIndex.TryGetValue(source, out var sourceList))
            {
                sourceList = new List<(TLink, TLink)>();
                _sourceIndex[source] = sourceList;
            }
            sourceList.Add((linkAddress, target));

            if (!_targetIndex.TryGetValue(target, out var targetList))
            {
                targetList = new List<(TLink, TLink)>();
                _targetIndex[target] = targetList;
            }
            targetList.Add((linkAddress, source));
        }

        public bool Remove(TLink linkAddress, TLink source, TLink target)
        {
            var key = new LinkKey(source, target);
            if (!_mainIndex.Remove(key))
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
            return _mainIndex.TryGetValue(key, out var linkAddress) ? linkAddress : default;
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

        public int Count => _mainIndex.Count;

        public void Clear()
        {
            _mainIndex.Clear();
            _sourceIndex.Clear();
            _targetIndex.Clear();
        }
    }
}
