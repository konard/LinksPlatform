using System;
using System.IO;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Singletons;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates AnyLink pattern matching in sequences.
    ///
    /// This example shows:
    /// - Simple AnyLink (_) matching - matches any single element
    /// - ZeroOrMany (*) matching - matches zero or more elements
    /// - Pattern matching with partial matches
    /// - Finding connections between elements
    /// </summary>
    public class SequencesPatternMatchingExample
    {
        private const string DbFilename = "sequences-pattern-matching-test.links";
        private static readonly LinksConstants<ulong> _constants = Default<LinksConstants<ulong>>.Instance;

        public static void Run()
        {
            Console.WriteLine("=== Sequences Pattern Matching Example ===\n");

            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(DbFilename, 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);

                // Initialize Unicode
                var unicodeMap = new UnicodeMap(syncLinks);
                unicodeMap.Init();

                var sequences = new Sequences(syncLinks, new SequencesOptions<ulong>
                {
                    UseSequenceMarker = true,
                    SequenceMarkerLink = 65537,
                    UseCompression = true
                });

                // Create test sequences
                SetupTestSequences(sequences);

                // Demonstrate different matching patterns
                DemonstrateAnyLinkMatching(sequences);
                DemonstrateZeroOrManyMatching(sequences);
                DemonstratePartialMatching(sequences);
                DemonstrateConnectionMatching(sequences);

                Console.WriteLine($"\nTotal links in database: {links.Count()}");
            }

            if (File.Exists(DbFilename))
            {
                File.Delete(DbFilename);
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Create test sequences for pattern matching demonstrations
        /// </summary>
        private static void SetupTestSequences(Sequences sequences)
        {
            Console.WriteLine("--- Setting up test sequences ---");

            var testStrings = new[]
            {
                "cat",
                "dog",
                "car",
                "cap",
                "bat",
                "hello",
                "world",
                "help"
            };

            foreach (var str in testStrings)
            {
                var chars = UnicodeMap.FromStringToLinkArray(str);
                var link = sequences.Create(chars);
                Console.WriteLine($"Created: '{str}' at link {link}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates AnyLink (_) matching - matches any single element in that position
        /// </summary>
        private static void DemonstrateAnyLinkMatching(Sequences sequences)
        {
            Console.WriteLine("--- AnyLink (_) Matching ---");
            Console.WriteLine("Pattern: 'c_t' - matches 'cat', 'cot', 'cut', etc.\n");

            // Create pattern: c + Any + t
            var c = UnicodeMap.FromCharToLink('c');
            var t = UnicodeMap.FromCharToLink('t');
            var pattern = new[] { c, _constants.Any, t };

            // Search for matches
            var matches = sequences.Each(pattern);

            Console.WriteLine($"Found {matches.Count} matches for pattern 'c_t':");
            foreach (var match in matches)
            {
                Console.WriteLine($"  Link {match}: {sequences.FormatSequence(match, AppendLink, true)}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates ZeroOrMany (*) matching - matches zero or more elements
        /// </summary>
        private static void DemonstrateZeroOrManyMatching(Sequences sequences)
        {
            Console.WriteLine("--- ZeroOrMany (*) Matching ---");
            Console.WriteLine("Pattern: 'h*' - matches 'h', 'he', 'hello', 'help', etc.\n");

            // Create pattern: h + ZeroOrMany
            var h = UnicodeMap.FromCharToLink('h');
            var pattern = new[] { h, Sequences.ZeroOrMany };

            // Search for matches using MatchPattern
            var matches = sequences.MatchPattern(pattern);

            Console.WriteLine($"Found {matches.Count} matches for pattern 'h*':");
            foreach (var match in matches)
            {
                var isSequence = sequences.IsSequence(match) ? "[S]" : "[P]";
                Console.WriteLine($"  Link {match}: {sequences.FormatSequence(match, AppendLink, true)} {isSequence}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates partial matching - finding sequences that contain the query elements
        /// </summary>
        private static void DemonstratePartialMatching(Sequences sequences)
        {
            Console.WriteLine("--- Partial Matching ---");
            Console.WriteLine("Query: 'el' - find sequences containing these elements\n");

            // Create query sequence
            var e = UnicodeMap.FromCharToLink('e');
            var l = UnicodeMap.FromCharToLink('l');
            var query = new[] { e, l };

            // Find partial matches
            var matches = sequences.GetAllPartiallyMatchingSequences1(query);

            Console.WriteLine($"Found {matches.Count} sequences containing 'el':");
            foreach (var match in matches)
            {
                var isSequence = sequences.IsSequence(match) ? "[S]" : "[P]";
                Console.WriteLine($"  Link {match}: {sequences.FormatSequence(match, AppendLink, true)} {isSequence}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates finding connections between query elements
        /// </summary>
        private static void DemonstrateConnectionMatching(Sequences sequences)
        {
            Console.WriteLine("--- Connection Matching ---");
            Console.WriteLine("Query: 'c', 'a' - find sequences that connect these elements\n");

            // Create query with elements that should be connected
            var c = UnicodeMap.FromCharToLink('c');
            var a = UnicodeMap.FromCharToLink('a');
            var query = new[] { c, a };

            // Find all connections
            var connections = sequences.GetAllConnections(query);

            Console.WriteLine($"Found {connections.Count} sequences connecting 'c' and 'a':");
            foreach (var connection in connections)
            {
                var isSequence = sequences.IsSequence(connection) ? "[S]" : "[P]";
                Console.WriteLine($"  Link {connection}: {sequences.FormatSequence(connection, AppendLink, true)} {isSequence}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates combined pattern matching with multiple wildcards
        /// </summary>
        private static void DemonstrateCombinedPatterns()
        {
            Console.WriteLine("--- Combined Patterns ---");
            Console.WriteLine("Patterns can combine _ (any single) and * (zero or more):");
            Console.WriteLine("  'c_*' - starts with 'c', then any character, then anything");
            Console.WriteLine("  '*a*' - contains 'a' anywhere");
            Console.WriteLine("  '_a_' - 3 characters with 'a' in the middle");
            Console.WriteLine();
        }

        /// <summary>
        /// Helper method to append link representation to string
        /// </summary>
        private static void AppendLink(System.Text.StringBuilder sb, ulong link)
        {
            if (link <= (char.MaxValue + 1))
            {
                sb.Append(UnicodeMap.FromLinkToChar(link));
            }
            else
            {
                sb.Append($"({link})");
            }
        }
    }
}
