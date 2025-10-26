# Experiments Folder

This folder contains experimental code and demonstrations for various LinksPlatform concepts and implementations.

## Current Experiments

### Hybrid Storage Architecture (Issue #645)

**Files:**
- `hybrid-storage-demo.md` - Comprehensive documentation of the hybrid storage concept
- `run-hybrid-storage-demo.sh` - Executable demo script
- `../Platform/Platform.Sandbox/HybridStorageExample.cs` - Full C# implementation

**Quick Start:**
```bash
# Run the demonstration
./experiments/run-hybrid-storage-demo.sh

# Or view the documentation
cat experiments/hybrid-storage-demo.md
```

**Description:**
Demonstrates a hybrid storage architecture that separates data based on mutability:
- **Mutable data** (current state Sn) stored in RAM using `HeapResizableDirectMemory`
- **Immutable data** (archived states S1...Sn-1) stored on Disk using `FileMappedResizableDirectMemory`
- **State transitions** (X→Y) logged in append-only file for complete audit trail

This architecture provides:
- Fast operations on current data
- Complete historical preservation
- Efficient memory usage
- Time-travel query capabilities
- Full audit trails

## Usage

Each experiment is self-contained and includes:
1. Documentation explaining the concept
2. Implementation code
3. Demo/test scripts
4. Usage examples

## Contributing

When adding new experiments:
1. Create a descriptive subfolder or file prefix
2. Include documentation explaining the concept
3. Provide runnable examples
4. Document any dependencies
5. Update this README

## Related Documentation

- [Links Theory](../doc/articles/links-theory.md) - Theoretical foundation
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets) - Main implementation
- [Platform.Memory](https://github.com/linksplatform/Memory) - Memory management abstractions
