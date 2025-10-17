using System;
using System.Diagnostics;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating the SequenceUnfoldIndex functionality.
    /// </summary>
    public class SequenceUnfoldIndexCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            Console.WriteLine("=== Sequence Unfold Index Demo ===");
            Console.WriteLine();

            // Parse arguments
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var indexDirectory = args.Length > 1 ? args[1] : ".links-sequences";
            var minSequenceLength = args.Length > 2 ? int.Parse(args[2]) : 10;

            Console.WriteLine($"Links file: {linksFile}");
            Console.WriteLine($"Index directory: {indexDirectory}");
            Console.WriteLine($"Minimum sequence length: {minSequenceLength}");
            Console.WriteLine();

            // Create or open links storage
            using (var cancellation = new ConsoleCancellation())
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(linksFile))
            using (var links = new UInt64Links(memoryAdapter))
            {
                Console.WriteLine("Links database opened.");
                Console.WriteLine();

                // Create the sequence unfold index
                var index = new SequenceUnfoldIndex<ulong>(links, indexDirectory, minSequenceLength);
                Console.WriteLine($"Sequence unfold index initialized. Indexed sequences: {index.IndexedSequenceCount}");
                Console.WriteLine();

                // Create the sequence unfolder
                var unfolder = new SequenceUnfolder<ulong>(links);

                // Demonstrate the functionality
                DemonstrateIndexing(links, index, unfolder);

                Console.WriteLine();
                Console.WriteLine($"Final indexed sequence count: {index.IndexedSequenceCount}");
            }

            Console.WriteLine();
            Console.WriteLine("Demo completed.");
        }

        private void DemonstrateIndexing(ILinks<ulong> links, SequenceUnfoldIndex<ulong> index, SequenceUnfolder<ulong> unfolder)
        {
            Console.WriteLine("--- Creating sample links ---");

            // Create some sample links to form sequences
            var link1 = links.Create();
            var link2 = links.Create();
            var link3 = links.Create();
            var link4 = links.Create();
            var link5 = links.Create();

            Console.WriteLine($"Created links: {link1}, {link2}, {link3}, {link4}, {link5}");

            // Update links to form a structure
            links.Update(link1, link1, link1); // Self-reference (leaf)
            links.Update(link2, link2, link2); // Self-reference (leaf)
            links.Update(link3, link1, link2);  // Connects link1 and link2
            links.Update(link4, link3, link1);  // Connects link3 and link1
            links.Update(link5, link4, link2);  // Connects link4 and link2

            Console.WriteLine("Links updated to form structures");
            Console.WriteLine();

            // Create a longer sequence by creating multiple links
            Console.WriteLine("--- Creating a long sequence ---");
            var sequenceLinks = new ulong[15];
            for (int i = 0; i < sequenceLinks.Length; i++)
            {
                sequenceLinks[i] = links.Create();
                if (i == 0)
                {
                    links.Update(sequenceLinks[i], sequenceLinks[i], sequenceLinks[i]);
                }
                else
                {
                    links.Update(sequenceLinks[i], sequenceLinks[i - 1], sequenceLinks[i]);
                }
            }
            Console.WriteLine($"Created a sequence of {sequenceLinks.Length} links");
            Console.WriteLine();

            // Unfold and index the long sequence
            Console.WriteLine("--- Testing sequence unfold and indexing ---");
            var lastLink = sequenceLinks[sequenceLinks.Length - 1];

            var stopwatch = Stopwatch.StartNew();
            var unfoldedSequence = unfolder.UnfoldChain(lastLink);
            stopwatch.Stop();
            Console.WriteLine($"Unfolded sequence length: {unfoldedSequence.Count}, Time: {stopwatch.ElapsedMilliseconds}ms");

            // Add to index
            stopwatch.Restart();
            var wasNew = index.Add(unfoldedSequence);
            stopwatch.Stop();
            Console.WriteLine($"Added to index (was new: {wasNew}), Time: {stopwatch.ElapsedMilliseconds}ms");

            // Check if contained
            stopwatch.Restart();
            var mightContain = index.MightContain(unfoldedSequence);
            stopwatch.Stop();
            Console.WriteLine($"Index might contain: {mightContain}, Time: {stopwatch.ElapsedMilliseconds}ms");

            // Try to add again (should return false)
            wasNew = index.Add(unfoldedSequence);
            Console.WriteLine($"Added to index again (was new: {wasNew}) - expected false");

            // Read from index
            if (unfoldedSequence.Count > 0)
            {
                stopwatch.Restart();
                var readSequence = index.ReadSequence(unfoldedSequence[0]);
                stopwatch.Stop();
                if (readSequence != null)
                {
                    Console.WriteLine($"Read sequence from index, length: {readSequence.Count}, Time: {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    Console.WriteLine("Failed to read sequence from index");
                }
            }

            Console.WriteLine();
            Console.WriteLine("--- Performance comparison ---");

            // Measure performance with and without index
            const int iterations = 1000;

            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var _ = unfolder.UnfoldChain(lastLink);
            }
            stopwatch.Stop();
            var unfoldTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Time to unfold {iterations} times: {unfoldTime}ms (avg: {unfoldTime / (double)iterations:F3}ms)");

            if (unfoldedSequence.Count > 0)
            {
                stopwatch.Restart();
                for (int i = 0; i < iterations; i++)
                {
                    var _ = index.ReadSequence(unfoldedSequence[0]);
                }
                stopwatch.Stop();
                var readTime = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Time to read from index {iterations} times: {readTime}ms (avg: {readTime / (double)iterations:F3}ms)");

                if (unfoldTime > 0)
                {
                    var speedup = unfoldTime / (double)readTime;
                    Console.WriteLine($"Speedup: {speedup:F2}x faster");
                }
            }
        }
    }
}
