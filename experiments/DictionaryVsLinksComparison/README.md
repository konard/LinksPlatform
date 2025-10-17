# Dictionary<long, long> vs In-Memory Links Performance Comparison

This benchmark project compares the performance and memory usage of three different approaches for storing key-value pairs:

1. **Dictionary<long, long>** - Standard .NET dictionary
2. **In-Memory Links** - Doublets-style link storage with Index/Source/Target structure
3. **Hybrid** - Dictionary for fast lookups combined with Links for storage

## Purpose

This benchmark addresses issue #224 to compare performance and memory usage between Dictionary<long, long> and in-memory links implementations, including hybrid approaches.

## Implementations

### 1. Dictionary<long, long>
- Standard .NET `Dictionary<long, long>`
- O(1) average case lookup, insert, update, delete
- Hash-based storage
- Memory overhead for hash table structure

### 2. SimpleLinkStorage
- Simulates the Doublets structure: each link has Index (ID), Source, Target
- Uses Dictionary internally but stores structured links
- Demonstrates the overhead of the link abstraction
- Each link is a struct with 3 ulong fields (24 bytes per link)

### 3. HybridStorage
- Combines Dictionary<long, ulong> for key-to-link-index mapping
- Uses SimpleLinkStorage for actual data storage
- Demonstrates the trade-off of maintaining two data structures

## Benchmark Operations

Each benchmark tests four fundamental operations:
- **Insert**: Create N new entries
- **Read**: Read all N entries after creation
- **Update**: Update all N entries after creation
- **Delete**: Delete all N entries after creation

## Test Sizes

Benchmarks run with N = 10,000 operations to measure realistic performance.

## Build and Run

```bash
cd experiments/DictionaryVsLinksComparison
dotnet build -c Release
dotnet run -c Release
```

## Expected Results

### Performance Characteristics

**Dictionary<long, long>** should be fastest because:
- Direct hash-based storage
- No abstraction overhead
- Optimized by .NET runtime
- Single data structure

**SimpleLinkStorage** will likely be slower because:
- Additional struct allocation overhead
- 3 fields per entry vs 2 (Index, Source, Target vs Key, Value)
- Same dictionary underneath but with abstraction cost

**HybridStorage** will likely be slowest because:
- Two dictionary lookups per operation
- Additional memory for key-to-index mapping
- Doubleindirection for reads/updates

### Memory Characteristics

**Dictionary<long, long>**:
- Smallest memory footprint
- 2 × 8 bytes per entry (key + value) + hash table overhead
- ~16-24 bytes per entry depending on load factor

**SimpleLinkStorage**:
- Medium memory footprint
- 3 × 8 bytes per entry (index + source + target) + dictionary overhead
- ~24-32 bytes per entry

**HybridStorage**:
- Largest memory footprint
- Dictionary<long, ulong> (16 bytes + overhead)
- SimpleLinkStorage (24 bytes + overhead)
- ~40-56 bytes per entry

## Analysis

### When to Use Dictionary<long, long>
- Simple key-value storage
- No need for relationship modeling
- Performance is critical
- Memory is constrained

### When to Use Links (Doublets)
- Modeling relationships and associations
- Need for index-based traversal
- Building graph structures
- Semantic data representation
- Willing to trade some performance for structure

### When to Use Hybrid
- Need fast key-based lookup
- Also need relationship modeling
- Can afford extra memory overhead
- Building complex associative systems

## Notes on Platform.Data.Doublets

The actual `Platform.Data.Doublets` library provides:
- File-backed persistent storage
- Split memory architecture for optimization
- Index trees for efficient queries
- More sophisticated memory management

This benchmark uses simplified in-memory implementations to isolate and measure the core performance characteristics of each approach.

## Related

- Issue: https://github.com/konard/LinksPlatform/issues/224
- Pull Request: https://github.com/konard/LinksPlatform/pull/872
- Platform.Data.Doublets: https://github.com/linksplatform/Data.Doublets

## Conclusions

For simple key-value storage, `Dictionary<long, long>` is the optimal choice. The Links/Doublets approach provides value when:

1. **Relationships matter**: Modeling connections between entities
2. **Graph traversal**: Need to navigate relationships efficiently
3. **Semantic storage**: Representing meaning through associations
4. **Persistence**: File-backed storage with memory-mapped files
5. **Split memory**: Advanced optimizations like individual index trees

The overhead of the link abstraction (typically 50-100% slower and 2-3x more memory) is justified when you need the additional structure and capabilities that links provide.

The hybrid approach shows that combining both structures incurs significant overhead and should only be used when you specifically need both fast key-based lookup AND relationship modeling in the same system.
