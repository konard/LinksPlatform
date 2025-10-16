# Virtual Programmer Bot (DevBot)

> An AI-powered development assistant that helps with code development, testing, and maintenance through GitHub Issues and Pull Requests.

## Overview

DevBot (also known as GnoBot, from Greek γνώσις — "knowledge") is a virtual programmer that integrates with your GitHub repository to provide automated development assistance. It was designed to help developers with routine tasks, test writing, bug reproduction, and code review.

## Features

### 🧪 Automatic Test Generation
- Analyzes existing code and generates comprehensive test cases
- Supports multiple testing frameworks (xUnit, NUnit, MSTest)
- Identifies missing test coverage
- Creates reproducible test cases for reported bugs

### 🔍 Breaking Change Detection
- Analyzes pull requests for API surface changes
- Detects removed or modified public methods
- Identifies signature changes that could break consumers
- Provides migration recommendations

### 📦 Version Management
- Suggests version increments based on changes
- Follows semantic versioning (SemVer)
- Automatically generates release notes from commit messages
- Tracks breaking changes for major version bumps

### 🐛 Bug Reproduction
- Attempts to reproduce reported bugs
- Creates minimal reproduction test cases
- Asks clarifying questions when bug reports are unclear
- Links bug reports to test cases

### 👀 Code Review Assistance
- Reviews pull requests for common issues
- Checks code style and complexity
- Suggests improvements and best practices
- Identifies potential bugs

### 📚 Documentation Support
- Generates documentation from code comments
- Keeps documentation in sync with code changes
- Creates API reference documentation
- Updates README files with new features

## How to Use

### Activating DevBot on Issues

There are several ways to activate DevBot on an issue:

1. **Add the `bot` label** to the issue
2. **Include `[bot]` in the issue title**
3. **Mention `@devbot` in a comment**

Example:
```
@devbot please write tests for the DatabaseManager class
```

### Activating DevBot on Pull Requests

1. **Add the `bot` label** to the PR
2. **Mention `@devbot` in a review comment**

Example:
```
@devbot review this change for potential breaking changes
```

### Specific Commands

#### Test Writing
```
@devbot write tests for [component/class name]
```
or add the `needs-tests` label to the issue.

#### Bug Reproduction
```
@devbot reproduce this bug
```
Include steps to reproduce in the issue description.

#### Breaking Change Analysis
DevBot automatically analyzes all pull requests for breaking changes. No explicit command needed.

#### Code Review
```
@devbot review this PR
```

## Configuration

DevBot's behavior can be customized by editing `.devbot/config.yml`. See the configuration file for all available options.

### Key Configuration Options

- **Auto-create branches**: Automatically create branches for fixes
- **Auto-create PRs**: Automatically create pull requests for fixes
- **Test coverage threshold**: Minimum code coverage percentage
- **Version increment rules**: Customize version bumping logic
- **Protected files**: Files that DevBot should never modify

## Architecture

DevBot is implemented as a GitHub Actions workflow that triggers on:
- New issues with specific labels
- Comments on issues and PRs mentioning the bot
- Pull request events (opened, synchronized)
- Review comments

The workflow integrates with:
- GitHub API for issue/PR management
- .NET SDK for code analysis and testing
- Git for version control operations

## Integration with Claude Code

This repository currently uses **Claude Code** as the AI backend for DevBot. Claude Code provides:
- Advanced code understanding
- Natural language interaction
- Context-aware suggestions
- Multi-step problem solving

## Related Issues

DevBot addresses several feature requests from the LinksPlatform project:

- #105 - Virtual Programmer Bot (this implementation)
- #471 - TypoBot for fixing typos across repositories
- #477 - Code translator bot
- #479 - Automated issue management
- #480 - PR auto-reviewer
- #565 - Line ending consistency bot
- #579 - Automated testing bot
- #533 - Documentation bot

## Future Enhancements

Planned features for DevBot:

1. **TypoBot Integration**: Automatically detect and fix typos in code and documentation
2. **Code Translation**: Translate code between programming languages
3. **Line Ending Normalization**: Ensure consistent line endings across the codebase
4. **Dependency Update Management**: Smart dependency updates with compatibility checking
5. **Performance Analysis**: Detect performance regressions in PRs
6. **Security Scanning**: Automated security vulnerability detection

## Example Workflows

### Writing Tests for Existing Code

1. Create an issue: "Write tests for the DatabaseManager class"
2. Add the `needs-tests` label
3. DevBot analyzes the class
4. DevBot creates a PR with test cases
5. Review and merge the PR

### Reproducing a Bug

1. User reports a bug in an issue
2. Comment: `@devbot reproduce this bug`
3. DevBot attempts to create a failing test case
4. If successful, DevBot creates a PR with the test
5. If unclear, DevBot asks clarifying questions

### PR Review with Breaking Change Detection

1. Developer creates a PR
2. DevBot automatically analyzes the changes
3. DevBot comments with breaking change analysis
4. DevBot suggests version increment
5. Developer addresses feedback and merges

## Contributing

To improve DevBot:

1. Submit issues for bugs or feature requests
2. Propose workflow improvements
3. Add new bot capabilities in `.github/workflows/virtual-programmer-bot.yml`
4. Update configuration options in `.devbot/config.yml`

## License

DevBot is part of the LinksPlatform project and follows the same license (Unlicense).

## Acknowledgments

- Original concept by Konstantin Seleznev (@konard) - Issue #105 (2015)
- Name suggestion "GnoBot" by Sergey Lubyagin (@lubyagin)
- Inspired by Probot and other GitHub bot frameworks

---

*"Knowledge-driven development automation"* 🤖
