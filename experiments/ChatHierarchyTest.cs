using System;
using System.Collections.Generic;

// Standalone test script for Chat Message Hierarchy Inference
// This can be compiled and run with: csc ChatHierarchyTest.cs && ./ChatHierarchyTest.exe

namespace ChatHierarchyTest
{
    public class ChatMessageNode
    {
        public int MessageId { get; set; }
        public int Position { get; set; }
        public int Level { get; set; }
        public ChatMessageNode Parent { get; set; }
        public List<ChatMessageNode> Children { get; set; }

        public ChatMessageNode()
        {
            Children = new List<ChatMessageNode>();
        }

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

    public static class ChatMessageHierarchyInference
    {
        public static List<ChatMessageNode> InferHierarchy(int[] flatMessageIds)
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
                    currentNode.Level = 0;
                    currentNode.Parent = null;
                    roots.Add(currentNode);
                }
                else
                {
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
                        currentNode.Parent = parentNode;
                        currentNode.Level = parentNode.Level + 1;
                        parentNode.Children.Add(currentNode);
                    }
                    else
                    {
                        currentNode.Level = 0;
                        currentNode.Parent = null;
                        roots.Add(currentNode);
                    }
                }
            }

            return roots;
        }

        public static void PrintHierarchy(List<ChatMessageNode> roots)
        {
            Console.WriteLine("=== Hierarchical Structure ===");
            foreach (var root in roots)
            {
                root.PrintHierarchy();
            }
            Console.WriteLine();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Chat Message Hierarchy Inference Test ===");
            Console.WriteLine();

            // Test case from issue #633: [1, 2, 1, 1]
            Console.WriteLine("Test: [1, 2, 1, 1] (from issue #633 diagram)");
            var test1 = new int[] { 1, 2, 1, 1 };
            var result1 = ChatMessageHierarchyInference.InferHierarchy(test1);
            ChatMessageHierarchyInference.PrintHierarchy(result1);

            Console.WriteLine("Expected structure:");
            Console.WriteLine("  [0] MessageId: 1 (root)");
            Console.WriteLine("    [1] MessageId: 2 (reply to 1)");
            Console.WriteLine("      [2] MessageId: 1 (reply to 2)");
            Console.WriteLine("        [3] MessageId: 1 (reply to previous 1)");
            Console.WriteLine();

            // Verify the structure
            if (result1.Count == 1 &&
                result1[0].MessageId == 1 &&
                result1[0].Children.Count == 1 &&
                result1[0].Children[0].MessageId == 2 &&
                result1[0].Children[0].Children.Count == 1 &&
                result1[0].Children[0].Children[0].MessageId == 1 &&
                result1[0].Children[0].Children[0].Children.Count == 1 &&
                result1[0].Children[0].Children[0].Children[0].MessageId == 1)
            {
                Console.WriteLine("✓ Test PASSED: Structure matches expected hierarchy from issue #633");
            }
            else
            {
                Console.WriteLine("✗ Test FAILED: Structure does not match expected hierarchy");
            }

            Console.WriteLine();
            Console.WriteLine("=== Test Complete ===");
        }
    }
}
