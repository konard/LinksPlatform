namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Parses natural language questions into structured question objects
    /// </summary>
    public interface IQuestionParser
    {
        /// <summary>
        /// Parses a question string into a structured question object
        /// </summary>
        /// <param name="questionText">The question text to parse</param>
        /// <returns>A parsed question object</returns>
        IQuestion Parse(string questionText);
    }
}
