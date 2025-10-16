using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Unicode;
using Platform.Collections.Arrays;

namespace Platform.Examples
{
    /// <summary>
    /// A simple implementation of ReferenceService for managing small data type collections.
    /// Supports loading data from GitHub in LiNo format and serving it to other links users.
    /// </summary>
    public class SimpleReferenceService : ReferenceService<ulong>
    {
        private readonly Dictionary<string, ulong> _referenceMap;
        private readonly UnicodeMap _unicodeMap;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the SimpleReferenceService class.
        /// </summary>
        /// <param name="links">The links storage to use.</param>
        /// <param name="sequences">The sequences manager.</param>
        /// <param name="unicodeMap">The Unicode map for string conversion.</param>
        /// <param name="dataType">The type of data this service manages.</param>
        public SimpleReferenceService(ILinks<ulong> links, Sequences sequences, UnicodeMap unicodeMap, string dataType)
            : base(links, sequences, dataType)
        {
            _unicodeMap = unicodeMap ?? throw new ArgumentNullException(nameof(unicodeMap));
            _referenceMap = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Loads reference data from a GitHub repository in LiNo format.
        /// </summary>
        /// <param name="githubUrl">The URL to the raw GitHub file containing LiNo data.</param>
        public override void LoadFromGitHub(string githubUrl)
        {
            if (string.IsNullOrWhiteSpace(githubUrl))
            {
                throw new ArgumentException("GitHub URL cannot be null or empty.", nameof(githubUrl));
            }

            try
            {
                var linoContent = FetchFromGitHubAsync(githubUrl).GetAwaiter().GetResult();
                ParseAndStoreLinoData(linoContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load data from GitHub: {githubUrl}", ex);
            }
        }

        /// <summary>
        /// Serves a reference by its identifier.
        /// </summary>
        /// <param name="referenceId">The identifier of the reference to serve.</param>
        /// <returns>The reference data in LiNo format.</returns>
        public override string ServeReference(string referenceId)
        {
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                throw new ArgumentException("Reference ID cannot be null or empty.", nameof(referenceId));
            }

            if (_referenceMap.TryGetValue(referenceId, out var linkId))
            {
                return FormatLinkAsLiNo(referenceId, linkId);
            }

            return null;
        }

        /// <summary>
        /// Lists all available references in this collection.
        /// </summary>
        /// <returns>A collection of reference identifiers.</returns>
        public override IEnumerable<string> ListReferences()
        {
            return _referenceMap.Keys.OrderBy(k => k);
        }

        /// <summary>
        /// Exports the reference collection to LiNo format for GitHub storage.
        /// </summary>
        /// <returns>The collection in LiNo format.</returns>
        public override string ExportToLiNo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {_dataType} Reference Collection");
            sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            foreach (var reference in ListReferences())
            {
                if (_referenceMap.TryGetValue(reference, out var linkId))
                {
                    sb.AppendLine(FormatLinkAsLiNo(reference, linkId));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Adds a new reference to the collection.
        /// </summary>
        /// <param name="referenceId">The reference identifier.</param>
        /// <param name="value">The value to store.</param>
        public void AddReference(string referenceId, string value)
        {
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                throw new ArgumentException("Reference ID cannot be null or empty.", nameof(referenceId));
            }

            var sequenceArray = ConvertStringToSequence(value);
            var linkId = _sequences.Create(sequenceArray.ShiftRight());

            _referenceMap[referenceId] = linkId;
        }

        private async Task<string> FetchFromGitHubAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private void ParseAndStoreLinoData(string linoContent)
        {
            if (string.IsNullOrWhiteSpace(linoContent))
            {
                return;
            }

            var lines = linoContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                // Simple LiNo parsing: "referenceId (value: actual value)"
                // or just "referenceId value"
                var parts = ParseLinoLine(trimmedLine);
                if (parts.referenceId != null && parts.value != null)
                {
                    AddReference(parts.referenceId, parts.value);
                }
            }
        }

        private (string referenceId, string value) ParseLinoLine(string line)
        {
            // Simple parser for basic LiNo format
            // Supports: "id value" or "id (label: value)"

            var parenIndex = line.IndexOf('(');
            if (parenIndex > 0)
            {
                var referenceId = line.Substring(0, parenIndex).Trim();
                var valueStart = line.IndexOf(':', parenIndex);
                var valueEnd = line.LastIndexOf(')');

                if (valueStart > 0 && valueEnd > valueStart)
                {
                    var value = line.Substring(valueStart + 1, valueEnd - valueStart - 1).Trim();
                    return (referenceId, value);
                }
            }
            else
            {
                var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return (parts[0], parts[1]);
                }
            }

            return (null, null);
        }

        private string FormatLinkAsLiNo(string referenceId, ulong linkId)
        {
            var sequence = _sequences.FormatSequence(linkId, AppendLinkToString, false);
            return $"{referenceId} ({_dataType}: {sequence})";
        }

        private ulong[] ConvertStringToSequence(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<ulong>();
            }

            var result = new ulong[text.Length];
            for (var i = 0; i < text.Length; i++)
            {
                result[i] = UnicodeMap.FromCharToLink(text[i]);
            }
            return result;
        }

        private static void AppendLinkToString(StringBuilder sb, ulong link)
        {
            if (link <= (char.MaxValue + 1))
            {
                sb.Append(UnicodeMap.FromLinkToChar(link));
            }
            else
            {
                sb.Append($"({link})");
            }
        }
    }
}
