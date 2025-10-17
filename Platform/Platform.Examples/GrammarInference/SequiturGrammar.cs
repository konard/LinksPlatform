using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Examples.GrammarInference
{
    /// <summary>
    /// Represents a symbol in the grammar (terminal or non-terminal)
    /// </summary>
    public class Symbol
    {
        public ulong Value { get; set; }
        public bool IsTerminal { get; set; }
        public Symbol Next { get; set; }
        public Symbol Previous { get; set; }
        public Rule Rule { get; set; }

        public Symbol(ulong value, bool isTerminal = true)
        {
            Value = value;
            IsTerminal = isTerminal;
        }

        public void InsertAfter(Symbol symbol)
        {
            symbol.Next = Next;
            symbol.Previous = this;
            if (Next != null)
            {
                Next.Previous = symbol;
            }
            Next = symbol;
        }

        public void Remove()
        {
            if (Previous != null)
            {
                Previous.Next = Next;
            }
            if (Next != null)
            {
                Next.Previous = Previous;
            }
        }

        public Digram GetDigram()
        {
            if (Next == null) return null;
            return new Digram(this, Next);
        }
    }

    /// <summary>
    /// Represents a pair of consecutive symbols (digram)
    /// </summary>
    public class Digram : IEquatable<Digram>
    {
        public Symbol First { get; set; }
        public Symbol Second { get; set; }

        public Digram(Symbol first, Symbol second)
        {
            First = first;
            Second = second;
        }

        public bool Equals(Digram other)
        {
            if (other == null) return false;
            return First.Value == other.First.Value &&
                   Second.Value == other.Second.Value &&
                   First.IsTerminal == other.First.IsTerminal &&
                   Second.IsTerminal == other.Second.IsTerminal;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + First.Value.GetHashCode();
                hash = hash * 31 + Second.Value.GetHashCode();
                hash = hash * 31 + First.IsTerminal.GetHashCode();
                hash = hash * 31 + Second.IsTerminal.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Digram);
        }
    }

    /// <summary>
    /// Represents a production rule in the grammar
    /// </summary>
    public class Rule
    {
        public ulong RuleId { get; set; }
        public Symbol Head { get; set; }
        public int UseCount { get; set; }

        public Rule(ulong ruleId)
        {
            RuleId = ruleId;
            UseCount = 0;
            Head = new Symbol(ruleId, false) { Rule = this };
        }

        public void AddSymbol(Symbol symbol)
        {
            if (Head.Next == null)
            {
                Head.InsertAfter(symbol);
            }
            else
            {
                Symbol last = Head;
                while (last.Next != null)
                {
                    last = last.Next;
                }
                last.InsertAfter(symbol);
            }
        }

        public int Length()
        {
            int count = 0;
            Symbol current = Head.Next;
            while (current != null)
            {
                count++;
                current = current.Next;
            }
            return count;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"R{RuleId} -> ");
            Symbol current = Head.Next;
            while (current != null)
            {
                if (current.IsTerminal)
                {
                    sb.Append($"{current.Value} ");
                }
                else
                {
                    sb.Append($"R{current.Value} ");
                }
                current = current.Next;
            }
            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Implements the Sequitur algorithm for grammar inference from examples
    /// </summary>
    public class SequiturGrammar
    {
        private Rule _startRule;
        private Dictionary<Digram, Digram> _digramIndex;
        private List<Rule> _rules;
        private ulong _nextRuleId;

        public SequiturGrammar()
        {
            _startRule = new Rule(0);
            _digramIndex = new Dictionary<Digram, Digram>();
            _rules = new List<Rule> { _startRule };
            _nextRuleId = 1;
        }

        /// <summary>
        /// Learns grammar from a sequence of symbols
        /// </summary>
        public void LearnFromSequence(IEnumerable<ulong> sequence)
        {
            foreach (var symbol in sequence)
            {
                AppendSymbol(symbol);
            }
        }

        /// <summary>
        /// Appends a symbol to the grammar and maintains constraints
        /// </summary>
        private void AppendSymbol(ulong value)
        {
            var symbol = new Symbol(value, true);

            // Add to end of start rule
            Symbol last = _startRule.Head;
            while (last.Next != null)
            {
                last = last.Next;
            }
            last.InsertAfter(symbol);

            // Check digram uniqueness
            if (last != _startRule.Head)
            {
                CheckDigram(last);
            }
        }

        /// <summary>
        /// Enforces digram uniqueness constraint
        /// </summary>
        private void CheckDigram(Symbol symbol)
        {
            var digram = symbol.GetDigram();
            if (digram == null) return;

            if (_digramIndex.TryGetValue(digram, out var existingDigram))
            {
                // Digram already exists, create or reuse rule
                if (existingDigram.First.Next == existingDigram.Second)
                {
                    ReplaceDigram(symbol, existingDigram);
                }
            }
            else
            {
                // New digram, add to index
                _digramIndex[digram] = digram;
            }
        }

        /// <summary>
        /// Replaces a digram with a rule
        /// </summary>
        private void ReplaceDigram(Symbol symbol, Digram existingDigram)
        {
            Rule rule;

            // Check if existing digram is already part of a rule
            if (existingDigram.First.Previous != null &&
                !existingDigram.First.Previous.IsTerminal &&
                existingDigram.First.Previous.Rule != null)
            {
                rule = existingDigram.First.Previous.Rule;
                rule.UseCount++;
            }
            else
            {
                // Create new rule
                rule = new Rule(_nextRuleId++);
                _rules.Add(rule);

                // Add symbols to rule
                var s1 = new Symbol(existingDigram.First.Value, existingDigram.First.IsTerminal);
                var s2 = new Symbol(existingDigram.Second.Value, existingDigram.Second.IsTerminal);
                rule.AddSymbol(s1);
                rule.AddSymbol(s2);

                // Replace existing digram with rule reference
                var ruleRef = new Symbol(rule.RuleId, false) { Rule = rule };
                existingDigram.First.Previous.InsertAfter(ruleRef);
                existingDigram.First.Remove();
                existingDigram.Second.Remove();

                rule.UseCount++;
            }

            // Replace current digram with rule reference
            var newRuleRef = new Symbol(rule.RuleId, false) { Rule = rule };
            symbol.Previous.InsertAfter(newRuleRef);
            symbol.Remove();
            symbol.Next?.Remove();
            rule.UseCount++;

            // Remove unused rules (rule utility constraint)
            CleanupRules();
        }

        /// <summary>
        /// Removes rules that are only used once (enforces rule utility constraint)
        /// </summary>
        private void CleanupRules()
        {
            for (int i = _rules.Count - 1; i >= 1; i--)
            {
                var rule = _rules[i];
                if (rule.UseCount < 2)
                {
                    // Replace rule reference with its contents
                    ExpandRule(rule);
                    _rules.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Expands a rule inline (replaces rule reference with its symbols)
        /// </summary>
        private void ExpandRule(Rule rule)
        {
            // Find all references to this rule and expand them
            foreach (var r in _rules)
            {
                Symbol current = r.Head;
                while (current != null)
                {
                    if (!current.IsTerminal && current.Value == rule.RuleId)
                    {
                        // Replace rule reference with rule contents
                        Symbol ruleContent = rule.Head.Next;
                        Symbol prev = current.Previous;

                        current.Remove();

                        while (ruleContent != null)
                        {
                            var copy = new Symbol(ruleContent.Value, ruleContent.IsTerminal);
                            if (prev != null)
                            {
                                prev.InsertAfter(copy);
                                prev = copy;
                            }
                            ruleContent = ruleContent.Next;
                        }
                    }
                    current = current?.Next;
                }
            }
        }

        /// <summary>
        /// Gets all rules in the grammar
        /// </summary>
        public List<Rule> GetRules()
        {
            return _rules;
        }

        /// <summary>
        /// Gets the start rule
        /// </summary>
        public Rule GetStartRule()
        {
            return _startRule;
        }

        /// <summary>
        /// Returns a string representation of the grammar
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Grammar Rules:");
            foreach (var rule in _rules)
            {
                sb.AppendLine(rule.ToString());
            }
            return sb.ToString();
        }
    }
}
