# Pattern-Based Sequence Storage Experiments

This directory contains experimental implementations related to issue [#142 - Store sequences using patterns](https://github.com/konard/LinksPlatform/issues/142).

## Overview

The experiments in this directory demonstrate how sequences can be stored more efficiently by detecting and reusing repeating patterns. This approach is inspired by the diagram in issue #142, which shows how a sequence like "mama" can be represented in multiple ways:

1. **Standard storage**: Each element is stored individually in sequence
2. **Pattern-based storage**: Repeated subsequences are detected, extracted as patterns, and referenced instead of being duplicated

## Concept

The key insight from issue #142 is that when storing sequences:
- Repeating patterns can be detected and stored once
- Pattern references can replace redundant full sequences
- When a step has only one option (single path), counting/indexing can be optimized

### Example: "mama"

```
Standard:     START → m → a → m → a → STOP
Pattern-based: START → m → {pattern: a,m} → STOP
```

The pattern `{a, m}` or `{m, a}` is recognized as repeating and can be stored once, then referenced multiple times.

## Files

### PatternBasedSequenceStorage.cs
Core implementation that provides:
- Pattern detection in sequences
- Analysis of pattern occurrences
- Optimized sequence creation using pattern links
- Pattern overlap resolution

## Building the Implementation

```bash
cd experiments
dotnet build Platform.Experiments.PatternStorage.csproj
```

This builds the pattern storage implementation as a library that can be referenced by other projects.

## Using in Your Application

To use the pattern-based sequence storage in your own application:

```csharp
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Experiments;

// Create your links storage
ILinks<ulong> links = ... // your links implementation
var sequences = new Sequences(links);
var patternStorage = new PatternBasedSequenceStorage(links, sequences);

// Analyze a sequence for patterns
var elements = new ulong[] { m, a, m, a }; // your sequence elements
var analysis = patternStorage.CreateWithPatternAnalysis(elements);

// Print the analysis results
analysis.PrintSummary();

// Access the detected patterns
foreach (var pattern in analysis.DetectedPatterns)
{
    Console.WriteLine(pattern); // Displays pattern details
}
```

## Benefits of Pattern-Based Storage

1. **Reduced Storage**: Patterns are stored once and referenced multiple times
2. **Efficient Queries**: Pattern matching operations become faster
3. **Better Compression**: Highly repetitive sequences compress significantly
4. **Semantic Clarity**: Patterns can represent meaningful concepts

## Future Enhancements

- Integration with OptimalVariantConverter from Platform.Data.Doublets.Sequences
- Hierarchical pattern detection (patterns within patterns)
- Pattern frequency tracking for compression metrics
- Automatic pattern threshold tuning based on sequence characteristics

## Related Work

This implementation builds upon:
- `Platform.Data.Doublets.Sequences` - Core sequence handling
- `OptimalVariantConverter` - Optimal sequence representation
- Issue #142 diagram - Visual concept of pattern-based storage
