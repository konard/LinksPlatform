# Sequences Examples - Issue #24

This directory contains comprehensive examples demonstrating the Sequences functionality requested in [Issue #24](https://github.com/konard/LinksPlatform/issues/24).

## Overview

These examples demonstrate practical usage of the `Platform.Data.Doublets.Sequences` package, covering all the major features and requirements from the issue.

## Examples

### 1. SequencesCUDExample.cs
**Demonstrates:** Sequences Create, Update, Delete operations with sequence markers

- ✓ Creating sequences with different configurations
- ✓ Using sequence markers to distinguish intended sequences from subsequences
- ✓ Enabling garbage collection through markers
- ✓ Update operations (create new, delete old pattern)
- ✓ Delete operations with garbage collection considerations

**Key Concepts:**
- Sequence markers help identify user-created sequences vs internal compression artifacts
- Markers enable safe garbage collection of unused subsequences
- Immutable nature of links means updates = create new + optionally delete old

### 2. SequencesPatternMatchingExample.cs
**Demonstrates:** Pattern matching with AnyLink and wildcards

- ✓ AnyLink (`_`) matching - matches any single element
- ✓ ZeroOrMany (`*`) matching - matches zero or more elements
- ✓ Partial matching - finding sequences containing query elements
- ✓ Connection matching - finding sequences connecting query elements

**Key Concepts:**
- `_` (Any) matches exactly one element in that position
- `*` (ZeroOrMany) matches 0+ elements
- Multiple query methods: exact match, pattern match, partial match, connections
- Results indicate whether match is Sequence [S] or Part [P]

### 3. SequencesAppendExample.cs
**Demonstrates:** Sequence append/continue operations with optimization

- ✓ Basic append by creating extended sequences
- ✓ Append with repeated patterns for compression
- ✓ Incremental sequence building (one element at a time)
- ✓ Optimization: finding longest repeated subsequence from append point
- ✓ Moving repeated subsequences to separate links for reuse

**Key Concepts:**
- Append = create new sequence with existing + new elements
- Optimization searches for longest repeated patterns from append point
- Only longest repeated sequence is optimized (step toward full compression)
- Compression during append improves storage efficiency

### 4. SequencesOptionsExample.cs
**Demonstrates:** Different SequencesOptions configurations and their tradeoffs

- ✓ No marker, no compression - fastest, simplest
- ✓ Marker only - enables garbage collection
- ✓ Compression only - best storage efficiency
- ✓ Marker + compression - fully optimized (recommended for production)
- ✓ Performance comparison of different configurations

**Key Concepts:**
- `UseSequenceMarker` - enables distinguishing sequences from subsequences
- `SequenceMarkerLink` - the link ID used as the marker
- `UseCompression` - enables pattern detection and reuse
- Tradeoffs: speed vs storage vs garbage collection capability

## Running the Examples

### Option 1: Run all examples
```bash
cd experiments
dotnet run SequencesExamplesRunner.cs
```

### Option 2: Run individual examples
Each example file contains a `Run()` method that can be called independently:
```csharp
SequencesCUDExample.Run();
SequencesPatternMatchingExample.Run();
SequencesAppendExample.Run();
SequencesOptionsExample.Run();
```

## Requirements Coverage

These examples cover the following items from [Issue #24](https://github.com/konard/LinksPlatform/issues/24):

- [x] Cover Sequences with tests (already completed)
- [x] **Demonstrate** Sequences CUD with sequence markers → `SequencesCUDExample.cs`
- [x] **Demonstrate** Testing CUD with different options → All examples show various options
- [x] **Demonstrate** SequencesOptions behavior changes → `SequencesOptionsExample.cs`
- [x] **Demonstrate** Simple AnyLink match algorithm → `SequencesPatternMatchingExample.cs`
- [x] **Demonstrate** SequenceAppend/SequenceContinue with optimization → `SequencesAppendExample.cs`
- [ ] Optimization on Search (Read) with request sequence tracking (future work)
- [ ] Store requests and results with triggers (future work)
- [ ] Unified Sequences interface for triple and pair links (future work)
- [ ] Unified Sequences format helper (future work)
- [ ] Recompile Sequences logic based on SequencesOptions (implementation in package)

## Notes

- These examples use `Platform.Data.Doublets.Sequences` package version 0.6.x
- All examples clean up their test database files automatically
- Examples are educational and demonstrate best practices
- Code is well-commented to explain concepts and patterns

## Related

- Main issue: https://github.com/konard/LinksPlatform/issues/24
- Pull Request: https://github.com/konard/LinksPlatform/pull/716
- Sequences package: https://github.com/linksplatform/Data.Doublets.Sequences
