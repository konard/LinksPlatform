# Platform.CodeEvolution

A comprehensive tool for analyzing code evolution patterns and providing intelligent recommendations based on Git repository history.

## Features

- **Git History Analysis**: Scans commit history to identify code changes at function/snippet level
- **Change Frequency Tracking**: Builds a database of most frequently applied code changes
- **Smart Filtering**: Filter by programming language, dependencies, frameworks, and more
- **Evolution Chains**: Track how code evolved through multiple commits
- **Intelligent Recommendations**: Get suggestions based on proven patterns from real projects
- **Multiple Export Formats**: JSON, CSV, HTML reports for easy integration

## Installation

Build the project using .NET 8.0:

```bash
cd Platform/Platform.CodeEvolution
dotnet build
```

## Usage

### Analyze Repository

Analyze a Git repository to extract code evolution patterns:

```bash
dotnet run -- analyze --repository /path/to/repo --max-commits 1000 --output results
```

Options:
- `--repository`: Path to Git repository (required)
- `--branch`: Specific branch to analyze (optional, defaults to current)
- `--max-commits`: Maximum commits to analyze (default: 1000)
- `--output`: Output directory for results (default: "output")

### Get Recommendations

Get recommendations for specific code:

```bash
dotnet run -- recommend --code "string result = \"Hello \" + name;" --language "C#"
```

Options:
- `--code`: Code snippet to analyze (required)
- `--language`: Programming language (required)
- `--dependencies`: Dependencies used in the code (optional)

## Output Files

### Analysis Results

When analyzing a repository, the following files are generated:

1. **changes.json**: Raw data of all detected code changes
2. **frequencies.json**: Analysis of most common change patterns
3. **chains.json**: Code evolution chains showing how code evolved
4. **statistics.json**: Overall statistics about the codebase evolution
5. **frequencies.csv**: Change frequencies in CSV format
6. **recommendations.html**: Visual report with recommendations
7. **report.md**: Text summary of the analysis

### Example Statistics

```json
{
  "TotalChangePatterns": 450,
  "TotalChangeChains": 23,
  "LanguageDistribution": {
    "C#": 320,
    "JavaScript": 89,
    "TypeScript": 41
  },
  "TopDependencies": {
    "System.Linq": 156,
    "Newtonsoft.Json": 78,
    "Microsoft.AspNetCore": 45
  }
}
```

## Supported Languages

- **C#**: Full syntax analysis with Roslyn
- **JavaScript/TypeScript**: Import/require dependency tracking
- **Python**: Import analysis
- **Java**: Package import tracking
- **C/C++**: Include directive analysis
- **And more**: Generic pattern matching for other languages

## Use Cases

### For Developers
- Learn best practices from your codebase
- Discover common refactoring patterns
- Get suggestions for code improvements
- Understand how your code evolved over time

### For Teams
- Establish coding standards based on actual usage
- Track technical debt evolution
- Identify frequently changed code areas
- Create training materials for new developers

### For AI/ML
- Build datasets for code completion models
- Train recommendation systems
- Analyze programming pattern trends
- Create benchmarks for code quality tools

## Architecture

### Core Components

1. **GitHistoryAnalyzer**: Scans Git history and extracts changes
2. **CodeAnalyzer**: Analyzes code syntax and extracts dependencies
3. **ChangeFrequencyTracker**: Builds frequency database and chains
4. **RecommendationEngine**: Provides intelligent suggestions
5. **DataExporter**: Exports results in various formats

### Change Detection

The system detects:
- Function additions, modifications, deletions
- Variable declaration patterns
- Import/dependency changes
- Code style improvements
- Refactoring patterns

### Recommendation Types

- **Best Practices**: Patterns used by many developers
- **Style Improvements**: Consistent formatting and naming
- **Modernization**: Updated language features and APIs
- **Evolution Paths**: How similar code evolved over time
- **Alternatives**: Different approaches to the same problem

## Configuration

### Built-in Rules

The system includes built-in rules for common improvements:

**C#**:
- Use `var` for obvious types
- Prefer string interpolation over `string.Format`
- Use `nameof()` for parameter names

**JavaScript**:
- Use `const` for non-reassigned variables
- Prefer arrow functions for callbacks
- Use template literals over string concatenation

### Custom Rules

You can extend the recommendation engine with custom rules:

```csharp
var rule = new RecommendationRule
{
    Name = "Use async/await",
    Pattern = @"\.Result\b",
    Description = "Use async/await instead of .Result",
    IsApplicable = (code, deps) => code.Contains(".Result"),
    Apply = (code) => new CodeRecommendation { /* ... */ }
};
```

## Examples

See the `/examples` directory for:
- Repository analysis scripts
- Recommendation queries
- Integration examples
- Sample outputs

## Contributing

1. Fork the repository
2. Create a feature branch
3. Implement your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is part of the LinksPlatform and follows the same licensing terms.

## Roadmap

- [ ] Support for more programming languages
- [ ] Real-time code analysis in IDEs
- [ ] Machine learning-based recommendations
- [ ] Integration with popular code review tools
- [ ] Performance optimizations for large repositories
- [ ] Cloud-based analysis service