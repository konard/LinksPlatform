using Newtonsoft.Json.Linq;

namespace Platform.Data.WebTerminal.Models.MongoDB
{
    /// <summary>
    /// Represents a MongoDB find query.
    /// </summary>
    public class FindQuery
    {
        /// <summary>
        /// Filter criteria for matching documents.
        /// </summary>
        public JObject Filter { get; set; }

        /// <summary>
        /// Projection to limit fields returned.
        /// </summary>
        public JObject Projection { get; set; }

        /// <summary>
        /// Maximum number of documents to return.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Number of documents to skip.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Sort specification.
        /// </summary>
        public JObject Sort { get; set; }
    }
}
