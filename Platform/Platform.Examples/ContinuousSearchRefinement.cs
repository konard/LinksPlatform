using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Implements continuous search refinement through iterative question-based narrowing of results.
    /// The system asks clarifying questions to reduce the search space by half with each iteration,
    /// composing a precise request before presenting final results.
    /// </summary>
    public class ContinuousSearchRefinement<TItem>
    {
        private readonly ISearchableCollection<TItem> _collection;
        private readonly IQuestionStrategy<TItem> _questionStrategy;

        public ContinuousSearchRefinement(ISearchableCollection<TItem> collection, IQuestionStrategy<TItem> questionStrategy)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _questionStrategy = questionStrategy ?? throw new ArgumentNullException(nameof(questionStrategy));
        }

        /// <summary>
        /// Refines search through iterative questioning until a precise result set is achieved.
        /// </summary>
        /// <param name="initialQuery">The initial search query from the user</param>
        /// <param name="interactionHandler">Handler for user interaction (asking questions and receiving answers)</param>
        /// <param name="targetResultCount">Target number of results to achieve (default: 1)</param>
        /// <returns>Refined search results</returns>
        public SearchRefinementResult<TItem> RefineSearch(
            string initialQuery,
            IInteractionHandler interactionHandler,
            int targetResultCount = 1)
        {
            var currentResults = _collection.Search(initialQuery);
            var refinementHistory = new List<RefinementStep>();
            var constraints = new List<SearchConstraint>();

            refinementHistory.Add(new RefinementStep
            {
                Query = initialQuery,
                ResultCount = currentResults.Count,
                Question = null,
                Answer = null
            });

            while (currentResults.Count > targetResultCount)
            {
                var question = _questionStrategy.GenerateQuestion(currentResults, constraints);

                if (question == null)
                {
                    // No more questions can be asked to further refine
                    break;
                }

                var answer = interactionHandler.AskQuestion(question);

                if (answer.IsSkipped)
                {
                    // User chose to skip this question
                    constraints.Add(new SearchConstraint
                    {
                        Attribute = question.Attribute,
                        IsSkipped = true
                    });
                    continue;
                }

                constraints.Add(new SearchConstraint
                {
                    Attribute = question.Attribute,
                    Value = answer.Value,
                    IsSkipped = false
                });

                currentResults = _collection.ApplyConstraint(currentResults, constraints.Last());

                refinementHistory.Add(new RefinementStep
                {
                    Query = initialQuery,
                    ResultCount = currentResults.Count,
                    Question = question,
                    Answer = answer
                });
            }

            return new SearchRefinementResult<TItem>
            {
                Results = currentResults,
                RefinementHistory = refinementHistory,
                Constraints = constraints
            };
        }
    }

    public interface ISearchableCollection<TItem>
    {
        List<TItem> Search(string query);
        List<TItem> ApplyConstraint(List<TItem> currentResults, SearchConstraint constraint);
    }

    public interface IQuestionStrategy<TItem>
    {
        /// <summary>
        /// Generates the next question that will ideally halve the result set.
        /// </summary>
        Question GenerateQuestion(List<TItem> currentResults, List<SearchConstraint> existingConstraints);
    }

    public interface IInteractionHandler
    {
        Answer AskQuestion(Question question);
    }

    public class Question
    {
        public string Text { get; set; }
        public string Attribute { get; set; }
        public List<string> Options { get; set; }
        public QuestionType Type { get; set; }
    }

    public enum QuestionType
    {
        MultipleChoice,
        Binary,
        Numerical,
        Text
    }

    public class Answer
    {
        public string Value { get; set; }
        public bool IsSkipped { get; set; }
    }

    public class SearchConstraint
    {
        public string Attribute { get; set; }
        public string Value { get; set; }
        public bool IsSkipped { get; set; }
    }

    public class RefinementStep
    {
        public string Query { get; set; }
        public int ResultCount { get; set; }
        public Question Question { get; set; }
        public Answer Answer { get; set; }
    }

    public class SearchRefinementResult<TItem>
    {
        public List<TItem> Results { get; set; }
        public List<RefinementStep> RefinementHistory { get; set; }
        public List<SearchConstraint> Constraints { get; set; }
    }
}
