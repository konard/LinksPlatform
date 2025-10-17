# Random Access Links API Specification

## Overview

This document specifies the "Random Access Links API" (also referred to as "Stackless API"), which extends the current stack-based Links API with random access capabilities.

## Background

### Current API Model (Stack-Based)

The current ILinks API follows a stack model where:
- `Create()` operation is analogous to `Push()` - creates a link at the next available location
- `Delete()` operation is analogous to `Pop()` - removes a link from its current location

This model doesn't allow for:
1. Moving a link to a specific index/location
2. Creating a link at a desired location (with replacement if occupied)

### Proposed API Model (Random Access)

The Random Access Links API introduces true random access capabilities, allowing links to be manipulated at arbitrary positions without following stack semantics.

## API Design

### Core Operations

#### 1. Move Operation
**Purpose**: Change the index/location of an existing link.

**Signature** (conceptual):
```csharp
TLinkAddress Move(TLinkAddress linkAddress, TLinkAddress newAddress, WriteHandler<TLinkAddress>? handler);
```

**Parameters**:
- `linkAddress`: The current address of the link to move
- `newAddress`: The desired target address for the link
- `handler`: Optional callback handler for write operations

**Behavior**:
- Moves an existing link from its current location to the specified target location
- If `newAddress` is occupied, the behavior depends on implementation strategy:
  - Option A: Fail with error/exception
  - Option B: Swap with existing link at target location
  - Option C: Replace existing link (delete old, move current)
- Returns the actual address where the link was moved

**Use Cases**:
- Reorganizing link storage for better locality
- Implementing custom memory management strategies
- Optimizing cache performance

#### 2. CreateAt Operation
**Purpose**: Create a link at a specific desired location.

**Signature** (conceptual):
```csharp
TLinkAddress CreateAt(TLinkAddress desiredAddress, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler);
```

**Parameters**:
- `desiredAddress`: The specific address where the link should be created
- `substitution`: The link data (typically source and target addresses)
- `handler`: Optional callback handler for write operations

**Behavior**:
- Attempts to create a link at the specified `desiredAddress`
- If location is already occupied:
  - Option A: Replace the existing link (destructive)
  - Option B: Fail with error/exception
  - Option C: Find nearest available location
- Returns the actual address where the link was created

**Use Cases**:
- Pre-allocating links at specific locations
- Implementing deterministic link addressing schemes
- Restoring links from backup/serialization

### Extended Interface

```csharp
/// <summary>
/// Represents an extended ILinks interface with random access capabilities.
/// </summary>
/// <typeparam name="TLinkAddress">The type used for link addresses.</typeparam>
/// <typeparam name="TConstants">The type containing link constants.</typeparam>
public interface IRandomAccessLinks<TLinkAddress, TConstants> : ILinks<TLinkAddress, TConstants>
    where TLinkAddress : IUnsignedNumber<TLinkAddress>
    where TConstants : LinksConstants<TLinkAddress>
{
    /// <summary>
    /// Moves an existing link to a new address location.
    /// </summary>
    /// <param name="linkAddress">The current address of the link to move.</param>
    /// <param name="newAddress">The desired target address.</param>
    /// <param name="handler">Optional write operation handler.</param>
    /// <returns>The actual address where the link was moved.</returns>
    TLinkAddress Move(TLinkAddress linkAddress, TLinkAddress newAddress, WriteHandler<TLinkAddress>? handler);

    /// <summary>
    /// Creates a link at a specific desired address location.
    /// </summary>
    /// <param name="desiredAddress">The specific address for the new link.</param>
    /// <param name="substitution">The link data containing source and target.</param>
    /// <param name="handler">Optional write operation handler.</param>
    /// <returns>The actual address where the link was created.</returns>
    TLinkAddress CreateAt(TLinkAddress desiredAddress, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler);
}
```

## Implementation Considerations

### Memory Management

The Random Access API works best when combined with **Issue #208** optimization:
- Use ranges list instead of just linked list for managing free memory
- This allows efficient allocation/deallocation at arbitrary positions
- Improves performance of Move and CreateAt operations

### Conflict Resolution Strategies

When target location is occupied, implementations should support configurable strategies:

1. **Replace (Default)**: Delete existing link and place new one
2. **Fail**: Return error/throw exception
3. **Swap**: Exchange positions of current and target links
4. **Shift**: Move occupying link to nearest free location

### Performance Characteristics

| Operation | Stack API | Random Access API |
|-----------|-----------|-------------------|
| Create at end | O(1) | O(1) |
| Create at specific location | N/A | O(1) with ranges* |
| Delete | O(1) | O(1) |
| Move | N/A | O(1) with ranges* |

*Assuming Issue #208 range-based memory management is implemented

## Migration Path

### Backward Compatibility

The Random Access API extends the existing ILinks interface, ensuring:
- Existing code using stack-based API continues to work
- New methods are additive, not breaking changes
- Implementations can provide both APIs

### Adoption Strategy

1. **Phase 1**: Define IRandomAccessLinks interface (this document)
2. **Phase 2**: Implement in Platform.Data.Doublets
3. **Phase 3**: Add tests and benchmarks
4. **Phase 4**: Document usage examples
5. **Phase 5**: Migrate existing code where beneficial

## Usage Examples

### Example 1: Moving a Link

```csharp
// Move link from current location to address 1000
var links = new RandomAccessLinks<ulong>();
var linkAddress = links.Create(); // Creates at next available location
var newAddress = 1000ul;
var actualAddress = links.Move(linkAddress, newAddress, null);
```

### Example 2: Creating at Specific Location

```csharp
// Create a link specifically at address 500
var links = new RandomAccessLinks<ulong>();
var desiredAddress = 500ul;
var substitution = new ulong[] { sourceLink, targetLink };
var actualAddress = links.CreateAt(desiredAddress, substitution, null);
```

### Example 3: Restoring from Backup

```csharp
// Restore links to their original addresses
foreach (var backup in backupData)
{
    links.CreateAt(backup.Address, backup.Substitution, null);
}
```

## Related Issues

- **Issue #207**: Random access links API (this document)
- **Issue #208**: Manage Links memory using ranges list instead of just linked list

## References

- [Platform.Data ILinks Interface](https://github.com/linksplatform/Data/blob/master/csharp/Platform.Data/ILinks.cs)
- [Platform.Data.Doublets Implementation](https://github.com/linksplatform/Data.Doublets)

## Status

**Draft Specification** - Open for feedback and discussion.

---

*Last Updated: 2025-10-17*
*Author: AI Issue Solver*
