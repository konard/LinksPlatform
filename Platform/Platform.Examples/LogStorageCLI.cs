using System;
using System.IO;
using Platform.IO;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences.Indexes;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for LogStorage functionality.
    /// Allows storing log files with automatic deduplication using Links (Doublets).
    /// </summary>
    public class LogStorageCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links database file", args);
            var logFileOrDirectory = ConsoleHelpers.GetOrReadArgument(1, "Log file or directory path", args);
            var storeByLines = true;

            if (args.Length > 2)
            {
                storeByLines = args[2].ToLower() != "stream";
            }

            if (!File.Exists(logFileOrDirectory) && !Directory.Exists(logFileOrDirectory))
            {
                Console.WriteLine("Error: Entered log file or directory does not exist.");
                Console.WriteLine($"Path: {logFileOrDirectory}");
                return;
            }

            Console.WriteLine("=== Log Storage using Links (Doublets) ===");
            Console.WriteLine($"Links database: {linksFile}");
            Console.WriteLine($"Storage mode: {(storeByLines ? "Line-by-line" : "Stream")}");
            Console.WriteLine("Press CTRL+C to stop.\n");

            using (var cancellation = new ConsoleCancellation())
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile, UInt64UnitedMemoryLinks.DefaultLinksSizeStep * 16))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();
                UnicodeMap.InitNew(syncLinks);

                var index = new SequenceIndex<ulong>(syncLinks);
                var logStorage = new LogStorage(syncLinks, index, storeByLines);

                try
                {
                    if (File.Exists(logFileOrDirectory))
                    {
                        // Single file
                        logStorage.StoreLogFile(logFileOrDirectory, cancellation.Token);
                    }
                    else if (Directory.Exists(logFileOrDirectory))
                    {
                        // Multiple files in directory
                        var logLogFiles = Directory.GetFiles(logFileOrDirectory, "*.log");
                        var txtLogFiles = Directory.GetFiles(logFileOrDirectory, "*.txt");
                        var logFiles = new string[logLogFiles.Length + txtLogFiles.Length];
                        Array.Copy(logLogFiles, 0, logFiles, 0, logLogFiles.Length);
                        Array.Copy(txtLogFiles, 0, logFiles, logLogFiles.Length, txtLogFiles.Length);

                        if (logFiles.Length == 0)
                        {
                            Console.WriteLine("No .log or .txt files found in directory.");
                            return;
                        }

                        Console.WriteLine($"Found {logFiles.Length} log files in directory.");
                        logStorage.StoreMultipleLogFiles(logFiles, cancellation.Token);
                    }

                    Console.WriteLine("\n=== Storage Complete ===");
                    Console.WriteLine($"Total links: {syncLinks.Count()}");
                    Console.WriteLine($"Data links (excluding unicode map): {syncLinks.Count() - UnicodeMap.MapSize}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError during storage: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }

            Console.WriteLine("\nLinks database saved to: {0}", linksFile);
        }
    }
}
