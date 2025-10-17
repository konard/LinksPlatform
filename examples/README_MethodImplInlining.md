# MethodImpl AggressiveInlining Transformer

A C# code transformer that can add or remove `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attributes to all methods in C# source code.

## Overview

This tool helps developers test the performance impact of aggressive inlining by making it easy to add or remove `MethodImpl` attributes across an entire codebase. It uses the [Platform.RegularExpressions.Transformer](https://github.com/linksplatform/RegularExpressions.Transformer) engine to perform reliable text transformations using regular expressions.

## Features

- ✅ Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to all methods
- ✅ Remove `[MethodImpl(MethodImplOptions.AggressiveInlining)]` from all methods
- ✅ Handles various method signatures:
  - Simple methods
  - Generic methods with constraints
  - Static, virtual, override, abstract methods
  - Async methods
  - Expression-bodied methods and properties
  - Auto-implemented properties
  - Properties with custom getters/setters
- ✅ Automatically adds `using System.Runtime.CompilerServices;` when needed
- ✅ Creates backup files before transformation
- ✅ Removes duplicate attributes

## Use Cases

1. **Performance Testing**: Compare performance with and without aggressive inlining
2. **Optimization Experiments**: Test if inlining affects your specific use case
3. **Code Analysis**: Understand the impact of inlining on your .NET project
4. **Educational**: Learn about method inlining and its effects

## Usage

### As a Library

```csharp
using Platform.Examples.MethodImplInliningTransformer;

// Add MethodImpl attributes to a string of C# code
string sourceCode = File.ReadAllText("MyClass.cs");
string transformed = MethodImplAggressiveInliningTransformer.AddMethodImplAttributes(sourceCode);
File.WriteAllText("MyClass.cs", transformed);

// Remove MethodImpl attributes from a string of C# code
string cleaned = MethodImplAggressiveInliningTransformer.RemoveMethodImplAttributes(transformed);
File.WriteAllText("MyClass.cs", cleaned);

// Transform a file directly (creates backup automatically)
MethodImplAggressiveInliningTransformer.TransformFileAdd("MyClass.cs");
MethodImplAggressiveInliningTransformer.TransformFileRemove("MyClass.cs");
```

### As a Command-Line Tool

```bash
# Add MethodImpl attributes to a file
dotnet run --project MethodImplInliningExample add MyClass.cs

# Remove MethodImpl attributes from a file
dotnet run --project MethodImplInliningExample remove MyClass.cs

# Run built-in tests
dotnet run --project MethodImplInliningExample test
```

## Examples

### Before Transformation

```csharp
using System;

public class Example
{
    public string Name { get; set; }

    public void DoSomething()
    {
        Console.WriteLine(Name);
    }

    public T GenericMethod<T>(T value) where T : class
    {
        return value;
    }
}
```

### After Adding MethodImpl

```csharp
using System;
using System.Runtime.CompilerServices;

public class Example
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public string Name { get; set; }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void DoSomething()
    {
        Console.WriteLine(Name);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public T GenericMethod<T>(T value) where T : class
    {
        return value;
    }
}
```

## Testing

Run the experiment script to see the transformer in action:

```bash
bash experiments/test_methodimpl_transformer.sh
```

## Implementation Details

The transformer uses regular expressions to:

1. **Identify method declarations**: Matches various method signatures including modifiers, generic parameters, constraints, etc.
2. **Insert attributes**: Adds the MethodImpl attribute before each method declaration
3. **Handle properties**: Adds attributes to auto-implemented properties and property accessors
4. **Clean up**: Removes duplicate attributes and ensures proper using directives
5. **Remove attributes**: Strips MethodImpl attributes in both short and fully-qualified forms

## Performance Considerations

When using this transformer:

- **Benchmark first**: Always measure performance before and after to see if inlining helps
- **Not always faster**: Aggressive inlining can sometimes hurt performance due to increased code size
- **Compiler decisions**: The .NET JIT compiler already makes good inlining decisions
- **Use selectively**: Consider adding MethodImpl only to hot-path methods

## Integration with Platform.RegularExpressions.Transformer

This tool is designed to work with the Platform.RegularExpressions.Transformer library:

```csharp
// You can use the rules directly with TextTransformer
var rules = MethodImplAggressiveInliningTransformer.GetAddRules();
// Convert to SubstitutionRule format and use with TextTransformer
```

## Files

- **MethodImplAggressiveInliningTransformer.cs**: Main transformer implementation
- **MethodImplInliningExample.cs**: Command-line interface and usage examples
- **TestSample.cs**: Sample C# file for testing
- **experiments/test_methodimpl_transformer.sh**: Shell script demonstrating the transformation

## Safety Features

- Creates `.backup` files before transforming
- Validates file existence before processing
- Handles edge cases (duplicate attributes, missing using directives)
- Non-destructive by default (backups are created)

## Limitations

- Works on source code text, not on compiled assemblies
- May not handle all edge cases in complex C# syntax
- Regular expressions have limitations compared to full C# parsing
- Does not validate that the transformed code compiles

## Contributing

This is a simple example project. To improve it:

1. Add more comprehensive tests
2. Handle additional edge cases
3. Integrate with MSBuild for batch processing
4. Add performance benchmarking tools
5. Support for async property accessors

## License

This tool follows the LinksPlatform license. See the main repository for details.

## Related Projects

- [Platform.RegularExpressions.Transformer](https://github.com/linksplatform/RegularExpressions.Transformer) - The transformation engine
- [Platform.RegularExpressions.Transformer.CSharpToCpp](https://github.com/linksplatform/RegularExpressions.Transformer.CSharpToCpp) - C# to C++ transformer
- [LinksPlatform](https://github.com/konard/LinksPlatform) - Main repository

## References

- [MethodImplOptions.AggressiveInlining Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.methodimploptions)
- [.NET JIT Compiler Inlining](https://docs.microsoft.com/en-us/dotnet/framework/performance/method-inlining)
