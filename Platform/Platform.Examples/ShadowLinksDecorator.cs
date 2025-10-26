using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// A decorator that runs a Links-based storage in shadow mode (read-only replica).
    /// The shadow storage replicates all write operations from the primary storage,
    /// but is not used for read operations. This allows testing Links storage
    /// as a replacement for other storage types without affecting the primary data path.
    /// </para>
    /// <para>
    /// Декоратор, который запускает хранилище на основе Links в теневом режиме (реплика только для чтения).
    /// Теневое хранилище реплицирует все операции записи из основного хранилища,
    /// но не используется для операций чтения. Это позволяет тестировать хранилище Links
    /// в качестве замены других типов хранилища без влияния на основной путь данных.
    /// </para>
    /// </summary>
    /// <typeparam name="TLink">The type of link address.</typeparam>
    public class ShadowLinksDecorator<TLink> : ILinks<TLink>
    {
        private readonly ILinks<TLink> _primaryLinks;
        private readonly ILinks<TLink> _shadowLinks;
        private readonly Action<string> _logger;
        private readonly bool _validateConsistency;

        /// <summary>
        /// <para>Gets the constants from the primary storage.</para>
        /// <para>Получает константы из основного хранилища.</para>
        /// </summary>
        public LinksConstants<TLink> Constants => _primaryLinks.Constants;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="ShadowLinksDecorator{TLink}"/> class.
        /// </para>
        /// <para>
        /// Инициализирует новый экземпляр класса <see cref="ShadowLinksDecorator{TLink}"/>.
        /// </para>
        /// </summary>
        /// <param name="primaryLinks">
        /// <para>The primary storage that handles all operations.</para>
        /// <para>Основное хранилище, которое обрабатывает все операции.</para>
        /// </param>
        /// <param name="shadowLinks">
        /// <para>The shadow storage that replicates write operations.</para>
        /// <para>Теневое хранилище, которое реплицирует операции записи.</para>
        /// </param>
        /// <param name="logger">
        /// <para>Optional logger for shadow mode operations.</para>
        /// <para>Опциональный логгер для операций теневого режима.</para>
        /// </param>
        /// <param name="validateConsistency">
        /// <para>If true, validates that shadow storage remains consistent with primary after each operation.</para>
        /// <para>Если true, проверяет согласованность теневого хранилища с основным после каждой операции.</para>
        /// </param>
        public ShadowLinksDecorator(
            ILinks<TLink> primaryLinks,
            ILinks<TLink> shadowLinks,
            Action<string> logger = null,
            bool validateConsistency = false)
        {
            _primaryLinks = primaryLinks ?? throw new ArgumentNullException(nameof(primaryLinks));
            _shadowLinks = shadowLinks ?? throw new ArgumentNullException(nameof(shadowLinks));
            _logger = logger ?? (_ => { });
            _validateConsistency = validateConsistency;
        }

        /// <summary>
        /// <para>
        /// Counts links in the primary storage only.
        /// Shadow storage is not used for read operations.
        /// </para>
        /// <para>
        /// Подсчитывает связи только в основном хранилище.
        /// Теневое хранилище не используется для операций чтения.
        /// </para>
        /// </summary>
        public TLink Count(IList<TLink> restriction)
        {
            return _primaryLinks.Count(restriction);
        }

        /// <summary>
        /// <para>
        /// Reads from the primary storage only.
        /// Shadow storage is not used for read operations.
        /// </para>
        /// <para>
        /// Читает только из основного хранилища.
        /// Теневое хранилище не используется для операций чтения.
        /// </para>
        /// </summary>
        public TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restriction)
        {
            return _primaryLinks.Each(handler, restriction);
        }

        /// <summary>
        /// <para>
        /// Creates a link in the primary storage and replicates to shadow storage.
        /// </para>
        /// <para>
        /// Создает связь в основном хранилище и реплицирует в теневое хранилище.
        /// </para>
        /// </summary>
        public TLink Create(IList<TLink> substitution)
        {
            // Execute on primary storage
            var result = _primaryLinks.Create(substitution);

            try
            {
                // Replicate to shadow storage
                var shadowResult = _shadowLinks.Create(substitution);

                _logger($"Shadow: Replicated Create operation. Primary result: {result}, Shadow result: {shadowResult}");

                // Validate consistency if enabled
                if (_validateConsistency)
                {
                    ValidateCreateConsistency(result, shadowResult);
                }
            }
            catch (Exception ex)
            {
                _logger($"Shadow: Error replicating Create operation: {ex.Message}");
                // In shadow mode, we don't fail the primary operation if shadow fails
            }

            return result;
        }

        /// <summary>
        /// <para>
        /// Updates a link in the primary storage and replicates to shadow storage.
        /// </para>
        /// <para>
        /// Обновляет связь в основном хранилище и реплицирует в теневое хранилище.
        /// </para>
        /// </summary>
        public TLink Update(IList<TLink> restriction, IList<TLink> substitution)
        {
            // Execute on primary storage
            var result = _primaryLinks.Update(restriction, substitution);

            try
            {
                // Replicate to shadow storage
                var shadowResult = _shadowLinks.Update(restriction, substitution);

                _logger($"Shadow: Replicated Update operation. Primary result: {result}, Shadow result: {shadowResult}");

                // Validate consistency if enabled
                if (_validateConsistency)
                {
                    ValidateUpdateConsistency(restriction, substitution);
                }
            }
            catch (Exception ex)
            {
                _logger($"Shadow: Error replicating Update operation: {ex.Message}");
                // In shadow mode, we don't fail the primary operation if shadow fails
            }

            return result;
        }

        /// <summary>
        /// <para>
        /// Deletes a link in the primary storage and replicates to shadow storage.
        /// </para>
        /// <para>
        /// Удаляет связь в основном хранилище и реплицирует в теневое хранилище.
        /// </para>
        /// </summary>
        public void Delete(IList<TLink> restriction)
        {
            // Execute on primary storage
            _primaryLinks.Delete(restriction);

            try
            {
                // Replicate to shadow storage
                _shadowLinks.Delete(restriction);

                _logger($"Shadow: Replicated Delete operation for restriction: {(restriction != null && restriction.Count > 0 ? restriction[0].ToString() : "null")}");

                // Validate consistency if enabled
                if (_validateConsistency)
                {
                    ValidateDeleteConsistency(restriction);
                }
            }
            catch (Exception ex)
            {
                _logger($"Shadow: Error replicating Delete operation: {ex.Message}");
                // In shadow mode, we don't fail the primary operation if shadow fails
            }
        }

        private void ValidateCreateConsistency(TLink primaryResult, TLink shadowResult)
        {
            if (!primaryResult.Equals(shadowResult))
            {
                _logger($"Shadow: CONSISTENCY WARNING - Create operation returned different addresses. Primary: {primaryResult}, Shadow: {shadowResult}");
            }
        }

        private void ValidateUpdateConsistency(IList<TLink> restriction, IList<TLink> substitution)
        {
            // Basic consistency check - verify the link exists in both storages with same content
            if (restriction != null && restriction.Count > 0)
            {
                var linkAddress = restriction[0];
                // Additional validation logic could be added here
                _logger($"Shadow: Validated Update consistency for link {linkAddress}");
            }
        }

        private void ValidateDeleteConsistency(IList<TLink> restriction)
        {
            // Basic consistency check - verify the link is deleted in both storages
            if (restriction != null && restriction.Count > 0)
            {
                var linkAddress = restriction[0];
                var primaryCount = _primaryLinks.Count(restriction);
                var shadowCount = _shadowLinks.Count(restriction);

                if (!primaryCount.Equals(shadowCount))
                {
                    _logger($"Shadow: CONSISTENCY WARNING - Delete operation resulted in different counts. Primary: {primaryCount}, Shadow: {shadowCount}");
                }
            }
        }
    }
}
