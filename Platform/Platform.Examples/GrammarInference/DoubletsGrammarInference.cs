using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples.GrammarInference
{
    /// <summary>
    /// Implements grammar inference using LinksPlatform Doublets for storage
    /// This allows efficient storage and querying of inferred grammar rules
    /// </summary>
    public class DoubletsGrammarInference<TLinkAddress>
        where TLinkAddress : struct
    {
        private readonly ILinks<TLinkAddress> _links;
        private readonly TLinkAddress _grammarMarker;
        private readonly TLinkAddress _ruleMarker;
        private readonly TLinkAddress _sequenceMarker;
        private readonly Dictionary<(TLinkAddress, TLinkAddress), TLinkAddress> _digramToRule;
        private readonly Dictionary<TLinkAddress, int> _ruleUsageCount;

        public DoubletsGrammarInference(ILinks<TLinkAddress> links)
        {
            _links = links;
            _digramToRule = new Dictionary<(TLinkAddress, TLinkAddress), TLinkAddress>();
            _ruleUsageCount = new Dictionary<TLinkAddress, int>();

            // Create marker links for grammar structure
            _grammarMarker = CreateMarker();
            _ruleMarker = CreateMarker();
            _sequenceMarker = CreateMarker();
        }

        /// <summary>
        /// Creates a marker link for organizing grammar structure
        /// </summary>
        private TLinkAddress CreateMarker()
        {
            // Create a self-referencing link as a unique marker
            var marker = _links.Create();
            _links.Update(marker, marker, marker);
            return marker;
        }

        /// <summary>
        /// Learns grammar from a sequence of links
        /// </summary>
        public TLinkAddress LearnFromSequence(IEnumerable<TLinkAddress> sequence)
        {
            var sequenceList = sequence.ToList();
            if (sequenceList.Count == 0)
            {
                throw new ArgumentException("Sequence cannot be empty", nameof(sequence));
            }

            // Create initial sequence representation
            var current = CreateSequenceLink(sequenceList[0], _sequenceMarker);
            for (int i = 1; i < sequenceList.Count; i++)
            {
                current = CreateSequenceLink(sequenceList[i], current);

                // Check for repeated digrams and create rules
                ProcessDigram(current);
            }

            return current;
        }

        /// <summary>
        /// Creates a link representing an element in a sequence
        /// </summary>
        private TLinkAddress CreateSequenceLink(TLinkAddress element, TLinkAddress previous)
        {
            return _links.GetOrCreate(previous, element);
        }

        /// <summary>
        /// Processes a digram and creates/applies rules based on Sequitur algorithm
        /// </summary>
        private void ProcessDigram(TLinkAddress currentLink)
        {
            var source = _links.GetSource(currentLink);
            var target = _links.GetTarget(currentLink);

            // Check if this digram has been seen before
            var digram = (source, target);

            if (_digramToRule.TryGetValue(digram, out var existingRule))
            {
                // Digram exists, increment rule usage and potentially apply it
                _ruleUsageCount[existingRule]++;

                // Replace digram with rule reference
                ReplaceDigramWithRule(currentLink, existingRule);
            }
            else
            {
                // Check if any sub-digram is repeated
                if (IsRepeatedPattern(source, target))
                {
                    // Create new rule for this digram
                    var rule = CreateRule(source, target);
                    _digramToRule[digram] = rule;
                    _ruleUsageCount[rule] = 1;
                }
            }
        }

        /// <summary>
        /// Checks if a pattern (digram) is repeated in the sequence
        /// </summary>
        private bool IsRepeatedPattern(TLinkAddress source, TLinkAddress target)
        {
            // Search for another occurrence of this digram
            int count = 0;
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex).Equals(source) &&
                    _links.GetTarget(linkIndex).Equals(target))
                {
                    count++;
                    if (count > 1)
                    {
                        return _links.Constants.Break;
                    }
                }
                return _links.Constants.Continue;
            });

            return count > 1;
        }

        /// <summary>
        /// Creates a new grammar rule for a repeated digram
        /// </summary>
        private TLinkAddress CreateRule(TLinkAddress source, TLinkAddress target)
        {
            // Create a rule link: Rule -> (source, target)
            var ruleBody = _links.GetOrCreate(source, target);
            var rule = _links.GetOrCreate(_ruleMarker, ruleBody);
            return rule;
        }

        /// <summary>
        /// Replaces a digram with a rule reference
        /// </summary>
        private void ReplaceDigramWithRule(TLinkAddress digramLink, TLinkAddress rule)
        {
            // In a full implementation, this would update the sequence structure
            // to replace the digram with a reference to the rule
            // For simplicity, we create a new link referencing the rule
            var source = _links.GetSource(digramLink);

            // Create new link that uses the rule instead
            var ruleRef = _links.GetOrCreate(source, rule);

            // Note: In a complete implementation, we would also need to update
            // all references to the original link to use the new rule reference
        }

        /// <summary>
        /// Gets all inferred rules
        /// </summary>
        public List<TLinkAddress> GetRules()
        {
            var rules = new List<TLinkAddress>();
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex).Equals(_ruleMarker))
                {
                    rules.Add(linkIndex);
                }
                return _links.Constants.Continue;
            });
            return rules;
        }

        /// <summary>
        /// Gets the usage count for a rule
        /// </summary>
        public int GetRuleUsageCount(TLinkAddress rule)
        {
            return _ruleUsageCount.TryGetValue(rule, out var count) ? count : 0;
        }

        /// <summary>
        /// Cleans up rules that are used only once (enforces rule utility)
        /// </summary>
        public void CleanupUnusedRules()
        {
            var rulesToRemove = new List<TLinkAddress>();

            foreach (var kvp in _ruleUsageCount)
            {
                if (kvp.Value < 2)
                {
                    rulesToRemove.Add(kvp.Key);
                }
            }

            foreach (var rule in rulesToRemove)
            {
                // Expand the rule back to its original digram
                ExpandRule(rule);
                _ruleUsageCount.Remove(rule);

                // Remove from digram index
                var toRemove = _digramToRule.Where(kvp => kvp.Value.Equals(rule)).Select(kvp => kvp.Key).ToList();
                foreach (var key in toRemove)
                {
                    _digramToRule.Remove(key);
                }
            }
        }

        /// <summary>
        /// Expands a rule back to its original pattern
        /// </summary>
        private void ExpandRule(TLinkAddress rule)
        {
            // Get the rule body
            var ruleBody = _links.GetTarget(rule);

            // Find all references to this rule and replace them with the rule body
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetTarget(linkIndex).Equals(rule))
                {
                    // Replace rule reference with rule body
                    var source = _links.GetSource(linkIndex);
                    _links.Update(linkIndex, source, ruleBody);
                }
                return _links.Constants.Continue;
            });
        }

        /// <summary>
        /// Prints the inferred grammar in a readable format
        /// </summary>
        public void PrintGrammar()
        {
            Console.WriteLine("Inferred Grammar Rules:");
            Console.WriteLine("======================");

            var rules = GetRules();
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                var ruleBody = _links.GetTarget(rule);
                var bodySource = _links.GetSource(ruleBody);
                var bodyTarget = _links.GetTarget(ruleBody);

                var usageCount = GetRuleUsageCount(rule);
                Console.WriteLine($"Rule {i}: {rule} -> ({bodySource}, {bodyTarget}) [Used {usageCount} times]");
            }
        }
    }
}
