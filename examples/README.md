# CommentEverything Examples

This directory contains example files for testing the CommentEverything tool.

## Test Files

### TestClass.cs
A simple C# class without documentation comments. Use this to test comment generation:

```bash
# Generate comments for TestClass.cs
dotnet run --project ../Platform/Platform.Examples generate TestClass.cs TestClass.commented.cs
```

### ScaffoldExample.txt
A file containing scaffold comments that can generate code:

```bash
# Generate code from scaffold comments
dotnet run --project ../Platform/Platform.Examples scaffold ScaffoldExample.txt GeneratedCode.cs
```

## Usage Examples

### Generate comments for a single file
```bash
dotnet run --project ../Platform/Platform.Examples generate TestClass.cs
```

### Generate comments for an entire directory
```bash
dotnet run --project ../Platform/Platform.Examples generate ../Platform/Platform.Sandbox/
```

### Generate code from scaffold comments
```bash
dotnet run --project ../Platform/Platform.Examples scaffold ScaffoldExample.txt output.cs
```

## Scaffold Comment Format

To generate code from comments, use the following format in your source files or text files:

```csharp
/// <scaffold type="class" name="MyNewClass" />
/// <scaffold type="method" name="MyMethod" />
```

The tool will parse these comments and generate basic code scaffolding.
