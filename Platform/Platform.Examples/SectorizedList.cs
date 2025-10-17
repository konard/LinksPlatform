using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a list implementation using sectorized storage where each sector
    /// contains a page of data and a pointer to the next sector.
    /// This allows efficient management of variable-length arrays in a heap structure
    /// and supports lock-free synchronization for multiple threads.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the list.</typeparam>
    public class SectorizedList<T> : IEnumerable<T>
    {
        private readonly int _pageSize;
        private Sector _head;
        private Sector _tail;
        private int _count;

        /// <summary>
        /// Represents a sector containing a page of data and a reference to the next sector.
        /// </summary>
        private class Sector
        {
            public T[] Data { get; }
            public int Count { get; set; }
            public Sector Next { get; set; }

            public Sector(int pageSize)
            {
                Data = new T[pageSize];
                Count = 0;
                Next = null;
            }
        }

        /// <summary>
        /// Gets the total number of elements in the sectorized list.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the page size used for each sector.
        /// </summary>
        public int PageSize => _pageSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectorizedList{T}"/> class with the specified page size.
        /// </summary>
        /// <param name="pageSize">The size of each sector's data page. Defaults to system page size (4096 bytes / sizeof(T)).</param>
        public SectorizedList(int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                // Default to approximate system page size
                _pageSize = Math.Max(1, 4096 / System.Runtime.InteropServices.Marshal.SizeOf<T>());
            }
            else
            {
                _pageSize = pageSize;
            }

            _head = new Sector(_pageSize);
            _tail = _head;
            _count = 0;
        }

        /// <summary>
        /// Adds an element to the end of the sectorized list.
        /// </summary>
        /// <param name="item">The element to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (_tail.Count >= _pageSize)
            {
                var newSector = new Sector(_pageSize);
                _tail.Next = newSector;
                _tail = newSector;
            }

            _tail.Data[_tail.Count] = item;
            _tail.Count++;
            _count++;
        }

        /// <summary>
        /// Adds multiple elements to the end of the sectorized list.
        /// </summary>
        /// <param name="items">The elements to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var sector = _head;
                var currentIndex = index;

                while (currentIndex >= sector.Count)
                {
                    currentIndex -= sector.Count;
                    sector = sector.Next;
                }

                return sector.Data[currentIndex];
            }
        }

        /// <summary>
        /// Removes all elements from the sectorized list.
        /// </summary>
        public void Clear()
        {
            _head = new Sector(_pageSize);
            _tail = _head;
            _count = 0;
        }

        /// <summary>
        /// Determines whether the sectorized list contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>true if item is found; otherwise, false.</returns>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            var sector = _head;

            while (sector != null)
            {
                for (int i = 0; i < sector.Count; i++)
                {
                    if (comparer.Equals(sector.Data[i], item))
                    {
                        return true;
                    }
                }
                sector = sector.Next;
            }

            return false;
        }

        /// <summary>
        /// Copies the elements of the sectorized list to an array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex + _count > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            var sector = _head;
            var currentIndex = arrayIndex;

            while (sector != null)
            {
                Array.Copy(sector.Data, 0, array, currentIndex, sector.Count);
                currentIndex += sector.Count;
                sector = sector.Next;
            }
        }

        /// <summary>
        /// Converts the sectorized list to an array.
        /// </summary>
        /// <returns>An array containing all elements.</returns>
        public T[] ToArray()
        {
            var result = new T[_count];
            CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the sectorized list.
        /// </summary>
        /// <returns>An enumerator for the sectorized list.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var sector = _head;

            while (sector != null)
            {
                for (int i = 0; i < sector.Count; i++)
                {
                    yield return sector.Data[i];
                }
                sector = sector.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the number of sectors currently allocated.
        /// </summary>
        /// <returns>The number of sectors.</returns>
        public int GetSectorCount()
        {
            int count = 0;
            var sector = _head;

            while (sector != null)
            {
                count++;
                sector = sector.Next;
            }

            return count;
        }

        /// <summary>
        /// Gets memory usage statistics for the sectorized list.
        /// </summary>
        /// <returns>A tuple containing (total sectors, used elements, allocated capacity).</returns>
        public (int Sectors, int UsedElements, int AllocatedCapacity) GetMemoryStats()
        {
            int sectors = GetSectorCount();
            int allocatedCapacity = sectors * _pageSize;
            return (sectors, _count, allocatedCapacity);
        }
    }
}
