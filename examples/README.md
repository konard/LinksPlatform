# Code Evolution Examples

This directory contains examples of how to use the Platform.CodeEvolution tool to analyze code changes and get recommendations.

## Examples

### 1. Repository Analysis (`analyze_repository.sh`)

Analyzes the LinksPlatform repository to find code evolution patterns:

```bash
chmod +x analyze_repository.sh
./analyze_repository.sh
```

This will:
- Build the CodeEvolution project
- Analyze the last 100 commits
- Generate various output files with analysis results
- Create a visual HTML report with recommendations

### 2. Code Recommendations (`get_recommendations.sh`)

Gets recommendations for specific code snippets:

```bash
chmod +x get_recommendations.sh
./get_recommendations.sh
```

This demonstrates how to get recommendations for:
- C# code improvements
- JavaScript modernization

## Output Files

After running the analysis, you'll find these files in the `analysis-results` directory:

- **changes.json**: Raw data of all detected code changes
- **frequencies.json**: Analysis of most common change patterns
- **chains.json**: Code evolution chains showing how code evolved over time
- **statistics.json**: Overall statistics about the codebase evolution
- **frequencies.csv**: Change frequencies in CSV format for spreadsheet analysis
- **recommendations.html**: Visual report with recommendations (open in browser)
- **report.md**: Text summary of the analysis

## Use Cases

1. **Code Review**: Find common patterns that developers in your team use
2. **Best Practices**: Identify the most frequently applied improvements
3. **Code Modernization**: See how code has evolved and what patterns are trending
4. **Learning**: Understand how experienced developers improve code
5. **AI Training**: Build datasets for training code improvement models

## Filtering Options

You can filter results by:
- Programming language
- Dependencies/frameworks used
- Time period
- Author
- File type

## Integration

The tool can be integrated into:
- CI/CD pipelines for code quality analysis
- IDEs as a plugin for real-time recommendations
- Code review tools for automated suggestions
- Documentation generation for coding standards