using System;
using System.IO;
using Platform.Numbers;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Provides functionality to push a snapshot of all existing links to a transaction log.
    /// Предоставляет функциональность для выгрузки снимка всех существующих связей в лог транзакций.
    /// </summary>
    /// <remarks>
    /// This utility addresses issue #46: allowing to push existing database links to transaction log
    /// when logging is enabled on an already populated database.
    /// Эта утилита решает проблему #46: позволяет выгрузить существующие связи БД в лог транзакций,
    /// когда логирование включается на уже заполненной базе данных.
    /// </remarks>
    public class LinksSnapshotLogger<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly ILinks<TLink> _links;

        /// <summary>
        /// Creates a new instance of LinksSnapshotLogger.
        /// Создаёт новый экземпляр LinksSnapshotLogger.
        /// </summary>
        /// <param name="links">The links storage to create snapshot from. Хранилище связей для создания снимка.</param>
        public LinksSnapshotLogger(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// Pushes a snapshot of all existing links to the specified log stream.
        /// Each link is written as a Create operation that can be replayed to restore the database.
        /// Выгружает снимок всех существующих связей в указанный поток лога.
        /// Каждая связь записывается как операция Create, которая может быть воспроизведена для восстановления БД.
        /// </summary>
        /// <param name="logStream">The stream to write the snapshot to. Поток для записи снимка.</param>
        /// <returns>The number of links written to the snapshot. Количество связей записанных в снимок.</returns>
        public TLink PushSnapshot(Stream logStream)
        {
            if (logStream == null)
            {
                throw new ArgumentNullException(nameof(logStream));
            }

            var writer = new StreamWriter(logStream, System.Text.Encoding.UTF8, 4096, leaveOpen: true);
            try
            {
                writer.AutoFlush = true;

                var constants = _links.Constants;
                var count = _zero;

                // Write snapshot header
                writer.WriteLine($"# Snapshot Start - {DateTime.UtcNow:O}");
                writer.WriteLine($"# Total links in database: {_links.Count()}");

                // Iterate through all links
                _links.Each(link =>
                {
                    var index = link[constants.IndexPart];

                    // Skip the null/void link (index 0 or constants.Null)
                    if (!index.Equals(constants.Null) && !index.Equals(_zero))
                    {
                        var source = link[constants.SourcePart];
                        var target = link[constants.TargetPart];

                        // Write in the same format as LoggingDecorator would write Create operations
                        writer.WriteLine($"Create. Before: {constants.Null}: {constants.Null}->{constants.Null}. After: {index}: {source}->{target}");

                        count = Arithmetic.Add(count, _one);
                    }

                    return constants.Continue;
                });

                // Write snapshot footer
                writer.WriteLine($"# Snapshot End - {count} links written");
                writer.Flush();

                return count;
            }
            finally
            {
                writer?.Dispose();
            }
        }

        /// <summary>
        /// Pushes a snapshot to a file at the specified path.
        /// Выгружает снимок в файл по указанному пути.
        /// </summary>
        /// <param name="logFilePath">The path to the log file. Путь к файлу лога.</param>
        /// <returns>The number of links written to the snapshot. Количество связей записанных в снимок.</returns>
        public TLink PushSnapshotToFile(string logFilePath)
        {
            using (var fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                return PushSnapshot(fileStream);
            }
        }
    }
}
