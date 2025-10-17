using System;
using System.Threading.Tasks;
using Platform.Data.TDDStackOverflow;
using Platform.Data.TDDStackOverflow.Models;
using Platform.Data.TDDStackOverflow.Services;

namespace TDDStackOverflowDemo
{
    /// <summary>
    /// Demonstration of the TDD-driven Stack Overflow platform
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== TDD-Driven Stack Overflow Demo ===\n");

            var manager = new QuestionManager();

            // Example 1: Simple addition question
            Console.WriteLine("Creating example question: 'Add Two Numbers'...\n");

            var additionTestSuite = new TestSuite
            {
                TimeoutMilliseconds = 3000,
                TestCases = new System.Collections.Generic.List<TestCase>
                {
                    new TestCase
                    {
                        Description = "Add 2 + 3",
                        Input = "2\n3\n",
                        ExpectedOutput = "5"
                    },
                    new TestCase
                    {
                        Description = "Add 10 + 20",
                        Input = "10\n20\n",
                        ExpectedOutput = "30"
                    },
                    new TestCase
                    {
                        Description = "Add negative numbers",
                        Input = "-5\n-10\n",
                        ExpectedOutput = "-15"
                    }
                }
            };

            var addQuestion = manager.CreateQuestion(
                "Add Two Numbers",
                "Write a program that reads two integers and outputs their sum",
                additionTestSuite
            );

            Console.WriteLine($"Question created with ID: {addQuestion.Id}");
            Console.WriteLine($"Test cases: {addQuestion.TestSuite.TestCases.Count}\n");

            // Submit a Python solution
            Console.WriteLine("Submitting Python solution...\n");

            var pythonCode = @"a = int(input())
b = int(input())
print(a + b)";

            var (pythonSolution, pythonResult) = await manager.SubmitSolutionAsync(
                addQuestion.Id,
                pythonCode,
                "Python",
                "python3 solution.py",
                "solution.py",
                "user1"
            );

            PrintTestResults(pythonResult);

            // Submit a failing solution
            Console.WriteLine("\nSubmitting a failing solution (multiplies instead of adds)...\n");

            var failingCode = @"a = int(input())
b = int(input())
print(a * b)";

            var (failingSolution, failingResult) = await manager.SubmitSolutionAsync(
                addQuestion.Id,
                failingCode,
                "Python",
                "python3 solution.py",
                "solution.py",
                "user2"
            );

            PrintTestResults(failingResult);

            // Vote for the passing solution
            manager.VoteSolution(addQuestion.Id, pythonSolution.Id, 1);
            manager.VoteSolution(addQuestion.Id, pythonSolution.Id, 1);
            manager.VoteSolution(addQuestion.Id, pythonSolution.Id, 1);

            Console.WriteLine($"\nVoted for solution #{pythonSolution.Id}");

            // Check if question is frozen
            Console.WriteLine($"\nQuestion frozen: {addQuestion.IsFrozen}");
            if (addQuestion.IsFrozen)
            {
                Console.WriteLine($"Frozen at: {addQuestion.FrozenAt}");
            }

            // Display sorted solutions
            Console.WriteLine("\n=== Solutions sorted by votes ===");
            var sortedByVotes = manager.GetSolutionsSorted(addQuestion.Id, SolutionSortCriteria.Votes);
            foreach (var sol in sortedByVotes)
            {
                Console.WriteLine($"Solution #{sol.Id}: {sol.Votes} votes, " +
                    $"Passed: {sol.PassedAllTests}, " +
                    $"Language: {sol.Language}");
            }

            // Create related questions
            Console.WriteLine("\n=== Creating related questions ===");

            var multiplicationTestSuite = new TestSuite
            {
                TimeoutMilliseconds = 3000,
                TestCases = new System.Collections.Generic.List<TestCase>
                {
                    new TestCase
                    {
                        Description = "Multiply 2 * 3",
                        Input = "2\n3\n",
                        ExpectedOutput = "6"
                    }
                }
            };

            var multiplyQuestion = manager.CreateQuestion(
                "Multiply Two Numbers",
                "Write a program that reads two integers and outputs their product",
                multiplicationTestSuite
            );

            var arithmeticTestSuite = new TestSuite
            {
                TimeoutMilliseconds = 3000,
                TestCases = new System.Collections.Generic.List<TestCase>
                {
                    new TestCase
                    {
                        Description = "Add and multiply",
                        Input = "2\n3\n+\n",
                        ExpectedOutput = "5"
                    }
                }
            };

            var arithmeticQuestion = manager.CreateQuestion(
                "Basic Arithmetic Operations",
                "Write a program that performs basic arithmetic operations",
                arithmeticTestSuite
            );

            // Link questions: Arithmetic is broader than Addition and Multiplication
            manager.LinkNarrowerThan(arithmeticQuestion.Id, addQuestion.Id);
            manager.LinkNarrowerThan(arithmeticQuestion.Id, multiplyQuestion.Id);

            Console.WriteLine($"Linked questions:");
            Console.WriteLine($"  Question #{arithmeticQuestion.Id} (broader)");
            Console.WriteLine($"    ├─ Question #{addQuestion.Id} (narrower)");
            Console.WriteLine($"    └─ Question #{multiplyQuestion.Id} (narrower)");

            var (broader, narrower) = manager.GetRelatedQuestions(addQuestion.Id);
            Console.WriteLine($"\nQuestion #{addQuestion.Id} related questions:");
            Console.WriteLine($"  Broader: {string.Join(", ", broader.ConvertAll(q => $"#{q.Id}"))}");
            Console.WriteLine($"  Narrower: {string.Join(", ", narrower.ConvertAll(q => $"#{q.Id}"))}");

            Console.WriteLine("\n=== Demo completed ===");
        }

        static void PrintTestResults(TestResult result)
        {
            Console.WriteLine($"Tests: {result.TestsPassed}/{result.TestCaseResults.Count} passed");
            Console.WriteLine($"Total time: {result.TotalExecutionTime:F2}ms");
            Console.WriteLine($"All passed: {result.AllTestsPassed}");

            foreach (var testCase in result.TestCaseResults)
            {
                var status = testCase.Passed ? "✓" : "✗";
                Console.WriteLine($"  {status} {testCase.TestCase.Description} ({testCase.ExecutionTime:F2}ms)");
                if (!testCase.Passed)
                {
                    Console.WriteLine($"     Expected: {testCase.TestCase.ExpectedOutput}");
                    Console.WriteLine($"     Got: {testCase.ActualOutput}");
                }
            }
        }
    }
}
