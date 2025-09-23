using Platform.CodeEvolution.Models;
using System.Text.RegularExpressions;

namespace Platform.CodeEvolution.Services;

public class ChangeFrequencyTracker
{
    private readonly Dictionary<string, ChangeFrequency> _changeFrequencies = new();
    private readonly Dictionary<string, ChangeChain> _changeChains = new();

    public void ProcessChanges(List<CodeChange> changes)
    {
        foreach (var change in changes)
        {
            ProcessSingleChange(change);
        }

        BuildChangeChains(changes);
    }

    private void ProcessSingleChange(CodeChange change)
    {
        if (string.IsNullOrEmpty(change.BeforeCode) || string.IsNullOrEmpty(change.AfterCode))
            return;

        var pattern = GenerateChangePattern(change.BeforeCode, change.AfterCode);
        var key = $"{pattern}|{change.Language}|{string.Join(",", change.Dependencies.Take(3))}";

        if (_changeFrequencies.ContainsKey(key))
        {
            var frequency = _changeFrequencies[key];
            frequency.Count++;
            frequency.CommitShas.Add(change.CommitSha);
            frequency.LastSeen = change.ChangeDate;

            if (!frequency.Authors.Contains(change.Author))
            {
                frequency.Authors.Add(change.Author);
            }
        }
        else
        {
            _changeFrequencies[key] = new ChangeFrequency
            {
                Pattern = pattern,
                FromCode = NormalizeCode(change.BeforeCode),
                ToCode = NormalizeCode(change.AfterCode),
                Count = 1,
                CommitShas = new List<string> { change.CommitSha },
                Language = change.Language,
                Dependencies = change.Dependencies.Take(3).ToList(),
                Confidence = CalculateConfidence(change),
                FirstSeen = change.ChangeDate,
                LastSeen = change.ChangeDate,
                Authors = new List<string> { change.Author }
            };
        }
    }

    private void BuildChangeChains(List<CodeChange> changes)
    {
        var functionGroups = changes
            .Where(c => !string.IsNullOrEmpty(c.FunctionName))
            .GroupBy(c => $"{c.FileName}|{c.FunctionName}")
            .Where(g => g.Count() > 1);

        foreach (var group in functionGroups)
        {
            var orderedChanges = group.OrderBy(c => c.ChangeDate).ToList();
            var chainId = group.Key;

            var chain = new ChangeChain
            {
                Id = chainId,
                Changes = orderedChanges,
                FinalCode = orderedChanges.Last().AfterCode,
                Quality = CalculateChainQuality(orderedChanges),
                IsComplete = IsChainComplete(orderedChanges)
            };

            _changeChains[chainId] = chain;
        }

        IdentifyAlternatives();
    }

    private void IdentifyAlternatives()
    {
        var similarChains = _changeChains.Values
            .GroupBy(c => NormalizeCode(c.FinalCode))
            .Where(g => g.Count() > 1);

        foreach (var group in similarChains)
        {
            var chains = group.OrderByDescending(c => c.Quality).ToList();
            var bestChain = chains.First();

            for (int i = 1; i < chains.Count; i++)
            {
                bestChain.Alternatives.Add(chains[i]);
            }
        }
    }

    public List<ChangeFrequency> GetTopChanges(int count = 100, string? language = null, List<string>? dependencies = null)
    {
        var query = _changeFrequencies.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(cf => cf.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
        }

        if (dependencies != null && dependencies.Count > 0)
        {
            query = query.Where(cf => dependencies.Any(dep => cf.Dependencies.Contains(dep)));
        }

        return query
            .OrderByDescending(cf => cf.Count * cf.Confidence)
            .Take(count)
            .ToList();
    }

    public List<ChangeChain> GetTopChains(int count = 50, bool completeChainsOnly = true)
    {
        var query = _changeChains.Values.AsEnumerable();

        if (completeChainsOnly)
        {
            query = query.Where(cc => cc.IsComplete);
        }

        return query
            .OrderByDescending(cc => cc.Quality)
            .Take(count)
            .ToList();
    }

