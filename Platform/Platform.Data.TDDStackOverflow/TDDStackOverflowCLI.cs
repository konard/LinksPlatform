using System;
using System.Linq;
using System.Threading.Tasks;
using Platform.Data.TDDStackOverflow.Models;
using Platform.Data.TDDStackOverflow.Services;

namespace Platform.Data.TDDStackOverflow
{
    /// <summary>
    /// Command-line interface for the TDD-driven Stack Overflow platform
    /// </summary>
    public class TDDStackOverflowCLI
    {
        private readonly QuestionManager _questionManager;

        public TDDStackOverflowCLI()
        {
            _questionManager = new QuestionManager();
        }

        public async Task RunAsync(params string[] args)
        {
            try
            {
                Console.WriteLine("=== TDD-Driven Stack Overflow ===");
                Console.WriteLine("A platform where questions are automated tests and solutions are code.");
                Console.WriteLine();

                if (args.Length == 0 || args[0] == "help")
                {
                    ShowHelp();
                    return;
                }

                var command = args[0].ToLower();

                switch (command)
                {
                    case "create-question":
                        await CreateQuestionInteractive();
                        break;
                    case "submit-solution":
                        await SubmitSolutionInteractive();
                        break;
                    case "list-questions":
                        ListQuestions();
                        break;
                    case "view-question":
                        if (args.Length > 1 && ulong.TryParse(args[1], out var qId))
                            ViewQuestion(qId);
                        else
                            Console.WriteLine("Usage: view-question <question-id>");
                        break;
                    case "vote":
                        if (args.Length > 3 && ulong.TryParse(args[1], out var questionId) &&
                            ulong.TryParse(args[2], out var solutionId) &&
                            int.TryParse(args[3], out var delta))
                        {
                            Vote(questionId, solutionId, delta);
                        }
                        else
                        {
                            Console.WriteLine("Usage: vote <question-id> <solution-id> <+1|-1>");
                        }
                        break;
                    case "link":
                        if (args.Length > 2 && ulong.TryParse(args[1], out var broaderId) &&
                            ulong.TryParse(args[2], out var narrowerId))
                        {
                            LinkQuestions(broaderId, narrowerId);
                        }
                        else
                        {
                            Console.WriteLine("Usage: link <broader-question-id> <narrower-question-id>");
                        }
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  create-question          - Create a new question with test suite");
            Console.WriteLine("  submit-solution          - Submit a solution to a question");
            Console.WriteLine("  list-questions           - List all questions");
            Console.WriteLine("  view-question <id>       - View question details and solutions");
            Console.WriteLine("  vote <qid> <sid> <+1|-1> - Vote for a solution");
            Console.WriteLine("  link <bid> <nid>         - Link broader and narrower questions");
            Console.WriteLine("  help                     - Show this help message");
        }

        private async Task CreateQuestionInteractive()
        {
            Console.WriteLine("\n--- Create New Question ---");
            Console.Write("Title: ");
            var title = Console.ReadLine();

            Console.Write("Description: ");
            var description = Console.ReadLine();

            var testSuite = new TestSuite();

            Console.Write("Timeout (ms) [5000]: ");
            var timeoutStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(timeoutStr) && int.TryParse(timeoutStr, out var timeout))
            {
                testSuite.TimeoutMilliseconds = timeout;
            }

            Console.WriteLine("\nAdd test cases (enter empty input to finish):");
            var testIndex = 1;
            while (true)
            {
                Console.WriteLine($"\nTest Case #{testIndex}:");
                Console.Write("  Description: ");
                var testDesc = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(testDesc)) break;

                Console.Write("  Input: ");
                var input = Console.ReadLine();

                Console.Write("  Expected Output: ");
                var expectedOutput = Console.ReadLine();

                testSuite.TestCases.Add(new TestCase
                {
                    Description = testDesc,
                    Input = input,
                    ExpectedOutput = expectedOutput
                });

                testIndex++;
            }

            if (testSuite.TestCases.Count == 0)
            {
                Console.WriteLine("Error: At least one test case is required.");
                return;
            }

            var question = _questionManager.CreateQuestion(title, description, testSuite);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✓ Question created successfully! ID: {question.Id}");
            Console.ResetColor();
        }

        private async Task SubmitSolutionInteractive()
        {
            Console.WriteLine("\n--- Submit Solution ---");
            Console.Write("Question ID: ");
            if (!ulong.TryParse(Console.ReadLine(), out var questionId))
            {
                Console.WriteLine("Invalid question ID");
                return;
            }

            var question = _questionManager.GetQuestion(questionId);
            if (question == null)
            {
                Console.WriteLine("Question not found");
                return;
            }

            if (question.IsFrozen)
            {
                Console.WriteLine($"Warning: This question was frozen at {question.FrozenAt}");
                Console.WriteLine("It already has a passing solution, but you can still view it.");
                return;
            }

            Console.Write("Language: ");
            var language = Console.ReadLine();

            Console.Write("File name (e.g., solution.py): ");
            var fileName = Console.ReadLine();

            Console.Write("Execution command (e.g., python solution.py): ");
            var execCommand = Console.ReadLine();

            Console.WriteLine("Enter your code (end with a line containing only '###'):");
            var codeLines = new System.Collections.Generic.List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "###") break;
                codeLines.Add(line);
            }
            var code = string.Join(Environment.NewLine, codeLines);

