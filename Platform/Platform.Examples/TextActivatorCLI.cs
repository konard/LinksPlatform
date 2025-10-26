using System;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the server-side text activator.
    /// Demonstrates how compression improves over time as the frequency dictionary learns patterns.
    /// </summary>
    public class TextActivatorCLI : ICommandLineInterface
    {
        private const string LinksDbPath = "text-activator.links";
        private const string FrequencyDictPath = "frequency-dict.dat";

        private UInt64UnitedMemoryLinks _memoryManager;
        private ILinks<ulong> _links;
        private Sequences _sequences;
        private ServerSideFrequencyDictionary<ulong> _frequencyDictionary;

        public void Run(string[] args)
        {
            Console.WriteLine("=== Server-Side Text Activator ===\n");
            Console.WriteLine("This demonstrates a server-side text compression system that");
            Console.WriteLine("improves its compression ratio over time by learning text patterns.\n");

            Initialize();

            try
            {
                ShowMenu();
            }
            finally
            {
                Cleanup();
            }
        }

        private void Initialize()
        {
            _memoryManager = new UInt64UnitedMemoryLinks(LinksDbPath, 64 * 1024 * 1024); // 64MB
            var baseLinks = new UInt64Links(_memoryManager);
            var syncLinks = new SynchronizedLinks<ulong>(baseLinks);
            _links = syncLinks;
            var unicodeMap = new UnicodeMap(syncLinks);
            unicodeMap.Init();
            _sequences = new Sequences(syncLinks);
            _frequencyDictionary = new ServerSideFrequencyDictionary<ulong>(FrequencyDictPath);

            Console.WriteLine("Initialized text activator:");
            ShowStats();
            Console.WriteLine();
        }

        private void Cleanup()
        {
            _memoryManager?.Dispose();
        }

        private void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Compress text");
                Console.WriteLine("2. Decompress link");
                Console.WriteLine("3. Show statistics");
                Console.WriteLine("4. Run demo");
                Console.WriteLine("5. Exit");
                Console.Write("\nSelect option: ");

                var input = Console.ReadLine();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        CompressText();
                        break;
                    case "2":
                        DecompressLink();
                        break;
                    case "3":
                        ShowStats();
                        break;
                    case "4":
                        RunDemo();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }
        }

        private void CompressText()
        {
            Console.Write("Enter text to compress: ");
            var text = Console.ReadLine();

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Text cannot be empty");
                return;
            }

            var originalSize = System.Text.Encoding.UTF8.GetByteCount(text);
            var sourceArray = UnicodeMap.FromStringToLinkArray(text);

            // Update frequency dictionary
            _frequencyDictionary.UpdateFromSequence(sourceArray);
            _frequencyDictionary.SaveToDisk();

            // Create compressed link
            var compressedLink = _sequences.Create(sourceArray);

            var linkRepresentation = compressedLink.ToString();
            var compressedSize = System.Text.Encoding.UTF8.GetByteCount(linkRepresentation);

            Console.WriteLine($"\nOriginal size: {originalSize} bytes");
            Console.WriteLine($"Compressed link ID: {compressedLink}");
            Console.WriteLine($"Link representation size: {compressedSize} bytes");
            Console.WriteLine($"Compression ratio: {(double)compressedSize / originalSize:P2}");

            // Verify decompression
            var decompressed = UnicodeMap.FromSequenceLinkToString(compressedLink, _links);
            Console.WriteLine($"Decompression verified: {decompressed == text}");
        }

        private void DecompressLink()
        {
            Console.Write("Enter link ID to decompress: ");
            if (ulong.TryParse(Console.ReadLine(), out var linkId))
            {
                try
                {
                    var text = UnicodeMap.FromSequenceLinkToString(linkId, _links);
                    Console.WriteLine($"\nDecompressed text: {text}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid link ID");
            }
        }

        private void ShowStats()
        {
            Console.WriteLine("Frequency Dictionary Statistics:");
            Console.WriteLine($"  Total symbols processed: {_frequencyDictionary.GetTotalSymbolCount()}");
            Console.WriteLine($"  Total doublets processed: {_frequencyDictionary.GetTotalDoubletCount()}");
            Console.WriteLine($"  Unique symbols: {_frequencyDictionary.GetSymbolsSortedByFrequency().Count()}");
            Console.WriteLine($"  Unique doublets: {_frequencyDictionary.GetDoubletsSortedByFrequency().Count()}");
        }

        private void RunDemo()
        {
            Console.WriteLine("Running demo with sample texts...\n");

            var sampleTexts = new[]
            {
                "Hello, world!",
                "The quick brown fox jumps over the lazy dog.",
                "Hello, world! Hello again!",
                "The quick brown fox jumps over the lazy fox.",
                "Compression improves with more data.",
                "More data means better compression.",
            };

            foreach (var text in sampleTexts)
            {
                Console.WriteLine($"Processing: \"{text}\"");

                var originalSize = System.Text.Encoding.UTF8.GetByteCount(text);
                var sourceArray = UnicodeMap.FromStringToLinkArray(text);

                _frequencyDictionary.UpdateFromSequence(sourceArray);
                _frequencyDictionary.SaveToDisk();

                var compressedLink = _sequences.Create(sourceArray);
                var linkSize = System.Text.Encoding.UTF8.GetByteCount(compressedLink.ToString());

                Console.WriteLine($"  Original: {originalSize} bytes → Compressed: {linkSize} bytes (ratio: {(double)linkSize / originalSize:P2})");
                Console.WriteLine();
            }

            Console.WriteLine("Demo complete!");
            ShowStats();
        }
    }
}
