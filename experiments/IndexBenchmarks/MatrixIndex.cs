using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexBenchmarks
{
    /// <summary>
    /// Matrix-based index implementation with sparse storage optimization
    /// Uses a 2D conceptual matrix but stores only non-empty cells
    /// </summary>
    public class MatrixIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>
    {
        // Maps (source, target) to link address using a dictionary as sparse matrix
        private readonly Dictionary<(TLink source, TLink target), TLink> _matrix;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink target)>> _sourceIndex;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink source)>> _targetIndex;

        public MatrixIndex(int capacity = 1000)
        {
            _matrix = new Dictionary<(TLink, TLink), TLink>(capacity);
            _sourceIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
            _targetIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            var key = (source, target);
            _matrix[key] = linkAddress;

            if (!_sourceIndex.TryGetValue(source, out var sourceList))
            {
                sourceList = new List<(TLink, TLink)>();
                _sourceIndex[source] = sourceList;
            }
            // Check if already exists and update, otherwise add
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
            var key = (source, target);
            if (!_matrix.Remove(key))
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
            var key = (source, target);
            return _matrix.TryGetValue(key, out var linkAddress) ? linkAddress : default;
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

        public int Count => _matrix.Count;

        public void Clear()
        {
            _matrix.Clear();
            _sourceIndex.Clear();
            _targetIndex.Clear();
        }
    }
}
