using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Platform.Memory.Experiments
{
    /// <summary>
    /// <para>
    /// Represents a pinned array memory implementation that uses only managed memory.
    /// This allows Links to run in restricted .NET environments where unmanaged memory allocation is not allowed.
    /// </para>
    /// <para>
    /// Представляет реализацию закреплённой памяти массива, которая использует только управляемую память.
    /// Это позволяет Links работать в ограниченных средах .NET, где выделение неуправляемой памяти не разрешено.
    /// </para>
    /// </summary>
    /// <typeparam name="TElement">
    /// <para>The type of elements stored in the array.</para>
    /// <para>Тип элементов, хранящихся в массиве.</para>
    /// </typeparam>
    public class PinnedArrayMemory<TElement> : IDirectMemory, IArrayMemory<TElement>
    {
        private readonly TElement[] _array;
        private GCHandle _handle;
        private bool _disposed;

        /// <summary>
        /// <para>Gets the size of the memory block in bytes.</para>
        /// <para>Получает размер блока памяти в байтах.</para>
        /// </summary>
        public long Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array.Length * Marshal.SizeOf<TElement>();
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
        /// Initializes a new instance of the <see cref="PinnedArrayMemory{TElement}"/> class with the specified size.
        /// The array is pinned in memory to provide a stable pointer for the lifetime of this object.
        /// </para>
        /// <para>
        /// Инициализирует новый экземпляр класса <see cref="PinnedArrayMemory{TElement}"/> с указанным размером.
        /// Массив закрепляется в памяти, чтобы обеспечить стабильный указатель на время жизни этого объекта.
        /// </para>
        /// </summary>
        /// <param name="size">
        /// <para>The number of elements in the array.</para>
        /// <para>Количество элементов в массиве.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PinnedArrayMemory(long size)
        {
            _array = new TElement[size];
            _handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
            _disposed = false;
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
                throw new ObjectDisposedException(nameof(PinnedArrayMemory<TElement>));
            }
        }
    }
}
