using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Sandbox
{
    /// <summary>
    /// Implements an algorithm to infer hierarchical chat message structure from a flat list of message IDs.
    /// The algorithm assumes that when a message ID repeats, it indicates a reply to the most recent message
    /// with a different ID, creating a threaded conversation structure.
    /// </summary>
    public static class ChatMessageHierarchyInference
    {
        /// <summary>
        /// Represents a chat message node in the hierarchical structure.
        /// </summary>
        public class ChatMessageNode
        {
            /// <summary>
            /// The message identifier (e.g., user ID or message type).
            /// </summary>
            public int MessageId { get; set; }

            /// <summary>
            /// The position of this message in the original flat list (0-based index).
            /// </summary>
            public int Position { get; set; }

            /// <summary>
            /// The depth level in the hierarchy (0 for root messages).
            /// </summary>
            public int Level { get; set; }

            /// <summary>
            /// Parent message node (null for root messages).
            /// </summary>
            public ChatMessageNode Parent { get; set; }

            /// <summary>
            /// List of child message nodes (replies to this message).
            /// </summary>
            public List<ChatMessageNode> Children { get; set; }

            public ChatMessageNode()
            {
                Children = new List<ChatMessageNode>();
            }

            public override string ToString()
            {
                return $"MessageId={MessageId}, Position={Position}, Level={Level}, Children={Children.Count}";
            }

            /// <summary>
            /// Recursively prints the hierarchical structure with indentation.
            /// </summary>
            public void PrintHierarchy(int indent = 0)
            {
                var indentStr = new string(' ', indent * 2);
                Console.WriteLine($"{indentStr}[{Position}] MessageId: {MessageId} (Level: {Level})");
                foreach (var child in Children)
                {
                    child.PrintHierarchy(indent + 1);
                }
            }
        }

        /// <summary>
        /// Infers hierarchical structure from a flat list of message IDs.
        ///
        /// Algorithm:
        /// 1. The first message is always a root message.
        /// 2. For each subsequent message:
        ///    - If the message ID differs from the previous message ID, it's a reply to the previous message.
        ///    - If the message ID is the same as the previous message ID, it continues at the same level as a sibling.
        ///    - The algorithm builds a tree structure based on these rules.
        /// </summary>
        /// <param name="flatMessageIds">Flat list of message IDs in chronological order.</param>
        /// <returns>List of root message nodes representing the hierarchical structure.</returns>
        public static List<ChatMessageNode> InferHierarchy(int[] flatMessageIds)
        {
            if (flatMessageIds == null || flatMessageIds.Length == 0)
            {
                return new List<ChatMessageNode>();
            }

            var roots = new List<ChatMessageNode>();
            ChatMessageNode previousNode = null;

            for (int i = 0; i < flatMessageIds.Length; i++)
            {
                int currentMessageId = flatMessageIds[i];
                var currentNode = new ChatMessageNode
                {
                    MessageId = currentMessageId,
                    Position = i
                };

                if (i == 0)
                {
                    // First message is a root
                    currentNode.Level = 0;
                    currentNode.Parent = null;
                    roots.Add(currentNode);
                }
                else
                {
                    if (currentMessageId == previousNode.MessageId)
                    {
                        // Same message ID as previous: sibling at the same level
                        currentNode.Parent = previousNode.Parent;
                        currentNode.Level = previousNode.Level;

                        if (currentNode.Parent != null)
                        {
                            currentNode.Parent.Children.Add(currentNode);
                        }
                        else
                        {
                            roots.Add(currentNode);
                        }
                    }
                    else
                    {
                        // Different message ID: reply to previous message
                        currentNode.Parent = previousNode;
                        currentNode.Level = previousNode.Level + 1;
                        previousNode.Children.Add(currentNode);
                    }
                }

                previousNode = currentNode;
            }

            return roots;
        }

        /// <summary>
        /// Sequential chain algorithm (matches issue #633 diagram):
        /// Each message is simply a reply to the immediately previous message,
        /// creating a linear conversation thread regardless of message IDs.
        /// </summary>
        public static List<ChatMessageNode> InferHierarchySequential(int[] flatMessageIds)
        {
            if (flatMessageIds == null || flatMessageIds.Length == 0)
            {
                return new List<ChatMessageNode>();
            }

            var roots = new List<ChatMessageNode>();
            ChatMessageNode previousNode = null;

            for (int i = 0; i < flatMessageIds.Length; i++)
            {
                int currentMessageId = flatMessageIds[i];
                var currentNode = new ChatMessageNode
                {
                    MessageId = currentMessageId,
                    Position = i
                };

                if (i == 0)
                {
                    // First message is always a root
                    currentNode.Level = 0;
                    currentNode.Parent = null;
                    roots.Add(currentNode);
                }
                else
                {
                    // Each message is a reply to the immediately previous message
                    currentNode.Parent = previousNode;
                    currentNode.Level = previousNode.Level + 1;
                    previousNode.Children.Add(currentNode);
                }

                previousNode = currentNode;
            }

            return roots;
        }

        /// <summary>
        /// Alternative algorithm that matches the diagram more closely:
        /// Messages with the same ID form reply chains where each occurrence
        /// is a reply to the most recent message with a different ID.
        /// </summary>
        public static List<ChatMessageNode> InferHierarchyAlternative(int[] flatMessageIds)
        {
            if (flatMessageIds == null || flatMessageIds.Length == 0)
            {
                return new List<ChatMessageNode>();
            }

            var roots = new List<ChatMessageNode>();
            var allNodes = new List<ChatMessageNode>();

            for (int i = 0; i < flatMessageIds.Length; i++)
            {
                int currentMessageId = flatMessageIds[i];
                var currentNode = new ChatMessageNode
                {
                    MessageId = currentMessageId,
                    Position = i
                };

                allNodes.Add(currentNode);

                if (i == 0)
                {
                    // First message is always a root
                    currentNode.Level = 0;
                    currentNode.Parent = null;
                    roots.Add(currentNode);
                }
                else
                {
                    // Find the most recent message with a different ID
                    ChatMessageNode parentNode = null;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (allNodes[j].MessageId != currentMessageId)
                        {
                            parentNode = allNodes[j];
                            break;
                        }
                    }

                    if (parentNode != null)
                    {
                        // This is a reply to the most recent different message
                        currentNode.Parent = parentNode;
                        currentNode.Level = parentNode.Level + 1;
                        parentNode.Children.Add(currentNode);
                    }
                    else
                    {
                        // All previous messages have the same ID, so this is a root sibling
                        currentNode.Level = 0;
                        currentNode.Parent = null;
                        roots.Add(currentNode);
                    }
                }
            }

            return roots;
        }

        /// <summary>
        /// Prints the entire hierarchy starting from the root nodes.
        /// </summary>
        public static void PrintHierarchy(List<ChatMessageNode> roots)
        {
            Console.WriteLine("=== Hierarchical Structure ===");
            foreach (var root in roots)
            {
                root.PrintHierarchy();
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Converts the hierarchical structure to a flat indented string representation.
        /// </summary>
        public static string ToIndentedString(List<ChatMessageNode> roots)
        {
            var lines = new List<string>();

            void AddNode(ChatMessageNode node, int indent)
            {
                var indentStr = new string(' ', indent * 2);
                lines.Add($"{indentStr}[{node.Position}] MessageId: {node.MessageId}");
                foreach (var child in node.Children)
                {
                    AddNode(child, indent + 1);
                }
            }

            foreach (var root in roots)
            {
                AddNode(root, 0);
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Runs demonstration experiments for the chat message hierarchy inference algorithm.
        /// </summary>
        public static void RunExperiment()
        {
            Console.WriteLine("=== Chat Message Hierarchy Inference Experiment ===");
            Console.WriteLine();

            // Example from the issue diagram: [1, 2, 1, 1]
            Console.WriteLine("Example 1 (from issue #633 diagram): [1, 2, 1, 1]");
            Console.WriteLine("Expected hierarchy:");
            Console.WriteLine("  1 (root)");
            Console.WriteLine("    -> 2 (reply to first 1)");
            Console.WriteLine("       -> 1 (reply to 2)");
            Console.WriteLine("          -> 1 (reply to previous 1)");
            Console.WriteLine();

            var example1 = new int[] { 1, 2, 1, 1 };

            Console.WriteLine("--- Using Sequential Algorithm (Recommended) ---");
            var hierarchy1Sequential = InferHierarchySequential(example1);
            PrintHierarchy(hierarchy1Sequential);

            Console.WriteLine("--- Using Primary Algorithm ---");
            var hierarchy1 = InferHierarchy(example1);
            PrintHierarchy(hierarchy1);

            Console.WriteLine("--- Using Alternative Algorithm ---");
            var hierarchy1Alt = InferHierarchyAlternative(example1);
            PrintHierarchy(hierarchy1Alt);

            // Additional test cases
            Console.WriteLine("Example 2: [1, 1, 1]");
            Console.WriteLine("Expected: Three root siblings with same ID");
            Console.WriteLine();
            var example2 = new int[] { 1, 1, 1 };
            var hierarchy2 = InferHierarchyAlternative(example2);
            PrintHierarchy(hierarchy2);

            Console.WriteLine("Example 3: [1, 2, 3, 2, 1]");
            Console.WriteLine("Expected: Nested replies and back-references");
            Console.WriteLine();
            var example3 = new int[] { 1, 2, 3, 2, 1 };
            var hierarchy3 = InferHierarchyAlternative(example3);
            PrintHierarchy(hierarchy3);

            Console.WriteLine("Example 4: [1, 2, 1, 3, 1]");
            Console.WriteLine("Expected: Complex threading pattern");
            Console.WriteLine();
            var example4 = new int[] { 1, 2, 1, 3, 1 };
            var hierarchy4 = InferHierarchyAlternative(example4);
            PrintHierarchy(hierarchy4);

            Console.WriteLine("Example 5: Empty array");
            var example5 = new int[] { };
            var hierarchy5 = InferHierarchyAlternative(example5);
            Console.WriteLine($"Result: {hierarchy5.Count} root nodes");
            Console.WriteLine();

            Console.WriteLine("Example 6: Single message [5]");
            var example6 = new int[] { 5 };
            var hierarchy6 = InferHierarchyAlternative(example6);
            PrintHierarchy(hierarchy6);

            Console.WriteLine("=== Experiment Complete ===");
        }
    }
}
