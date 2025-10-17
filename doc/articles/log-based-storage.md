# Log-Based Storage Architecture

## Overview

This document describes the log-based storage approach for LinksPlatform databases, particularly optimized for small databases where the entire state can fit in memory.

## Motivation

Traditional database systems write full snapshots of the database state to disk. While this works well, it has several drawbacks:

- **Space inefficiency**: Full snapshots contain redundant information
- **Write amplification**: Small changes require writing the entire database state
- **Complexity**: Managing snapshots and compaction adds complexity

For small databases (where the full state fits in memory), a log-based approach offers several advantages:

- **Minimal disk writes**: Only operations are logged, not the full state
- **Compact storage**: Logs can be extremely compact, especially with create-only mode
- **Simplicity**: No snapshot management needed for small databases
- **Fast recovery**: Replay log on startup to rebuild in-memory state

## Architecture

### Core Principles

1. **In-Memory State**: The current database state (snapshot) exists only in memory
2. **Append-Only Log**: All operations are appended to a disk-based log
3. **Log Replay**: On startup, replay the log to reconstruct the in-memory state
4. **Create-Only Operations**: Combined with issue #466, all changes use only create operations

### Log Format

When combined with the create-only approach (issue #466), the log becomes extremely compact. Each entry is simply a pair of values:

```
[Source, Target]
```

For a doublets database, each link creation is logged as:
- Source address/value
- Target address/value

### Storage Components

#### 1. In-Memory Doublets Store
- Holds the current state of all links
- Optimized for fast reads and writes
- No direct disk persistence

#### 2. Append-Only Operation Log
- Sequential file on disk
- Contains only link creation operations
- Minimal format: just source-target pairs
- Can be buffered for performance

#### 3. Log Replayer
- Reads log on database open
- Reconstructs in-memory state
- Validates log integrity

### Operation Flow

#### Database Initialization
```
1. Open operation log file
2. Read all entries sequentially
3. Replay each create operation into memory
4. Database ready for use
```

#### Link Creation
```
1. Create link in memory
2. Append [source, target] to log
3. Optionally flush log buffer
4. Return link address
```

#### Database Shutdown
```
1. Flush any buffered log entries
2. Close log file
3. Memory state is discarded
```

### Advantages

1. **Extreme Compactness**: With create-only mode, log entries are minimal
2. **High Write Performance**: Only append operations to log
3. **Simple Implementation**: No complex snapshot management
4. **Fast Read Performance**: All reads from memory
5. **Crash Recovery**: Log replay reconstructs state

### Trade-offs

1. **Startup Time**: Log must be replayed on each startup
   - Acceptable for small databases
   - Linear time complexity: O(n) where n is number of operations

2. **Memory Requirement**: Full database must fit in memory
   - Appropriate for small to medium databases
   - Need fallback to snapshot-based approach for large databases

3. **Log Growth**: Log grows unbounded without compaction
   - For create-only mode, this is actually the full history
   - Compaction can be optional periodic operation

### Scaling Strategy

For databases that outgrow memory:

1. **Threshold Detection**: Monitor memory usage
2. **Snapshot Creation**: Write current state to snapshot file
3. **Log Rotation**: Start new log after snapshot
4. **Hybrid Mode**: Keep recent changes in log, older state in snapshot

## Implementation Considerations

### Create-Only Mode Integration

When combined with create-only approach (issue #466):
- All modifications are modeled as new link creations
- Deletions are modeled as creating tombstone links or unlinking operations
- Log becomes a complete, immutable history
- No update or delete operations in log format

### Log Entry Format

Minimal binary format for maximum compactness:

```
For 32-bit links:
[uint32_t source] [uint32_t target]  // 8 bytes per entry

For 64-bit links:
[uint64_t source] [uint64_t target]  // 16 bytes per entry
```

Optional extensions:
- Timestamp (for temporal queries)
- Operation type flag (if supporting more than create)
- Checksum (for integrity verification)

### Buffering Strategy

For optimal performance:
- Buffer log writes in memory
- Flush on:
  - Buffer full
  - Explicit sync request
  - Database close
  - Periodic timer
- Configurable trade-off between durability and performance

### Recovery and Validation

On startup:
1. Open log file
2. Validate file integrity (size, checksums if present)
3. Read entries sequentially
4. Replay each operation
5. Validate final state (optional)
6. Ready for operations

Error handling:
- Corrupted log entry: truncate and recover up to last valid entry
- Incomplete log: recover what's readable
- Log conflicts with expected state: alert and recover

## Comparison with Current Approach

### Current: Memory-Mapped Files
- Full database state persisted to disk
- Direct memory mapping to file
- No separate log
- Efficient for medium to large databases

### Proposed: Log-Based
- Only operations logged to disk
- In-memory state rebuilt from log
- Optimal for small databases
- Much more compact storage

## Use Cases

### Ideal For:
- Small databases (< 100MB in memory)
- High write throughput requirements
- Applications with frequent restarts where startup time is acceptable
- Scenarios requiring complete history
- Embedded systems with limited storage

### Not Ideal For:
- Very large databases (> available memory)
- Applications requiring instant startup
- Systems where log replay time is unacceptable

## Future Extensions

1. **Incremental Snapshots**: Periodic snapshots to speed up recovery
2. **Log Compaction**: Remove redundant entries (if not using create-only)
3. **Parallel Log Replay**: Multi-threaded recovery for faster startup
4. **Compression**: Compress log entries for even smaller storage
5. **Replication**: Log can be easily replicated to other systems

## Conclusion

The log-based storage approach provides an elegant solution for small databases, offering extreme compactness and simplicity. When combined with the create-only mode (issue #466), it creates a highly efficient storage system where the log is both compact and a complete historical record.

This approach represents a valuable addition to LinksPlatform's storage options, complementing the existing memory-mapped file approach for different use cases and database sizes.
