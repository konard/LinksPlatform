using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Uses binary search algorithm to isolate code blocks that cause interference or bugs.
    /// </summary>
    public class BinarySearchIsolator
    {
        private readonly Func<List<CodeBlock>, bool> _testPredicate;
        private readonly Action<string> _logger;

        /// <summary>
        /// Initializes a new instance of the BinarySearchIsolator.
        /// </summary>
        /// <param name="testPredicate">Function that tests if the current configuration passes (returns true) or fails (returns false).</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        public BinarySearchIsolator(Func<List<CodeBlock>, bool> testPredicate, Action<string> logger = null)
        {
            _testPredicate = testPredicate ?? throw new ArgumentNullException(nameof(testPredicate));
            _logger = logger ?? (msg => { });
        }

        /// <summary>
        /// Finds the minimal set of code blocks that cause the test to fail.
        /// Uses binary search to efficiently narrow down the interfering code.
        /// </summary>
        /// <param name="codeBlocks">List of all code blocks to analyze.</param>
        /// <returns>List of code blocks identified as causing interference.</returns>
        public List<CodeBlock> FindInterferingBlocks(List<CodeBlock> codeBlocks)
        {
            if (codeBlocks == null || codeBlocks.Count == 0)
            {
                _logger("No code blocks to analyze.");
                return new List<CodeBlock>();
            }

            _logger($"Starting binary search isolation with {codeBlocks.Count} code blocks...");

            // First, test with all blocks enabled
            EnableAll(codeBlocks);
            bool allEnabled = _testPredicate(codeBlocks);
            _logger($"All blocks enabled: Test {(allEnabled ? "PASSED" : "FAILED")}");

            // If test passes with all enabled, no interference
            if (allEnabled)
            {
                _logger("No interference detected - all blocks can coexist.");
                return new List<CodeBlock>();
            }

            // Test with all blocks disabled
            DisableAll(codeBlocks);
            bool allDisabled = _testPredicate(codeBlocks);
            _logger($"All blocks disabled: Test {(allDisabled ? "PASSED" : "FAILED")}");

            // If test fails even with all disabled, the problem is external
            if (!allDisabled)
            {
                _logger("Test fails even with all blocks disabled - problem is external to analyzed code.");
                return new List<CodeBlock>();
            }

            // Binary search to find interfering blocks
            var interferingBlocks = new List<CodeBlock>();
            var remainingBlocks = new List<CodeBlock>(codeBlocks);

            while (remainingBlocks.Count > 0)
            {
                var suspect = FindInterferingBlockBinarySearch(remainingBlocks);
                if (suspect != null)
                {
                    interferingBlocks.Add(suspect);
                    _logger($"Found interfering block: {suspect}");
                    remainingBlocks.Remove(suspect);
                }
                else
                {
                    break;
                }
            }

            return interferingBlocks;
        }

        /// <summary>
        /// Uses binary search to find a single interfering block.
        /// </summary>
        private CodeBlock FindInterferingBlockBinarySearch(List<CodeBlock> blocks)
        {
            if (blocks.Count == 0) return null;
            if (blocks.Count == 1)
            {
                // Test this single block
                DisableAll(blocks);
                blocks[0].IsEnabled = true;
                bool testResult = _testPredicate(blocks);
                return testResult ? null : blocks[0];
            }

            int midPoint = blocks.Count / 2;
            var firstHalf = blocks.Take(midPoint).ToList();
            var secondHalf = blocks.Skip(midPoint).ToList();

            _logger($"Testing binary split: {firstHalf.Count} vs {secondHalf.Count} blocks");

            // Test first half
            DisableAll(blocks);
            EnableBlocks(firstHalf);
            bool firstHalfResult = _testPredicate(blocks);
            _logger($"First half: Test {(firstHalfResult ? "PASSED" : "FAILED")}");

            // If first half fails, the interfering block is in first half
            if (!firstHalfResult)
            {
                return FindInterferingBlockBinarySearch(firstHalf);
            }

            // Test second half
            DisableAll(blocks);
            EnableBlocks(secondHalf);
            bool secondHalfResult = _testPredicate(blocks);
            _logger($"Second half: Test {(secondHalfResult ? "PASSED" : "FAILED")}");

            // If second half fails, the interfering block is in second half
            if (!secondHalfResult)
            {
                return FindInterferingBlockBinarySearch(secondHalf);
            }

            // Neither half fails alone - might be an interaction between halves
            _logger("No single block causes failure in this set - may be an interaction.");
            return null;
        }

        private void EnableAll(List<CodeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                block.IsEnabled = true;
            }
        }

        private void DisableAll(List<CodeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                block.IsEnabled = false;
            }
        }

        private void EnableBlocks(List<CodeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                block.IsEnabled = true;
            }
        }
    }
}
