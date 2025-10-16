# Index Benchmarks for LinksPlatform Issue #5

This project implements and benchmarks alternative forms of index (lists of connections between links) as requested in [Issue #5](https://github.com/konard/LinksPlatform/issues/5).

## Implemented Index Types

1. **Hashtable Index** (`HashtableIndex.cs`)
   - Uses `Dictionary<TKey, TValue>` for O(1) average-case lookups
   - Best for exact key lookups
   - Good memory efficiency with moderate load

2. **Linked-List Index** (`LinkedListIndex.cs`)
   - Simple linear search implementation
   - O(n) operations
   - Baseline for comparison purposes

3. **AVL Tree Index** (`AvlTreeIndex.cs`)
   - Self-balancing binary search tree
   - O(log n) operations guaranteed
   - Maintains strict balance (height difference ≤ 1)

4. **Skip-List Index** (`SkipListIndex.cs`)
   - Probabilistic data structure
   - O(log n) average-case operations
   - Simpler implementation than balanced trees

5. **B-Tree Index** (`BTreeIndex.cs`)
   - Balanced tree structure (using SortedDictionary)
   - O(log n) operations
   - Good cache locality properties

6. **Matrix Index** (`MatrixIndex.cs`)
   - Sparse matrix representation
   - Uses dictionary to store only non-empty cells
   - Optimized for dense link relationships

7. **Bit Index** (`BitIndexIndex.cs`)
   - Bit string with optimization for empty spaces
   - Uses BitArray for existence flags
   - Sparse storage for actual data
   - Good for densely packed link addresses

## Project Structure

```
IndexBenchmarks/
├── ILinksIndex.cs              # Common interface for all implementations
├── HashtableIndex.cs           # Hashtable implementation
├── LinkedListIndex.cs          # Linked-list implementation
├── AvlTreeIndex.cs             # AVL tree implementation
├── SkipListIndex.cs            # Skip-list implementation
├── BTreeIndex.cs               # B-Tree implementation
├── MatrixIndex.cs              # Matrix implementation
├── BitIndexIndex.cs            # Bit index implementation
├── LinkIndexBenchmarks.cs      # BenchmarkDotNet benchmarks
├── Program.cs                  # Entry point
├── IndexBenchmarks.csproj      # Project file
└── README.md                   # This file
```

## Building and Running

```bash
# Navigate to the experiments directory
cd experiments/IndexBenchmarks

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run benchmarks
dotnet run -c Release
```

## Benchmark Operations

The benchmarks measure performance for three key operations:

1. **Add**: Insert links into the index
2. **Search**: Find links by (source, target) pair
3. **Remove**: Delete links from the index

Each benchmark uses:
- 1,000 insert operations
- 100 search operations
- 1,000 remove operations

## Expected Results

Based on theoretical complexity:

### Add Operations
- **Fastest**: Hashtable, Matrix (O(1) average)
- **Moderate**: Trees (AVL, B-Tree, Skip-List) (O(log n))
- **Slowest**: Linked-List (O(1) add, but may have overhead)

### Search Operations
- **Fastest**: Hashtable, Matrix, Bit Index (O(1))
- **Moderate**: Trees (AVL, B-Tree, Skip-List) (O(log n))
- **Slowest**: Linked-List (O(n))

### Remove Operations
- **Fastest**: Hashtable (O(1) average)
- **Moderate**: Trees, Matrix (O(log n) to O(1))
- **Slowest**: Linked-List (O(n))

## Memory Usage

The benchmark includes memory diagnostics to compare:
- Memory allocations
- Gen 0/1/2 garbage collections
- Total memory footprint

## Future Improvements

Potential enhancements for production use:

1. **Hybrid Approaches**: Combine index types based on workload
2. **Size-Based Selection**: Use different indexes based on data size
3. **Concurrent Indexes**: Thread-safe implementations
4. **Persistent Indexes**: Disk-based storage integration
5. **Query Optimization**: Specialized indexes for common query patterns

## Related Work

This experiment relates to:
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets) - Current LinksPlatform implementation using Size-Balanced Trees
- [Issue #64](https://github.com/Konard/LinksPlatform/issues/64) - Modular, loosely coupled system modules

## License

This code follows the same license as the LinksPlatform project.
