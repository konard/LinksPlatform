using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// A simple in-memory implementation of ILinksMap using dictionaries.
    /// <para>Простая реализация ILinksMap в памяти с использованием словарей.</para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The link address type. <para>Тип адреса связи.</para></typeparam>
    /// <remarks>
    /// This implementation provides fast O(1) lookups for bidirectional mapping between
    /// local and permanent indices. It is suitable for scenarios where the mapping
    /// can fit entirely in memory.
    /// <para>
    /// Эта реализация обеспечивает быстрый O(1) поиск для двунаправленного отображения между
    /// локальными и постоянными индексами. Она подходит для сценариев, где отображение
    /// может полностью поместиться в памяти.
    /// </para>
    /// </remarks>
    public class MemoryLinksMap<TLinkAddress> : ILinksMap<TLinkAddress>
        where TLinkAddress : struct
    {
        private readonly Dictionary<TLinkAddress, TLinkAddress> _localToPermanent;
        private readonly Dictionary<TLinkAddress, TLinkAddress> _permanentToLocal;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLinksMap{TLinkAddress}"/> class.
        /// <para>Инициализирует новый экземпляр класса <see cref="MemoryLinksMap{TLinkAddress}"/>.</para>
        /// </summary>
        public MemoryLinksMap()
        {
            _localToPermanent = new Dictionary<TLinkAddress, TLinkAddress>();
            _permanentToLocal = new Dictionary<TLinkAddress, TLinkAddress>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLinksMap{TLinkAddress}"/> class with the specified capacity.
        /// <para>Инициализирует новый экземпляр класса <see cref="MemoryLinksMap{TLinkAddress}"/> с указанной емкостью.</para>
        /// </summary>
        /// <param name="capacity">The initial capacity. <para>Начальная емкость.</para></param>
        public MemoryLinksMap(int capacity)
        {
            _localToPermanent = new Dictionary<TLinkAddress, TLinkAddress>(capacity);
            _permanentToLocal = new Dictionary<TLinkAddress, TLinkAddress>(capacity);
        }

        /// <inheritdoc/>
        public bool TryGetPermanentIndex(TLinkAddress localIndex, out TLinkAddress permanentIndex)
        {
            return _localToPermanent.TryGetValue(localIndex, out permanentIndex);
        }

        /// <inheritdoc/>
        public bool TryGetLocalIndex(TLinkAddress permanentIndex, out TLinkAddress localIndex)
        {
            return _permanentToLocal.TryGetValue(permanentIndex, out localIndex);
        }

        /// <inheritdoc/>
        public void Map(TLinkAddress localIndex, TLinkAddress permanentIndex)
        {
            // Remove old mappings if they exist
            if (_localToPermanent.TryGetValue(localIndex, out var oldPermanentIndex))
            {
                _permanentToLocal.Remove(oldPermanentIndex);
            }
            if (_permanentToLocal.TryGetValue(permanentIndex, out var oldLocalIndex))
            {
                _localToPermanent.Remove(oldLocalIndex);
            }

            // Add new mappings
            _localToPermanent[localIndex] = permanentIndex;
            _permanentToLocal[permanentIndex] = localIndex;
        }

        /// <inheritdoc/>
        public bool Unmap(TLinkAddress localIndex)
        {
            if (_localToPermanent.TryGetValue(localIndex, out var permanentIndex))
            {
                _localToPermanent.Remove(localIndex);
                _permanentToLocal.Remove(permanentIndex);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool HasMapping(TLinkAddress localIndex)
        {
            return _localToPermanent.ContainsKey(localIndex);
        }

        /// <inheritdoc/>
        public long Count => _localToPermanent.Count;

        /// <summary>
        /// Clears all mappings.
        /// <para>Очищает все отображения.</para>
        /// </summary>
        public void Clear()
        {
            _localToPermanent.Clear();
            _permanentToLocal.Clear();
        }
    }
}
