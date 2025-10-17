# Automatic Interface Implementation Using Code Generation

## Solution Overview

This solution addresses issue #523 by providing a comprehensive approach to automatically implement interfaces using code generation. The solution finds existing code pieces (from GitHub and StackOverflow patterns), explores available transformations, and constructs transformation paths from method arguments to return values.

## Key Components

### 1. Transformation Path Finder (`TransformationPathFinder.cs`)

Core algorithm that discovers and constructs transformation paths between method signatures.

**Features:**
- Type conversion transformations (int → long, object → string, etc.)
- Async/sync transformations (Task<T> ↔ T)
- Result unwrapping transformations (Result<T> → T)
- Parameter matching and default value generation
- Dijkstra's algorithm for multi-step transformation paths
- Cost calculation for optimization

**Example Usage:**
```csharp
var finder = new TransformationPathFinder();

// Define target interface method
var target = new MethodSignature
{
    Name = "GetData",
    Parameters = new List<Parameter> { new Parameter { Name = "id", Type = typeof(int) } },
    ReturnType = typeof(string)
};

// Define available source method
var source = new MethodSignature
{
    Name = "Fetch",
    Parameters = new List<Parameter> { new Parameter { Name = "identifier", Type = typeof(long) } },
    ReturnType = typeof(string)
};

// Find transformation path
var path = finder.FindPath(source, target);
Console.WriteLine(path); // Shows transformation steps
Console.WriteLine(path.GenerateImplementation()); // Generates C# code
```

### 2. Transformation Demo (`TransformationPathDemo.cs`)

Practical demonstrations of the transformation system:

- **Demo 1**: Simple type conversion (int → long)
- **Demo 2**: Multiple parameter transformations
- **Demo 3**: Return type transformation (int → string)
- **Demo 4**: Complex chains (async → sync, type conversion)

**Run the demo:**
```bash
cd experiments
dotnet run TransformationPathDemo.cs
```

### 3. Roslyn Source Generator (`RoslynSourceGenerator.cs`)

Production-ready source generator for automatic interface implementation.

**Features:**
- Compile-time code generation
- Attribute-based interface marking (`[AutoImplement]`)
- Automatic discovery of implementation targets
- Generates partial classes with full documentation
- Integrates with .NET build process

**Usage:**
```csharp
// Mark interface for auto-implementation
[AutoImplement(SourceType = typeof(DataRepository))]
public interface IDataService
{
    string GetData(int id);
    Task<bool> SaveAsync(string data);
}

// Generator automatically creates:
public partial class DataServiceAutoImplementation : IDataService
{
    public string GetData(int id)
    {
        // Auto-generated with transformation path
        var param0 = (long)id;
        var result = sourceInstance.Fetch(param0);
        return result;
    }

    public Task<bool> SaveAsync(string data)
    {
        // Auto-generated implementation
        return Task.FromResult(false);
    }
}
```

### 4. Comprehensive Documentation (`AutomaticInterfaceImplementation.md`)

Complete guide covering:
- Research findings from GitHub and StackOverflow
- Transformation pattern catalog (Adapter, Wrapper, etc.)
- Transformation path architecture
- Step-by-step implementation strategies
- Real-world examples and references

## Transformation Types

### Argument Transformations
1. **Type Casting**: `(long)intValue`
2. **Type Conversion**: `value.ToString()`
3. **Default Values**: `default(Type)` or `TimeSpan.FromSeconds(30)`
4. **Parameter Reordering**: Match by name or position

### Method Call Transformations
1. **Async to Sync**: `task.Result` or `task.GetAwaiter().GetResult()`
2. **Sync to Async**: `Task.FromResult(result)`
3. **Delegation**: Route to appropriate method

