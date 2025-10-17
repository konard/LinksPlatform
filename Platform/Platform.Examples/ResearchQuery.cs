using System;
using System.Collections.Generic;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a research query with defined data selection, transformation, and interpretation methods.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    public class ResearchQuery<TLink>
    {
        /// <summary>
        /// Gets the unique identifier of the research query.
        /// </summary>
        public TLink Id { get; set; }

        /// <summary>
        /// Gets the name of the research query.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the description of the research query.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the data selection criteria.
        /// This func selects links based on specific criteria.
        /// </summary>
        public Func<SynchronizedLinks<TLink>, IList<IList<TLink>>> SelectionMethod { get; set; }

        /// <summary>
        /// Gets or sets the data transformation method.
        /// This func transforms selected data into a desired format.
        /// </summary>
        public Func<IList<IList<TLink>>, object> TransformationMethod { get; set; }

        /// <summary>
        /// Gets or sets the result interpretation method.
        /// This func interprets and formats the final results.
        /// </summary>
        public Func<object, string> InterpretationMethod { get; set; }

        /// <summary>
        /// Gets or sets whether this research should be computed continuously.
        /// </summary>
        public bool IsContinuous { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this research was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this research was last executed.
        /// </summary>
        public DateTime? LastExecutedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchQuery{TLink}"/> class.
        /// </summary>
        public ResearchQuery()
        {
            CreatedAt = DateTime.UtcNow;
            IsContinuous = false;
        }

        /// <summary>
        /// Executes the research query on the provided links storage.
        /// </summary>
        /// <param name="links">The links storage to query.</param>
        /// <returns>The interpreted result as a string.</returns>
        public string Execute(SynchronizedLinks<TLink> links)
        {
            try
            {
                // Step 1: Data Selection
                var selectedData = SelectionMethod?.Invoke(links) ?? new List<IList<TLink>>();

                // Step 2: Data Transformation
                var transformedData = TransformationMethod?.Invoke(selectedData) ?? selectedData;

                // Step 3: Result Interpretation
                var result = InterpretationMethod?.Invoke(transformedData) ?? transformedData?.ToString() ?? "No result";

                LastExecutedAt = DateTime.UtcNow;

                return result;
            }
            catch (Exception ex)
            {
                return $"Error executing research '{Name}': {ex.Message}";
            }
        }
    }
}
