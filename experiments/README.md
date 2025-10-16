# Memory Storage Strategy Benchmarks

## Purpose

This experiment compares the performance of different storage strategies for link data, addressing [issue #74](https://github.com/konard/LinksPlatform/issues/74).

## Background

The issue explores data-oriented design principles, specifically the concept of [parallel arrays](https://en.wikipedia.org/wiki/Data-oriented_design), to optimize cache locality and memory access patterns.

### Storage Strategies Compared

1. **UnitedMemoryLinks** - All link fields in single memory/file
   - Traditional approach where all data (indexes, source, target) are stored together
   - Simple memory layout, one file

2. **SplitMemoryLinks** - Data separated from indexes (2 files)
   - Data file: Contains link source and target values
   - Index file: Contains tree structures for lookups
   - Follows data-oriented design by separating frequently-accessed data from index structures

3. **Further splits** (future work)
   - One file per index (LeftAsSource, LeftAsTarget, RightAsSource, RightAsTarget)
   - One file per field (complete parallel arrays: Sources[], Targets[], each index type in separate files)

## Benchmarks

The experiment includes three types of workloads:

1. **Create and Delete** - Tests mixed write operations (creation, deletion, reuse)
2. **Read-Heavy** - Tests random read performance with many lookups
3. **Sequential Access** - Tests cache-friendly sequential iteration

Each benchmark compares UnitedMemoryLinks vs SplitMemoryLinks performance.

## Running the Benchmarks

```bash
cd experiments
dotnet run -c Release
```

Results will be output to the console and saved as markdown in `BenchmarkDotNet.Artifacts/results/`.

## Expected Insights

- **UnitedMemoryLinks**: Simpler, potentially better for small datasets with good cache hit rates
- **SplitMemoryLinks**: May show benefits when:
  - Working with large datasets that don't fit in cache
  - Performing operations that primarily access data (not indexes) or vice versa
  - Cache locality improvements from grouping similar data together

## Results

### Benchmark Summary

BenchmarkDotNet v0.13.5, running on Ubuntu 24.04, .NET 8.0.21

| Method | Mean | StdDev | Memory Allocated |
|--------|------|--------|-----------------|
| **Create/Delete Workload** |
| United: All fields in one file | 250.2 ms | 9.3 ms | 14,956 KB |
| Split: Data + Indexes (2 files) | 435.9 ms | 109.3 ms | 19,153 KB |
| **Read-Heavy Workload** |
| United: Read-heavy workload | 87.4 ms | 19.4 ms | 2,663 KB |
| Split: Read-heavy workload | **32.4 ms** | 2.6 ms | 2,663 KB |
| **Sequential Access** |
| United: Sequential access | 1.048 ms | 0.089 ms | 312 KB |
| Split: Sequential access | 1.002 ms | 0.108 ms | 311 KB |

### Analysis

#### 1. Create and Delete Operations (Mixed Write Workload)

**Winner: UnitedMemoryLinks (1.74x faster)**

- UnitedMemoryLinks: 250 ms
- SplitMemoryLinks: 436 ms

For write-heavy operations involving creation and deletion, the traditional united approach performs better. This is likely because:
- Single contiguous memory block reduces pointer indirection
- Fewer memory allocations (14,956 KB vs 19,153 KB)
- Simpler memory management with all data co-located

#### 2. Read-Heavy Workload

**Winner: SplitMemoryLinks (2.69x faster!) 🏆**

- UnitedMemoryLinks: 87.4 ms
- SplitMemoryLinks: **32.4 ms**

This is the most significant finding! SplitMemoryLinks shows dramatic performance improvement for read operations. This validates the data-oriented design principles:
- **Better cache locality**: Separating data from indexes means when searching/reading, only relevant data needs to be loaded into cache
- **Reduced cache pollution**: Index structures don't pollute the cache when we're only reading link values
- **Improved memory access patterns**: Sequential data layout benefits CPU prefetcher

#### 3. Sequential Access

**Result: Nearly Identical (~4% difference)**

- UnitedMemoryLinks: 1.048 ms
- SplitMemoryLinks: 1.002 ms

For sequential iteration through all links, both approaches perform similarly. The overhead of split storage is negligible when accessing all data anyway.

### Key Insights

1. **Data-Oriented Design Pays Off**: The parallel array approach (splitting data from indexes) delivers **2.69x speedup** for read operations, confirming the cache locality benefits mentioned in the Wikipedia article on data-oriented design.

2. **Trade-offs Exist**: Write performance suffers somewhat (1.74x slower) with split storage, likely due to managing two separate memory regions.

3. **Workload Matters**: The optimal storage strategy depends on your access patterns:
   - **Read-heavy workloads**: Use SplitMemoryLinks
   - **Write-heavy workloads**: Use UnitedMemoryLinks
   - **Balanced workloads**: Benchmark your specific use case

4. **Future Exploration**: Per the issue comments, further splits could be tested:
   - One file per index (4 files total)
   - One file per field (complete parallel arrays: separate files for sources, targets, and each index type)

   These could potentially show even better cache locality for specific access patterns, though with increasing complexity.

### Conclusion

This benchmark demonstrates that **storage strategy significantly impacts performance**. For the LinksPlatform use case with frequent searches and lookups, **SplitMemoryLinks provides substantial performance benefits** (2.69x faster reads) at the cost of slightly slower writes (1.74x) and slightly more memory usage (~28% more allocations).

The results validate the data-oriented design approach and the parallel arrays pattern for improving cache locality in memory-constrained or performance-critical scenarios.
