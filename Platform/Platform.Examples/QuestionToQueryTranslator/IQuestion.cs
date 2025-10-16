namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Represents a parsed question with its components
    /// </summary>
    public interface IQuestion
    {
        /// <summary>
        /// Gets the original question text
        /// </summary>
        string OriginalText { get; }

        /// <summary>
        /// Gets the question type (who, what, when, where, why, how, etc.)
        /// </summary>
        string QuestionType { get; }

        /// <summary>
        /// Gets the subject of the question
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Gets the predicate/action of the question
        /// </summary>
        string Predicate { get; }

        /// <summary>
        /// Gets additional context or constraints
        /// </summary>
        string[] Context { get; }
    }
}
