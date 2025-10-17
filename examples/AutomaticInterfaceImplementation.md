# Automatic Interface Implementation Using Code Generation

## Overview

This document demonstrates approaches to automatically implement interfaces using code generation, finding existing code pieces from GitHub and StackOverflow, exploring available transformations, and constructing transformation paths from arguments to return values.

## Research Findings

### 1. Existing Code Generation Solutions

Based on research of GitHub and StackOverflow, several approaches exist:

#### A. Roslyn Source Generators (.NET)
- **Project**: codecentric/net_automatic_interface
- **Approach**: Uses `[GenerateAutomaticInterface]` attribute to automatically create interfaces from classes
- **Transformation**: Copies public members from class to interface
- **Limitations**: Additive only, cannot modify existing code

#### B. T4 Text Templates
- **Project**: konard/T4GenericsExample
- **Approach**: Uses T4 templates with `*.tt` and `*.ttinclude` files
- **Transformation**: Generates type-specific implementations from generic templates
- **Use case**: Multiple generic interface implementations for different types

#### C. C++/CLI Templates
- **Project**: konard/CppCLIGenericsExample
- **Approach**: Uses C++/CLI template to generate implementations
- **Limitation**: Windows-only

### 2. Transformation Patterns

#### Adapter Pattern
Transforms method signatures between incompatible interfaces:

```csharp
// Target Interface
interface ITarget {
    string Request();
}

// Adaptee (existing code to adapt)
class Adaptee {
    public string SpecificRequest() => "Specific request";
}

// Adapter (auto-generated)
class Adapter : ITarget {
    private Adaptee _adaptee;

    public Adapter(Adaptee adaptee) {
        _adaptee = adaptee;
    }

    public string Request() {
        // Transform: no arguments -> call method -> return value
        return _adaptee.SpecificRequest();
    }
}
```

#### Wrapper Pattern
Simplifies complex interfaces:

```csharp
// Complex interface
interface IComplexService {
    Task<Result<Data, Error>> ProcessAsync(Request req, Context ctx, CancellationToken ct);
}

// Simplified interface (generated)
interface ISimpleService {
    Data Process(Request req);
}

// Auto-generated wrapper
class SimpleServiceWrapper : ISimpleService {
    private IComplexService _complex;

    public Data Process(Request req) {
        // Transform: arguments -> add defaults -> await -> unwrap -> return
        var result = _complex.ProcessAsync(req, Context.Default, CancellationToken.None).Result;
        return result.Value;
    }
}
```

### 3. Transformation Path Architecture

#### Transformation Categories

1. **Argument Transformations**
   - Add default parameters
   - Convert types (int → long, string → Uri)
   - Wrap primitives in objects
   - Split composite arguments
   - Reorder parameters

2. **Method Call Transformations**
   - Synchronous ↔ Asynchronous
   - Direct call → Delegated call
   - Single method → Method chain
   - Static → Instance method

3. **Return Value Transformations**
   - Unwrap complex types (Result<T> → T)
   - Convert types (object → string)
   - Aggregate multiple results
   - Transform exceptions to error codes

#### Transformation Path Example

```
Input: ITarget.Get(id: int) : string
Target: ISource.FetchAsync(id: long, timeout: TimeSpan) : Task<Result<object>>

Transformation Path:
1. Convert argument: int → long
2. Add default argument: timeout = TimeSpan.FromSeconds(30)
3. Call method: FetchAsync(id, timeout)
4. Await result: Task<Result<object>> → Result<object>
5. Unwrap result: Result<object> → object
6. Convert return: object → string (ToString())
```

### 4. Implementation Strategy

#### Phase 1: Discovery
```csharp
// Discover available methods in existing code
var methods = DiscoverMethods(githubCode, stackoverflowCode);

// Example: Find methods that could satisfy interface requirements
interface IDataProcessor {
    string Process(int input);
}

// Discovered candidates:
// - string Convert(int value) ✓ Direct match
// - Task<string> ProcessAsync(int id) → Transform: await
// - object Transform(long value) → Transform: cast int→long, cast object→string
```

#### Phase 2: Transformation Selection
```csharp
// Build transformation graph
var transformations = new TransformationGraph();
transformations.Add(new TypeCastTransformation(typeof(int), typeof(long)));
transformations.Add(new AsyncToSyncTransformation());
transformations.Add(new ResultUnwrapTransformation());

// Find shortest path from source to target
var path = transformations.FindPath(sourceMethod, targetSignature);
```

#### Phase 3: Code Generation
```csharp
// Generate implementation using selected transformations
public class GeneratedImplementation : IDataProcessor {
    private readonly SourceClass _source;

    public string Process(int input) {
        // Apply transformation path
        long converted = (long)input; // Transformation 1: int→long
        var task = _source.ProcessAsync(converted); // Method call
        var result = task.Result; // Transformation 2: async→sync
        return result; // Return
    }
}
```

## Practical Example: Roslyn Source Generator

