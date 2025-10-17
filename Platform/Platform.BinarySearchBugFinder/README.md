# Binary Search Bug Finder

Automated tool for locating bugs in source code using binary search algorithm with AST-based code manipulation.

## Overview

This tool implements an automated bug localization technique that uses binary search to systematically comment out sections of code to identify the source of bugs. It uses Roslyn's Abstract Syntax Tree (AST) parsing to ensure safe and syntactically correct code modifications.

## How It Works

1. **Initial Verification**: Confirms the bug exists in the original code
2. **AST Parsing**: Parses the source code into an AST to identify commentable statements
3. **Binary Search**: Iteratively comments out half the code and tests:
   - If tests pass, the bug is in the commented section
   - If tests fail, the bug is in the remaining code
   - Recursively narrows down to the problematic statements
4. **Result**: Reports the specific statements causing the bug

## Algorithm

The tool uses a recursive binary search approach:
- Split commentable statements into two halves
- Comment out the first half and run tests
- If tests pass, bug is in first half → recurse on first half
- Otherwise, comment out second half
- If tests pass, bug is in second half → recurse on second half
- Continue until bug is isolated to specific statements

## Usage

```bash
dotnet run --project Platform.BinarySearchBugFinder -- \
  --source /path/to/source.cs \
  --test-command "dotnet test" \
  --working-dir /path/to/project \
  --timeout 300
```

### Options

- `--source, -s` (required): Path to the source file to analyze
- `--test-command, -t` (required): Command to run tests (e.g., 'dotnet test', 'npm test')
- `--working-dir, -w`: Working directory for test execution (default: current directory)
- `--timeout`: Timeout for test execution in seconds (default: 300)
- `--mode, -m`: Execution mode: TestCommand or Compilation (default: TestCommand)

### Examples

**Using test command:**
```bash
dotnet run -- -s ./MyProject/Program.cs -t "dotnet test ./MyProject.Tests" -w ./
```

**Using compilation mode:**
```bash
dotnet run -- -s ./MyProject/Program.cs --mode Compilation
```

## Features

- **AST-Based**: Uses Roslyn for safe code manipulation
- **Recursive**: Automatically narrows down to problematic code
- **Flexible Testing**: Supports custom test commands or compilation checks
- **Safe**: Preserves original code structure and restores after analysis
- **Detailed Output**: Shows progress and identifies exact problematic statements

## Requirements

- .NET Core 3.1 or later
- Microsoft.CodeAnalysis.CSharp (Roslyn)
- System.CommandLine

## Limitations

- Currently supports C# files only
- Requires compilable code to start with
- May not work well with code that has complex inter-statement dependencies
- Best suited for identifying specific buggy statements rather than architectural issues

## Related Issue

This tool addresses issue #670: Implementing an automated binary search approach for bug localization.
