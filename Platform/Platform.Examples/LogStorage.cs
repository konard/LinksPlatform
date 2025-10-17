using System;
using System.IO;
using System.Threading;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Stores log files using Links (Doublets) with automatic deduplication.
    /// Repetitive log data is stored efficiently by referencing existing sequences.
    /// </summary>
    public class LogStorage
    {
        private readonly SequenceIndex<ulong> _index;
        private readonly SynchronizedLinks<ulong> _links;
        private readonly bool _storeByLines;

        /// <summary>
        /// Initializes a new instance of the LogStorage class.
        /// </summary>
        /// <param name="links">The synchronized links storage.</param>
        /// <param name="index">The sequence index for efficient deduplication.</param>
        /// <param name="storeByLines">If true, stores log line-by-line; otherwise stores as continuous stream.</param>
        public LogStorage(SynchronizedLinks<ulong> links, SequenceIndex<ulong> index, bool storeByLines = true)
        {
            _links = links;
            _index = index;
            _storeByLines = storeByLines;
        }

        /// <summary>
        /// Stores a log file using doublets with deduplication.
        /// </summary>
        /// <param name="logFilePath">Path to the log file to store.</param>
        /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
        public void StoreLogFile(string logFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(logFilePath))
            {
                throw new FileNotFoundException($"Log file not found: {logFilePath}");
            }

            Console.WriteLine($"Storing log file: {logFilePath}");

            if (_storeByLines)
            {
                StoreLogFileByLines(logFilePath, cancellationToken);
            }
            else
            {
                StoreLogFileAsStream(logFilePath, cancellationToken);
            }
        }

        private void StoreLogFileByLines(string logFilePath, CancellationToken cancellationToken)
        {
            var lineNumber = 0L;
            var totalChars = 0L;
            var initialLinksCount = _links.Count() - UnicodeMap.MapSize;

            using (var reader = File.OpenText(logFilePath))
            {
                string line;
                while (!cancellationToken.IsCancellationRequested && (line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        lineNumber++;
                        continue;
                    }

                    // Store the line as a sequence of character links
                    var linkArray = UnicodeMap.FromStringToLinkArray(line);
                    _index.Add(linkArray);

                    // Create newline marker
                    if (lineNumber > 0)
                    {
                        // Link previous line's last character to newline, then newline to current line's first character
                        _links.GetOrCreate(UnicodeMap.FromCharToLink('\n'), linkArray[0]);
                    }

                    totalChars += line.Length;
                    lineNumber++;

                    if (lineNumber % 1000 == 0)
                    {
                        var currentLinksCount = _links.Count() - UnicodeMap.MapSize;
                        var compressionRatio = totalChars > 0 ? (double)currentLinksCount / totalChars : 0;
                        Console.WriteLine($"Lines: {lineNumber}, Chars: {totalChars}, Links: {currentLinksCount}, Compression: {compressionRatio:F4}");
                    }
                }
            }

            var finalLinksCount = _links.Count() - UnicodeMap.MapSize;
            var totalCompressionRatio = totalChars > 0 ? (double)finalLinksCount / totalChars : 0;
            Console.WriteLine($"Completed: {lineNumber} lines, {totalChars} chars, {finalLinksCount} links (added {finalLinksCount - initialLinksCount})");
            Console.WriteLine($"Compression ratio: {totalCompressionRatio:F4} (lower is better - indicates more deduplication)");
        }

        private void StoreLogFileAsStream(string logFilePath, CancellationToken cancellationToken)
        {
            const int bufferSize = 1024 * 1024; // 1MB buffer
            var totalChars = 0L;
            var initialLinksCount = _links.Count() - UnicodeMap.MapSize;

            using (var reader = File.OpenText(logFilePath))
            {
                char[] buffer = new char[bufferSize];
                int readChars;
                char lastChar = '\0';

                while (!cancellationToken.IsCancellationRequested && (readChars = reader.Read(buffer, 0, bufferSize)) > 0)
                {
                    // Connect last character of previous chunk to first character of current chunk
                    if (lastChar != '\0')
                    {
                        _links.GetOrCreate(UnicodeMap.FromCharToLink(lastChar), UnicodeMap.FromCharToLink(buffer[0]));
                    }

                    lastChar = buffer[readChars - 1];
                    var linkArray = UnicodeMap.FromCharsToLinkArray(buffer, readChars);
                    _index.Add(linkArray);

                    totalChars += readChars;

                    if (totalChars % (bufferSize * 10) == 0)
                    {
                        var currentLinksCount = _links.Count() - UnicodeMap.MapSize;
                        var compressionRatio = totalChars > 0 ? (double)currentLinksCount / totalChars : 0;
                        Console.WriteLine($"Chars: {totalChars}, Links: {currentLinksCount}, Compression: {compressionRatio:F4}");
                    }
                }
            }

            var finalLinksCount = _links.Count() - UnicodeMap.MapSize;
            var totalCompressionRatio = totalChars > 0 ? (double)finalLinksCount / totalChars : 0;
            Console.WriteLine($"Completed: {totalChars} chars, {finalLinksCount} links (added {finalLinksCount - initialLinksCount})");
            Console.WriteLine($"Compression ratio: {totalCompressionRatio:F4} (lower is better - indicates more deduplication)");
        }

        /// <summary>
        /// Stores multiple log files using the same links storage for maximum deduplication.
        /// </summary>
        /// <param name="logFilePaths">Array of log file paths to store.</param>
        /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
        public void StoreMultipleLogFiles(string[] logFilePaths, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Storing {logFilePaths.Length} log files with shared dictionary...");

            for (int i = 0; i < logFilePaths.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Operation cancelled.");
                    break;
                }

                Console.WriteLine($"\n[{i + 1}/{logFilePaths.Length}] Processing: {logFilePaths[i]}");

                if (File.Exists(logFilePaths[i]))
                {
                    StoreLogFile(logFilePaths[i], cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Warning: File not found - {logFilePaths[i]}");
                }
            }

            Console.WriteLine("\nAll log files processed.");
            Console.WriteLine($"Total links in storage: {_links.Count() - UnicodeMap.MapSize}");
        }
    }
}
