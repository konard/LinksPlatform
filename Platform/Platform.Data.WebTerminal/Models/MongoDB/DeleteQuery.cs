using Newtonsoft.Json.Linq;

namespace Platform.Data.WebTerminal.Models.MongoDB
{
    /// <summary>
    /// Represents a MongoDB delete query.
    /// </summary>
    public class DeleteQuery
    {
        /// <summary>
        /// Filter criteria for matching documents to delete.
        /// </summary>
        public JObject Filter { get; set; }

        /// <summary>
        /// Whether to delete only one document (true) or all matching documents (false).
        /// </summary>
        public bool Single { get; set; }
    }
}
