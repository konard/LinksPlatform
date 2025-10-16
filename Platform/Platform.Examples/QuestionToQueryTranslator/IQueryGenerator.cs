using System.Collections.Generic;

namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Generates possible queries from parsed questions
    /// </summary>
    public interface IQueryGenerator
    {
        /// <summary>
        /// Generates all possible query interpretations for a given question
        /// </summary>
        /// <param name="question">The parsed question</param>
        /// <returns>A collection of possible queries, ordered by confidence</returns>
        IEnumerable<IQuery> GenerateQueries(IQuestion question);
    }
}