            Console.WriteLine("\nRunning tests...");
            var (solution, testResult) = await _questionManager.SubmitSolutionAsync(
                questionId, code, language, execCommand, fileName);

            Console.WriteLine($"\n=== Test Results ===");
            Console.WriteLine($"Tests Passed: {testResult.TestsPassed}/{testResult.TestCaseResults.Count}");
            Console.WriteLine($"Total Execution Time: {testResult.TotalExecutionTime:F2}ms");
            Console.WriteLine($"Average Time per Test: {solution.Metrics.AverageExecutionTimeMs:F2}ms");

            foreach (var testCaseResult in testResult.TestCaseResults)
            {
                var status = testCaseResult.Passed ? "✓ PASS" : "✗ FAIL";
                var color = testCaseResult.Passed ? ConsoleColor.Green : ConsoleColor.Red;

                Console.ForegroundColor = color;
                Console.WriteLine($"\n{status} - {testCaseResult.TestCase.Description}");
                Console.ResetColor();

                Console.WriteLine($"  Execution Time: {testCaseResult.ExecutionTime:F2}ms");
                if (!testCaseResult.Passed)
                {
                    Console.WriteLine($"  Expected: {testCaseResult.TestCase.ExpectedOutput}");
                    Console.WriteLine($"  Actual:   {testCaseResult.ActualOutput}");
                    if (!string.IsNullOrWhiteSpace(testCaseResult.ErrorMessage))
                        Console.WriteLine($"  Error:    {testCaseResult.ErrorMessage}");
                }
            }

            if (testResult.AllTestsPassed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ All tests passed! Solution ID: {solution.Id}");
                if (question.IsFrozen)
                {
                    Console.WriteLine("⚠ This question is now frozen (first passing solution).");
                }
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠ Some tests failed. Solution ID: {solution.Id}");
                Console.ResetColor();
            }
        }

        private void ListQuestions()
        {
            var questions = _questionManager.GetAllQuestions();

            if (questions.Count == 0)
            {
                Console.WriteLine("No questions found.");
                return;
            }

            Console.WriteLine("\n=== Questions ===");
            foreach (var question in questions)
            {
                var status = question.IsFrozen ? "[FROZEN]" : "[OPEN]";
                var statusColor = question.IsFrozen ? ConsoleColor.Blue : ConsoleColor.Green;

                Console.ForegroundColor = statusColor;
                Console.Write(status);
                Console.ResetColor();

                Console.WriteLine($" #{question.Id}: {question.Title}");
                Console.WriteLine($"   Solutions: {question.Solutions.Count} " +
                    $"(Passing: {question.Solutions.Count(s => s.PassedAllTests)})");
            }
        }

        private void ViewQuestion(ulong questionId)
        {
            var question = _questionManager.GetQuestion(questionId);
            if (question == null)
            {
                Console.WriteLine("Question not found.");
                return;
            }

            Console.WriteLine($"\n=== Question #{question.Id} ===");
            Console.WriteLine($"Title: {question.Title}");
            Console.WriteLine($"Description: {question.Description}");
            Console.WriteLine($"Status: {(question.IsFrozen ? "FROZEN" : "OPEN")}");
            Console.WriteLine($"Test Cases: {question.TestSuite.TestCases.Count}");

            var (broader, narrower) = _questionManager.GetRelatedQuestions(questionId);
            if (broader.Count > 0)
            {
                Console.WriteLine($"Broader Questions: {string.Join(", ", broader.Select(q => $"#{q.Id}"))}");
            }
            if (narrower.Count > 0)
            {
                Console.WriteLine($"Narrower Questions: {string.Join(", ", narrower.Select(q => $"#{q.Id}"))}");
            }

            if (question.Solutions.Count > 0)
            {
                Console.WriteLine($"\n=== Solutions ({question.Solutions.Count}) ===");
                var sortedSolutions = _questionManager.GetSolutionsSorted(questionId, SolutionSortCriteria.Votes);

                foreach (var solution in sortedSolutions.Take(10))
                {
                    var status = solution.PassedAllTests ? "✓" : "✗";
                    Console.WriteLine($"\n{status} Solution #{solution.Id} ({solution.Language}) - Votes: {solution.Votes}");
                    Console.WriteLine($"   Success Rate: {solution.Metrics.SuccessRate:F1}% " +
                        $"({solution.Metrics.TestsPassed}/{solution.Metrics.TotalTests})");
                    Console.WriteLine($"   Avg Time: {solution.Metrics.AverageExecutionTimeMs:F2}ms");
                }
            }
            else
            {
                Console.WriteLine("\nNo solutions yet.");
            }
        }

        private void Vote(ulong questionId, ulong solutionId, int delta)
        {
            _questionManager.VoteSolution(questionId, solutionId, delta);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Vote recorded for solution #{solutionId}");
            Console.ResetColor();
        }

        private void LinkQuestions(ulong broaderId, ulong narrowerId)
        {
            _questionManager.LinkNarrowerThan(broaderId, narrowerId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Linked question #{narrowerId} as narrower than #{broaderId}");
            Console.ResetColor();
        }
    }
}
