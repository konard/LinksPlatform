using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Memory;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the ChatCrawler.
    /// Allows importing chat data from various formats into links storage.
    /// </summary>
    public class ChatCrawlerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLower();
            var inputPath = args[1];

            var dbPath = args.Length > 2 ? args[2] : "chat.links";

            Console.WriteLine($"ChatCrawler CLI");
            Console.WriteLine($"Command: {command}");
            Console.WriteLine($"Input: {inputPath}");
            Console.WriteLine($"Database: {dbPath}");
            Console.WriteLine();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                    Console.WriteLine("Cancellation requested...");
                };

                using (var memoryAdapter = new FileMappedResizableDirectMemory(dbPath))
                using (var links = new UnitedMemoryLinks<ulong>(memoryAdapter))
                {
                    var sequences = new SequenceIndex<ulong>(links);
                    var crawler = new ChatCrawler(new SynchronizedLinks<ulong>(links), sequences);

                    switch (command)
                    {
                        case "import-csv":
                            ImportFromCsv(crawler, inputPath, cancellationTokenSource.Token);
                            break;
                        case "import-json":
                            ImportFromJson(crawler, inputPath, cancellationTokenSource.Token);
                            break;
                        case "stats":
                            ShowStats(crawler);
                            break;
                        default:
                            Console.WriteLine($"Unknown command: {command}");
                            PrintUsage();
                            break;
                    }
                }
            }
        }

        private void ImportFromCsv(ChatCrawler crawler, string csvPath, CancellationToken cancellationToken)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Error: File not found: {csvPath}");
                return;
            }

            Console.WriteLine($"Importing messages from CSV: {csvPath}");

            var messages = new List<ChatMessage>();
            var lineNumber = 0;

            foreach (var line in File.ReadLines(csvPath))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                lineNumber++;
                if (lineNumber == 1) continue; // Skip header

                var parts = line.Split(',');
                if (parts.Length < 4)
                {
                    Console.WriteLine($"Warning: Skipping malformed line {lineNumber}");
                    continue;
                }

                try
                {
                    var chatId = parts[0].Trim('"');
                    var username = parts[1].Trim('"');
                    var timestamp = DateTime.Parse(parts[2].Trim('"'));
                    var text = parts[3].Trim('"');

                    messages.Add(new ChatMessage(username, text, timestamp, chatId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error parsing line {lineNumber}: {ex.Message}");
                }
            }

            crawler.StoreMessages(messages, cancellationToken);
        }

        private void ImportFromJson(ChatCrawler crawler, string jsonPath, CancellationToken cancellationToken)
        {
            Console.WriteLine($"JSON import not yet implemented for: {jsonPath}");
            Console.WriteLine("Please use CSV format with columns: ChatId,Username,Timestamp,Text");
        }

        private void ShowStats(ChatCrawler crawler)
        {
            Console.WriteLine("Chat Database Statistics:");
            Console.WriteLine($"Total messages: {crawler.GetMessageCount()}");
        }

        private void PrintUsage()
        {
            Console.WriteLine("Usage: ChatCrawlerCLI <command> <input> [database]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  import-csv <csv-file> [db-file]  - Import messages from CSV");
            Console.WriteLine("  import-json <json-file> [db-file] - Import messages from JSON (not yet implemented)");
            Console.WriteLine("  stats <any> [db-file]            - Show database statistics");
            Console.WriteLine();
            Console.WriteLine("CSV Format:");
            Console.WriteLine("  ChatId,Username,Timestamp,Text");
            Console.WriteLine("  \"chat1\",\"user1\",\"2025-01-01T12:00:00\",\"Hello world\"");
            Console.WriteLine();
            Console.WriteLine("Default database: chat.links");
        }
    }
}
