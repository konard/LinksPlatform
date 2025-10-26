# Solution for Issue #645: Store mutable data in RAM, and immutable data on Disk

## Overview

This solution implements a hybrid storage architecture for the LinksPlatform that separates data based on mutability, as described in [Issue #645](https://github.com/konard/LinksPlatform/issues/645).

## Architecture

### Concept from Issue Diagram

```
State Evolution:
S₁ → S₂ → S₃ → ... → Sₙ₋₁ → Sₙ
│    │    │          │       │
└────┴────┴──────────┴───────┘
  Disk (Immutable)      RAM (Mutable)

Where:
- Figi = Example of a type
- ArchivedFigi = Archived version of type
- S = State
- S₁, S₂, ..., Sₙ₋₁ = Historical states (archived on Disk)
- Sₙ = Current state (active in RAM)
- X→Y = State transition (logged in append-only file)
```

### Implementation Strategy

**1. Memory Abstraction Layers**
- **RAM Storage**: `HeapResizableDirectMemory` - For current mutable state (Sₙ)
- **Disk Storage**: `FileMappedResizableDirectMemory` - For archived immutable states (S₁...Sₙ₋₁)
- **Transition Log**: Append-only file - For state transitions (X→Y)

**2. Core Components**
- `HybridStorageExample.cs` - Main implementation demonstrating the concept
- State transition logging for complete audit trail
- Archive operation to move current state to historical storage

**3. Key Features**
- ✅ Fast operations on current data (RAM-based)
- ✅ Historical preservation (Disk-based archive)
- ✅ Complete audit trail (transition log)
- ✅ Memory efficiency (only current state in RAM)
- ✅ Time-travel capabilities (state reconstruction)

## Files Added/Modified

### New Files
1. `Platform/Platform.Sandbox/HybridStorageExample.cs`
   - Complete implementation of hybrid storage architecture
   - Demonstrates RAM/Disk separation
   - Includes state transition logging
   - Provides archiving functionality

2. `experiments/hybrid-storage-demo.md`
   - Comprehensive documentation
   - Architecture diagrams
   - Usage examples
   - Performance characteristics

3. `experiments/run-hybrid-storage-demo.sh`
   - Executable demonstration script
   - Shows concept visually
   - Easy to run and understand

4. `experiments/README.md`
   - Documentation for experiments folder
   - Usage instructions
   - Guidelines for contributors

5. `SOLUTION.md` (this file)
   - Summary of solution
   - Architecture overview
   - Usage guide

### Modified Files
1. `Platform/Platform.Sandbox/Program.cs`
   - Added reference to run HybridStorageExample
   - Commented out for default behavior
   - Easy to enable for testing

## Usage

### Running the Example

**Option 1: Via demo script**
```bash
./experiments/run-hybrid-storage-demo.sh
```

**Option 2: Via C# code**
```csharp
// In Program.cs, uncomment:
HybridStorageExample.Run();
```

**Option 3: Via programmatic API**
```csharp
using var hybrid = new HybridStorageExample("archive.links");

// Create current state in RAM
using (var currentLinks = hybrid.CreateCurrentStateLinks())
{
    var link = currentLinks.Create();
    hybrid.LogTransition(link, 0, 0, 0, 0, TransitionType.Create);
}

// Archive to disk
hybrid.ArchiveCurrentState();

// Read history
var transitions = hybrid.ReadTransitionLog();
```

## Benefits

### Performance
- **Current State Operations**: O(1) - Fast RAM access
- **Archived State Access**: O(1) - Memory-mapped file access
- **Transition Logging**: O(1) - Append-only writes
- **State Reconstruction**: O(n) - Linear scan of transitions

### Scalability
- Only current working set consumes RAM
- Unlimited historical storage on disk
- Incremental archiving possible
- Efficient for long-running systems

### Reliability
- Complete audit trail of all changes
- Point-in-time recovery
- Reproducible state transitions
- Debugging and analysis capabilities

## Technical Details

### State Transition Structure
```csharp
struct StateTransition
{
    long Timestamp;          // When transition occurred
    ulong LinkAddress;       // Which link changed
    ulong OldSource;         // Previous source
    ulong OldTarget;         // Previous target
    ulong NewSource;         // New source
    ulong NewTarget;         // New target
    TransitionType Type;     // Create/Update/Delete
}
```

### Transition Types
- **Create**: New link created (S → S')
- **Update**: Link modified (X → Y)
- **Delete**: Link removed (S → ∅)

## Future Enhancements

1. **Compression**: Compress archived states to save disk space
2. **Incremental Archiving**: Archive only changed portions
3. **Index Optimization**: Index transition log for fast queries
4. **Parallel Operations**: Archive in background thread
5. **Smart Caching**: Cache frequently accessed archived states
6. **Snapshot Management**: Named snapshots for important states
7. **Replication**: Replicate archives across multiple locations

## Related Work

- **Platform.Memory**: Provides memory abstraction interfaces
- **Platform.Data.Doublets**: Core doublets implementation
- **Platform.Sandbox.Transactions**: Earlier transaction logging experiment

## Testing

The implementation includes:
- Conceptual demonstration via shell script
- Full C# implementation with example usage
- Documentation with visual diagrams
- Clear comments explaining the architecture

To verify:
```bash
# Run the demo
./experiments/run-hybrid-storage-demo.sh

# Read the documentation
cat experiments/hybrid-storage-demo.md

# Review the implementation
cat Platform/Platform.Sandbox/HybridStorageExample.cs
```

## Conclusion

This solution provides a practical implementation of the hybrid storage architecture described in Issue #645. It demonstrates how to:

1. ✅ Store mutable data (current state) in RAM for performance
2. ✅ Store immutable data (archived states) on Disk for persistence
3. ✅ Log all state transitions for audit and reconstruction
4. ✅ Provide a clear API for archiving and querying historical states

The implementation is ready for further development, testing, and integration into the main LinksPlatform architecture.
