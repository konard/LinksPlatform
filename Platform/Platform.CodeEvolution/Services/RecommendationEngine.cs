using Platform.CodeEvolution.Models;
using System.Text.RegularExpressions;

namespace Platform.CodeEvolution.Services;

public class RecommendationEngine
{
    private readonly ChangeFrequencyTracker _frequencyTracker;
    private readonly Dictionary<string, List<RecommendationRule>> _rules = new();

    public RecommendationEngine(ChangeFrequencyTracker frequencyTracker)
    {
        _frequencyTracker = frequencyTracker;
        InitializeDefaultRules();
    }

    public List<CodeRecommendation> GetRecommendations(string code, string language, List<string>? dependencies = null)
    {
        var recommendations = new List<CodeRecommendation>();

        var applicableChanges = _frequencyTracker.SearchChanges(code, language);

        foreach (var change in applicableChanges.Take(10))
        {
            if (IsApplicable(code, change))
            {
                var recommendation = CreateRecommendation(code, change);
                if (recommendation != null)
                {
                    recommendations.Add(recommendation);
                }
            }
        }

        var ruleBasedRecommendations = ApplyRules(code, language, dependencies);
        recommendations.AddRange(ruleBasedRecommendations);

        return recommendations
            .OrderByDescending(r => r.Confidence * r.Frequency)
            .Take(5)
            .ToList();
    }

    public List<CodeRecommendation> GetBestPractices(string language, List<string>? dependencies = null)
    {
        var topChanges = _frequencyTracker.GetTopChanges(50, language, dependencies);
        var recommendations = new List<CodeRecommendation>();

        foreach (var change in topChanges)
        {
            if (change.Count >= 5 && change.Confidence > 0.7)
            {
                recommendations.Add(new CodeRecommendation
                {
                    Type = RecommendationType.BestPractice,
                    Title = $"Popular pattern: {change.Pattern}",
                    Description = $"This change pattern has been used {change.Count} times by {change.Authors.Count} different authors",
                    BeforeCode = change.FromCode,
                    AfterCode = change.ToCode,
                    Confidence = change.Confidence,
                    Frequency = change.Count,
                    Language = change.Language,
                    Dependencies = change.Dependencies,
                    Examples = change.CommitShas.Take(3).ToList()
                });
            }
        }

        return recommendations.OrderByDescending(r => r.Frequency).Take(10).ToList();
    }

    public List<CodeRecommendation> GetEvolutionPath(string functionName, string fileName)
    {
        var chains = _frequencyTracker.GetTopChains(20, false);
        var relevantChains = chains.Where(c =>
            c.Changes.Any(change =>
                change.FunctionName.Contains(functionName, StringComparison.OrdinalIgnoreCase) ||
                change.FileName.Contains(fileName, StringComparison.OrdinalIgnoreCase))).ToList();

        var recommendations = new List<CodeRecommendation>();

        foreach (var chain in relevantChains.Take(5))
        {
            if (chain.Changes.Count >= 2)
            {
                recommendations.Add(new CodeRecommendation
                {
                    Type = RecommendationType.Evolution,
                    Title = $"Evolution path for {functionName}",
                    Description = $"This function evolved through {chain.Changes.Count} changes with quality score {chain.Quality:F2}",
                    BeforeCode = chain.Changes.First().BeforeCode,
                    AfterCode = chain.FinalCode,
                    Confidence = chain.Quality,
                    Frequency = chain.Changes.Count,
                    Language = chain.Changes.First().Language,
                    Dependencies = chain.Changes.SelectMany(c => c.Dependencies).Distinct().ToList(),
                    Examples = chain.Changes.Select(c => c.CommitSha).ToList(),
                    Alternatives = chain.Alternatives.Select(alt => new CodeRecommendation
                    {
                        Type = RecommendationType.Alternative,
                        Title = "Alternative approach",
                        BeforeCode = alt.Changes.First().BeforeCode,
                        AfterCode = alt.FinalCode,
                        Confidence = alt.Quality,
                        Language = alt.Changes.First().Language
                    }).ToList()
                });
            }
        }

        return recommendations;
    }

    private bool IsApplicable(string code, ChangeFrequency change)
    {
        var normalizedCode = NormalizeCode(code);
        var normalizedFromCode = NormalizeCode(change.FromCode);

        var similarity = CalculateSimilarity(normalizedCode, normalizedFromCode);
        return similarity > 0.6;
    }

    private CodeRecommendation? CreateRecommendation(string code, ChangeFrequency change)
    {
        if (change.Count < 2 || change.Confidence < 0.5)
            return null;

        return new CodeRecommendation
        {
            Type = RecommendationType.Improvement,
            Title = $"Suggested improvement based on {change.Count} similar changes",
            Description = $"This pattern has been successfully applied {change.Count} times",
            BeforeCode = change.FromCode,
            AfterCode = change.ToCode,
            Confidence = change.Confidence,
            Frequency = change.Count,
            Language = change.Language,
            Dependencies = change.Dependencies,
            Examples = change.CommitShas.Take(3).ToList()
        };
    }

