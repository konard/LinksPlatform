# Doublets vs Redis: Code Examples

This directory contains practical code examples demonstrating the usage of both Doublets and Redis for comparison purposes.

## Examples

### 1. Doublets Example (C#)

**File**: `doublets_example.cs`

Demonstrates:
- Creating a links database with memory-mapped files
- Creating points (self-referential links)
- Building relationships between entities
- Creating sequences using binary tree structures
- Performance benchmarks for link creation
- Querying link structures

**Prerequisites**:
```bash
# Install .NET SDK (6.0 or later)
# Add Platform.Data.Doublets NuGet package
dotnet add package Platform.Data.Doublets
```

**Run**:
```bash
dotnet run doublets_example.cs
```

### 2. Redis Example (Python)

**File**: `redis_example.py`

Demonstrates:
- Connecting to Redis server
- Creating entities using hashes
- Building relationships using sets and hashes
- Creating sequences using lists
- Graph representation using sorted sets
- Performance benchmarks for key operations
- Querying data structures

**Prerequisites**:
```bash
# Install Redis server
# On Ubuntu/Debian:
sudo apt-get install redis-server

# On macOS:
brew install redis

# Start Redis server
redis-server

# Install Python Redis client
pip install redis
```

**Run**:
```bash
python3 redis_example.py
```

## Comparison Highlights

### Data Model

**Doublets**:
- Everything is a binary link (pair of references)
- Homogeneous structure (single primitive)
- Requires building higher-level abstractions

**Redis**:
- Multiple specialized data structures
- Ready-to-use abstractions (hashes, sets, lists, etc.)
- Heterogeneous approach

### Performance

Both examples include performance tests that demonstrate:
- **Doublets**: Fast link creation with memory-mapped file persistence
- **Redis**: Extremely fast in-memory operations with optional persistence

### Use Case Suitability

**Doublets** excels at:
- Graph-like structures with shared substructures
- Associative memory for AI systems
- Research and experimentation with link-based models
- Scenarios requiring maximum flexibility

**Redis** excels at:
- Caching and session storage
- Real-time counters and analytics
- Message queuing and pub/sub
- Standard data structure operations
- Production systems requiring maturity and tooling

## Running Benchmarks

Both examples include performance tests. To compare:

1. Run both examples on the same hardware
2. Note the timing results for:
   - Creating 10,000 simple entities (points/keys)
   - Creating 10,000 relationships (pairs/hashes)
3. Consider the different persistence models:
   - Doublets: Memory-mapped files (always persisted)
   - Redis: In-memory (optional persistence)

## Further Reading

See the main comparison document: `COMPARISON.md` in the repository root for a comprehensive analysis of both systems.

## Notes

- The Doublets example creates a file `example.links` in the current directory
- The Redis example requires a running Redis server on `localhost:6379`
- Both examples include cleanup instructions in their output
