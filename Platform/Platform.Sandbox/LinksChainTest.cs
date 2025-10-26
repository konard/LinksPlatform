using System;
using System.IO;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Examples;

namespace Platform.Sandbox
{
    /// <summary>
    /// Tests for LinksChain functionality in the Sandbox environment.
    /// </summary>
    public static class LinksChainTest
    {
        public static void Test()
        {
            const string filename = "test_linkschain.db";

            try
            {
                // Clean up
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                Console.WriteLine("Starting LinksChain Test...\n");

                using (var memoryAdapter = new UInt64UnitedMemoryLinks(filename, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);

                    TestBasicChainOperations(syncLinks);
                    TestChainNavigation(syncLinks);
                    TestChainValidation(syncLinks);
                    TestSequenceCreation(syncLinks);

                    Console.WriteLine($"\nTotal links created: {syncLinks.Count()}");
                }

                // Clean up
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                Console.WriteLine("\nAll LinksChain tests passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nTest failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void TestBasicChainOperations(ILinks<ulong> links)
        {
            Console.WriteLine("=== Test: Basic Chain Operations ===");

            var chain = new LinksChain<ulong>(links);

            // Create data
            var data1 = links.CreatePoint();
            var data2 = links.CreatePoint();
            var data3 = links.CreatePoint();

            // Create chain
            var head = chain.CreateChain(data1);
            Assert(head != 0, "Chain head should be created");

            // Append elements
            var elem2 = chain.Append(head, data2);
            Assert(elem2 != 0, "Second element should be created");

            var elem3 = chain.Append(elem2, data3);
            Assert(elem3 != 0, "Third element should be created");

            // Verify data retrieval
            Assert(chain.GetData(head) == data1, "First element should contain data1");
            Assert(chain.GetData(elem2) == data2, "Second element should contain data2");
            Assert(chain.GetData(elem3) == data3, "Third element should contain data3");

            Console.WriteLine("✓ Basic chain operations test passed\n");
        }

        private static void TestChainNavigation(ILinks<ulong> links)
        {
            Console.WriteLine("=== Test: Chain Navigation ===");

            var chain = new LinksChain<ulong>(links);

            // Create a chain
            var data = new ulong[5];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = links.CreatePoint();
            }

            var head = chain.CreateChainFromSequence(data);

            // Test forward navigation
            var current = head;
            for (int i = 0; i < data.Length; i++)
            {
                Assert(current != 0, $"Element {i} should exist");
                Assert(chain.GetData(current) == data[i], $"Element {i} should have correct data");

                if (i < data.Length - 1)
                {
                    current = chain.GetNext(current);
                }
            }

            // Test backward navigation
            var last = chain.GetLastElement(head);
            current = last;
            for (int i = data.Length - 1; i >= 0; i--)
            {
                Assert(current != 0, $"Element {i} should exist in reverse");
                Assert(chain.GetData(current) == data[i], $"Element {i} should have correct data in reverse");

                if (i > 0)
                {
                    current = chain.GetPrevious(current);
                }
            }

            // Verify we're back at the head
            Assert(current == head, "Should navigate back to head");

            Console.WriteLine("✓ Chain navigation test passed\n");
        }

        private static void TestChainValidation(ILinks<ulong> links)
        {
            Console.WriteLine("=== Test: Chain Validation ===");

            var chain = new LinksChain<ulong>(links);

            // Create a valid chain
            var data1 = links.CreatePoint();
            var data2 = links.CreatePoint();
            var data3 = links.CreatePoint();

            var head = chain.CreateChain(data1);
            chain.Append(head, data2);
            chain.Append(head, data3);

            // Validate
            var isValid = chain.ValidateChain(head);
            Assert(isValid, "Chain should be valid");

            // Test chain length
            var length = chain.GetChainLength(head);
            Assert(length == 3, $"Chain length should be 3, but was {length}");

            Console.WriteLine("✓ Chain validation test passed\n");
        }

        private static void TestSequenceCreation(ILinks<ulong> links)
        {
            Console.WriteLine("=== Test: Sequence Creation ===");

            var chain = new LinksChain<ulong>(links);

            // Create sequence
            var sequence = new ulong[10];
            for (int i = 0; i < sequence.Length; i++)
            {
                sequence[i] = links.CreatePoint();
            }

            var head = chain.CreateChainFromSequence(sequence);
            Assert(head != 0, "Chain head should be created");

            // Verify length
            var length = chain.GetChainLength(head);
            Assert(length == sequence.Length, $"Chain length should be {sequence.Length}, but was {length}");

            // Verify all elements
            int index = 0;
            foreach (var element in chain.GetAllElements(head))
            {
                var elementData = chain.GetData(element);
                Assert(elementData == sequence[index], $"Element {index} should have correct data");
                index++;
            }

            Assert(index == sequence.Length, "Should iterate through all elements");

            Console.WriteLine("✓ Sequence creation test passed\n");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Assertion failed: {message}");
            }
        }
    }
}
