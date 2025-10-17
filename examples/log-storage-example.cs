// Example demonstrating log storage with deduplication using Links (Doublets)
// This example shows how to store log files efficiently with automatic deduplication
// of repetitive content, which is common in application logs.

using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Examples;

namespace LogStorageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example 1: Store a single log file
            Console.WriteLine("=== Example 1: Single Log File Storage ===\n");
            StoreSingleLogFile();

            Console.WriteLine("\n\n");

            // Example 2: Store multiple log files with shared dictionary
            Console.WriteLine("=== Example 2: Multiple Log Files with Shared Dictionary ===\n");
            StoreMultipleLogFiles();
        }

        static void StoreSingleLogFile()
        {
            // Create a sample log file
            var logFile = "sample.log";
            CreateSampleLogFile(logFile);

            var linksDb = "single-log.links";

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksDb))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();
                UnicodeMap.InitNew(syncLinks);

                var index = new SequenceIndex<ulong>(syncLinks);
                var logStorage = new LogStorage(syncLinks, index, storeByLines: true);

                logStorage.StoreLogFile(logFile, System.Threading.CancellationToken.None);

                Console.WriteLine($"\nLog stored in: {linksDb}");
                Console.WriteLine($"Original file size: {new FileInfo(logFile).Length} bytes");
                Console.WriteLine($"Links database size: {new FileInfo(linksDb).Length} bytes");
            }

            // Cleanup
            File.Delete(logFile);
        }

        static void StoreMultipleLogFiles()
        {
            // Create multiple sample log files with repetitive content
            var logFiles = new[]
            {
                "app1.log",
                "app2.log",
                "app3.log"
            };

            foreach (var logFile in logFiles)
            {
                CreateSampleLogFile(logFile, 500);
            }

            var linksDb = "multiple-logs.links";

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksDb))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();
                UnicodeMap.InitNew(syncLinks);

                var index = new SequenceIndex<ulong>(syncLinks);
                var logStorage = new LogStorage(syncLinks, index, storeByLines: true);

                logStorage.StoreMultipleLogFiles(logFiles, System.Threading.CancellationToken.None);

                var totalOriginalSize = 0L;
                foreach (var logFile in logFiles)
                {
                    totalOriginalSize += new FileInfo(logFile).Length;
                }

                Console.WriteLine($"\n=== Summary ===");
                Console.WriteLine($"Total original size: {totalOriginalSize} bytes");
                Console.WriteLine($"Links database size: {new FileInfo(linksDb).Length} bytes");
                Console.WriteLine($"Space savings: {(1 - (double)new FileInfo(linksDb).Length / totalOriginalSize) * 100:F2}%");
            }

            // Cleanup
            foreach (var logFile in logFiles)
            {
                File.Delete(logFile);
            }
        }

        static void CreateSampleLogFile(string filename, int lineCount = 1000)
        {
            var messages = new[]
            {
                "INFO: Application started successfully",
                "INFO: Database connection established",
                "DEBUG: Processing request from 192.168.1.1",
                "DEBUG: Query executed in 45ms",
                "WARNING: Cache miss for key 'user_session_12345'",
                "INFO: Request completed successfully",
                "ERROR: Failed to connect to external API",
                "ERROR: Timeout after 30 seconds",
                "INFO: Retrying connection...",
                "INFO: Connection restored"
            };

            using (var writer = File.CreateText(filename))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var timestamp = DateTime.Now.AddSeconds(-lineCount + i).ToString("yyyy-MM-dd HH:mm:ss");
                    var message = messages[i % messages.Length];
                    writer.WriteLine($"[{timestamp}] {message}");
                }
            }

            Console.WriteLine($"Created sample log: {filename} ({lineCount} lines)");
        }
    }
}
