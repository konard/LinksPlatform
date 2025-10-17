using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Platform.IO;

namespace Platform.Examples
{
    public class FileDigramFrequencyCalculator
    {
        private readonly ConcurrentDictionary<string, long> _digramFrequencies;

        public FileDigramFrequencyCalculator()
        {
            _digramFrequencies = new ConcurrentDictionary<string, long>();
        }

        public IReadOnlyDictionary<string, long> DigramFrequencies => _digramFrequencies;

        public void CalculateSync(string path, CancellationToken cancellationToken)
        {
            const int stepSize = 1024 * 1024;

            using (var reader = File.OpenText(path))
            {
                var steps = 0;
                char[] buffer = new char[stepSize];
                int readChars = 0;
                char lastCharOfPreviousChunk = '\0';

                while (!cancellationToken.IsCancellationRequested && (readChars = reader.Read(buffer, 0, stepSize)) > 0)
                {
                    if (lastCharOfPreviousChunk != '\0')
                    {
                        var digram = new string(new[] { lastCharOfPreviousChunk, buffer[0] });
                        _digramFrequencies.AddOrUpdate(digram, 1, (key, oldValue) => oldValue + 1);
                    }

                    for (int i = 0; i < readChars - 1; i++)
                    {
                        var digram = new string(new[] { buffer[i], buffer[i + 1] });
                        _digramFrequencies.AddOrUpdate(digram, 1, (key, oldValue) => oldValue + 1);
                    }

                    lastCharOfPreviousChunk = buffer[readChars - 1];
                    var totalDigrams = _digramFrequencies.Values.Sum();
                    Console.WriteLine($"chars: {(ulong)steps * stepSize + (ulong)readChars}, digrams: {totalDigrams}, unique: {_digramFrequencies.Count}");
                    steps++;
                }
            }
        }

        public async Task CalculateAsync(string path, CancellationToken cancellationToken)
        {
            const int stepSize = 1024 * 1024;
            using (var reader = File.OpenText(path))
            {
                var steps = 0;
                char[] buffer = new char[stepSize];
                int readChars = 0;
                char lastCharOfPreviousChunk = '\0';
                ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();

                while (!cancellationToken.IsCancellationRequested && (readChars = reader.Read(buffer, 0, stepSize)) > 0)
                {
                    if (lastCharOfPreviousChunk != '\0')
                    {
                        var digram = new string(new[] { lastCharOfPreviousChunk, buffer[0] });
                        _digramFrequencies.AddOrUpdate(digram, 1, (key, oldValue) => oldValue + 1);
                    }

                    var bufferCopy = new char[readChars];
                    Array.Copy(buffer, bufferCopy, readChars);

                    tasks.Enqueue(Task.Run(() =>
                    {
                        for (int i = 0; i < bufferCopy.Length - 1; i++)
                        {
                            var digram = new string(new[] { bufferCopy[i], bufferCopy[i + 1] });
                            _digramFrequencies.AddOrUpdate(digram, 1, (key, oldValue) => oldValue + 1);
                        }
                    }));

                    lastCharOfPreviousChunk = bufferCopy[readChars - 1];

                    if (tasks.Count > 3)
                    {
                        if (tasks.TryDequeue(out var task))
                        {
                            await task;
                        }
                    }

                    var totalDigrams = _digramFrequencies.Values.Sum();
                    Console.WriteLine($"chars: {(ulong)steps * stepSize + (ulong)readChars}, digrams: {totalDigrams}, unique: {_digramFrequencies.Count}");
                    steps++;
                }

                while (tasks.TryDequeue(out var task))
                {
                    await task;
                }
            }
        }

        public void PrintTopDigrams(int topCount = 20)
        {
            Console.WriteLine($"\nTop {topCount} most frequent digrams:");
            var sortedDigrams = _digramFrequencies
                .OrderByDescending(kvp => kvp.Value)
                .Take(topCount);

            foreach (var kvp in sortedDigrams)
            {
                var displayDigram = kvp.Key.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
                Console.WriteLine($"{displayDigram}: {kvp.Value}");
            }
        }

        public void SaveToFile(string outputPath)
        {
            var sortedDigrams = _digramFrequencies
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => $"{kvp.Key}\t{kvp.Value}");

            File.WriteAllLines(outputPath, sortedDigrams);
            Console.WriteLine($"\nDigram frequencies saved to: {outputPath}");
        }
    }
}