    private List<CodeRecommendation> ApplyRules(string code, string language, List<string>? dependencies)
    {
        var recommendations = new List<CodeRecommendation>();

        if (_rules.ContainsKey(language))
        {
            foreach (var rule in _rules[language])
            {
                if (rule.IsApplicable(code, dependencies))
                {
                    var recommendation = rule.Apply(code);
                    if (recommendation != null)
                    {
                        recommendations.Add(recommendation);
                    }
                }
            }
        }

        return recommendations;
    }

    private void InitializeDefaultRules()
    {
        _rules["C#"] = new List<RecommendationRule>
        {
            new RecommendationRule
            {
                Name = "Use var for obvious types",
                Pattern = @"(\w+)\s+(\w+)\s*=\s*new\s+\1\s*\(",
                Replacement = "var $2 = new $1(",
                Description = "Use 'var' when the type is obvious from the right side",
                IsApplicable = (code, deps) => Regex.IsMatch(code, @"(\w+)\s+(\w+)\s*=\s*new\s+\1\s*\("),
                Apply = (code) => new CodeRecommendation
                {
                    Type = RecommendationType.StyleImprovement,
                    Title = "Use var for obvious types",
                    Description = "Use 'var' when the type is obvious from the right side",
                    BeforeCode = code,
                    AfterCode = Regex.Replace(code, @"(\w+)\s+(\w+)\s*=\s*new\s+\1\s*\(", "var $2 = new $1("),
                    Confidence = 0.8,
                    Language = "C#"
                }
            },
            new RecommendationRule
            {
                Name = "Use string interpolation",
                Pattern = @"string\.Format\s*\(",
                Replacement = "$\"",
                Description = "Use string interpolation instead of string.Format",
                IsApplicable = (code, deps) => code.Contains("string.Format("),
                Apply = (code) => new CodeRecommendation
                {
                    Type = RecommendationType.Modernization,
                    Title = "Use string interpolation",
                    Description = "String interpolation is more readable than string.Format",
                    BeforeCode = code,
                    AfterCode = "// Consider using string interpolation: $\"text {variable}\"",
                    Confidence = 0.7,
                    Language = "C#"
                }
            }
        };

        _rules["JavaScript"] = new List<RecommendationRule>
        {
            new RecommendationRule
            {
                Name = "Use const for non-reassigned variables",
                Pattern = @"let\s+(\w+)\s*=",
                Replacement = "const $1 =",
                Description = "Use 'const' for variables that are not reassigned",
                IsApplicable = (code, deps) => code.Contains("let ") && !ContainsReassignment(code),
                Apply = (code) => new CodeRecommendation
                {
                    Type = RecommendationType.BestPractice,
                    Title = "Use const for non-reassigned variables",
                    Description = "Using 'const' makes intent clearer and prevents accidental reassignment",
                    BeforeCode = code,
                    AfterCode = code.Replace("let ", "const "),
                    Confidence = 0.8,
                    Language = "JavaScript"
                }
            }
        };
    }

    private bool ContainsReassignment(string code)
    {
        var variables = Regex.Matches(code, @"let\s+(\w+)")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value);

        foreach (var variable in variables)
        {
            if (Regex.IsMatch(code, $@"\b{variable}\s*=(?!=)"))
            {
                return true;
            }
        }

        return false;
    }

    private double CalculateSimilarity(string text1, string text2)
    {
        if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            return 0.0;

        var tokens1 = TokenizeCode(text1);
        var tokens2 = TokenizeCode(text2);

        var intersection = tokens1.Intersect(tokens2).Count();
        var union = tokens1.Union(tokens2).Count();

        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private List<string> TokenizeCode(string code)
    {
        return Regex.Matches(code, @"\b\w+\b")
            .Cast<Match>()
            .Select(m => m.Value.ToLowerInvariant())
            .ToList();
    }

    private string NormalizeCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return string.Empty;

        code = Regex.Replace(code, @"\s+", " ");
        code = code.Trim();
        return code;
    }
}

public class CodeRecommendation
{
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BeforeCode { get; set; } = string.Empty;
    public string AfterCode { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Frequency { get; set; }
    public string Language { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public List<string> Examples { get; set; } = new();
    public List<CodeRecommendation> Alternatives { get; set; } = new();
}

public enum RecommendationType
{
    Improvement,
    BestPractice,
    StyleImprovement,
    Modernization,
    Evolution,
    Alternative
}

public class RecommendationRule
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Func<string, List<string>?, bool> IsApplicable { get; set; } = (code, deps) => false;
    public Func<string, CodeRecommendation?> Apply { get; set; } = code => null;
}