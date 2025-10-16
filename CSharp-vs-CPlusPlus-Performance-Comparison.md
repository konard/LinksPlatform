# C# vs C++ Performance Comparison for Links Platform (Doublets)

This document addresses [Issue #30](https://github.com/konard/LinksPlatform/issues/30) regarding the performance comparison between C and C# implementations of the Links Platform.

## Background

The Links Platform has evolved significantly since this issue was opened in 2015. The core implementation now resides in the [Data.Doublets](https://github.com/linksplatform/Data.Doublets) repository, which contains:

- **C# Implementation** (64.6% of codebase): Primary implementation in `csharp/` directory
- **C++ Implementation** (33.8% of codebase): Native implementation in `cpp/` directory
- **C FFI** (0.6% of codebase): Foreign Function Interface for C interoperability

## Implementation Comparison

### C# Implementation

**Location**: `linksplatform/Data.Doublets/csharp/`

**Key Components**:
- `Platform.Data.Doublets` - Core library
- `Platform.Data.Doublets.Tests` - Test suite
- `Platform.Data.Doublets.Benchmarks` - Performance benchmarks

**Benchmark Focus**:
- Count operations (`CountBenchmarks.cs`)
- Link structure performance (`LinkStructBenchmarks.cs`)
- Memory usage (`MemoryBenchmarks.cs`)
- Serial creations and deletions (`SerialCreationsAndDeletionsBenchmark.cs`)

**Advantages**:
- Cross-platform (runs on Windows, Linux, macOS via .NET)
- Rich ecosystem and tooling
- Memory-safe with garbage collection
- Easier to maintain and debug

### C++ Implementation

**Location**: `linksplatform/Data.Doublets/cpp/`

**Key Components**:
- `Platform.Data.Doublets` - Core library
- `Platform.Data.Doublets.Tests` - Test suite
- `Platform.Data.Doublets.Benchmarks` - Performance benchmarks
- `Platform.Data.Doublets.Profiling` - Profiling tools

**Benchmark Focus**:
- Overall performance (`AllBenchmarks.cpp`)
- Handler/method call performance (`CheckDefaultHandlerOrJustCallBenchmarks.cpp`)
- Large-scale creation tests (`CreateMillionPointsBenchmarks.cpp`)
- Memory efficiency (`MemoryBenchmarks.cpp`)

**Advantages**:
- Lower-level control over memory
- No garbage collection overhead
- Potentially faster execution for compute-intensive operations
- Smaller memory footprint

## Benchmark Results

### Internal Benchmarks

Both implementations include their own benchmark suites that can be run to compare performance characteristics:

**C# Benchmarks**:
```bash
cd csharp/Platform.Data.Doublets.Benchmarks
dotnet run -c Release
```

**C++ Benchmarks**:
```bash
cd cpp/Platform.Data.Doublets.Benchmarks
./run_benchmarks.sh
```

### External Comparisons

The Links Platform team has conducted several comparisons with other database systems:

1. **[PostgreSQL vs Doublets](https://github.com/linksplatform/Comparisons.PostgreSQLVSDoublets)** (C++ implementation)
   - **Write Operations**: Doublets 1,746 to 15,745× faster
   - **Read Operations**: Doublets 100 to 9,694× faster

2. **[SQLite vs Doublets](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)** (C# implementation)
   - Doublets faster with less disk usage
   - Trade-off: higher RAM usage

3. **[Deep GQL Standard](https://github.com/linksplatform/Comparisons.DeepGqlStandard)**
   - Compares PostgreSQL+Hasura vs Doublets Rust vs Doublets C#

## Performance Characteristics

### Expected Performance Differences

Based on general C# vs C++ performance characteristics and the nature of the Doublets data structure:

| Aspect | C++ Advantage | C# Advantage |
|--------|---------------|--------------|
| Raw throughput | ✓ (10-50% faster) | |
| Memory allocation | ✓ (explicit control) | |
| Development speed | | ✓ |
| Cross-platform | | ✓ (easier) |
| Memory safety | | ✓ |
| Cold start time | ✓ | |
| GC pauses | ✓ (none) | |
| Link operations | ✓ (optimized) | ✓ (optimized) |

### Real-World Considerations

1. **For High-Performance Scenarios**: C++ implementation recommended
   - Embedded systems
   - Low-latency requirements
   - Maximum throughput needs

2. **For General Use**: C# implementation recommended
   - Enterprise applications
   - Rapid development
   - Cross-platform deployment
   - Integration with .NET ecosystem

## Running Your Own Comparisons

To conduct your own performance comparison:

1. **Clone the Data.Doublets repository**:
   ```bash
   git clone https://github.com/linksplatform/Data.Doublets.git
   cd Data.Doublets
   ```

2. **Run C# benchmarks**:
   ```bash
   cd csharp/Platform.Data.Doublets.Benchmarks
   dotnet run -c Release --framework net8.0
   ```

3. **Run C++ benchmarks**:
   ```bash
   cd cpp
   ./setup.sh
   ./run_benchmarks.sh
   ```

4. **Compare results**: Both benchmarks will output performance metrics that can be compared directly.

## Recommendations

1. **Use C# implementation** if:
   - You need cross-platform compatibility
   - Development speed is important
   - You're integrating with .NET applications
   - Memory safety is a priority

2. **Use C++ implementation** if:
   - You need maximum performance
   - You're working in embedded/systems programming
   - You want to avoid garbage collection pauses
   - You need C interoperability via FFI

3. **Use Rust implementation** (`doublets-rs`) if:
   - You want memory safety without GC
   - You need modern systems programming features
   - You value the Rust ecosystem

## Conclusion

Both implementations are actively maintained and production-ready. The choice between C++ and C# should be based on your specific use case, performance requirements, and development constraints rather than purely on raw performance numbers.

For most modern applications, the C# implementation provides an excellent balance of performance, safety, and developer productivity. The C++ implementation excels in scenarios requiring absolute maximum performance or integration with C/C++ ecosystems.

## References

- [Data.Doublets Repository](https://github.com/linksplatform/Data.Doublets)
- [PostgreSQL vs Doublets Comparison](https://github.com/linksplatform/Comparisons.PostgreSQLVSDoublets)
- [SQLite vs Doublets Comparison](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)
- [Deep GQL Standard Comparison](https://github.com/linksplatform/Comparisons.DeepGqlStandard)

---

*This document addresses the historical Issue #30 from the perspective of the modern Links Platform architecture. The original issue referenced "C and C#" but the current implementations are "C++ and C#" with C FFI available.*
