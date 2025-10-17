using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Analyzer for grouping media event comments (e.g., YouTube video comments) by semantic similarity.
    /// Groups comment fragments that express similar meanings and counts their occurrences.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier used in the Links storage.</typeparam>
    public class MediaCommentsAnalyzer<TLink>
    {
        private readonly SynchronizedLinks<TLink> _links;
        private readonly TLink _commentType;
        private readonly TLink _fragmentType;
        private readonly TLink _semanticGroupType;
        private readonly TLink _occurrenceCountType;

        public MediaCommentsAnalyzer(SynchronizedLinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));

            // Initialize concept types in the Links storage
            _commentType = CreateOrGetConceptLink("Comment");
            _fragmentType = CreateOrGetConceptLink("Fragment");
            _semanticGroupType = CreateOrGetConceptLink("SemanticGroup");
            _occurrenceCountType = CreateOrGetConceptLink("OccurrenceCount");
        }

        /// <summary>
        /// Creates or retrieves a concept link by name.
        /// </summary>
        private TLink CreateOrGetConceptLink(string conceptName)
        {
            // In a real implementation, this would create a link representing the concept
            // For now, we create a simple link structure
            var nameLinks = ConvertStringToLinks(conceptName);
            return _links.Create();
        }

        /// <summary>
        /// Converts a string to a sequence of links representing the text.
        /// </summary>
        private TLink ConvertStringToLinks(string text)
        {
            // Simplified implementation - in production, use Platform.Data.Doublets.Unicode
            return _links.Create();
        }

        /// <summary>
        /// Analyzes a collection of comments and groups fragments by semantic similarity.
        /// </summary>
        /// <param name="comments">The comments to analyze.</param>
        /// <returns>A collection of semantic groups with their occurrence counts.</returns>
        public IEnumerable<SemanticGroup<TLink>> AnalyzeComments(IEnumerable<string> comments)
        {
            if (comments == null) throw new ArgumentNullException(nameof(comments));

            var fragmentGroups = new Dictionary<string, SemanticGroup<TLink>>();

            foreach (var comment in comments)
            {
                if (string.IsNullOrWhiteSpace(comment)) continue;

                // Store the comment in Links
                var commentLink = StoreComment(comment);

                // Extract and analyze fragments from the comment
                var fragments = ExtractFragments(comment);

                foreach (var fragment in fragments)
                {
                    // Normalize the fragment for grouping
                    var normalizedFragment = NormalizeFragment(fragment);

                    // Find or create semantic group
                    if (!fragmentGroups.TryGetValue(normalizedFragment, out var group))
                    {
                        group = new SemanticGroup<TLink>
                        {
                            RepresentativeText = fragment,
                            NormalizedForm = normalizedFragment,
                            Variations = new List<string>(),
                            Count = 0
                        };
                        fragmentGroups[normalizedFragment] = group;

                        // Store the semantic group in Links
                        group.LinkId = StoreSemanticGroup(normalizedFragment);
                    }

                    // Add variation if it's different from existing ones
                    if (!group.Variations.Contains(fragment))
                    {
                        group.Variations.Add(fragment);
                    }

                    group.Count++;

                    // Update the count in Links storage
                    UpdateGroupCount(group.LinkId, group.Count);
                }
            }

            // Return groups sorted by frequency (most common first)
            return fragmentGroups.Values.OrderByDescending(g => g.Count);
        }

        /// <summary>
        /// Stores a comment in the Links database.
        /// </summary>
        private TLink StoreComment(string comment)
        {
            var textLink = ConvertStringToLinks(comment);
            var commentLink = _links.Create();
            _links.Update(commentLink, _commentType, textLink);
            return commentLink;
        }

        /// <summary>
        /// Stores a semantic group in the Links database.
        /// </summary>
        private TLink StoreSemanticGroup(string normalizedText)
        {
            var textLink = ConvertStringToLinks(normalizedText);
            var groupLink = _links.Create();
            _links.Update(groupLink, _semanticGroupType, textLink);
            return groupLink;
        }

        /// <summary>
        /// Updates the occurrence count for a semantic group.
        /// </summary>
        private void UpdateGroupCount(TLink groupLink, int count)
        {
            // Create a link representing the count
            // In a full implementation, this would use Platform.Numbers for numeric representation
            var countLink = _links.Create();
            var associationLink = _links.Create();
            _links.Update(associationLink, groupLink, countLink);
        }

        /// <summary>
        /// Extracts meaningful fragments from a comment.
        /// </summary>
        private IEnumerable<string> ExtractFragments(string comment)
        {
            // Split by sentences and phrases
            var fragments = new List<string>();

            // Split by common sentence terminators
            var sentences = comment.Split(new[] { '.', '!', '?', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var sentence in sentences)
            {
                var trimmed = sentence.Trim();
                if (trimmed.Length > 0)
                {
                    fragments.Add(trimmed);

                    // Also extract sub-fragments (phrases separated by commas)
                    var phrases = trimmed.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (phrases.Length > 1)
                    {
                        foreach (var phrase in phrases)
                        {
                            var trimmedPhrase = phrase.Trim();
                            if (trimmedPhrase.Length > 10) // Minimum meaningful length
                            {
                                fragments.Add(trimmedPhrase);
                            }
                        }
                    }
                }
            }

            return fragments;
        }

        /// <summary>
        /// Normalizes a fragment for semantic grouping.
        /// This basic implementation uses case normalization and whitespace trimming.
        /// A production version would use more sophisticated NLP techniques.
        /// </summary>
        private string NormalizeFragment(string fragment)
        {
            // Convert to lowercase
            var normalized = fragment.ToLowerInvariant();

            // Normalize whitespace
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ");

            // Remove common punctuation from the end
            normalized = normalized.TrimEnd('.', '!', '?', ',', ';', ':');

            // Basic stemming/lemmatization simulation - remove common suffixes
            // In a real implementation, use a proper NLP library
            normalized = RemoveCommonSuffixes(normalized);

            return normalized.Trim();
        }

        /// <summary>
        /// Simple suffix removal for basic word normalization.
        /// </summary>
        private string RemoveCommonSuffixes(string text)
        {
            // This is a very basic implementation
            // For production, use proper stemming algorithms like Porter Stemmer
            var words = text.Split(' ');
            var processedWords = new List<string>();

            foreach (var word in words)
            {
                var processed = word;

                // Remove common English suffixes (very simplified)
                if (word.EndsWith("ing") && word.Length > 5)
                    processed = word.Substring(0, word.Length - 3);
                else if (word.EndsWith("ed") && word.Length > 4)
                    processed = word.Substring(0, word.Length - 2);
                else if (word.EndsWith("s") && word.Length > 3 && !word.EndsWith("ss"))
                    processed = word.Substring(0, word.Length - 1);

                processedWords.Add(processed);
            }

            return string.Join(" ", processedWords);
        }
    }

    /// <summary>
    /// Represents a group of semantically similar comment fragments.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class SemanticGroup<TLink>
    {
        /// <summary>
        /// The link ID in the Links database.
        /// </summary>
        public TLink LinkId { get; set; }

        /// <summary>
        /// A representative text for this semantic group.
        /// </summary>
        public string RepresentativeText { get; set; }

        /// <summary>
        /// The normalized form used for grouping.
        /// </summary>
        public string NormalizedForm { get; set; }

        /// <summary>
        /// Different variations of text expressing this meaning.
        /// </summary>
        public List<string> Variations { get; set; }

        /// <summary>
        /// Number of times this semantic meaning appears in the comments.
        /// </summary>
        public int Count { get; set; }

        public override string ToString()
        {
            return $"\"{RepresentativeText}\" (appears {Count} times, {Variations.Count} variations)";
        }
    }
}
