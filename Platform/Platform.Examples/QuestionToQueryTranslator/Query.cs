namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Represents a query that can be executed to answer a question
    /// </summary>
    public class Query : IQuery
    {
        /// <summary>
        /// Gets the query path/method description
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the confidence level of this interpretation (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; }

        /// <summary>
        /// Gets a human-readable description of what this query does
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the source question this query was derived from
        /// </summary>
        public IQuestion SourceQuestion { get; }

        public Query(IQuestion sourceQuestion, string path, double confidence, string description)
        {
            SourceQuestion = sourceQuestion;
            Path = path;
            Confidence = confidence;
            Description = description;
        }
    }
}
