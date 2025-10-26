# Platform.Transformer.Prototype

A prototype implementation of a better translator architecture for the Links Platform.

## Overview

This prototype addresses issue [#609](https://github.com/konard/LinksPlatform/issues/609) by implementing a new translator architecture that improves upon the existing regex-based approach.

## Architecture Comparison

### Old Architecture (RegularExpressions.Transformer)

- **Translation Unit**: Single file
- **Parsing Method**: Regular expressions
- **Algorithm**: Markov algorithm (Turing Complete)
- **Intermediate Storage**: File system
- **Transformation Rules**: Regex pattern substitution
- **Context**: Limited to single file

### New Architecture (This Prototype)

- **Translation Unit**: Entire repository/project
- **Parsing Method**: Roslyn Compiler API (full AST)
- **Algorithm**: Function-based transformations
- **Intermediate Storage**: Doublets graph database
- **Transformation Rules**: C# functions operating on graph
- **Context**: Entire project with full semantic information

## Key Components

### 1. IAstToDoubletsConverter
Converts Abstract Syntax Trees (AST) from Roslyn into Doublets graph representation.

```csharp
var converter = new AstToDoubletsConverter<ulong>(links);
var rootLink = converter.ConvertSyntaxTree(syntaxTree, filePath);
```

### 2. TransformationEngine
Executes transformation functions on the Doublets representation. Unlike regex substitutions, these functions can:
- Access the full graph structure
- Maintain semantic information
- Perform complex multi-step transformations
- Query relationships across the entire codebase

```csharp
var engine = new TransformationEngine<ulong>(links);
engine.AddTransformation(new MyTransformation());
var result = engine.TransformRecursively(rootLink);
```

### 3. IDoubletsToCodeGenerator
Generates source code from the transformed Doublets representation.

```csharp
var generator = new DoubletsToCodeGenerator<ulong>(links, "cpp");
var files = generator.GenerateCode(rootLink, targetPath);
```

### 4. RepositoryTransformer
Orchestrates the entire transformation pipeline:

1. **Parse** entire repository → AST (using Roslyn)
2. **Convert** AST → Doublets graph
3. **Transform** using functions on Doublets
4. **Generate** code from transformed graph

```csharp
var transformer = new RepositoryTransformer("csharp", "cpp");
transformer.AddTransformation(new StringToCppStringTransformation());
await transformer.TransformAsync(sourceRepo, targetRepo);
```

## Advantages of New Architecture

### 1. Repository-Level Context
- Access to entire codebase structure
- Can analyze cross-file dependencies
- Preserve relationships between classes, interfaces, etc.

### 2. Semantic Understanding
- Full AST with type information
- Can query semantic model for type resolution
- Better handling of language-specific constructs

### 3. Flexible Transformations
- Functions instead of regex patterns
- Can implement complex logic
- Easier to maintain and test
- Can access full graph structure

### 4. Intermediate Graph Storage
- All code structure in Doublets
- Can apply multiple transformation passes
- Can inspect and debug intermediate state
- Enables advanced graph queries

### 5. Extensibility
- Easy to add new transformation functions
- Can support multiple source/target languages
- Plugin architecture for transformations

## Example Usage

See `examples/transformer-prototype/DemoProgram.cs` for a complete example.

```csharp
// Create transformer
var transformer = new RepositoryTransformer("csharp", "cpp");

// Add transformations
transformer.AddTransformation(new StringTypeTransformation());
transformer.AddTransformation(new NamespaceToNamespaceTransformation());

// Transform entire repository
await transformer.TransformAsync(
    sourceRepositoryPath: "./MyProject",
    targetRepositoryPath: "./MyProject.Cpp"
);
```

## Implementation Status

This is a **prototype** implementation that demonstrates the architecture. For production use, the following would need to be added:

1. **Complete AST → Doublets encoding**
   - Currently simplified
   - Need full node type coverage
   - String interning for efficiency

2. **Language-specific code generators**
   - C++ code generator
   - Java code generator
   - Python code generator

3. **Comprehensive transformation library**
   - Type mapping transformations
   - Namespace/package transformations
   - Memory management transformations
   - Standard library mapping

4. **Semantic analysis**
   - Type resolution
   - Symbol lookup
   - Cross-reference analysis

5. **Optimization**
   - Graph traversal optimization
   - Caching mechanisms
   - Parallel processing

## Comparison with RegularExpressions.Transformer.CSharpToCpp

The existing `RegularExpressions.Transformer.CSharpToCpp` has ~100 regex rules like:

```csharp
new SubstitutionRule(@"\bstring\b", "std::string"),
new SubstitutionRule(@"\bvar\b", "auto"),
```

In the new architecture, these become functions:

```csharp
public class StringTypeTransformation : ITransformationFunction<ulong>
{
    public bool CanTransform(ulong link)
    {
        // Check if link represents a 'string' type node
        return IsTypeNode(link) && GetTypeName(link) == "string";
    }

    public ulong Transform(ulong link)
    {
        // Create new link representing 'std::string'
        return CreateTypeNode("std::string");
    }
}
```

This provides:
- **Better accuracy**: Can distinguish between `string` type vs. `string` in comments
- **Context awareness**: Can handle qualified names like `System.String`
- **Semantic understanding**: Can check if it's actually a type reference

## Future Work

1. Implement complete transformation library matching CSharpToCpp capabilities
2. Add support for multiple target languages
3. Implement graph query language for transformations
4. Create visualization tools for Doublets representation
5. Add semantic model support from Roslyn
6. Performance optimization for large repositories
7. Support for incremental transformations

## Related Issues

- [#609](https://github.com/konard/LinksPlatform/issues/609) - A better translator architecture prototype

## License

This prototype follows the same license as the Links Platform project.
