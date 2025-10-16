using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Main translator that converts questions to executable queries
    /// </summary>
    public class QuestionToQueryTranslator
    {
        private readonly IQuestionParser _parser;
        private readonly IQueryGenerator _generator;

        public QuestionToQueryTranslator(IQuestionParser parser, IQueryGenerator generator)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public QuestionToQueryTranslator()
            : this(new BasicQuestionParser(), new BasicQueryGenerator())
        {
        }

        /// <summary>
        /// Translates a question into possible query interpretations
        /// </summary>
        /// <param name="questionText">The question text to translate</param>
        /// <returns>A collection of possible queries ordered by confidence</returns>
        public IEnumerable<IQuery> Translate(string questionText)
        {
            var question = _parser.Parse(questionText);
            return _generator.GenerateQueries(question);
        }

        /// <summary>
        /// Translates a question and returns the most likely query
        /// </summary>
        /// <param name="questionText">The question text to translate</param>
        /// <returns>The query with the highest confidence</returns>
        public IQuery TranslateBest(string questionText)
        {
            return Translate(questionText).FirstOrDefault();
        }

        /// <summary>
        /// Checks if the question is ambiguous and needs clarification
        /// </summary>
        /// <param name="questionText">The question text to check</param>
        /// <param name="confidenceThreshold">The confidence threshold below which clarification is needed (default 0.7)</param>
        /// <returns>True if clarification is needed, false otherwise</returns>
        public bool NeedsClarification(string questionText, double confidenceThreshold = 0.7)
        {
            var queries = Translate(questionText).ToList();

            if (!queries.Any())
            {
                return true;
            }

            var bestQuery = queries.First();

            // Need clarification if best confidence is below threshold
            if (bestQuery.Confidence < confidenceThreshold)
            {
                return true;
            }

            // Need clarification if multiple queries have similar confidence
            var similarConfidenceQueries = queries
                .Where(q => Math.Abs(q.Confidence - bestQuery.Confidence) < 0.1)
                .Count();

            return similarConfidenceQueries > 1;
        }

        /// <summary>
        /// Generates clarification questions for ambiguous input
        /// </summary>
        /// <param name="questionText">The question text that needs clarification</param>
        /// <returns>A list of clarification questions</returns>
        public IEnumerable<string> GetClarificationQuestions(string questionText)
        {
            var queries = Translate(questionText).Take(5).ToList();
            var clarifications = new List<string>();

            if (!queries.Any())
            {
                clarifications.Add("I don't understand the question. Could you rephrase it?");
                return clarifications;
            }

            if (queries.Count > 1)
            {
                clarifications.Add("I found multiple possible interpretations. Which one did you mean?");
                for (int i = 0; i < Math.Min(queries.Count, 5); i++)
                {
                    clarifications.Add($"  {i + 1}. {queries[i].Description}");
                }
            }

            var bestQuery = queries.First();
            if (bestQuery.Confidence < 0.7)
            {
                clarifications.Add($"I'm not very confident about this interpretation. Did you mean: {bestQuery.Description}?");
            }

            return clarifications;
        }
    }
}
