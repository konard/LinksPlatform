namespace Platform.Examples
{
    /// <summary>
    /// Represents a mapping between link indices across different databases.
    /// <para>Представляет отображение между индексами связей в разных базах данных.</para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The link address type. <para>Тип адреса связи.</para></typeparam>
    /// <remarks>
    /// This interface enables:
    /// <list type="number">
    /// <item>Permanent link indices that remain stable across database operations</item>
    /// <item>Unified interpretation of data across multiple databases</item>
    /// <item>Protocol/API definition for interlinks communication</item>
    /// <item>Efficient synchronization in decentralized networks</item>
    /// </list>
    /// <para>
    /// Этот интерфейс обеспечивает:
    /// <list type="number">
    /// <item>Постоянные индексы связей, которые остаются стабильными при операциях с базой данных</item>
    /// <item>Единую интерпретацию данных в нескольких базах данных</item>
    /// <item>Определение протокола/API для коммуникации между связями</item>
    /// <item>Эффективную синхронизацию в децентрализованных сетях</item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface ILinksMap<TLinkAddress> where TLinkAddress : struct
    {
        /// <summary>
        /// Maps a local link index to its permanent/external index.
        /// <para>Отображает локальный индекс связи на его постоянный/внешний индекс.</para>
        /// </summary>
        /// <param name="localIndex">The local link index. <para>Локальный индекс связи.</para></param>
        /// <param name="permanentIndex">When this method returns, contains the permanent/external index if the mapping exists; otherwise, the default value. <para>Когда этот метод возвращается, содержит постоянный/внешний индекс, если отображение существует; в противном случае, значение по умолчанию.</para></param>
        /// <returns>True if the mapping exists; otherwise, false. <para>True если отображение существует; в противном случае, false.</para></returns>
        bool TryGetPermanentIndex(TLinkAddress localIndex, out TLinkAddress permanentIndex);

        /// <summary>
        /// Maps a permanent/external index to its local link index.
        /// <para>Отображает постоянный/внешний индекс на его локальный индекс связи.</para>
        /// </summary>
        /// <param name="permanentIndex">The permanent/external index. <para>Постоянный/внешний индекс.</para></param>
        /// <param name="localIndex">When this method returns, contains the local link index if the mapping exists; otherwise, the default value. <para>Когда этот метод возвращается, содержит локальный индекс связи, если отображение существует; в противном случае, значение по умолчанию.</para></param>
        /// <returns>True if the mapping exists; otherwise, false. <para>True если отображение существует; в противном случае, false.</para></returns>
        bool TryGetLocalIndex(TLinkAddress permanentIndex, out TLinkAddress localIndex);

        /// <summary>
        /// Creates or updates a mapping between local and permanent indices.
        /// <para>Создает или обновляет отображение между локальным и постоянным индексами.</para>
        /// </summary>
        /// <param name="localIndex">The local link index. <para>Локальный индекс связи.</para></param>
        /// <param name="permanentIndex">The permanent/external index. <para>Постоянный/внешний индекс.</para></param>
        void Map(TLinkAddress localIndex, TLinkAddress permanentIndex);

        /// <summary>
        /// Removes a mapping for the specified local index.
        /// <para>Удаляет отображение для указанного локального индекса.</para>
        /// </summary>
        /// <param name="localIndex">The local link index. <para>Локальный индекс связи.</para></param>
        /// <returns>True if the mapping was removed, false if it didn't exist. <para>True если отображение было удалено, false если оно не существовало.</para></returns>
        bool Unmap(TLinkAddress localIndex);

        /// <summary>
        /// Checks if a mapping exists for the specified local index.
        /// <para>Проверяет, существует ли отображение для указанного локального индекса.</para>
        /// </summary>
        /// <param name="localIndex">The local link index. <para>Локальный индекс связи.</para></param>
        /// <returns>True if a mapping exists. <para>True если отображение существует.</para></returns>
        bool HasMapping(TLinkAddress localIndex);

        /// <summary>
        /// Gets the total number of mappings.
        /// <para>Получает общее количество отображений.</para>
        /// </summary>
        long Count { get; }
    }
}
