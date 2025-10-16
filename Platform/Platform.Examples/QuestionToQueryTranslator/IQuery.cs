namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Represents a query that can be executed to answer a question
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Gets the query path/method description
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the confidence level of this interpretation (0.0 to 1.0)
        /// </summary>
        double Confidence { get; }

        /// <summary>
        /// Gets a human-readable description of what this query does
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the source question this query was derived from
        /// </summary>
        IQuestion SourceQuestion { get; }
    }
}
