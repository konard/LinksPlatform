using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Manages a global research database for storing and sharing research queries and data.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    public class ResearchDatabase<TLink>
    {
        private readonly SynchronizedLinks<TLink> _links;
        private readonly Dictionary<TLink, ResearchQuery<TLink>> _researches;
        private TLink _nextId;

        /// <summary>
        /// Gets the collection of all registered research queries.
        /// </summary>
        public IReadOnlyDictionary<TLink, ResearchQuery<TLink>> Researches => _researches;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchDatabase{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage to use for research data.</param>
        public ResearchDatabase(SynchronizedLinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _researches = new Dictionary<TLink, ResearchQuery<TLink>>();
            _nextId = default(TLink);
        }

        /// <summary>
        /// Registers a new research query in the database.
        /// </summary>
        /// <param name="research">The research query to register.</param>
        /// <returns>The assigned research ID.</returns>
        public TLink RegisterResearch(ResearchQuery<TLink> research)
        {
            if (research == null)
            {
                throw new ArgumentNullException(nameof(research));
            }

            var id = GenerateNextId();
            research.Id = id;
            _researches[id] = research;

            return id;
        }

        /// <summary>
        /// Retrieves a research query by its ID.
        /// </summary>
        /// <param name="id">The research ID.</param>
        /// <returns>The research query, or null if not found.</returns>
        public ResearchQuery<TLink> GetResearch(TLink id)
        {
            return _researches.TryGetValue(id, out var research) ? research : null;
        }

        /// <summary>
        /// Removes a research query from the database.
        /// </summary>
        /// <param name="id">The research ID to remove.</param>
        /// <returns>True if the research was removed, false if not found.</returns>
        public bool RemoveResearch(TLink id)
        {
            return _researches.Remove(id);
        }

        /// <summary>
        /// Exports all research data to a file for sharing.
        /// </summary>
        /// <param name="path">The file path to export to.</param>
        public void ExportData(string path)
        {
            using (var file = File.Create(path))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            {
                writer.WriteLine("# Global Research Database Export");
                writer.WriteLine($"# Exported at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                var linkCount = 0;
                _links.Each(link => { linkCount++; return _links.Constants.Continue; });
                writer.WriteLine($"# Total Links: {linkCount}");
                writer.WriteLine($"# Total Researches: {_researches.Count}");
                writer.WriteLine();

                // Export links data
                writer.WriteLine("## Links Data");
                _links.Each(link =>
                {
                    var source = _links.GetSource(link[_links.Constants.IndexPart]);
                    var target = _links.GetTarget(link[_links.Constants.IndexPart]);
                    writer.WriteLine($"{link[_links.Constants.IndexPart]},{source},{target}");
                    return _links.Constants.Continue;
                });

                writer.WriteLine();
                writer.WriteLine("## Research Queries");

                foreach (var kvp in _researches)
                {
                    var research = kvp.Value;
                    writer.WriteLine($"ID: {research.Id}");
                    writer.WriteLine($"Name: {research.Name}");
                    writer.WriteLine($"Description: {research.Description}");
                    writer.WriteLine($"IsContinuous: {research.IsContinuous}");
                    writer.WriteLine($"CreatedAt: {research.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");

                    if (research.LastExecutedAt.HasValue)
                    {
                        writer.WriteLine($"LastExecutedAt: {research.LastExecutedAt.Value:yyyy-MM-dd HH:mm:ss} UTC");
                    }

                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Gets statistics about the research database.
        /// </summary>
        /// <returns>A formatted string with database statistics.</returns>
        public string GetStatistics()
        {
            var sb = new StringBuilder();
            var linkCount = 0;
            _links.Each(link => { linkCount++; return _links.Constants.Continue; });

            sb.AppendLine("=== Research Database Statistics ===");
            sb.AppendLine($"Total Links: {linkCount}");
            sb.AppendLine($"Total Researches: {_researches.Count}");
            sb.AppendLine($"Continuous Researches: {_researches.Values.Count(r => r.IsContinuous)}");

            if (_researches.Any())
            {
                var oldestResearch = _researches.Values.OrderBy(r => r.CreatedAt).First();
                var newestResearch = _researches.Values.OrderByDescending(r => r.CreatedAt).First();

                sb.AppendLine($"Oldest Research: {oldestResearch.Name} ({oldestResearch.CreatedAt:yyyy-MM-dd})");
                sb.AppendLine($"Newest Research: {newestResearch.Name} ({newestResearch.CreatedAt:yyyy-MM-dd})");

                var executedResearches = _researches.Values.Where(r => r.LastExecutedAt.HasValue).ToList();
                if (executedResearches.Any())
                {
                    sb.AppendLine($"Executed Researches: {executedResearches.Count}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Lists all research queries with their basic information.
        /// </summary>
        /// <returns>A formatted string listing all researches.</returns>
        public string ListResearches()
        {
            if (!_researches.Any())
            {
                return "No research queries registered.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== Registered Research Queries ===");

            foreach (var kvp in _researches.OrderBy(r => r.Value.CreatedAt))
            {
                var research = kvp.Value;
                var status = research.IsContinuous ? "[CONTINUOUS]" : "[ON-DEMAND]";
                sb.AppendLine($"[{research.Id}] {status} {research.Name}");
                sb.AppendLine($"    Description: {research.Description}");
                sb.AppendLine($"    Created: {research.CreatedAt:yyyy-MM-dd HH:mm:ss}");

                if (research.LastExecutedAt.HasValue)
                {
                    sb.AppendLine($"    Last Executed: {research.LastExecutedAt.Value:yyyy-MM-dd HH:mm:ss}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private TLink GenerateNextId()
        {
            var result = _nextId;
            // Simple increment - works for numeric types
            if (typeof(TLink) == typeof(ulong))
            {
                _nextId = (TLink)(object)((ulong)(object)_nextId + 1);
            }
            else if (typeof(TLink) == typeof(long))
            {
                _nextId = (TLink)(object)((long)(object)_nextId + 1);
            }
            else if (typeof(TLink) == typeof(uint))
            {
                _nextId = (TLink)(object)((uint)(object)_nextId + 1);
            }
            else
            {
                // Default for int
                _nextId = (TLink)(object)((int)(object)_nextId + 1);
            }
            return result;
        }
    }
}
