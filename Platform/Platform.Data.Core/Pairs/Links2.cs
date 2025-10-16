using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.Core.Pairs
{
    /// <summary>
    /// Represents a garbage-collected implementation of a pairs (doublets) Links storage.
    /// This implementation uses .NET's Garbage Collector for memory management,
    /// storing links as managed objects in memory.
    /// </summary>
    /// <typeparam name="TLink">The type used to represent link addresses/identifiers (must be numeric).</typeparam>
    public class Links2<TLink> : ILinksMemoryManager<TLink>
        where TLink : struct, IEquatable<TLink>, IComparable<TLink>
    {
        private readonly List<LinkNode> _storage;
        private readonly Stack<TLink> _freeList;
        private Func<long, TLink> _longToLink;
        private Func<TLink, long> _linkToLong;
        private TLink _nullLink;
        private bool _disposed;

        /// <summary>
        /// Represents a single link node with source and target.
        /// </summary>
        private class LinkNode
        {
            public TLink Source { get; set; }
            public TLink Target { get; set; }
            public bool IsAllocated { get; set; }

            public LinkNode(TLink nullLink)
            {
                Source = nullLink;
                Target = nullLink;
                IsAllocated = false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Links2 class.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the links storage.</param>
        public Links2(long initialCapacity = 1024)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Initial capacity must be non-negative.");

            // Setup converters based on TLink type
            SetupConverters();

            _nullLink = _longToLink(0);
            _storage = new List<LinkNode>((int)initialCapacity);
            _freeList = new Stack<TLink>();
            _disposed = false;

            // Add a null/reserved link at index 0
            _storage.Add(new LinkNode(_nullLink));
        }

        /// <inheritdoc/>
        public TLink Capacity => _longToLink(_storage.Capacity);

        /// <inheritdoc/>
        public TLink Allocate()
        {
            ThrowIfDisposed();

            // Try to reuse a freed link
            if (_freeList.Count > 0)
            {
                var link = _freeList.Pop();
                var index = _linkToLong(link);
                _storage[(int)index].IsAllocated = true;
                return link;
            }

            // Allocate a new link
            var newIndex = _storage.Count;
            var newNode = new LinkNode(_nullLink) { IsAllocated = true };
            _storage.Add(newNode);
            return _longToLink(newIndex);
        }

        /// <inheritdoc/>
        public void Free(TLink link)
        {
            ThrowIfDisposed();

            var index = _linkToLong(link);
            if (index < 0 || index >= _storage.Count)
                throw new ArgumentOutOfRangeException(nameof(link), "Link identifier is out of range.");

            var node = _storage[(int)index];
            if (!node.IsAllocated)
                throw new InvalidOperationException($"Link {link} is not allocated.");

            // Clear the link data
            node.Source = _nullLink;
            node.Target = _nullLink;
            node.IsAllocated = false;

            // Add to free list for reuse
            _freeList.Push(link);
        }

        /// <inheritdoc/>
        public TLink GetSource(TLink link)
        {
            ThrowIfDisposed();
            ValidateLink(link);
            return _storage[(int)_linkToLong(link)].Source;
        }

        /// <inheritdoc/>
        public TLink GetTarget(TLink link)
        {
            ThrowIfDisposed();
            ValidateLink(link);
            return _storage[(int)_linkToLong(link)].Target;
        }

        /// <inheritdoc/>
        public void SetSource(TLink link, TLink source)
        {
            ThrowIfDisposed();
            ValidateLink(link);
            _storage[(int)_linkToLong(link)].Source = source;
        }

        /// <inheritdoc/>
        public void SetTarget(TLink link, TLink target)
        {
            ThrowIfDisposed();
            ValidateLink(link);
            _storage[(int)_linkToLong(link)].Target = target;
        }

        /// <inheritdoc/>
        public bool IsAllocated(TLink link)
        {
            ThrowIfDisposed();

            var index = _linkToLong(link);
            if (index < 0 || index >= _storage.Count)
                return false;

            return _storage[(int)index].IsAllocated;
        }

        /// <inheritdoc/>
        public void EnsureCapacity(TLink capacity)
        {
            ThrowIfDisposed();

            var requiredCapacity = (int)_linkToLong(capacity);
            if (_storage.Capacity < requiredCapacity)
            {
                _storage.Capacity = requiredCapacity;
            }
        }

        /// <summary>
        /// Gets the count of currently allocated links (excluding the null link at index 0).
        /// </summary>
        public long Count => _storage.Skip(1).Count(node => node.IsAllocated);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _storage.Clear();
                _freeList.Clear();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Links2<TLink>));
        }

        private void ValidateLink(TLink link)
        {
            var index = _linkToLong(link);
            if (index <= 0 || index >= _storage.Count)
                throw new ArgumentOutOfRangeException(nameof(link), "Link identifier is out of range.");

            if (!_storage[(int)index].IsAllocated)
                throw new InvalidOperationException($"Link {link} is not allocated.");
        }

        private void SetupConverters()
        {
            var type = typeof(TLink);

            if (type == typeof(uint))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, uint>)(x => (uint)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<uint, long>)(x => (long)x);
            }
            else if (type == typeof(ulong))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, ulong>)(x => (ulong)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<ulong, long>)(x => (long)x);
            }
            else if (type == typeof(int))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, int>)(x => (int)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<int, long>)(x => (long)x);
            }
            else if (type == typeof(long))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, long>)(x => x);
                _linkToLong = (Func<TLink, long>)(object)(Func<long, long>)(x => x);
            }
            else if (type == typeof(ushort))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, ushort>)(x => (ushort)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<ushort, long>)(x => (long)x);
            }
            else if (type == typeof(short))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, short>)(x => (short)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<short, long>)(x => (long)x);
            }
            else if (type == typeof(byte))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, byte>)(x => (byte)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<byte, long>)(x => (long)x);
            }
            else if (type == typeof(sbyte))
            {
                _longToLink = (Func<long, TLink>)(object)(Func<long, sbyte>)(x => (sbyte)x);
                _linkToLong = (Func<TLink, long>)(object)(Func<sbyte, long>)(x => (long)x);
            }
            else
            {
                throw new NotSupportedException($"Type {type.Name} is not supported as TLink.");
            }
        }
    }
}
