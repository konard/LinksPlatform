using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Unicode;
using Platform.Singletons;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates various pattern matching capabilities in the LinksPlatform.
    /// This example addresses issue #110: Patterns
    /// </summary>
    public class PatternsExample
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;
        private static readonly LinksConstants<ulong> _constants = Default<LinksConstants<ulong>>.Instance;

        public PatternsExample(ILinks<ulong> links, Sequences sequences)
        {
            _links = links;
            _sequences = sequences;
        }

        /// <summary>
        /// Match data with a pattern using wildcards.
        /// Supports:
        /// - '_' for matching any single element
        /// - '*' for matching zero or more elements
        /// </summary>
        public IList<ulong> MatchDataWithPattern(ulong[] pattern)
        {
            // Pattern can contain:
            // - Constants.Any (_) to match any single link
            // - Sequences.ZeroOrMany (*) to match zero or more links
            // - Specific link addresses to match exact values

            var containsWildcard = Array.IndexOf(pattern, Sequences.ZeroOrMany) >= 0;

            if (containsWildcard)
            {
                // Use pattern matching with wildcards
                var matchSet = _sequences.MatchPattern(pattern);
                return new List<ulong>(matchSet);
            }
            else
            {
                // Use exact matching
                var matchSet = _sequences.Each(pattern);
                return new List<ulong>(matchSet);
            }
        }

        /// <summary>
        /// Match by Id/Index of one or multiple links.
        /// Direct link access by address/index.
        /// </summary>
        public bool MatchById(ulong linkId)
        {
            // Check if link exists by testing if it's within valid range
            var totalLinks = _links.Count(new Link<ulong>(_constants.Any, _constants.Any, _constants.Any));
            return linkId > _constants.Null && linkId <= totalLinks;
        }

        /// <summary>
        /// Match multiple links by their Ids/Indices.
        /// </summary>
        public IList<ulong> MatchByIds(params ulong[] linkIds)
        {
            var matches = new List<ulong>();
            foreach (var id in linkIds)
            {
                if (MatchById(id))
                {
                    matches.Add(id);
                }
            }
            return matches;
        }

        /// <summary>
        /// Match by graph structure of link.
        /// Find links based on their source and target connections.
        /// </summary>
        public IList<ulong> MatchByGraphStructure(ulong? source = null, ulong? target = null)
        {
            var matches = new List<ulong>();
            var any = _constants.Any;

            // Query pattern: (index, source, target)
            // Use Any for unspecified parameters
            var querySource = source ?? any;
            var queryTarget = target ?? any;

            _links.Each(linkArray =>
            {
                var link = linkArray[_constants.IndexPart];
                var linkSource = _links.GetSource(link);
                var linkTarget = _links.GetTarget(link);

                var sourceMatches = querySource == any || linkSource == querySource;
                var targetMatches = queryTarget == any || linkTarget == queryTarget;

                if (sourceMatches && targetMatches)
                {
                    matches.Add(link);
                }

                return _constants.Continue;
            }, new Link<ulong>(any, querySource, queryTarget));

            return matches;
        }

        /// <summary>
        /// Match by tree structure of link.
        /// Find sequences that form tree structures (balanced variants).
        /// </summary>
        public IList<ulong> MatchByTreeStructure(ulong[] elements)
        {
            // Tree structures in LinksPlatform are represented as balanced variants
            // of sequences where each link splits the sequence into left and right parts

            var matches = new List<ulong>();

            // Find all sequences that contain these elements regardless of tree structure
            var allMatches = _sequences.GetAllMatchingSequences1(elements);

            // Filter for sequences (tree structures)
            foreach (var match in allMatches)
            {
                if (_sequences.IsSequence((ulong)match))
                {
                    matches.Add((ulong)match);
                }
            }

            return matches;
        }

        /// <summary>
        /// Match by sequence structure of link.
        /// Find sequences that exactly or partially match the given pattern.
        /// </summary>
        public SequenceMatchResult MatchBySequenceStructure(ulong[] pattern)
        {
            var result = new SequenceMatchResult
            {
                FullyMatched = new List<ulong>(),
                PartiallyMatched = new List<ulong>(),
                Connections = new List<ulong>()
            };

            // Fully matched: sequences that contain all elements in exact order
            var fullyMatched = _sequences.GetAllMatchingSequences1(pattern);
            foreach (var match in fullyMatched)
            {
                result.FullyMatched.Add((ulong)match);
            }

            // Partially matched: sequences that contain some elements from the pattern
            var partiallyMatched = _sequences.GetAllPartiallyMatchingSequences1(pattern);
            foreach (var match in partiallyMatched)
            {
                result.PartiallyMatched.Add((ulong)match);
            }

            // Connections: sequences that connect elements from the pattern
            var connections = _sequences.GetAllConnections(pattern);
            foreach (var match in connections)
            {
                result.Connections.Add((ulong)match);
            }

            return result;
        }

        /// <summary>
        /// Match by surroundings (parents set, children set).
        /// Find links that reference or are referenced by specific links.
        /// </summary>
        public SurroundingsMatchResult MatchBySurroundings(ulong linkId)
        {
            var result = new SurroundingsMatchResult
            {
                Parents = new List<ulong>(),  // Links that reference this link as source
                Children = new List<ulong>()   // Links that reference this link as target
            };

            var any = _constants.Any;

            _links.Each(linkArray =>
            {
                var link = linkArray[_constants.IndexPart];
                var source = _links.GetSource(link);
                var target = _links.GetTarget(link);

                // Parent: this link is used as source
                if (source == linkId)
                {
                    result.Parents.Add(link);
                }

                // Child: this link is used as target
                if (target == linkId)
                {
                    result.Children.Add(link);
                }

                return _constants.Continue;
            }, new Link<ulong>(any, any, any));

            return result;
        }

        /// <summary>
        /// Generate data using a pattern.
        /// Creates sequences based on pattern specifications with wildcards replaced by actual values.
        /// </summary>
        public ulong GenerateFromPattern(ulong[] pattern, Dictionary<int, ulong> wildcardReplacements)
        {
            // Replace wildcards in pattern with actual values
            var generatedSequence = new ulong[pattern.Length];

            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == _constants.Any && wildcardReplacements.ContainsKey(i))
                {
                    // Replace wildcard with specified value
                    generatedSequence[i] = wildcardReplacements[i];
                }
                else if (pattern[i] == Sequences.ZeroOrMany)
                {
                    // Cannot generate from zero-or-many wildcard without specification
                    throw new ArgumentException($"Cannot generate from pattern with unspecified zero-or-many wildcard at position {i}");
                }
                else
                {
                    // Use the pattern value as-is
                    generatedSequence[i] = pattern[i];
                }
            }

            // Create the sequence
            return _sequences.Create(generatedSequence);
        }

        /// <summary>
        /// Format and display a sequence for debugging.
        /// </summary>
        public string FormatSequence(ulong sequenceLink)
        {
            return _sequences.FormatSequence(sequenceLink, (sb, link) =>
            {
                if (link <= (char.MaxValue + 1))
                {
                    sb.Append(UnicodeMap.FromLinkToChar(link));
                }
                else
                {
                    sb.Append($"({link})");
                }
            }, true);
        }

        /// <summary>
        /// Example demonstrating all pattern matching capabilities.
        /// </summary>
        public void DemonstrateAllPatterns()
        {
            Console.WriteLine("=== Pattern Matching Examples ===\n");

            // 1. Create some test data
            var helloArray = UnicodeMap.FromStringToLinkArray("hello");
            var worldArray = UnicodeMap.FromStringToLinkArray("world");
            var helloLink = _sequences.Create(helloArray);
            var worldLink = _sequences.Create(worldArray);

            Console.WriteLine($"Created sequences: 'hello' at {helloLink}, 'world' at {worldLink}");

            // 2. Match by Id
            Console.WriteLine($"\n--- Match by Id ---");
            Console.WriteLine($"Link {helloLink} exists: {MatchById(helloLink)}");
            Console.WriteLine($"Link 999999 exists: {MatchById(999999)}");

            // 3. Match by graph structure
            Console.WriteLine($"\n--- Match by Graph Structure ---");
            var source = _links.GetSource(helloLink);
            var graphMatches = MatchByGraphStructure(source: source);
            Console.WriteLine($"Found {graphMatches.Count} links with source = {source}");

            // 4. Match by tree structure
            Console.WriteLine($"\n--- Match by Tree Structure ---");
            var treeMatches = MatchByTreeStructure(helloArray);
            Console.WriteLine($"Found {treeMatches.Count} tree structures containing 'hello'");

            // 5. Match by sequence structure
            Console.WriteLine($"\n--- Match by Sequence Structure ---");
            var seqMatches = MatchBySequenceStructure(new[] { helloArray[0], helloArray[1] });
            Console.WriteLine($"Fully matched: {seqMatches.FullyMatched.Count}");
            Console.WriteLine($"Partially matched: {seqMatches.PartiallyMatched.Count}");
            Console.WriteLine($"Connections: {seqMatches.Connections.Count}");

            // 6. Match by surroundings
            Console.WriteLine($"\n--- Match by Surroundings ---");
            var surroundings = MatchBySurroundings(helloLink);
            Console.WriteLine($"Parents (references as source): {surroundings.Parents.Count}");
            Console.WriteLine($"Children (references as target): {surroundings.Children.Count}");

            // 7. Pattern matching with wildcards
            Console.WriteLine($"\n--- Pattern Matching with Wildcards ---");
            var pattern = new[] { helloArray[0], _constants.Any, helloArray[2] }; // 'h_l' pattern
            var patternMatches = MatchDataWithPattern(pattern);
            Console.WriteLine($"Matches for pattern 'h_l': {patternMatches.Count}");

            // 8. Generate from pattern
            Console.WriteLine($"\n--- Generate from Pattern ---");
            var replacements = new Dictionary<int, ulong> { { 1, helloArray[1] } }; // Replace '_' with 'e'
            var generated = GenerateFromPattern(pattern, replacements);
            Console.WriteLine($"Generated sequence at: {generated}");
            Console.WriteLine($"Formatted: {FormatSequence(generated)}");
        }
    }

    /// <summary>
    /// Result of sequence structure matching.
    /// </summary>
    public class SequenceMatchResult
    {
        public List<ulong> FullyMatched { get; set; }
        public List<ulong> PartiallyMatched { get; set; }
        public List<ulong> Connections { get; set; }
    }

    /// <summary>
    /// Result of surroundings-based matching.
    /// </summary>
    public class SurroundingsMatchResult
    {
        public List<ulong> Parents { get; set; }
        public List<ulong> Children { get; set; }
    }
}
