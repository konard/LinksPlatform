# Breaking Changes Migration Tool

A Roslyn-based tool for automatically updating user code when libraries introduce breaking changes.

## Overview

This tool addresses the challenge of migrating code to new library versions with breaking changes. It:

1. **Detects** code patterns that need updating based on configurable rules
2. **Analyzes** source code to identify breaking change impacts
3. **Migrates** code automatically by applying transformation rules

## Architecture

The tool is built on the .NET Compiler Platform (Roslyn) and consists of:

- **MigrationRule**: Defines patterns for breaking changes (old → new)
- **CodeMigrator**: Traverses syntax trees and applies transformations
- **MigrationRuleLoader**: Loads rules from JSON configuration files
- **Program**: CLI interface for running migrations

## Usage

### 1. Initialize Migration Rules

Create a sample migration rules file:

```bash
dotnet run -- init migration-rules.json
```

This creates a JSON file with sample rules that you can customize:

```json
[
  {
    "FromVersion": "0.5.0",
    "ToVersion": "0.6.0",
    "Description": "Method 'GetLink' renamed to 'GetLinkById'",
    "ChangeType": "MethodRename",
    "OldPattern": {
      "TypeName": "LinksOperator",
      "MemberName": "GetLink",
      "ParameterTypes": ["ulong"]
    },
    "NewPattern": {
      "MemberName": "GetLinkById"
    }
  }
]
```

### 2. Analyze Code

Analyze your code to see what changes would be applied:

```bash
dotnet run -- analyze migration-rules.json YourCode.cs
```

### 3. Migrate Code

Apply the migration rules to update your code:

```bash
dotnet run -- migrate migration-rules.json YourCode.cs YourCode.migrated.cs
```

## Rule Types

### MethodRename

Renames a method while preserving arguments:

```json
{
  "ChangeType": "MethodRename",
  "OldPattern": {
    "TypeName": "MyClass",
    "MemberName": "OldMethod"
  },
  "NewPattern": {
    "MemberName": "NewMethod"
  }
}
```

### TypeRename

Renames a type throughout the codebase:

```json
{
  "ChangeType": "TypeRename",
  "OldPattern": {
    "TypeName": "OldClassName"
  },
  "NewPattern": {
    "TypeName": "NewClassName"
  }
}
```

### MethodChange

Handles method signature changes (parameters, return types):

```json
{
  "ChangeType": "MethodChange",
  "OldPattern": {
    "TypeName": "MyClass",
    "MemberName": "MyMethod",
    "ParameterTypes": ["int", "string"]
  },
  "NewPattern": {
    "MemberName": "MyMethod",
    "ReplacementTemplate": "{0}.MyMethod({1}, {2}, false)"
  }
}
```

## Benefits

1. **Automated Migration**: No manual find-and-replace needed
2. **Type-Safe**: Uses semantic analysis to ensure correct transformations
3. **Configurable**: Rules defined in JSON for easy maintenance
4. **Version Tracking**: Rules linked to specific version transitions
5. **Verifiable**: Analyze before migrating to preview changes

## Integration

This tool can be integrated into:

- **CI/CD Pipelines**: Automatically migrate code on dependency updates
- **IDE Extensions**: Real-time migration suggestions
- **Library Release Process**: Generate migration rules from API diffs
- **Documentation**: Auto-generate migration guides

## Future Enhancements

- Roslyn Analyzer for real-time IDE feedback
- Code Fix Provider for quick actions
- Git integration for automatic commit/PR creation
- API diff analyzer to auto-generate rules
- Support for more complex transformations
- Multi-project solution migration