### Return Value Transformations
1. **Unwrapping**: `result.Value` for Result<T>
2. **Type Conversion**: `object.ToString()`, `(string)value`
3. **Aggregation**: Combine multiple results

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Interface Definition                     │
│              [AutoImplement] IDataService                    │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│              Roslyn Source Generator                         │
│  • Discovers marked interfaces                               │
│  • Analyzes method signatures                                │
│  • Invokes TransformationPathFinder                          │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│           TransformationPathFinder                           │
│  • Searches for compatible methods                           │
│  • Calculates transformation paths                           │
│  • Optimizes using cost analysis                             │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│              Code Generation                                 │
│  • Applies transformations                                   │
│  • Generates implementation code                             │
│  • Adds to compilation                                       │
└─────────────────────────────────────────────────────────────┘
```

## Example: Complete Workflow

### 1. Define Target Interface

```csharp
[AutoImplement(SourceType = typeof(DataRepository))]
public interface IDataService
{
    string GetData(int id);
}
```

### 2. Existing Source Code

```csharp
public class DataRepository
{
    public string Fetch(long identifier)
    {
        return $"Data for ID: {identifier}";
    }
}
```

### 3. Transformation Path Analysis

```
Target: string GetData(int id)
Source: string Fetch(long identifier)

Transformation Path:
  Parameter[0]: id (Int32) → identifier (Int64)
    Transformation: TypeCast (cost: 1)
    Code: (long)id

  Return: String → String
    No transformation needed
    Code: result

Total Cost: 1
```

### 4. Generated Implementation

```csharp
public partial class DataServiceAutoImplementation : IDataService
{
    private readonly DataRepository _repository;

    public string GetData(int id)
    {
        // Auto-generated implementation
        // Source: String Fetch(Int64 identifier)

        // Prepare parameters
        var param0 = (long)id; // Int32 to Int64

        // Call source method
        var result = _repository.Fetch(param0);
        return result;
    }
}
```

## Integration with Build Process

### As Source Generator

1. Create a new .NET Standard 2.0 project for the generator
2. Reference Microsoft.CodeAnalysis packages
3. Implement ISourceGenerator or IIncrementalGenerator
4. Reference the generator project as an analyzer

```xml
<ItemGroup>
  <ProjectReference Include="..\YourGenerator\YourGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Standalone Tool

Run the transformation finder as a standalone tool:

```bash
dotnet run -- --interface IDataService --source DataRepository --output Generated/
```

## Extensibility

### Custom Transformations

Add your own transformations:

```csharp
public class CustomTransformation : ITransformation
{
    public Type From => typeof(MySourceType);
    public Type To => typeof(MyTargetType);
    public int Cost => 2;

    public string GenerateCode(string input)
    {
        return $"CustomConverter.Convert({input})";
    }
}

// Register
finder.RegisterTransformation(new CustomTransformation());
```

### Custom Discovery

Extend discovery to search:
- GitHub repositories
- StackOverflow code snippets
- Internal codebases
- Package repositories (NuGet, npm, etc.)

```csharp
public class GitHubDiscovery
{
    public async Task<List<MethodSignature>> SearchAsync(string query)
    {
        // Search GitHub API for code examples
        // Parse and extract method signatures
        // Return candidates
    }
}
```

## Research References

Based on research from:

1. **GitHub Projects:**
   - codecentric/net_automatic_interface
   - konard/T4GenericsExample
   - konard/CppCLIGenericsExample
   - ignatandrei/RSCG_Examples

2. **Documentation:**
   - Roslyn Source Generators Cookbook
   - Microsoft Learn: Source Generators Overview
   - Design Patterns: Adapter and Wrapper patterns

3. **StackOverflow Discussions:**
   - Auto-generate interface implementations in C#
   - Transformation patterns for method signatures
   - Code generation best practices

## Benefits

1. **Reduces Boilerplate**: Eliminates manual implementation of adapter/wrapper code
2. **Maintains Type Safety**: Compile-time generation ensures type correctness
3. **Improves Maintainability**: Single source of truth for interface contracts
4. **Enables Reuse**: Adapts existing code to new interfaces without modification
5. **Optimizes Paths**: Finds minimal transformation cost automatically

## Future Enhancements

1. **Machine Learning**: Train model to predict best transformation paths
2. **Cloud Discovery**: Search cloud repositories for implementation patterns
3. **Interactive Mode**: Suggest transformations during development
4. **Performance Analysis**: Profile generated code and optimize
5. **Testing Generation**: Auto-generate unit tests for implementations

## License

This solution follows the repository's existing license (see LICENSE file).

## Contributing

Contributions welcome! Areas for improvement:
- Additional transformation types
- Better discovery algorithms
- Performance optimizations
- More example scenarios
- Integration with popular frameworks
