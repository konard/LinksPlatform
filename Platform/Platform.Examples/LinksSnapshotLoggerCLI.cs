using System;
using System.IO;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface example demonstrating snapshot logging functionality.
    /// Пример интерфейса командной строки, демонстрирующий функциональность логирования снимков.
    /// </summary>
    /// <remarks>
    /// This example demonstrates the solution to issue #46: pushing existing database links
    /// to a transaction log, enabling full database recovery from the log.
    /// Этот пример демонстрирует решение проблемы #46: выгрузка существующих связей БД
    /// в лог транзакций, позволяющая полное восстановление БД из лога.
    /// </remarks>
    public class LinksSnapshotLoggerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            var dbPath = args[0];
            var logPath = args[1];

            try
            {
                Console.WriteLine($"Opening database: {dbPath}");
                Console.WriteLine($"Target log file: {logPath}");
                Console.WriteLine();

                using (var links = new UnitedMemoryLinks<ulong>(dbPath))
                {
                    var totalLinks = links.Count();
                    Console.WriteLine($"Database contains {totalLinks} links");

                    var snapshotLogger = new LinksSnapshotLogger<ulong>(links);

                    Console.WriteLine("Pushing snapshot to transaction log...");
                    var snapshotCount = snapshotLogger.PushSnapshotToFile(logPath);

                    Console.WriteLine($"✓ Successfully wrote {snapshotCount} links to {logPath}");
                    Console.WriteLine();
                    Console.WriteLine("The transaction log can now be used to fully restore the database.");
                    Console.WriteLine("Any future changes can be logged using LoggingDecorator (Platform.Data.Doublets 0.7+)");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine("LinksSnapshotLogger - Push database snapshot to transaction log");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  LinksSnapshotLogger <database-path> <log-file-path>");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  database-path    Path to the links database file (e.g., db.links)");
            Console.WriteLine("  log-file-path    Path to the transaction log file (will be created/appended)");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  LinksSnapshotLogger ./db.links ./transaction.log");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("  This tool creates a snapshot of all existing links in the database and");
            Console.WriteLine("  writes them to a transaction log in the same format as LoggingDecorator.");
            Console.WriteLine("  This allows enabling transaction logging on existing databases while");
            Console.WriteLine("  preserving the ability to fully restore the database from the log.");
            Console.WriteLine();
            Console.WriteLine("  Issue: https://github.com/konard/LinksPlatform/issues/46");
        }
    }
}
