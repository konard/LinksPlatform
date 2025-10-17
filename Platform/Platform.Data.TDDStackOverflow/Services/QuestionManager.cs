using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platform.Data.TDDStackOverflow.Models;

namespace Platform.Data.TDDStackOverflow.Services
{
    /// <summary>
    /// Manages questions, solutions, and their relationships
    /// </summary>
    public class QuestionManager
    {
        private readonly Dictionary<ulong, Question> _questions;
        private readonly TestRunner _testRunner;
        private ulong _nextQuestionId;
        private ulong _nextSolutionId;

        public QuestionManager(TestRunner testRunner = null)
        {
            _questions = new Dictionary<ulong, Question>();
            _testRunner = testRunner ?? new TestRunner();
            _nextQuestionId = 1;
            _nextSolutionId = 1;
        }

        /// <summary>
        /// Creates a new question with test suite
        /// </summary>
        public Question CreateQuestion(string title, string description, TestSuite testSuite)
        {
            var question = new Question
            {
                Id = _nextQuestionId++,
                Title = title,
                Description = description,
                TestSuite = testSuite
            };

            _questions[question.Id] = question;
            return question;
        }

        /// <summary>
        /// Submits a solution to a question
        /// </summary>
        public async Task<(Solution solution, TestResult testResult)> SubmitSolutionAsync(
            ulong questionId,
            string code,
            string language,
            string executionCommand,
            string fileName,
            string submittedBy = "anonymous")
        {
            if (!_questions.TryGetValue(questionId, out var question))
            {
                throw new ArgumentException($"Question {questionId} not found");
            }

            if (question.IsFrozen)
            {
                throw new InvalidOperationException($"Question {questionId} is frozen and no longer accepts solutions");
            }

            var solution = new Solution
            {
                Id = _nextSolutionId++,
                QuestionId = questionId,
                Code = code,
                Language = language,
                ExecutionCommand = executionCommand,
                FileName = fileName,
                SubmittedBy = submittedBy
            };

            // Run tests
            var testResult = await _testRunner.RunTestsAsync(solution, question.TestSuite);

            // Update solution metrics
            solution.PassedAllTests = testResult.AllTestsPassed;
            solution.LastTestedAt = DateTime.UtcNow;
            solution.Metrics.TestsPassed = testResult.TestsPassed;
            solution.Metrics.TestsFailed = testResult.TestsFailed;
            solution.Metrics.TotalTests = testResult.TestCaseResults.Count;
            solution.Metrics.AverageExecutionTimeMs = testResult.TotalExecutionTime / testResult.TestCaseResults.Count;
            solution.Metrics.PeakMemoryUsageBytes = testResult.PeakMemoryUsage;

            // Add solution to question
            question.Solutions.Add(solution);

            // Freeze question if this is the first passing solution
            if (testResult.AllTestsPassed && !question.IsFrozen)
            {
                question.Freeze();
            }

            return (solution, testResult);
        }

        /// <summary>
        /// Votes for a solution
        /// </summary>
        public void VoteSolution(ulong questionId, ulong solutionId, int delta)
        {
            if (!_questions.TryGetValue(questionId, out var question))
            {
                throw new ArgumentException($"Question {questionId} not found");
            }

            var solution = question.Solutions.FirstOrDefault(s => s.Id == solutionId);
            if (solution == null)
            {
                throw new ArgumentException($"Solution {solutionId} not found in question {questionId}");
            }

            solution.Votes += delta;
        }

        /// <summary>
        /// Gets solutions sorted by criteria
        /// </summary>
        public List<Solution> GetSolutionsSorted(ulong questionId, SolutionSortCriteria criteria)
        {
            if (!_questions.TryGetValue(questionId, out var question))
            {
                throw new ArgumentException($"Question {questionId} not found");
            }

            return criteria switch
            {
                SolutionSortCriteria.Votes => question.Solutions.OrderByDescending(s => s.Votes).ToList(),
                SolutionSortCriteria.Performance => question.Solutions
                    .Where(s => s.PassedAllTests)
                    .OrderBy(s => s.Metrics.AverageExecutionTimeMs)
                    .ToList(),
                SolutionSortCriteria.Memory => question.Solutions
                    .Where(s => s.PassedAllTests)
                    .OrderBy(s => s.Metrics.PeakMemoryUsageBytes)
                    .ToList(),
                SolutionSortCriteria.Language => question.Solutions.OrderBy(s => s.Language).ToList(),
                _ => question.Solutions.ToList()
            };
        }

        /// <summary>
        /// Links questions with NarrowerThan relationship
        /// </summary>
        public void LinkNarrowerThan(ulong broaderQuestionId, ulong narrowerQuestionId)
        {
            if (!_questions.TryGetValue(broaderQuestionId, out var broaderQuestion))
            {
                throw new ArgumentException($"Question {broaderQuestionId} not found");
            }

            if (!_questions.TryGetValue(narrowerQuestionId, out var narrowerQuestion))
            {
                throw new ArgumentException($"Question {narrowerQuestionId} not found");
            }

            if (!broaderQuestion.NarrowerThanLinks.Contains(narrowerQuestionId))
            {
                broaderQuestion.NarrowerThanLinks.Add(narrowerQuestionId);
            }

            if (!narrowerQuestion.BroaderThanLinks.Contains(broaderQuestionId))
            {
                narrowerQuestion.BroaderThanLinks.Add(broaderQuestionId);
            }
        }

        /// <summary>
        /// Gets a question by ID
        /// </summary>
        public Question GetQuestion(ulong id)
        {
            return _questions.TryGetValue(id, out var question) ? question : null;
        }

        /// <summary>
        /// Gets all questions
        /// </summary>
        public List<Question> GetAllQuestions()
        {
            return _questions.Values.ToList();
        }

        /// <summary>
        /// Finds related questions (broader and narrower)
        /// </summary>
        public (List<Question> broader, List<Question> narrower) GetRelatedQuestions(ulong questionId)
        {
            if (!_questions.TryGetValue(questionId, out var question))
            {
                throw new ArgumentException($"Question {questionId} not found");
            }

            var broader = question.BroaderThanLinks
                .Select(id => GetQuestion(id))
                .Where(q => q != null)
                .ToList();

            var narrower = question.NarrowerThanLinks
                .Select(id => GetQuestion(id))
                .Where(q => q != null)
                .ToList();

            return (broader, narrower);
        }
    }

    /// <summary>
    /// Criteria for sorting solutions
    /// </summary>
    public enum SolutionSortCriteria
    {
        Votes,
        Performance,
        Memory,
        Language
    }
}
