using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Base class for reference collection services that serve data to other links users on demand.
    /// Each service can act as a neuron in a larger Links Platform system.
    /// Supports backing data via GitHub using LiNo (Links Notation) protocol.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public abstract class ReferenceService<TLink>
    {
        protected readonly ILinks<TLink> _links;
        protected readonly Sequences _sequences;
        protected readonly string _dataType;

        /// <summary>
        /// Initializes a new instance of the ReferenceService class.
        /// </summary>
        /// <param name="links">The links storage to use.</param>
        /// <param name="sequences">The sequences manager for working with link sequences.</param>
        /// <param name="dataType">The type of data this service manages (e.g., "colors", "countries").</param>
        protected ReferenceService(ILinks<TLink> links, Sequences sequences, string dataType)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
            _dataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        }

        /// <summary>
        /// Loads reference data from a GitHub repository in LiNo format.
        /// </summary>
        /// <param name="githubUrl">The URL to the raw GitHub file containing LiNo data.</param>
        public abstract void LoadFromGitHub(string githubUrl);

        /// <summary>
        /// Serves a reference by its identifier.
        /// </summary>
        /// <param name="referenceId">The identifier of the reference to serve.</param>
        /// <returns>The reference data in LiNo format.</returns>
        public abstract string ServeReference(string referenceId);

        /// <summary>
        /// Lists all available references in this collection.
        /// </summary>
        /// <returns>A collection of reference identifiers.</returns>
        public abstract IEnumerable<string> ListReferences();

        /// <summary>
        /// Searches for references matching the given criteria.
        /// </summary>
        /// <param name="searchPattern">The search pattern to match.</param>
        /// <returns>A collection of matching references.</returns>
        public virtual IEnumerable<string> SearchReferences(string searchPattern)
        {
            return ListReferences().Where(r => r.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Exports the reference collection to LiNo format for GitHub storage.
        /// </summary>
        /// <returns>The collection in LiNo format.</returns>
        public abstract string ExportToLiNo();

        /// <summary>
        /// Gets the data type name this service manages.
        /// </summary>
        public string DataType => _dataType;
    }
}
