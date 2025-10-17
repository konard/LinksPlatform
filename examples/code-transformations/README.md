# Code Transformations Example

This example demonstrates how to use the Platform.CodeTransformations tool to manage breaking changes when upgrading/downgrading code between different library versions.

## Files

- **example-config.json**: Configuration file defining transformation rules for upgrading from version 1.0.0 to 2.0.0
- **sample-v1.cs**: Sample code using version 1.0.0 API
- **sample-v2-expected.cs**: Expected result after transforming to version 2.0.0

## Transformation Rules Defined

The example configuration includes the following transformations:

### Version 1.0.0 → 1.1.0
- Rename method `GetLinks()` to `GetAll()`

### Version 1.1.0 → 2.0.0
- Rename method `CreateLink()` to `Create()`
- Rename namespace `Platform.Data.Core` to `Platform.Data`
- Rename type `LinkStorage` to `Links`

## Usage

### Install the tool (if published as NuGet tool)
```bash
dotnet tool install -g Platform.CodeTransformations.CLI
```

### Initialize a configuration
```bash
code-transform init
```

### Upgrade code to version 2.0.0
```bash
code-transform upgrade ./sample-v1.cs 2.0.0 --config example-config.json
```

### Downgrade code to version 1.0.0
```bash
code-transform downgrade ./sample-v2.cs 1.0.0 --config example-config.json
```

## How It Works

1. The tool parses C# source code using Roslyn (Microsoft's .NET compiler platform)
2. It applies transformation rules based on the source and target versions
3. The rules can modify method names, type names, namespace declarations, and more
4. The tool automatically finds the transformation path through intermediate versions
5. Updated code is written back to the file with preserved formatting

## Extending the Tool

You can create custom transformation rules by:

1. Implementing the `ITransformationRule` interface
2. Extending `TransformationRuleBase` abstract class
3. Creating a custom `CSharpSyntaxRewriter` to modify the syntax tree

See the existing rules in `Platform.CodeTransformations/Rules/` for examples.
