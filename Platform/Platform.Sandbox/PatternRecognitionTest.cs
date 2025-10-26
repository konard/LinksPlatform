using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Sandbox
{
    /// <summary>
    /// Unit tests for pattern recognition strategy.
    /// </summary>
    public static class PatternRecognitionTest
    {
        private static int _passedTests = 0;
        private static int _failedTests = 0;

        private static void AssertTrue(bool condition, string message)
        {
            if (condition)
            {
                Console.WriteLine($"  ✓ PASS: {message}");
                _passedTests++;
            }
            else
            {
                Console.WriteLine($"  ✗ FAIL: {message}");
                _failedTests++;
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Console.WriteLine($"  ✓ PASS: {message}");
                _passedTests++;
            }
            else
            {
                Console.WriteLine($"  ✗ FAIL: {message}");
                Console.WriteLine($"    Expected: {expected}");
                Console.WriteLine($"    Actual: {actual}");
                _failedTests++;
            }
        }

        /// <summary>
        /// Test basic pattern analysis.
        /// </summary>
        public static void TestBasicPatternAnalysis()
        {
            Console.WriteLine("\n[Test] Basic Pattern Analysis");

            var contexts = new List<List<string>>
            {
                new List<string> { "a", "a", "b" },
                new List<string> { "a", "a", "c" }
            };

            var patterns = PatternRecognition.AnalyzePatterns(contexts);

            AssertTrue(patterns.ContainsKey("a"), "Pattern 'a' should exist");
            AssertTrue(patterns.ContainsKey("a a"), "Pattern 'a a' should exist");
            AssertTrue(patterns.ContainsKey("b"), "Pattern 'b' should exist");
            AssertTrue(patterns.ContainsKey("c"), "Pattern 'c' should exist");

            // Check frequencies
            AssertEqual(1.0, patterns["a a"].Frequency, "Pattern 'a a' should have frequency 1.0");
            AssertEqual(0.5, patterns["b"].Frequency, "Pattern 'b' should have frequency 0.5");
            AssertEqual(0.5, patterns["c"].Frequency, "Pattern 'c' should have frequency 0.5");
        }

        /// <summary>
        /// Test finding universal patterns (100% frequency).
        /// </summary>
        public static void TestUniversalPatterns()
        {
            Console.WriteLine("\n[Test] Universal Patterns");

            var contexts = new List<List<string>>
            {
                new List<string> { "a", "b", "b" },
                new List<string> { "c", "b", "b" }
            };

            var patterns = PatternRecognition.AnalyzePatterns(contexts);
            var universalPatterns = PatternRecognition.FindUniversalPatterns(patterns);

            var hasPatternBB = universalPatterns.Any(p => p.AsString() == "b b");
            AssertTrue(hasPatternBB, "Universal patterns should include 'b b'");

            var hasPatternB = universalPatterns.Any(p => p.AsString() == "b");
            AssertTrue(hasPatternB, "Universal patterns should include 'b'");
        }

        /// <summary>
        /// Test common pattern generation.
        /// </summary>
        public static void TestCommonPatternGeneration()
        {
            Console.WriteLine("\n[Test] Common Pattern Generation");

            var contexts1 = new List<List<string>>
            {
                new List<string> { "a", "a", "b" },
                new List<string> { "a", "a", "c" }
            };

            var patterns1 = PatternRecognition.AnalyzePatterns(contexts1);
            var commonPattern1 = PatternRecognition.GenerateCommonPattern(contexts1, patterns1);
            var expectedPattern1 = new List<string> { "a", "a", "*" };

            AssertEqual(string.Join(" ", expectedPattern1), string.Join(" ", commonPattern1),
                "Common pattern for (a a b, a a c) should be 'a a *'");

            var contexts2 = new List<List<string>>
            {
                new List<string> { "x", "b", "b" },
                new List<string> { "y", "b", "b" }
            };

            var patterns2 = PatternRecognition.AnalyzePatterns(contexts2);
            var commonPattern2 = PatternRecognition.GenerateCommonPattern(contexts2, patterns2);
            var expectedPattern2 = new List<string> { "*", "b", "b" };

            AssertEqual(string.Join(" ", expectedPattern2), string.Join(" ", commonPattern2),
                "Common pattern for (x b b, y b b) should be '* b b'");
        }

        /// <summary>
        /// Test frequency grouping.
        /// </summary>
        public static void TestFrequencyGrouping()
        {
            Console.WriteLine("\n[Test] Frequency Grouping");

            var contexts = new List<List<string>>
            {
                new List<string> { "a", "x" },
                new List<string> { "b", "x" },
                new List<string> { "c", "x" }
            };

            var patterns = PatternRecognition.AnalyzePatterns(contexts);
            var grouped = PatternRecognition.GroupByFrequency(patterns);

            // Elements a, b, c should all have frequency 1/3 ≈ 0.3333
            var freq033 = Math.Round(1.0 / 3.0, 4);
            AssertTrue(grouped.ContainsKey(freq033), "Should have frequency group for 0.3333");

            if (grouped.ContainsKey(freq033))
            {
                var count = grouped[freq033].Count(p => p.Elements.Count == 1 &&
                    (p.Elements[0] == "a" || p.Elements[0] == "b" || p.Elements[0] == "c"));
                AssertEqual(3, count, "Frequency group 0.3333 should contain a, b, and c");
            }
        }

        /// <summary>
        /// Test with identical contexts.
        /// </summary>
        public static void TestIdenticalContexts()
        {
            Console.WriteLine("\n[Test] Identical Contexts");

            var contexts = new List<List<string>>
            {
                new List<string> { "same", "same" },
                new List<string> { "same", "same" }
            };

            var patterns = PatternRecognition.AnalyzePatterns(contexts);
            var commonPattern = PatternRecognition.GenerateCommonPattern(contexts, patterns);
            var expectedPattern = new List<string> { "same", "same" };

            AssertEqual(string.Join(" ", expectedPattern), string.Join(" ", commonPattern),
                "Common pattern for identical contexts should be the same");
        }

        /// <summary>
        /// Test occurrence counting.
        /// </summary>
        public static void TestOccurrenceCounting()
        {
            Console.WriteLine("\n[Test] Occurrence Counting");

            var contexts = new List<List<string>>
            {
                new List<string> { "a", "a", "b" },
                new List<string> { "a", "a", "c" }
            };

            var patterns = PatternRecognition.AnalyzePatterns(contexts);

            // Pattern "a a" appears once per context = 2 occurrences total
            AssertEqual(2, patterns["a a"].Occurrences, "Pattern 'a a' should occur 2 times");

            // Pattern "a" appears twice in each context, but we count once per context = 2 occurrences
            AssertEqual(2, patterns["a"].Occurrences, "Pattern 'a' should occur in 2 contexts");

            // Pattern "b" appears once in only one context = 1 occurrence
            AssertEqual(1, patterns["b"].Occurrences, "Pattern 'b' should occur 1 time");
        }

        /// <summary>
        /// Runs all unit tests.
        /// </summary>
        public static void RunAll()
        {
            _passedTests = 0;
            _failedTests = 0;

            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Pattern Recognition - Unit Tests                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");

            TestBasicPatternAnalysis();
            TestUniversalPatterns();
            TestCommonPatternGeneration();
            TestFrequencyGrouping();
            TestIdenticalContexts();
            TestOccurrenceCounting();

            Console.WriteLine("\n" + new string('=', 64));
            Console.WriteLine($"Test Results: {_passedTests} passed, {_failedTests} failed");
            Console.WriteLine(new string('=', 64));

            if (_failedTests == 0)
            {
                Console.WriteLine("✓ All tests passed!");
            }
            else
            {
                Console.WriteLine($"✗ {_failedTests} test(s) failed.");
            }
        }
    }
}
