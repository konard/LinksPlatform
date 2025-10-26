# LinksPlatform Examples

This directory contains example code demonstrating various concepts and optimizations in the LinksPlatform.

## Multiple Types with Doublets Optimization

**File:** `MultipleTypesDoubletOptimization.cs`

Demonstrates the cost optimization pattern described in issue #588, where multiple triplets with the same source and target but different types can be represented more efficiently using doublets.

### Key Concept

Instead of representing each typed relationship as a separate structure, share the base relationship and add type annotations as additional links.

### Running the Example

```bash
cd examples
dotnet run MultipleTypesDoubletOptimization.cs
```

Or compile and run manually:
```bash
csc MultipleTypesDoubletOptimization.cs
./MultipleTypesDoubletOptimization
```

### Expected Output

The example will:
1. Compare traditional vs optimized representations for various numbers of types
2. Show a detailed walkthrough with 3 different relationship types
3. Verify the mathematical formula: Optimized = n+1 (vs Traditional = 2n)
4. Demonstrate the space savings approaching 50% efficiency

### Related Documentation

See `doc/articles/triplet-to-doublet-optimization.md` for comprehensive documentation of this optimization pattern.

## See Also

- [Platform.Examples](../Platform/Platform.Examples/) - Additional examples in the main Platform directory
- [Platform.Sandbox](../Platform/Platform.Sandbox/) - Experimental code and tests
