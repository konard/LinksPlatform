# Hybrid Storage Architecture Demo

## Issue Reference
This document demonstrates the solution for [Issue #645: Store mutable data in RAM, and immutable data on Disk](https://github.com/konard/LinksPlatform/issues/645)

## Concept Overview

The hybrid storage architecture separates data based on mutability:

```
┌─────────────────────────────────────────────────────────┐
│                    Archived Types                        │
│           (Immutable States: S₁, S₂, ..., Sₙ₋₁)        │
│                                                          │
│  ┌────────────┐    X→Y    ┌────────────┐    X→Y       │
│  │   Figi     │  ────────▶ │ Archived   │  ────────▶   │
│  │   (S₁)     │  transition│   Figi     │  transition  │
│  │            │            │   (S₂)     │              │
│  └────────────┘            └────────────┘              │
│                                                          │
│              Stored on DISK (FileMappedMemory)          │
└─────────────────────────────────────────────────────────┘
                          ▲
                          │ X→Y (state transitions)
                          │ Append-only log
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    Current Type                          │
│                (Mutable State: Sₙ)                      │
│                                                          │
│               ┌────────────┐                            │
│               │   Figi     │                            │
│               │   (Sₙ)     │                            │
│               │  [ACTIVE]  │                            │
│               └────────────┘                            │
│                                                          │
│              Stored in RAM (HeapMemory)                 │
└─────────────────────────────────────────────────────────┘
```

## Key Components

### 1. Memory Implementations
- **RAM Storage**: `HeapResizableDirectMemory` - Fast access for current state
- **Disk Storage**: `FileMappedResizableDirectMemory` - Persistent archive storage
- **Transition Log**: Append-only file for state transitions (X→Y)

### 2. Architecture Benefits
- **Fast Operations**: Current working set in RAM for optimal performance
- **Historical Preservation**: All previous states archived on disk
- **Complete Audit Trail**: Every state transition logged
- **Memory Efficiency**: Only current state consumes RAM
- **Time-Travel Queries**: Reconstruct any historical state from logs

### 3. Use Cases
- **Version Control**: Track all changes to data structures
- **Debugging**: Replay state transitions to understand bugs
- **Compliance**: Maintain complete audit logs
- **Recovery**: Restore to any previous state
- **Analytics**: Analyze how data evolved over time

## Implementation

See `Platform/Platform.Sandbox/HybridStorageExample.cs` for the complete implementation.

### Basic Usage

```csharp
using var hybrid = new HybridStorageExample("archive.links");

// Create current state in RAM (mutable)
using (var currentLinks = hybrid.CreateCurrentStateLinks())
{
    var link = currentLinks.Create();
    hybrid.LogTransition(link, 0, 0, 0, 0, TransitionType.Create);

    currentLinks.Update(link, link, link);
    hybrid.LogTransition(link, 0, 0, link, link, TransitionType.Update);
}

// Archive current state to disk
hybrid.ArchiveCurrentState();

// Read historical transitions
var transitions = hybrid.ReadTransitionLog();
```

## Performance Characteristics

| Operation | Current State (RAM) | Archive (Disk) | Transition Log |
|-----------|-------------------|----------------|----------------|
| Create    | O(1) - Fast       | N/A            | O(1) - Append  |
| Update    | O(1) - Fast       | N/A            | O(1) - Append  |
| Delete    | O(1) - Fast       | N/A            | O(1) - Append  |
| Query     | O(1) - Fast       | O(1) - Mapped  | O(n) - Scan    |
| Archive   | O(n) - One-time   | O(1) - Append  | N/A            |

## Future Enhancements

1. **Incremental Archiving**: Archive only changed portions
2. **Compression**: Compress archived states
3. **Index Optimization**: Index transition log for faster queries
4. **Parallel Archiving**: Archive in background thread
5. **Smart Caching**: Cache frequently accessed archived states

## Related Work

- Platform.Memory: Base memory abstractions
- Platform.Data.Doublets: Links storage implementation
- Platform.Sandbox.Transactions: Earlier transaction logging experiment
