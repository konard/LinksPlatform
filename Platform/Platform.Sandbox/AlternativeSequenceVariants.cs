using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Sandbox
{
    /// <summary>
    /// Implementation of alternative sequence variants as described in issue #139.
    /// Explores different approaches to representing sequences using doublets (pairs).
    /// </summary>
    public class AlternativeSequenceVariants
    {
        /// <summary>
        /// Variant 1: Classic Sequences Map
        /// Represents sequences as START → element → element → ... → STOP
        /// This provides automatic maximum compression when inserting any pair of a sequence.
        /// </summary>
        public class SequencesMap<T>
        {
            private readonly T _startMarker;
            private readonly T _stopMarker;
            private readonly Dictionary<(T, T), T> _pairs = new Dictionary<(T, T), T>();
            private long _nextId = 0;

            public SequencesMap(T startMarker, T stopMarker)
            {
                _startMarker = startMarker;
                _stopMarker = stopMarker;
            }

            /// <summary>
            /// Creates a sequence using the classic START→elements→STOP pattern.
            /// Automatically achieves compression by reusing existing pairs.
            /// </summary>
            public List<(T, T)> CreateSequence(IEnumerable<T> elements)
            {
                var pairs = new List<(T, T)>();
                var elemList = new List<T> { _startMarker };
                elemList.AddRange(elements);
                elemList.Add(_stopMarker);

                for (int i = 0; i < elemList.Count - 1; i++)
                {
                    pairs.Add((elemList[i], elemList[i + 1]));
                }

                return pairs;
            }

            /// <summary>
            /// Reconstructs the original sequence from pairs.
            /// </summary>
            public IEnumerable<T> ReadSequence(List<(T, T)> pairs)
            {
                if (pairs.Count == 0)
                    return Enumerable.Empty<T>();

                var result = new List<T>();
                foreach (var pair in pairs)
                {
                    if (!EqualityComparer<T>.Default.Equals(pair.Item1, _startMarker))
                        result.Add(pair.Item1);
                    if (!EqualityComparer<T>.Default.Equals(pair.Item2, _stopMarker))
                        result.Add(pair.Item2);
                }

                return result.Distinct().ToList();
            }
        }

        /// <summary>
        /// Variant 2: Insertion-Based Sequences
        /// Instead of marking direct succession, a link indicates what needs to be inserted
        /// between the first and last elements. For example, for "amma":
        /// - The green link doesn't denote sequence (amma)
        /// - Instead it says: to get the sequence, insert "am" between "m" and "a"
        /// This creates a list structure rather than a tree.
        /// Finding a sequence by its first and last letter becomes extremely simple.
        /// </summary>
        public class InsertionSequences<T>
        {
            // Structure: (first_element, last_element) → elements_to_insert_between
            private readonly Dictionary<(T, T), List<T>> _insertionMap = new Dictionary<(T, T), List<T>>();

            /// <summary>
            /// Creates an insertion-based sequence representation.
            /// The link specifies what to insert between first and last elements.
            /// </summary>
            public void CreateSequence(IEnumerable<T> elements)
            {
                var elemList = elements.ToList();
                if (elemList.Count < 2)
                    return;

                var first = elemList[0];
                var last = elemList[elemList.Count - 1];
                var middle = elemList.Skip(1).Take(elemList.Count - 2).ToList();

                _insertionMap[(first, last)] = middle;
            }

            /// <summary>
            /// Finds a sequence by its first and last elements.
            /// This is the key advantage of this approach - O(1) lookup by endpoints.
            /// </summary>
            public IEnumerable<T> FindByEndpoints(T first, T last)
            {
                if (!_insertionMap.TryGetValue((first, last), out var middle))
                    return Enumerable.Empty<T>();

                var result = new List<T> { first };
                result.AddRange(middle);
                result.Add(last);
                return result;
            }

            /// <summary>
            /// Gets all sequences stored in this structure.
            /// </summary>
            public IEnumerable<IEnumerable<T>> GetAllSequences()
            {
                foreach (var kvp in _insertionMap)
                {
                    var result = new List<T> { kvp.Key.Item1 };
                    result.AddRange(kvp.Value);
                    result.Add(kvp.Key.Item2);
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Variant 3: Hybrid with Duplicate Alphabets
        /// Uses the same symbol in 3 global data structures simultaneously.
        /// Each structure has a duplicating alphabet to avoid semantic conflicts.
        /// This allows creating hybrids of different structures.
        /// Note: The "map" is problematic to reference - it's either global or not a "map".
        /// </summary>
        public class HybridSequences<T>
        {
            public enum AlphabetType
            {
                SequencesMap,      // For classic sequences
                InsertSequences,    // For insertion-based sequences
                TreeSequences       // For tree-based sequences
            }

            private class AlphabetElement
            {
                public T OriginalValue { get; set; }
                public AlphabetType Type { get; set; }
                public int InstanceId { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj is AlphabetElement other)
                    {
                        return EqualityComparer<T>.Default.Equals(OriginalValue, other.OriginalValue)
                               && Type == other.Type
                               && InstanceId == other.InstanceId;
                    }
                    return false;
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(OriginalValue, Type, InstanceId);
                }
            }

            private readonly SequencesMap<AlphabetElement> _sequencesMap;
            private readonly InsertionSequences<AlphabetElement> _insertSequences;
            private readonly Dictionary<AlphabetElement, List<AlphabetElement>> _treeSequences;
            private int _instanceCounter = 0;

            public HybridSequences(T startMarker, T stopMarker)
            {
                var start = CreateElement(startMarker, AlphabetType.SequencesMap);
                var stop = CreateElement(stopMarker, AlphabetType.SequencesMap);
                _sequencesMap = new SequencesMap<AlphabetElement>(start, stop);
                _insertSequences = new InsertionSequences<AlphabetElement>();
                _treeSequences = new Dictionary<AlphabetElement, List<AlphabetElement>>();
            }

            private AlphabetElement CreateElement(T value, AlphabetType type)
            {
                return new AlphabetElement
                {
                    OriginalValue = value,
                    Type = type,
                    InstanceId = _instanceCounter++
                };
            }

            /// <summary>
            /// Creates a sequence in a specific alphabet/structure type.
            /// </summary>
            public void CreateSequence(IEnumerable<T> elements, AlphabetType type)
            {
                var wrappedElements = elements.Select(e => CreateElement(e, type)).ToList();

                switch (type)
                {
                    case AlphabetType.SequencesMap:
                        _sequencesMap.CreateSequence(wrappedElements);
                        break;
                    case AlphabetType.InsertSequences:
                        _insertSequences.CreateSequence(wrappedElements);
                        break;
                    case AlphabetType.TreeSequences:
                        // Tree structure: parent → children
                        if (wrappedElements.Count > 0)
                        {
                            _treeSequences[wrappedElements[0]] = wrappedElements.Skip(1).ToList();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Variant 4: Doubly-Linked List Sequences
        /// Implements sequences through element assignment for doubly-linked list structure.
        /// Each element has prev and next pointers.
        /// </summary>
        public class DoublyLinkedSequences<T>
        {
            public class Node
            {
                public T Value { get; set; }
                public Node Prev { get; set; }
                public Node Next { get; set; }

                public override string ToString()
                {
                    return $"Node({Value})";
                }
            }

            private readonly Dictionary<T, Node> _nodes = new Dictionary<T, Node>();

            /// <summary>
            /// Creates a doubly-linked sequence from elements.
            /// Each node maintains prev and next references.
            /// </summary>
            public Node CreateSequence(IEnumerable<T> elements)
            {
                var elemList = elements.ToList();
                if (elemList.Count == 0)
                    return null;

                Node first = null;
                Node prev = null;

                foreach (var elem in elemList)
                {
                    var node = new Node { Value = elem };
                    _nodes[elem] = node;

                    if (first == null)
                        first = node;

                    if (prev != null)
                    {
                        prev.Next = node;
                        node.Prev = prev;
                    }

                    prev = node;
                }

                return first;
            }

            /// <summary>
            /// Traverses the sequence forward from a given node.
            /// </summary>
            public IEnumerable<T> TraverseForward(Node start)
            {
                var current = start;
                while (current != null)
                {
                    yield return current.Value;
                    current = current.Next;
                }
            }

            /// <summary>
            /// Traverses the sequence backward from a given node.
            /// </summary>
            public IEnumerable<T> TraverseBackward(Node start)
            {
                var current = start;
                while (current != null)
                {
                    yield return current.Value;
                    current = current.Prev;
                }
            }

            /// <summary>
            /// Finds a node by its value.
            /// </summary>
            public Node FindNode(T value)
            {
                return _nodes.TryGetValue(value, out var node) ? node : null;
            }
        }

        /// <summary>
        /// Example usage and comparison of all four variants.
        /// </summary>
        public static void DemonstrateVariants()
        {
            Console.WriteLine("=== Alternative Sequence Variants Demo ===\n");

            var testSequence = new[] { 'm', 'a', 'm', 'a' };

            // Variant 1: Sequences Map
            Console.WriteLine("1. Sequences Map (START → elements → STOP):");
            var seqMap = new SequencesMap<char>('⊳', '⊲');
            var pairs = seqMap.CreateSequence(testSequence);
            Console.WriteLine($"   Input: {string.Join("", testSequence)}");
            Console.WriteLine($"   Pairs: {string.Join(", ", pairs.Select(p => $"({p.Item1}→{p.Item2})"))}");
            Console.WriteLine($"   Advantages: Automatic compression, reuses existing pairs\n");

            // Variant 2: Insertion Sequences
            Console.WriteLine("2. Insertion Sequences (first, last) → middle:");
            var insertSeq = new InsertionSequences<char>();
            insertSeq.CreateSequence(testSequence);
            var foundSeq = insertSeq.FindByEndpoints('m', 'a');
            Console.WriteLine($"   Input: {string.Join("", testSequence)}");
            Console.WriteLine($"   Storage: (m, a) → [a, m]");
            Console.WriteLine($"   Found by endpoints (m, a): {string.Join("", foundSeq)}");
            Console.WriteLine($"   Advantages: O(1) lookup by first and last elements\n");

            // Variant 3: Hybrid with Duplicate Alphabets
            Console.WriteLine("3. Hybrid Sequences (3 separate alphabets):");
            var hybrid = new HybridSequences<char>('⊳', '⊲');
            hybrid.CreateSequence(testSequence, HybridSequences<char>.AlphabetType.SequencesMap);
            hybrid.CreateSequence(testSequence, HybridSequences<char>.AlphabetType.InsertSequences);
            Console.WriteLine($"   Input: {string.Join("", testSequence)}");
            Console.WriteLine($"   Stored in 3 separate alphabets simultaneously");
            Console.WriteLine($"   Advantages: Avoids semantic conflicts, enables hybrids\n");

            // Variant 4: Doubly-Linked List
            Console.WriteLine("4. Doubly-Linked List (prev ↔ next):");
            var dll = new DoublyLinkedSequences<char>();
            var head = dll.CreateSequence(testSequence);
            var forward = dll.TraverseForward(head);
            Console.WriteLine($"   Input: {string.Join("", testSequence)}");
            Console.WriteLine($"   Forward: {string.Join(" → ", forward)}");
            var lastNode = dll.FindNode('a');
            var backward = dll.TraverseBackward(lastNode);
            Console.WriteLine($"   Backward from 'a': {string.Join(" ← ", backward)}");
            Console.WriteLine($"   Advantages: Bidirectional traversal, simple structure\n");

            Console.WriteLine("=== Comparison ===");
            Console.WriteLine("• Sequences Map: Best for compression and standard operations");
            Console.WriteLine("• Insertion Sequences: Best for endpoint-based queries");
            Console.WriteLine("• Hybrid: Best for complex multi-structure scenarios");
            Console.WriteLine("• Doubly-Linked: Best for bidirectional navigation");
        }
    }
}
