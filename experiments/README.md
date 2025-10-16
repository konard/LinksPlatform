# Compression Optimization Experiments

This directory contains experimental implementations of compression optimizations for issue [#95](https://github.com/konard/LinksPlatform/issues/95).

## Overview

The compression system uses a byte-pair encoding (BPE) approach to identify and replace frequently occurring pairs of elements in sequences. These experiments explore various optimizations to improve compression speed, effectiveness, and cross-document efficiency.

## Implemented Optimizations

### 1. Compression Benchmark (`CompressionBenchmark.cs`)

**Addresses requirement:** "Check if local compression actually helpful when main sequences are loaded (compare to balanced variant creation)"

Provides benchmarking tools to compare:
- Balanced variant creation alone
- Balanced variant with compression
- Time overhead vs space savings analysis

**Usage:**
```csharp
CompressionBenchmark.CompareCompressionVsBalancedVariant();
```

**Key Insights:**
- Measures compression ratio (how much space is saved)
- Calculates time overhead (how much slower compression is)
- Provides verdict on whether compression is beneficial for given data

### 2. Batch Replacement Compressor (`BatchReplacementCompressor.cs`)

**Addresses requirement:** "Update multiple pairs at a time of replacement"

Traditional BPE compresses one pair per iteration. This optimization identifies and replaces the top N most frequent pairs in a single pass through the sequence.

**Key Features:**
- Configurable batch size (default: 10 pairs per iteration)
- Two variants:
  - `Compress()`: Replaces by frequency
  - `CompressWithScoring()`: Replaces by compression score (frequency × savings)
- Significantly reduces number of iterations needed

**Usage:**
```csharp
var compressor = new BatchReplacementCompressor(links, batchSize: 10);
var compressed = compressor.Compress(sequence);
```

**Benefits:**
- Faster compression for sequences with many repeated pairs
- More efficient for data with multiple high-frequency patterns
- Reduces I/O operations on the links storage

### 3. Global Compression Dictionary (`GlobalCompressionDictionary.cs`)

**Addresses requirement:** "Global dictionary / count of pairs across multiple compressions"

Maintains a persistent dictionary of frequently occurring pairs across multiple documents or compression sessions.

**Key Features:**
- Persistent storage (saves/loads from file)
- Tracks pair frequency across documents
- Configurable thresholds:
  - `MinUsageThreshold`: Minimum frequency to store (default: 5)
  - `MinElementThreshold`: Maximum elements to track (default: 3)
  - `MaxDictionarySize`: Maximum pairs to keep (default: 10,000)
- Timestamp tracking for first and last occurrence

**Usage:**
```csharp
var dict = new GlobalCompressionDictionary("dictionary.txt", settings);
dict.RecordPairFrequencies(pairFrequencies);
dict.SaveToFile();

// Later, use for pre-compression
var preCompressionPairs = dict.GetPreCompressionPairs(links);
```

**Use Cases:**
- Compressing logs of the same type
- Document collections with similar structure
- Time-series data with repeating patterns

**File Format:**
```
# Global Compression Dictionary
# Generated: 2025-10-16 20:30:00 UTC
Source	Target	TotalFrequency	DocumentCount	FirstSeen	LastSeen
65	66	150	5	2025-10-16 20:00:00	2025-10-16 20:30:00
```

### 4. Relative Frequency Tracker (`RelativeFrequencyTracker.cs`)

**Addresses requirement:** "Relative frequency in % (possible with total characters weight of that average frequency)"

Provides statistical analysis of pair frequencies in percentage terms.

**Key Features:**
- Absolute frequency: raw count
- Relative frequency: percentage of total pairs
- Compression scoring: identifies best compression candidates
- Distribution statistics: mean, median, standard deviation
- Detailed reporting

**Usage:**
```csharp
var tracker = new RelativeFrequencyTracker();
tracker.AnalyzeSequence(sequence);

// Get top pairs
var topPairs = tracker.GetPairsByRelativeFrequency(10);

// Get compression candidates
var candidates = tracker.GetCompressionCandidates(minRelativeFrequencyPercent: 0.5);

// Generate report
string report = tracker.GenerateFrequencyReport(20);
```

**Metrics Provided:**
- Relative frequency percentage
- Frequency density (occurrences per sequence length)
- Potential savings
- Compression score (combines frequency and savings)

### 5. Integrated Example (`IntegratedCompressionExample.cs`)

Demonstrates all optimizations working together in a cohesive system.

**Usage:**
```csharp
IntegratedCompressionExample.RunFullDemo();
IntegratedCompressionExample.CompareBatchVsStandard();
```

## Performance Considerations

### When to Use Each Optimization

1. **Batch Replacement:**
   - Use when: Sequences have many repeated patterns
   - Batch size: 5-10 for most cases, higher for highly repetitive data
   - Trade-off: Larger batches = faster but potentially lower compression ratio

2. **Global Dictionary:**
   - Use when: Compressing multiple related documents
   - Benefits: Shared patterns compressed once, faster subsequent compressions
   - Storage: Dictionary file grows with unique pairs (configure MaxDictionarySize)

3. **Relative Frequency:**
   - Use when: Need to analyze compression opportunities
   - Benefits: Identifies best compression targets
   - Use case: Tuning compression parameters, understanding data patterns

## Future Optimizations

Additional optimizations mentioned in issue #95 but not yet implemented:

### Complete Rewrite to C Code
- Native C implementation with optimized hash table
- Custom hash function tuned for pair patterns
- Potential 5-10x speed improvement
- Would require P/Invoke or C++/CLI wrapper

**Challenges:**
- Integration with existing C# codebase
- Memory management across managed/unmanaged boundary
- Platform-specific optimizations

**Suggested Approach:**
1. Profile current C# implementation to identify bottlenecks
2. Prototype critical sections in C
3. Benchmark C vs C# for specific operations
4. If justified, implement full C version with C# bindings

## Testing

To test these implementations:

1. Build the project:
   ```bash
   cd Platform
   dotnet build
   ```

2. Run experiments from Platform.Sandbox or create test harness

3. Verify compression is lossless by decompressing and comparing

## Contributing

When adding new compression optimizations:

1. Create new file in `experiments/` directory
2. Include XML documentation explaining the optimization
3. Reference the specific requirement from issue #95
4. Provide usage examples
5. Document performance characteristics
6. Add to this README

## References

- [Issue #95: Compression optimization](https://github.com/konard/LinksPlatform/issues/95)
- [Issue #9: Original compression implementation](https://github.com/konard/LinksPlatform/issues/9)
- [Byte Pair Encoding (Wikipedia)](https://en.wikipedia.org/wiki/Byte_pair_encoding)
