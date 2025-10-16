using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Platform.Memory.Experiments
{
    /// <summary>
    /// <para>
    /// Represents a resizable pinned array memory implementation that uses only managed memory.
    /// This implementation supports resizing by allocating a new larger array and copying data,
    /// then re-pinning the new array. While not as efficient as unmanaged reallocation,
    /// it allows Links to run in restricted .NET environments.
    /// </para>
    /// <para>
    /// Представляет реализацию изменяемой закреплённой памяти массива, которая использует только управляемую память.
    /// Эта реализация поддерживает изменение размера путём выделения нового большего массива и копирования данных,
    /// затем повторного закрепления нового массива. Хотя это не так эффективно, как перераспределение неуправляемой памяти,
    /// это позволяет Links работать в ограниченных средах .NET.
    /// </para>
    /// </summary>
    /// <typeparam name="TElement">
    /// <para>The type of elements stored in the array.</para>
    /// <para>Тип элементов, хранящихся в массиве.</para>
    /// </typeparam>
    public class PinnedResizableArrayMemory<TElement> : IResizableDirectMemory, IArrayMemory<TElement>
    {
        private TElement[] _array;
        private GCHandle _handle;
        private bool _disposed;
        private long _usedCapacity;
        private long _reservedCapacity;

        /// <summary>
        /// <para>Gets or sets the reserved capacity in bytes.</para>
        /// <para>Получает или устанавливает зарезервированную ёмкость в байтах.</para>
        /// </summary>
        public long ReservedCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNotDisposed();
                return _reservedCapacity;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                EnsureNotDisposed();
                if (value != _reservedCapacity)
                {
                    Resize(value);
                }
            }
        }

        /// <summary>
        /// <para>Gets or sets the used capacity in bytes.</para>
        /// <para>Получает или устанавливает используемую ёмкость в байтах.</para>
        /// </summary>
        public long UsedCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNotDisposed();
                return _usedCapacity;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                EnsureNotDisposed();
                if (value > _reservedCapacity)
                {
                    ReservedCapacity = value;
                }
                _usedCapacity = value;
            }
        }

        /// <summary>
        /// <para>Gets the size of the memory block in bytes (equivalent to UsedCapacity).</para>
        /// <para>Получает размер блока памяти в байтах (эквивалентно UsedCapacity).</para>
        /// </summary>
        public long Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UsedCapacity;
        }

        /// <summary>
        /// <para>Gets the pointer to the beginning of the pinned array.</para>
        /// <para>Получает указатель на начало закреплённого массива.</para>
        /// </summary>
        public IntPtr Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNotDisposed();
                return _handle.AddrOfPinnedObject();
            }
        }

        /// <summary>
        /// <para>Gets or sets the element at the specified index.</para>
        /// <para>Получает или устанавливает элемент по указанному индексу.</para>
        /// </summary>
        /// <param name="index">
        /// <para>The index of the element.</para>
        /// <para>Индекс элемента.</para>
        /// </param>
        public TElement this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNotDisposed();
                return _array[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                EnsureNotDisposed();
                _array[index] = value;
            }
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="PinnedResizableArrayMemory{TElement}"/> class with the specified initial capacity.
        /// </para>
        /// <para>
        /// Инициализирует новый экземпляр класса <see cref="PinnedResizableArrayMemory{TElement}"/> с указанной начальной ёмкостью.
        /// </para>
        /// </summary>
        /// <param name="minimumReservedCapacity">
        /// <para>The minimum initial reserved capacity in bytes.</para>
        /// <para>Минимальная начальная зарезервированная ёмкость в байтах.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PinnedResizableArrayMemory(long minimumReservedCapacity = 0)
        {
            var elementSize = Marshal.SizeOf<TElement>();
            var elementCount = Math.Max(1, (minimumReservedCapacity + elementSize - 1) / elementSize);

            _array = new TElement[elementCount];
            _handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
            _reservedCapacity = elementCount * elementSize;
            _usedCapacity = 0;
            _disposed = false;
        }

        /// <summary>
        /// <para>Resizes the internal array to the new capacity.</para>
        /// <para>Изменяет размер внутреннего массива до новой ёмкости.</para>
        /// </summary>
        /// <param name="newCapacityInBytes">
        /// <para>The new capacity in bytes.</para>
        /// <para>Новая ёмкость в байтах.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(long newCapacityInBytes)
        {
            var elementSize = Marshal.SizeOf<TElement>();
            var newElementCount = (newCapacityInBytes + elementSize - 1) / elementSize;

            // Create new array
            var newArray = new TElement[newElementCount];

            // Copy existing data
            var elementsToCopy = Math.Min(_array.Length, newArray.Length);
            Array.Copy(_array, newArray, elementsToCopy);

            // Free old handle
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }

            // Pin new array
            _array = newArray;
            _handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
            _reservedCapacity = newElementCount * elementSize;

            // Adjust used capacity if necessary
            if (_usedCapacity > _reservedCapacity)
            {
                _usedCapacity = _reservedCapacity;
            }
        }

        /// <summary>
        /// <para>Releases the pinned array handle.</para>
        /// <para>Освобождает дескриптор закреплённого массива.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                }
                _disposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PinnedResizableArrayMemory<TElement>));
            }
        }
    }
}