### Step 1: Create Generator Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.1" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.3" />
  </ItemGroup>
</Project>
```

### Step 2: Implement Generator

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;

[Generator]
public class InterfaceImplementationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new InterfaceSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxContextReceiver is InterfaceSyntaxReceiver receiver))
            return;

        foreach (var interfaceDeclaration in receiver.Interfaces)
        {
            var semanticModel = context.Compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree);
            var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);

            // Generate implementation
            var implementation = GenerateImplementation(interfaceSymbol);
            context.AddSource($"{interfaceSymbol.Name}Implementation.g.cs", implementation);
        }
    }

    private string GenerateImplementation(INamedTypeSymbol interfaceSymbol)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated implementation");
        sb.AppendLine($"public class {interfaceSymbol.Name.Substring(1)}Implementation : {interfaceSymbol.Name}");
        sb.AppendLine("{");

        foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            sb.AppendLine($"    public {member.ReturnType} {member.Name}(");

            var parameters = member.Parameters.Select(p => $"{p.Type} {p.Name}");
            sb.AppendLine($"        {string.Join(", ", parameters)})");
            sb.AppendLine("    {");

            // Apply transformations to generate implementation
            sb.AppendLine("        // TODO: Apply transformation path");

            if (member.ReturnsVoid)
                sb.AppendLine("        return;");
            else
                sb.AppendLine($"        return default({member.ReturnType});");

            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}

class InterfaceSyntaxReceiver : ISyntaxContextReceiver
{
    public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is InterfaceDeclarationSyntax interfaceDecl)
        {
            // Check for auto-generation attribute
            var hasAttribute = interfaceDecl.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("AutoImplement"));

            if (hasAttribute)
                Interfaces.Add(interfaceDecl);
        }
    }
}
```

### Step 3: Usage

```csharp
// Mark interface for auto-implementation
[AutoImplement]
public interface IDataService
{
    string GetData(int id);
    Task<bool> SaveAsync(string data);
}

// Generator creates:
public class DataServiceImplementation : IDataService
{
    public string GetData(int id)
    {
        // Auto-generated with transformation path
        return default(string);
    }

    public Task<bool> SaveAsync(string data)
    {
        // Auto-generated with transformation path
        return Task.FromResult(false);
    }
}
```

## Advanced: Transformation Path Discovery

### Algorithm

```csharp
public class TransformationPathFinder
{
    private readonly Dictionary<(Type, Type), ITransformation> _transformations = new();

    public void RegisterTransformation(Type from, Type to, ITransformation transformation)
    {
        _transformations[(from, to)] = transformation;
    }

    public List<ITransformation> FindPath(MethodSignature source, MethodSignature target)
    {
        var path = new List<ITransformation>();

        // 1. Transform arguments
        for (int i = 0; i < target.Parameters.Count; i++)
        {
            var targetParam = target.Parameters[i];
            var sourceParam = FindMatchingParameter(source.Parameters, targetParam);

            if (sourceParam != null && sourceParam.Type != targetParam.Type)
            {
                var transformation = FindTransformation(sourceParam.Type, targetParam.Type);
                if (transformation != null)
                    path.Add(transformation);
            }
        }

        // 2. Transform return type
        if (source.ReturnType != target.ReturnType)
        {
            var transformation = FindTransformation(source.ReturnType, target.ReturnType);
            if (transformation != null)
                path.Add(transformation);
        }

        return path;
    }

    private ITransformation FindTransformation(Type from, Type to)
    {
        // Direct transformation
        if (_transformations.TryGetValue((from, to), out var direct))
            return direct;

        // Use Dijkstra's algorithm for multi-step transformations
        // ... (implementation omitted for brevity)

        return null;
    }
}

public interface ITransformation
{
    string GenerateCode(string input);
}

public class TypeCastTransformation : ITransformation
{
    public Type From { get; }
    public Type To { get; }

    public string GenerateCode(string input) => $"({To.Name}){input}";
}

public class AsyncToSyncTransformation : ITransformation
{
    public string GenerateCode(string input) => $"{input}.Result";
}
```

## References

1. **Roslyn Source Generators**: https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
2. **net_automatic_interface**: https://github.com/codecentric/net_automatic_interface
3. **T4GenericsExample**: https://github.com/konard/T4GenericsExample
4. **RSCG Examples**: https://github.com/ignatandrei/RSCG_Examples
5. **Adapter Pattern**: https://refactoring.guru/design-patterns/adapter
6. **Microsoft Learn - Source Generators**: https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview

## Conclusion

Automatic interface implementation using code generation is achievable through:

1. **Discovery**: Finding existing code from GitHub/StackOverflow
2. **Analysis**: Identifying compatible method signatures
3. **Transformation**: Building transformation paths from arguments to return values
4. **Generation**: Using Roslyn Source Generators, T4 templates, or similar tools to generate implementations

The key is identifying the correct transformation path that converts the source method signature to match the target interface requirements.
