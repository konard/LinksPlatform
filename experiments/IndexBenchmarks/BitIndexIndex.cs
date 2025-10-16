using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IndexBenchmarks
{
    /// <summary>
    /// Bit index implementation with optimization for empty spaces
    /// Uses BitArray for existence flags and sparse storage for actual data
    /// Suitable when link addresses are densely packed integers
    /// </summary>
    public class BitIndexIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>
    {
        private class LinkData
        {
            public TLink LinkAddress;
            public TLink Source;
            public TLink Target;
        }

        // Bit arrays to track existence in different dimensions
        private BitArray _existenceBits;
        private readonly Dictionary<int, LinkData> _sparseData;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink target)>> _sourceIndex;
        private readonly Dictionary<TLink, List<(TLink linkAddress, TLink source)>> _targetIndex;
        private int _capacity;
        private int _count;

        public BitIndexIndex(int initialCapacity = 1000)
        {
            _capacity = initialCapacity;
            _existenceBits = new BitArray(_capacity);
            _sparseData = new Dictionary<int, LinkData>();
            _sourceIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
            _targetIndex = new Dictionary<TLink, List<(TLink, TLink)>>();
            _count = 0;
        }

        private int GetHashIndex(TLink source, TLink target)
        {
            // Use hash code to map to bit index
            int hash = HashCode.Combine(source, target);
            return Math.Abs(hash % _capacity);
        }

        private void EnsureCapacity(int index)
        {
            if (index >= _capacity)
            {
                int newCapacity = Math.Max(_capacity * 2, index + 1);
                var newBits = new BitArray(newCapacity);
                for (int i = 0; i < _capacity; i++)
                    newBits[i] = _existenceBits[i];
                _existenceBits = newBits;
                _capacity = newCapacity;
            }
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            int index = GetHashIndex(source, target);
            EnsureCapacity(index);

            var data = new LinkData
            {
                LinkAddress = linkAddress,
                Source = source,
                Target = target
            };

            if (!_existenceBits[index])
            {
                _existenceBits[index] = true;
                _count++;
            }

            _sparseData[index] = data;

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
            int index = GetHashIndex(source, target);

            if (index >= _capacity || !_existenceBits[index])
                return false;

            if (_sparseData.TryGetValue(index, out var data) &&
                data.Source.Equals(source) && data.Target.Equals(target))
            {
                _existenceBits[index] = false;
                _sparseData.Remove(index);
                _count--;

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

            return false;
        }

        public TLink Search(TLink source, TLink target)
        {
            int index = GetHashIndex(source, target);

            if (index >= _capacity || !_existenceBits[index])
                return default;

            if (_sparseData.TryGetValue(index, out var data) &&
                data.Source.Equals(source) && data.Target.Equals(target))
            {
                return data.LinkAddress;
            }

            return default;
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

        public int Count => _count;

        public void Clear()
        {
            _existenceBits.SetAll(false);
            _sparseData.Clear();
            _sourceIndex.Clear();
            _targetIndex.Clear();
            _count = 0;
        }
    }
}
