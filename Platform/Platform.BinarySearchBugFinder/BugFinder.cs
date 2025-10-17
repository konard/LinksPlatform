using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Platform.BinarySearchBugFinder
{
    /// <summary>
    /// Implements binary search algorithm to locate bugs in source code
    /// by iteratively commenting out code sections and testing
    /// </summary>
    public class BugFinder
    {
        private readonly CodeCommenter _commenter;
        private readonly ITestExecutor _testExecutor;
        private readonly string _sourceFilePath;
        private readonly string _originalCode;
        private int _iterationCount;

        public BugFinder(string sourceFilePath, ITestExecutor testExecutor)
        {
            _sourceFilePath = sourceFilePath ?? throw new ArgumentNullException(nameof(sourceFilePath));
            _testExecutor = testExecutor ?? throw new ArgumentNullException(nameof(testExecutor));
            _commenter = new CodeCommenter();

            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException($"Source file not found: {sourceFilePath}");

            _originalCode = File.ReadAllText(sourceFilePath);
            _iterationCount = 0;
        }

        /// <summary>
        /// Runs the binary search algorithm to find the problematic code section
        /// </summary>
        /// <returns>Result containing the line numbers of problematic code</returns>
        public BugFinderResult FindBug()
        {
            Console.WriteLine($"Starting binary search bug finder on: {_sourceFilePath}");
            Console.WriteLine("Testing original code...");

            // First, verify that the bug exists in the original code
            var originalTestResult = _testExecutor.RunTests();
            if (originalTestResult.Success)
            {
                return new BugFinderResult
                {
                    Success = false,
                    Message = "No bug detected in original code. Tests pass successfully."
                };
            }

            Console.WriteLine($"Bug confirmed: {originalTestResult.ErrorMessage}");

            var statements = _commenter.GetCommentableStatements(_originalCode);
            if (statements.Count == 0)
            {
                return new BugFinderResult
                {
                    Success = false,
                    Message = "No commentable statements found in the source file."
                };
            }

            Console.WriteLine($"Found {statements.Count} commentable statements.");

            // Start binary search
            var problematicIndices = BinarySearchRecursive(0, statements.Count - 1, statements.Count);

            // Restore original code
            File.WriteAllText(_sourceFilePath, _originalCode);

            if (problematicIndices.Count > 0)
            {
                return new BugFinderResult
                {
                    Success = true,
                    Message = $"Bug found after {_iterationCount} iterations.",
                    ProblematicStatements = problematicIndices.Select(i => statements[i]).ToList(),
                    StatementIndices = problematicIndices
                };
            }

            return new BugFinderResult
            {
                Success = false,
                Message = "Unable to isolate the bug to specific statements."
            };
        }

        /// <summary>
        /// Recursive binary search implementation
        /// </summary>
        private List<int> BinarySearchRecursive(int start, int end, int totalCount)
        {
            _iterationCount++;
            Console.WriteLine($"\nIteration {_iterationCount}: Searching range [{start}, {end}]");

            if (start > end)
                return new List<int>();

            if (start == end)
            {
                // Single statement - test if commenting it removes the bug
                var indicesToComment = new HashSet<int> { start };
                var modifiedCode = _commenter.CommentOutStatements(_originalCode, indicesToComment);
                File.WriteAllText(_sourceFilePath, modifiedCode);

                var result = _testExecutor.RunTests();

                if (result.Success)
                {
                    Console.WriteLine($"✓ Bug isolated to statement at index {start}");
                    return new List<int> { start };
                }
                else
                {
                    Console.WriteLine($"✗ Statement at index {start} is not the sole cause");
                    return new List<int>();
                }
            }

            // Split range in half
            int mid = start + (end - start) / 2;

            // Test first half
            var firstHalfIndices = Enumerable.Range(start, mid - start + 1).ToHashSet();
            var codeWithFirstHalfCommented = _commenter.CommentOutStatements(_originalCode, firstHalfIndices);
            File.WriteAllText(_sourceFilePath, codeWithFirstHalfCommented);

            var firstHalfResult = _testExecutor.RunTests();

            if (firstHalfResult.Success)
            {
                // Bug is in the first half (when we comment it out, bug disappears)
                Console.WriteLine($"Bug is in first half [{start}, {mid}]");
                return BinarySearchRecursive(start, mid, totalCount);
            }

            // Test second half
            var secondHalfIndices = Enumerable.Range(mid + 1, end - mid).ToHashSet();
            var codeWithSecondHalfCommented = _commenter.CommentOutStatements(_originalCode, secondHalfIndices);
            File.WriteAllText(_sourceFilePath, codeWithSecondHalfCommented);

            var secondHalfResult = _testExecutor.RunTests();

            if (secondHalfResult.Success)
            {
                // Bug is in the second half
                Console.WriteLine($"Bug is in second half [{mid + 1}, {end}]");
                return BinarySearchRecursive(mid + 1, end, totalCount);
            }

            // Bug might be in interaction between both halves
            // Try to narrow down further in both halves
            Console.WriteLine("Bug might involve multiple statements or interactions");
            var firstResults = BinarySearchRecursive(start, mid, totalCount);
            var secondResults = BinarySearchRecursive(mid + 1, end, totalCount);

            firstResults.AddRange(secondResults);
            return firstResults;
        }

        /// <summary>
        /// Restores the original source code
        /// </summary>
        public void RestoreOriginal()
        {
            File.WriteAllText(_sourceFilePath, _originalCode);
        }
    }

    /// <summary>
    /// Result of the bug finding process
    /// </summary>
    public class BugFinderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> ProblematicStatements { get; set; } = new List<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>();
        public List<int> StatementIndices { get; set; } = new List<int>();
    }
}
