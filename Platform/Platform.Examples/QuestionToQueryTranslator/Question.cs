namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Represents a parsed question with its components
    /// </summary>
    public class Question : IQuestion
    {
        /// <summary>
        /// Gets the original question text
        /// </summary>
        public string OriginalText { get; }

        /// <summary>
        /// Gets the question type (who, what, when, where, why, how, etc.)
        /// </summary>
        public string QuestionType { get; }

        /// <summary>
        /// Gets the subject of the question
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets the predicate/action of the question
        /// </summary>
        public string Predicate { get; }

        /// <summary>
        /// Gets additional context or constraints
        /// </summary>
        public string[] Context { get; }

        public Question(string originalText, string questionType, string subject, string predicate, params string[] context)
        {
            OriginalText = originalText;
            QuestionType = questionType;
            Subject = subject;
            Predicate = predicate;
            Context = context ?? new string[0];
        }
    }
}
