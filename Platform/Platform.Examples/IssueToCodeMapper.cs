using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    public class IssueToCodeMapper
    {
        private readonly string _codebaseRoot;
        private readonly string[] _searchableExtensions = { ".cs", ".js", ".ts", ".py", ".cpp", ".h", ".md", ".txt" };

        public IssueToCodeMapper(string codebaseRoot)
        {
            _codebaseRoot = codebaseRoot ?? throw new ArgumentNullException(nameof(codebaseRoot));
        }

        public IssueToCodeMappingResult MapIssueToCode(string issueText, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(issueText))
                throw new ArgumentException("Issue text cannot be null or empty", nameof(issueText));

            var words = ExtractWords(issueText);
            var fileScores = new Dictionary<string, FileScore>();

            foreach (var file in GetSourceFiles())
            {
                var score = CalculateFileScore(file, words);
                if (score.TotalScore > 0)
                {
                    fileScores[file] = score;
                }
            }

            var topFiles = fileScores
                .OrderByDescending(kvp => kvp.Value.TotalScore)
                .Take(maxResults)
                .Select(kvp => new FileMatch
                {
                    FilePath = kvp.Key,
                    Score = kvp.Value.TotalScore,
                    MaxSequenceLength = kvp.Value.MaxSequenceLength,
                    MatchedWords = kvp.Value.MatchedWords,
                    LineMatches = kvp.Value.LineMatches
                })
                .ToList();

            return new IssueToCodeMappingResult
            {
                Query = issueText,
                ExtractedWords = words,
                Results = topFiles,
                TotalFilesScanned = fileScores.Count
            };
        }

        private List<string> ExtractWords(string text)
        {
            var words = Regex.Matches(text.ToLowerInvariant(), @"\b[a-zA-Z][a-zA-Z0-9_]*\b")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(w => w.Length > 2) // Filter out very short words
                .Distinct()
                .OrderByDescending(w => w.Length) // Longer words first for better matching
                .ToList();

            return words;
        }

        private IEnumerable<string> GetSourceFiles()
        {
            if (!Directory.Exists(_codebaseRoot))
                return Enumerable.Empty<string>();

            return Directory.GetFiles(_codebaseRoot, "*", SearchOption.AllDirectories)
                .Where(file => _searchableExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .Where(file => !IsExcludedPath(file));
        }

        private bool IsExcludedPath(string filePath)
        {
            var excludedPaths = new[] { "bin", "obj", "packages", "node_modules", ".git", ".vs" };
            return excludedPaths.Any(excluded => filePath.Contains(Path.DirectorySeparatorChar + excluded + Path.DirectorySeparatorChar));
        }

        private FileScore CalculateFileScore(string filePath, List<string> words)
        {
            var score = new FileScore();

            try
            {
                var lines = File.ReadAllLines(filePath);
                var fileContent = string.Join(" ", lines).ToLowerInvariant();

                // Track word positions for sequence detection
                var wordPositions = new Dictionary<string, List<int>>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].ToLowerInvariant();
                    var lineMatches = new List<string>();

                    foreach (var word in words)
                    {
                        if (line.Contains(word))
                        {
                            lineMatches.Add(word);
                            score.MatchedWords.Add(word);

                            // Record word positions
                            if (!wordPositions.ContainsKey(word))
                                wordPositions[word] = new List<int>();
                            wordPositions[word].Add(i);
                        }
                    }

                    if (lineMatches.Any())
                    {
                        score.LineMatches.Add(new LineMatch
                        {
                            LineNumber = i + 1,
                            LineContent = lines[i].Trim(),
                            MatchedWords = lineMatches
                        });
                    }
                }

                // Calculate sequence scores
                score.MaxSequenceLength = CalculateMaxSequenceLength(words, wordPositions);
                score.TotalScore = CalculateTotalScore(score.MatchedWords.Count, score.MaxSequenceLength, words.Count);
            }
            catch (Exception)
            {
                // Skip files that can't be read
                return new FileScore();
            }

            return score;
        }

        private int CalculateMaxSequenceLength(List<string> words, Dictionary<string, List<int>> wordPositions)
        {
            if (wordPositions.Count < 2)
                return wordPositions.Count;

            int maxSequence = 1;

            // Find the longest sequence of consecutive words
            for (int i = 0; i < words.Count - 1; i++)
            {
                var currentSequence = 1;
                var currentWord = words[i];

                if (!wordPositions.ContainsKey(currentWord))
                    continue;

                for (int j = i + 1; j < words.Count; j++)
                {
                    var nextWord = words[j];
                    if (!wordPositions.ContainsKey(nextWord))
                        break;

                    // Check if next word appears close to current word
                    bool foundCloseMatch = false;
                    foreach (var currentPos in wordPositions[currentWord])
                    {
                        foreach (var nextPos in wordPositions[nextWord])
                        {
                            if (Math.Abs(nextPos - currentPos) <= 10) // Within 10 lines
                            {
                                foundCloseMatch = true;
                                break;
                            }
                        }
                        if (foundCloseMatch) break;
                    }

                    if (foundCloseMatch)
                    {
                        currentSequence++;
                        currentWord = nextWord;
                    }
                    else
                    {
                        break;
                    }
                }

                maxSequence = Math.Max(maxSequence, currentSequence);
            }

            return maxSequence;
        }

        private double CalculateTotalScore(int matchedWordsCount, int maxSequenceLength, int totalWords)
        {
            if (matchedWordsCount == 0)
                return 0;

            // Base score from matched words ratio
            double wordRatio = (double)matchedWordsCount / totalWords;

            // Bonus for sequence length (longer sequences are more relevant)
            double sequenceBonus = maxSequenceLength > 1 ? Math.Log(maxSequenceLength) : 0;

            // Final score combining both factors
            return (wordRatio * 100) + (sequenceBonus * 20);
        }
    }

    public class IssueToCodeMappingResult
    {
        public string Query { get; set; }
        public List<string> ExtractedWords { get; set; } = new List<string>();
        public List<FileMatch> Results { get; set; } = new List<FileMatch>();
        public int TotalFilesScanned { get; set; }
    }

    public class FileMatch
    {
        public string FilePath { get; set; }
        public double Score { get; set; }
        public int MaxSequenceLength { get; set; }
        public HashSet<string> MatchedWords { get; set; } = new HashSet<string>();
        public List<LineMatch> LineMatches { get; set; } = new List<LineMatch>();
    }

    public class LineMatch
    {
        public int LineNumber { get; set; }
        public string LineContent { get; set; }
        public List<string> MatchedWords { get; set; } = new List<string>();
    }

    internal class FileScore
    {
        public HashSet<string> MatchedWords { get; set; } = new HashSet<string>();
        public List<LineMatch> LineMatches { get; set; } = new List<LineMatch>();
        public int MaxSequenceLength { get; set; }
        public double TotalScore { get; set; }
    }
}