using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace Platform.Examples
{
    /// <summary>
    /// Searches for similar code snippets across GitHub repositories to support bug fixing by analogy.
    /// Uses GitHub's code search to find code patterns similar to a given snippet.
    /// </summary>
    public class CodeSimilaritySearcher
    {
        private readonly string _githubToken;

        public CodeSimilaritySearcher(string githubToken = null)
        {
            _githubToken = githubToken ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        }

        /// <summary>
        /// Searches for similar code snippets on GitHub.
        /// </summary>
        /// <param name="codeSnippet">The code snippet to search for</param>
        /// <param name="language">Programming language filter (e.g., "C#", "Python", "JavaScript")</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <returns>List of similar code results</returns>
        public List<CodeSearchResult> SearchSimilarCode(string codeSnippet, string language = null, int limit = 10)
        {
            var results = new List<CodeSearchResult>();

            try
            {
                // Extract key terms from the code snippet for search
                var searchTerms = ExtractSearchTerms(codeSnippet);

                if (searchTerms.Count == 0)
                {
                    Console.WriteLine("Warning: No significant search terms found in the code snippet.");
                    return results;
                }

                // Build GitHub code search query
                var query = string.Join(" ", searchTerms.Take(5)); // Use top 5 terms

                if (!string.IsNullOrEmpty(language))
                {
                    query += $" language:{NormalizeLanguage(language)}";
                }

                // Execute GitHub CLI search
                var ghArgs = $"search code --limit {limit} \"{query}\" --json path,repository,url";
                var output = ExecuteCommand("gh", ghArgs);

                if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var searchResults = JsonSerializer.Deserialize<List<GitHubSearchResult>>(output);

                        if (searchResults != null)
                        {
                            results = searchResults.Select(r => new CodeSearchResult
                            {
                                Repository = r.repository?.nameWithOwner ?? "Unknown",
                                FilePath = r.path ?? "Unknown",
                                Url = r.url ?? string.Empty,
                                Similarity = CalculateSimilarity(codeSnippet, searchTerms)
                            }).ToList();
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error parsing search results: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for similar code: {ex.Message}");
            }

            return results;
        }

        private List<string> ExtractSearchTerms(string codeSnippet)
        {
            var terms = new List<string>();

            // Common programming keywords to include in search
            var keywords = new[]
            {
                "class", "interface", "function", "void", "return", "if", "else", "for", "while",
                "try", "catch", "throw", "async", "await", "const", "var", "let", "public", "private"
            };

            var words = codeSnippet.Split(new[] { ' ', '\n', '\r', '\t', '(', ')', '{', '}', ';', ',', '.' },
                StringSplitOptions.RemoveEmptyEntries);

            // Extract identifiers and keywords
            foreach (var word in words)
            {
                if (word.Length >= 3 && !IsCommonWord(word))
                {
                    if (keywords.Contains(word.ToLower()) || char.IsUpper(word[0]) || word.Contains("_"))
                    {
                        terms.Add(word);
                    }
                }
            }

            return terms.Distinct().ToList();
        }

        private bool IsCommonWord(string word)
        {
            var commonWords = new[] { "the", "and", "for", "with", "this", "that", "from", "have", "has" };
            return commonWords.Contains(word.ToLower());
        }

        private string NormalizeLanguage(string language)
        {
            // Normalize language names for GitHub search
            var languageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "C#", "csharp" },
                { "CSharp", "csharp" },
                { "JavaScript", "javascript" },
                { "TypeScript", "typescript" },
                { "Python", "python" },
                { "Java", "java" },
                { "C++", "cpp" },
                { "CPP", "cpp" }
            };

            return languageMap.TryGetValue(language, out var normalized) ? normalized : language.ToLower();
        }

        private double CalculateSimilarity(string original, List<string> extractedTerms)
        {
            // Simple similarity metric based on term coverage
            // In a real implementation, this would be more sophisticated
            return extractedTerms.Count > 0 ? Math.Min(100.0, extractedTerms.Count * 10.0) : 0.0;
        }

        private string ExecuteCommand(string command, string arguments)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        return string.Empty;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
                    {
                        Console.WriteLine($"Command error: {error}");
                        return string.Empty;
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
                return string.Empty;
            }
        }

        // Helper classes for JSON deserialization
        private class GitHubSearchResult
        {
            public string path { get; set; }
            public Repository repository { get; set; }
            public string url { get; set; }
        }

        private class Repository
        {
            public string nameWithOwner { get; set; }
        }
    }

    /// <summary>
    /// Represents a code search result from GitHub.
    /// </summary>
    public class CodeSearchResult
    {
        public string Repository { get; set; }
        public string FilePath { get; set; }
        public string Url { get; set; }
        public double Similarity { get; set; }

        public override string ToString()
        {
            return $"Repository: {Repository}\n  File: {FilePath}\n  URL: {Url}\n  Similarity: {Similarity:F1}%";
        }
    }
}
