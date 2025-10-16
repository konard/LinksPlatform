using System;
using System.Collections.Generic;

namespace IndexBenchmarks
{
    /// <summary>
    /// Skip List-based index implementation
    /// Probabilistic data structure with O(log n) average-case operations
    /// </summary>
    public class SkipListIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>, IComparable<TLink>
    {
        private class SkipNode
        {
            public TLink Source;
            public TLink Target;
            public TLink LinkAddress;
            public SkipNode[] Forward;

            public SkipNode(int level, TLink source, TLink target, TLink linkAddress)
            {
                Source = source;
                Target = target;
                LinkAddress = linkAddress;
                Forward = new SkipNode[level + 1];
            }
        }

        private const int MaxLevel = 16;
        private const double Probability = 0.5;
        private readonly SkipNode _header;
        private int _level;
        private int _count;
        private readonly Random _random;

        public SkipListIndex()
        {
            _header = new SkipNode(MaxLevel, default, default, default);
            _level = 0;
            _count = 0;
            _random = new Random();
        }

        private int RandomLevel()
        {
            int level = 0;
            while (_random.NextDouble() < Probability && level < MaxLevel)
                level++;
            return level;
        }

        private int CompareLinks(TLink s1, TLink t1, TLink s2, TLink t2)
        {
            int sourceCmp = s1.CompareTo(s2);
            if (sourceCmp != 0) return sourceCmp;
            return t1.CompareTo(t2);
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            var update = new SkipNode[MaxLevel + 1];
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null &&
                       CompareLinks(current.Forward[i].Source, current.Forward[i].Target, source, target) < 0)
                {
                    current = current.Forward[i];
                }
                update[i] = current;
            }

            current = current.Forward[0];

            if (current != null && CompareLinks(current.Source, current.Target, source, target) == 0)
            {
                current.LinkAddress = linkAddress;
            }
            else
            {
                int newLevel = RandomLevel();

                if (newLevel > _level)
                {
                    for (int i = _level + 1; i <= newLevel; i++)
                        update[i] = _header;
                    _level = newLevel;
                }

                var newNode = new SkipNode(newLevel, source, target, linkAddress);

                for (int i = 0; i <= newLevel; i++)
                {
                    newNode.Forward[i] = update[i].Forward[i];
                    update[i].Forward[i] = newNode;
                }

                _count++;
            }
        }

        public TLink Search(TLink source, TLink target)
        {
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null &&
                       CompareLinks(current.Forward[i].Source, current.Forward[i].Target, source, target) < 0)
                {
                    current = current.Forward[i];
                }
            }

            current = current.Forward[0];

            if (current != null && CompareLinks(current.Source, current.Target, source, target) == 0)
                return current.LinkAddress;

            return default;
        }

        public bool Remove(TLink linkAddress, TLink source, TLink target)
        {
            var update = new SkipNode[MaxLevel + 1];
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null &&
                       CompareLinks(current.Forward[i].Source, current.Forward[i].Target, source, target) < 0)
                {
                    current = current.Forward[i];
                }
                update[i] = current;
            }

            current = current.Forward[0];

            if (current != null && CompareLinks(current.Source, current.Target, source, target) == 0)
            {
                for (int i = 0; i <= _level; i++)
                {
                    if (update[i].Forward[i] != current)
                        break;
                    update[i].Forward[i] = current.Forward[i];
                }

                while (_level > 0 && _header.Forward[_level] == null)
                    _level--;

                _count--;
                return true;
            }

            return false;
        }

        public IEnumerable<(TLink linkAddress, TLink target)> GetBySource(TLink source)
        {
            var result = new List<(TLink, TLink)>();
            var current = _header.Forward[0];

            while (current != null)
            {
                if (current.Source.Equals(source))
                    result.Add((current.LinkAddress, current.Target));
                else if (current.Source.CompareTo(source) > 0)
                    break;
                current = current.Forward[0];
            }

            return result;
        }

        public IEnumerable<(TLink linkAddress, TLink source)> GetByTarget(TLink target)
        {
            var result = new List<(TLink, TLink)>();
            var current = _header.Forward[0];

            while (current != null)
            {
                if (current.Target.Equals(target))
                    result.Add((current.LinkAddress, current.Source));
                current = current.Forward[0];
            }

            return result;
        }

        public int Count => _count;

        public void Clear()
        {
            for (int i = 0; i <= MaxLevel; i++)
                _header.Forward[i] = null;
            _level = 0;
            _count = 0;
        }
    }
}
