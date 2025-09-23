using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Platform.CodeEvolution.Services;

public class CodeAnalyzer
{
    public async Task<List<string>> ExtractDependenciesAsync(string code, string language)
    {
        var dependencies = new List<string>();

        switch (language)
        {
            case "C#":
                dependencies.AddRange(ExtractCSharpDependencies(code));
                break;
            case "JavaScript":
            case "TypeScript":
                dependencies.AddRange(ExtractJavaScriptDependencies(code));
                break;
            case "Python":
                dependencies.AddRange(ExtractPythonDependencies(code));
                break;
            case "Java":
                dependencies.AddRange(ExtractJavaDependencies(code));
                break;
            default:
                dependencies.AddRange(ExtractGenericDependencies(code));
                break;
        }

        return dependencies.Distinct().ToList();
    }

    public async Task<List<FunctionChange>> AnalyzeFunctionChangesAsync(string oldCode, string newCode, string language)
    {
        var changes = new List<FunctionChange>();

        switch (language)
        {
            case "C#":
                changes.AddRange(AnalyzeCSharpFunctionChanges(oldCode, newCode));
                break;
            default:
                changes.AddRange(AnalyzeGenericFunctionChanges(oldCode, newCode));
                break;
        }

        return changes;
    }

    private List<string> ExtractCSharpDependencies(string code)
    {
        var dependencies = new List<string>();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();

            var usingDirectives = root.Usings.Select(u => u.Name?.ToString()).Where(name => !string.IsNullOrEmpty(name));
            dependencies.AddRange(usingDirectives!);

            var packageReferences = Regex.Matches(code, @"PackageReference.*Include=""([^""]+)""")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value);
            dependencies.AddRange(packageReferences);
        }
        catch
        {
            // Fallback to regex parsing if syntax analysis fails
            dependencies.AddRange(ExtractGenericDependencies(code));
        }

        return dependencies;
    }

    private List<string> ExtractJavaScriptDependencies(string code)
    {
        var dependencies = new List<string>();

        var importMatches = Regex.Matches(code, @"import .* from ['""]([^'""]+)['""]");
        dependencies.AddRange(importMatches.Cast<Match>().Select(m => m.Groups[1].Value));

        var requireMatches = Regex.Matches(code, @"require\(['""]([^'""]+)['""]\)");
        dependencies.AddRange(requireMatches.Cast<Match>().Select(m => m.Groups[1].Value));

        return dependencies;
    }

    private List<string> ExtractPythonDependencies(string code)
    {
        var dependencies = new List<string>();

        var importMatches = Regex.Matches(code, @"import ([a-zA-Z_][a-zA-Z0-9_.]*)");
        dependencies.AddRange(importMatches.Cast<Match>().Select(m => m.Groups[1].Value));

        var fromImportMatches = Regex.Matches(code, @"from ([a-zA-Z_][a-zA-Z0-9_.]*)");
        dependencies.AddRange(fromImportMatches.Cast<Match>().Select(m => m.Groups[1].Value));

        return dependencies;
    }

    private List<string> ExtractJavaDependencies(string code)
    {
        var dependencies = new List<string>();

        var importMatches = Regex.Matches(code, @"import ([a-zA-Z_][a-zA-Z0-9_.]*);");
        dependencies.AddRange(importMatches.Cast<Match>().Select(m => m.Groups[1].Value));

        return dependencies;
    }

    private List<string> ExtractGenericDependencies(string code)
    {
        var dependencies = new List<string>();

        var includeMatches = Regex.Matches(code, @"#include [<""]([^>""]+)[>""]");
        dependencies.AddRange(includeMatches.Cast<Match>().Select(m => m.Groups[1].Value));

        return dependencies;
    }

    private List<FunctionChange> AnalyzeCSharpFunctionChanges(string oldCode, string newCode)
    {
        var changes = new List<FunctionChange>();

        try
        {
            var oldFunctions = ExtractCSharpFunctions(oldCode);
            var newFunctions = ExtractCSharpFunctions(newCode);

            foreach (var newFunc in newFunctions)
            {
                var oldFunc = oldFunctions.FirstOrDefault(f => f.Name == newFunc.Name);
                if (oldFunc == null)
                {
                    changes.Add(new FunctionChange
                    {
                        FunctionName = newFunc.Name,
                        BeforeCode = string.Empty,
                        AfterCode = newFunc.Body,
                        LineNumber = newFunc.LineNumber
                    });
                }
                else if (oldFunc.Body != newFunc.Body)
                {
                    changes.Add(new FunctionChange
                    {
                        FunctionName = newFunc.Name,
                        BeforeCode = oldFunc.Body,
                        AfterCode = newFunc.Body,
                        LineNumber = newFunc.LineNumber
                    });
                }
            }
        }
        catch
        {
            changes.AddRange(AnalyzeGenericFunctionChanges(oldCode, newCode));
        }

        return changes;
    }

    private List<FunctionInfo> ExtractCSharpFunctions(string code)
    {
        var functions = new List<FunctionInfo>();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var lineNumber = tree.GetLineSpan(method.Span).StartLinePosition.Line + 1;
                functions.Add(new FunctionInfo
                {
                    Name = method.Identifier.ValueText,
                    Body = method.ToString(),
                    LineNumber = lineNumber
                });
            }
        }
        catch
        {
            // Fallback to regex parsing
        }

        return functions;
    }

    private List<FunctionChange> AnalyzeGenericFunctionChanges(string oldCode, string newCode)
    {
        var changes = new List<FunctionChange>();

        var oldLines = oldCode.Split('\n');
        var newLines = newCode.Split('\n');

        for (int i = 0; i < Math.Max(oldLines.Length, newLines.Length); i++)
        {
            var oldLine = i < oldLines.Length ? oldLines[i] : string.Empty;
            var newLine = i < newLines.Length ? newLines[i] : string.Empty;

            if (oldLine != newLine && IsFunctionLine(newLine))
            {
                changes.Add(new FunctionChange
                {
                    FunctionName = ExtractFunctionName(newLine),
                    BeforeCode = oldLine,
                    AfterCode = newLine,
                    LineNumber = i + 1
                });
            }
        }

        return changes;
    }

    private bool IsFunctionLine(string line)
    {
        var functionPatterns = new[]
        {
            @"\b(public|private|protected|internal)?\s*(static)?\s*\w+\s+\w+\s*\(",  // C#
            @"\bfunction\s+\w+\s*\(",  // JavaScript
            @"\bdef\s+\w+\s*\(",       // Python
            @"\b(public|private|protected)?\s*(static)?\s*\w+\s+\w+\s*\("  // Java
        };

        return functionPatterns.Any(pattern => Regex.IsMatch(line.Trim(), pattern));
    }

    private string ExtractFunctionName(string line)
    {
        var functionNameMatches = new[]
        {
            @"\b\w+\s+(\w+)\s*\(",  // General pattern
            @"\bfunction\s+(\w+)\s*\(",  // JavaScript
            @"\bdef\s+(\w+)\s*\("       // Python
        };

        foreach (var pattern in functionNameMatches)
        {
            var match = Regex.Match(line, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return "Unknown";
    }
}

public class FunctionChange
{
    public string FunctionName { get; set; } = string.Empty;
    public string BeforeCode { get; set; } = string.Empty;
    public string AfterCode { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

public class FunctionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}