using System;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates the LinksChain functionality with practical examples.
    /// </summary>
    public static class LinksChainExperiment
    {
        /// <summary>
        /// Runs a basic example of creating and traversing a LinksChain.
        /// </summary>
        /// <typeparam name="TLink">The type of link identifier.</typeparam>
        /// <param name="links">The links storage.</param>
        public static void RunBasicExample<TLink>(ILinks<TLink> links)
        {
            Console.WriteLine("=== Basic LinksChain Example ===\n");

            var chain = new LinksChain<TLink>(links);

            // Create some data points
            var data1 = links.CreatePoint();
            var data2 = links.CreatePoint();
            var data3 = links.CreatePoint();

            Console.WriteLine($"Created data points: {data1}, {data2}, {data3}");

            // Create a chain with the first element
            var chainHead = chain.CreateChain(data1);
            Console.WriteLine($"Chain created with head: {chainHead}");

            // Append more elements
            var element2 = chain.Append(chainHead, data2);
            Console.WriteLine($"Appended element 2: {element2}");

            var element3 = chain.Append(element2, data3);
            Console.WriteLine($"Appended element 3: {element3}");

            // Traverse the chain
            Console.WriteLine("\n--- Traversing chain forward ---");
            foreach (var element in chain.GetAllElements(chainHead))
            {
                var data = chain.GetData(element);
                Console.WriteLine($"Element: {element}, Data: {data}");
            }

            // Validate the chain
            var isValid = chain.ValidateChain(chainHead);
            Console.WriteLine($"\nChain is valid: {isValid}");

            // Get chain length
            var length = chain.GetChainLength(chainHead);
            Console.WriteLine($"Chain length: {length}");

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates creating a chain from a sequence of data.
        /// </summary>
        /// <typeparam name="TLink">The type of link identifier.</typeparam>
        /// <param name="links">The links storage.</param>
        public static void RunSequenceExample<TLink>(ILinks<TLink> links)
        {
            Console.WriteLine("=== LinksChain from Sequence Example ===\n");

            var chain = new LinksChain<TLink>(links);

            // Create a sequence of data points
            var dataSequence = new TLink[5];
            for (int i = 0; i < dataSequence.Length; i++)
            {
                dataSequence[i] = links.CreatePoint();
            }

            Console.WriteLine($"Created {dataSequence.Length} data points");

            // Create chain from sequence
            var chainHead = chain.CreateChainFromSequence(dataSequence);
            Console.WriteLine($"Chain created from sequence, head: {chainHead}");

            // Display the chain
            Console.WriteLine("\n--- Chain elements ---");
            int index = 0;
            foreach (var element in chain.GetAllElements(chainHead))
            {
                var data = chain.GetData(element);
                Console.WriteLine($"[{index}] Element: {element}, Data: {data}");
                index++;
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates blockchain-like immutable chain properties.
        /// </summary>
        /// <typeparam name="TLink">The type of link identifier.</typeparam>
        /// <param name="links">The links storage.</param>
        public static void RunBlockchainExample<TLink>(ILinks<TLink> links)
        {
            Console.WriteLine("=== Blockchain-like LinksChain Example ===\n");

            var chain = new LinksChain<TLink>(links);

            // Simulate blockchain blocks
            Console.WriteLine("Creating blockchain-like structure...");

            // Block 1: Genesis block
            var genesisData = links.CreatePoint();
            var block1 = chain.CreateChain(genesisData);
            Console.WriteLine($"Block 1 (Genesis): {block1}, Data: {genesisData}");

            // Block 2: References block 1
            var block2Data = links.GetOrCreate(genesisData, block1); // Data includes reference to previous block
            var block2 = chain.Append(block1, block2Data);
            Console.WriteLine($"Block 2: {block2}, Data: {block2Data}");

            // Block 3: References block 2
            var block3Data = links.GetOrCreate(block2Data, block2);
            var block3 = chain.Append(block2, block3Data);
            Console.WriteLine($"Block 3: {block3}, Data: {block3Data}");

            // Verify chain integrity
            Console.WriteLine("\n--- Verifying blockchain integrity ---");
            var isValid = chain.ValidateChain(block1);
            Console.WriteLine($"Blockchain is valid: {isValid}");

            // Traverse from any point
            Console.WriteLine("\n--- Traversing from middle (block 2) ---");
            var first = chain.GetFirstElement(block2);
            Console.WriteLine($"First block from block2: {first}");

            var last = chain.GetLastElement(block2);
            Console.WriteLine($"Last block from block2: {last}");

            // Show full chain
            Console.WriteLine("\n--- Full blockchain ---");
            int blockNum = 1;
            foreach (var block in chain.GetAllElements(block1))
            {
                var data = chain.GetData(block);
                var prev = chain.GetPrevious(block);
                var next = chain.GetNext(block);
                Console.WriteLine($"Block {blockNum}: {block}");
                Console.WriteLine($"  Data: {data}");
                Console.WriteLine($"  Prev: {prev}");
                Console.WriteLine($"  Next: {next}");
                blockNum++;
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Runs all LinksChain examples.
        /// </summary>
        /// <typeparam name="TLink">The type of link identifier.</typeparam>
        /// <param name="links">The links storage.</param>
        public static void RunAllExamples<TLink>(ILinks<TLink> links)
        {
            RunBasicExample(links);
            RunSequenceExample(links);
            RunBlockchainExample(links);
        }
    }
}
