# Pattern Discovery Extension for Visual Studio

A Visual Studio extension that discovers substitution patterns as you code and offers to repeat well-formed patterns based on AST (Abstract Syntax Tree) analysis.

## Overview

This extension continuously monitors your code changes in real-time, analyzes the AST modifications using Roslyn, and detects repetitive editing patterns. When a pattern is well-formed (occurred multiple times with high confidence), it notifies you and offers to repeat the pattern.

## Features

- **Real-time AST Analysis**: Uses Roslyn to analyze code structure changes
- **Intelligent Pattern Detection**: Identifies repetitive sequences of code modifications
- **Non-intrusive Notifications**: Suggests patterns without disrupting your workflow
- **Confidence-based Suggestions**: Only suggests patterns with high confidence scores
- **Customizable Detection**: Configurable parameters for pattern detection sensitivity

## Architecture

### Components

1. **PatternDiscoveryPackage**: Main entry point and Visual Studio package registration
2. **PatternMonitor**: Monitors workspace changes and coordinates pattern detection
3. **ASTAnalyzer**: Analyzes AST differences between document versions using Roslyn
4. **PatternDetector**: Identifies repetitive patterns in change sequences
5. **PatternSuggestionUI**: Handles user notifications and pattern application

### Data Models

- **ASTChange**: Represents a single AST modification (addition, removal, or modification)
- **Pattern**: Represents a detected sequence of changes with occurrence count and confidence score

## How It Works

1. **Change Detection**: The extension subscribes to Visual Studio workspace changes
2. **AST Comparison**: When a document changes, it compares old and new AST structures
3. **Change Recording**: Structural changes are recorded with their type and context
4. **Pattern Matching**: The detector looks for repeating sequences in recent changes
5. **User Notification**: Well-formed patterns trigger suggestions to the user
6. **Pattern Application**: Users can choose to apply the detected pattern

## Pattern Detection Algorithm

- Maintains a sliding window of the last 100 changes
- Looks for repeating sequences of length 1-10
- Requires 80% structural similarity between sequences
- Patterns need 2+ occurrences with 70%+ confidence to be "well-formed"
- Confidence score based on occurrence count and recency

## Requirements

- Visual Studio 2022 (version 17.0 or later)
- .NET 8.0
- Roslyn Language Services

## Building

```bash
dotnet build PatternDiscoveryExtension.csproj
```

This will generate a `.vsix` file that can be installed in Visual Studio.

## Testing

Unit tests are provided in the `PatternDiscoveryExtension.Tests` project:

```bash
dotnet test PatternDiscoveryExtension.Tests/PatternDiscoveryExtension.Tests.csproj
```

## Installation

1. Build the project to generate the `.vsix` file
2. Double-click the `.vsix` file to install
3. Restart Visual Studio
4. The extension will automatically activate when a solution is opened

## Usage Examples

See [examples/pattern-discovery-examples.md](../examples/pattern-discovery-examples.md) for detailed usage scenarios.

## Configuration

Currently, the extension uses these default settings:
- Maximum change history: 100 changes
- Minimum pattern length: 1 change
- Maximum pattern length: 10 changes
- Suggestion cooldown: 30 seconds
- Similarity threshold: 80%
- Well-formed threshold: 2 occurrences, 70% confidence

Future versions may expose these as configurable options.

## Future Enhancements

- **Pattern Application**: Fully implement automatic pattern application to code
- **Pattern Library**: Save and reuse patterns across sessions
- **Smart Suggestions**: Context-aware pattern suggestions based on current code
- **Multi-language Support**: Extend beyond C# to other Roslyn-supported languages
- **Pattern Categories**: Classify patterns (refactoring, boilerplate, etc.)
- **Team Patterns**: Share common patterns across development teams
- **IDE Integration**: Quick actions and code fixes integration

## Technical Details

### Technologies Used

- **.NET 8.0**: Target framework
- **Roslyn (Microsoft.CodeAnalysis)**: AST analysis and code parsing
- **Visual Studio SDK**: Extension framework and VS integration
- **Community.VisualStudio.Toolkit**: Simplified VS extension development
- **xUnit**: Unit testing framework

### Performance Considerations

- Change analysis runs asynchronously to avoid blocking the UI
- Only C# files are analyzed to reduce overhead
- Pattern detection uses efficient sliding window algorithm
- Suggestion cooldown prevents notification spam

## Contributing

This extension is part of the LinksPlatform project. Contributions are welcome!

## License

See the LICENSE file in the repository root.

## Links

- Issue: https://github.com/konard/LinksPlatform/issues/476
- Repository: https://github.com/konard/LinksPlatform
