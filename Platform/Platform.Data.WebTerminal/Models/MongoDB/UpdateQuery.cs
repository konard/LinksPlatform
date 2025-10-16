using Newtonsoft.Json.Linq;

namespace Platform.Data.WebTerminal.Models.MongoDB
{
    /// <summary>
    /// Represents a MongoDB update query.
    /// </summary>
    public class UpdateQuery
    {
        /// <summary>
        /// Filter criteria for matching documents to update.
        /// </summary>
        public JObject Filter { get; set; }

        /// <summary>
        /// Update operations to apply.
        /// </summary>
        public JObject Update { get; set; }

        /// <summary>
        /// Whether to update multiple documents (true) or just one (false).
        /// </summary>
        public bool Multi { get; set; }

        /// <summary>
        /// Whether to insert a new document if no match is found.
        /// </summary>
        public bool Upsert { get; set; }
    }
}