    public List<ChangeFrequency> SearchChanges(string searchPattern, string? language = null)
    {
        var query = _changeFrequencies.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(cf => cf.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .Where(cf =>
                cf.FromCode.Contains(searchPattern, StringComparison.OrdinalIgnoreCase) ||
                cf.ToCode.Contains(searchPattern, StringComparison.OrdinalIgnoreCase) ||
                cf.Pattern.Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(cf => cf.Count * cf.Confidence)
            .ToList();
    }

    private string GenerateChangePattern(string beforeCode, string afterCode)
    {
        var beforeNormalized = NormalizeCode(beforeCode);
        var afterNormalized = NormalizeCode(afterCode);

        var beforeTokens = TokenizeCode(beforeNormalized);
        var afterTokens = TokenizeCode(afterNormalized);

        return $"{string.Join(" ", beforeTokens.Take(5))} -> {string.Join(" ", afterTokens.Take(5))}";
    }

    private string NormalizeCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return string.Empty;

        code = Regex.Replace(code, @"\s+", " ");
        code = code.Trim();
        code = Regex.Replace(code, @"//.*$", "", RegexOptions.Multiline);
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);

        return code;
    }

    private List<string> TokenizeCode(string code)
    {
        var tokens = Regex.Matches(code, @"\b\w+\b")
            .Cast<Match>()
            .Select(m => m.Value.ToLowerInvariant())
            .Where(token => !IsCommonKeyword(token))
            .ToList();

        return tokens;
    }

    private bool IsCommonKeyword(string token)
    {
        var commonKeywords = new HashSet<string>
        {
            "if", "else", "for", "while", "do", "switch", "case", "break", "continue",
            "return", "public", "private", "protected", "static", "class", "interface",
            "function", "var", "let", "const", "def", "import", "from", "as"
        };

        return commonKeywords.Contains(token);
    }

    private double CalculateConfidence(CodeChange change)
    {
        double confidence = 0.5;

        if (!string.IsNullOrEmpty(change.CommitMessage))
        {
            confidence += 0.1;
        }

        if (change.Dependencies.Count > 0)
        {
            confidence += 0.1;
        }

        if (!string.IsNullOrEmpty(change.FunctionName))
        {
            confidence += 0.2;
        }

        var codeComplexity = Math.Min(change.AfterCode.Length / 100.0, 0.2);
        confidence += codeComplexity;

        return Math.Min(confidence, 1.0);
    }

    private double CalculateChainQuality(List<CodeChange> changes)
    {
        if (changes.Count == 0)
            return 0.0;

        double quality = 0.0;

        quality += Math.Min(changes.Count / 10.0, 0.3);

        var uniqueAuthors = changes.Select(c => c.Author).Distinct().Count();
        quality += Math.Min(uniqueAuthors / 5.0, 0.2);

        var timeSpan = changes.Last().ChangeDate - changes.First().ChangeDate;
        quality += Math.Min(timeSpan.TotalDays / 365.0, 0.2);

        var avgConfidence = changes.Average(c => CalculateConfidence(c));
        quality += avgConfidence * 0.3;

        return Math.Min(quality, 1.0);
    }

    private bool IsChainComplete(List<CodeChange> changes)
    {
        if (changes.Count < 2)
            return false;

        var lastChange = changes.Last();
        var daysSinceLastChange = (DateTime.Now - lastChange.ChangeDate).TotalDays;

        return daysSinceLastChange > 30;
    }

    public Dictionary<string, object> GetStatistics()
    {
        var stats = new Dictionary<string, object>
        {
            ["TotalChangePatterns"] = _changeFrequencies.Count,
            ["TotalChangeChains"] = _changeChains.Count,
            ["LanguageDistribution"] = _changeFrequencies.Values
                .GroupBy(cf => cf.Language)
                .ToDictionary(g => g.Key, g => g.Count()),
            ["TopDependencies"] = _changeFrequencies.Values
                .SelectMany(cf => cf.Dependencies)
                .GroupBy(dep => dep)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count()),
            ["MostActiveAuthors"] = _changeFrequencies.Values
                .SelectMany(cf => cf.Authors)
                .GroupBy(author => author)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }
}