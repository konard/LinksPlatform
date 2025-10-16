# Performance Comparison: LinksPlatform vs Google's LevelDB

This directory contains benchmark code for comparing the performance of LinksPlatform's associative data storage with Google's LevelDB key-value store.

## Overview

### Google's LevelDB
[LevelDB](https://github.com/google/leveldb) is a fast key-value storage library written at Google that provides an ordered mapping from string keys to string values. Key features:
- **Persistent Storage**: Data is stored on disk
- **Ordered Keys**: Keys are stored in sorted order
- **Batch Operations**: Support for atomic batch writes
- **Snapshots**: Consistent read-only views of the data
- **Compression**: Optional Snappy compression

### LinksPlatform
[LinksPlatform](https://github.com/linksplatform) is an associative data model implementation that uses "links" (doublets/triplets) as the fundamental unit of storage. Key features:
- **Associative Storage**: Data stored as relationships (source-target pairs)
- **Generic Implementation**: Flexible type support
- **Memory-Mapped Files**: Efficient file-based storage
- **Query by Pattern**: Powerful pattern-based querying
- **Graph-like Structure**: Natural representation of interconnected data

## Architectural Differences

### Data Model

**LevelDB (Key-Value)**:
```
Key    -> Value
"user:1" -> "{ name: 'John', age: 30 }"
"user:2" -> "{ name: 'Jane', age: 25 }"
```

**LinksPlatform (Doublets)**:
```
Link(Source, Target)
Link(1, 1000001)  // Similar to key-value: key=1, value=1000001
Link(2, 1000002)  // But can also represent: Link(User, Name) or Link(Parent, Child)
```

### Use Cases

**LevelDB is optimized for**:
- Simple key-value lookups
- Range scans over sorted keys
- Time-series data
- Cache systems
- IndexedDB backend (used in Chrome)

**LinksPlatform is optimized for**:
- Graph-like data structures
- Associative relationships
- Data compression through deduplication
- Semantic networks
- Knowledge graphs

## Performance Considerations

### Expected Performance Characteristics

**LevelDB Strengths**:
- Fast sequential writes (LSM-tree based)
- Efficient range queries
- Mature and battle-tested
- Wide industry adoption

**LevelDB Weaknesses**:
- Write amplification (multiple writes for single operation)
- Compaction overhead
- No native support for relationships

**LinksPlatform Strengths**:
- Direct memory-mapped access
- Natural representation of relationships
- Potential for structural compression
- Flexible querying

**LinksPlatform Considerations**:
- Different programming model
- Less mature ecosystem
- Performance depends on access patterns

## Benchmark Methodology

The benchmark suite compares common operations:

### Operations Tested

1. **Sequential Writes**: Creating N records in order
2. **Batch Writes**: Creating multiple records atomically (LevelDB only)
3. **Sequential Reads**: Reading N records in order
4. **Random Reads**: Reading N records in random order
5. **Sequential Deletes**: Deleting N records in order
6. **Full Iteration**: Iterating through all records
7. **Mixed Operations**: Random mix of reads and writes

### Test Parameters

- **Operations Count**: 10,000 per test
- **Key Format**: `key_XXXXXXXX` (LevelDB)
- **Value Format**: `value_XXXXXXXX_data_payload` (LevelDB)
- **Link IDs**: Sequential numeric IDs (LinksPlatform)

## Building and Running

### Prerequisites

```bash
dotnet --version  # Requires .NET 8.0 or higher
```

### Dependencies

- `BenchmarkDotNet` 0.14.0 - Industry-standard .NET benchmarking framework
- `LevelDB.Standard` 2.1.6.1 - .NET Standard binding for LevelDB
- `Platform.Data.Doublets` 0.13.4 - LinksPlatform doublets storage

### Build

```bash
cd experiments/LevelDBComparison
dotnet restore
dotnet build -c Release
```

### Run Benchmarks

```bash
dotnet run -c Release
```

Note: Benchmarks may take significant time to complete (15-30 minutes).

## Current Status

**⚠️ Work in Progress**: The benchmark code has been prepared but requires API adjustments to work with the latest Platform.Data.Doublets package (0.13.4). The API has evolved from the older versions used in the Platform.Sandbox examples.

### Known Issues

1. The `UInt64Links` and `UInt64UnitedMemoryLinks` types from older versions have been replaced with the generic `UnitedMemoryLinks<TLink>` API
2. The `Create()` method now requires additional parameters
3. Read operations use different method signatures
4. The `Each()` iteration API has changed

### Next Steps for Completion

1. **Update to Latest API**: Consult the latest Platform.Data.Doublets documentation at https://linksplatform.github.io/Data.Doublets/
2. **API Examples**: Review recent code examples in the Data.Doublets repository
3. **Simplify Tests**: Start with basic Create/Read/Delete operations before adding complex scenarios
4. **Incremental Development**: Build and test one operation at a time

## Alternative Approaches

If the current API complexity is blocking progress, consider:

1. **Use Older Package Version**: Pin to Platform.Data.Doublets 0.1.0 (as used in Platform.Sandbox)
2. **Manual Testing**: Create simple console apps to manually test performance
3. **Analysis-Only Comparison**: Document theoretical performance characteristics based on architecture analysis
4. **Reach Out**: Ask for help on the Links Platform Discord or StackOverflow

## Resources

- [LevelDB Documentation](https://github.com/google/leveldb/blob/main/doc/index.md)
- [LinksPlatform Organization](https://github.com/linksplatform)
- [Data.Doublets GitHub](https://github.com/linksplatform/Data.Doublets)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [Issue #79](https://github.com/konard/LinksPlatform/issues/79)
- [Issue #35](https://github.com/konard/LinksPlatform/issues/35) (Related: Compare with all database engines)

## Contributing

This benchmark is part of ongoing research to compare LinksPlatform with established database systems. Contributions to improve the benchmark methodology, fix API usage, or add additional test scenarios are welcome!

## Conclusion

This comparison aims to:
1. Provide objective performance metrics
2. Understand trade-offs between different data models
3. Identify optimal use cases for each system
4. Guide future development of LinksPlatform

Both systems have different strengths and are optimized for different use cases. The goal is not to declare a "winner" but to understand where each system excels.
