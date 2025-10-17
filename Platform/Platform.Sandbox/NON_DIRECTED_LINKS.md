# Non-Directed Links Implementation

## Overview

This document describes the experimental implementation of non-directed (undirected) links for both doublets and triplets, addressing issue #315.

## Background

From the Links Theory (doc/articles/links-theory.md, line 346):

> "В отличии от единичной связи двойная связь может связывать любые другие две связи путём установки на них двух ссылок. Есть так же два варианта интерпретации связей, **они могут быть направлены**, тогда первая ссылка зовётся "началом", а последняя (в данном случае вторая) зовётся "концом"; **они могут быть и не направлены**, тогда обе ссылки равнозначны и дополнительного смысла кроме связывание двух в единое не несут."

**Translation**: "Unlike a single link, a double link can connect any two links by setting two references to them. There are also two variants of link interpretation: **they can be directed**, then the first reference is called 'source/beginning' and the last (in this case the second) is called 'target/end'; **they can be non-directed**, then both references are equivalent and carry no additional meaning other than connecting two into one."

## Concept

### Directed Links (Current Standard)
- **Doublets**: (Source, Target) where Source → Target has directional meaning
- **Triplets**: (Source, Linker, Target) where Source → Target via Linker
- (A, B) ≠ (B, A) - these are two different links

### Non-Directed Links (This Implementation)
- **Doublets**: (A, B) where A and B are equivalent, no direction
- **Triplets**: (A, Linker, B) where A and B are equivalent, but Linker retains meaning
- (A, B) = (B, A) - these represent the same link

## Implementation Strategy

### 1. Decorator Pattern

Both implementations use a decorator/wrapper pattern:
- `NonDirectedDoublets<TLinkAddress>` wraps any `ILinks<TLinkAddress>` implementation
- `NonDirectedTriplets<TLinkAddress>` wraps triplet implementations

This approach allows:
- Reuse of existing directed implementations
- No changes to core Platform.Data.Doublets or Platform.Data.Triplets packages
- Easy experimentation and testing

### 2. Normalization

**Key Principle**: Store links in a normalized canonical form to ensure (A, B) and (B, A) refer to the same link.

#### For Doublets:
```csharp
private (TLinkAddress first, TLinkAddress second) Normalize(TLinkAddress source, TLinkAddress target)
{
    // Always store with smaller address first
    if (_comparer.Compare(source, target) <= 0)
        return (source, target);
    else
        return (target, source);
}
```

#### For Triplets:
```csharp
private (TLinkAddress source, TLinkAddress linker, TLinkAddress target) Normalize(
    TLinkAddress source, TLinkAddress linker, TLinkAddress target)
{
    // Source and target are interchangeable, but linker retains its position
    if (_comparer.Compare(source, target) <= 0)
        return (source, linker, target);
    else
        return (target, linker, source);
}
```

### 3. API Behavior

#### Create & Update
- `Update(link, A, B)` stores the link as `(min(A,B), max(A,B))`
- This prevents duplicates: attempting to create both (A, B) and (B, A) results in the same stored link

#### Search
- `SearchOrDefault(A, B)` automatically normalizes the query
- Returns the same result for (A, B) and (B, A)

#### Iteration
- `Each()` returns links in their stored (normalized) form
- Applications interpret these as non-directed relationships

## Files

### Implementation Files

1. **NonDirectedDoubletsExperiment.cs**
   - Wrapper class for non-directed doublets
   - Implements normalization logic
   - Provides standard `ILinks<TLinkAddress>` interface

2. **NonDirectedTripletsExperiment.cs**
   - Wrapper class for non-directed triplets
   - Normalizes source and target while preserving linker semantics
   - Extends triplet operations

3. **NonDirectedLinksExample.cs**
   - Usage examples demonstrating both implementations
   - Comparison between directed and non-directed approaches
   - Educational examples for understanding the concepts

4. **NON_DIRECTED_LINKS.md** (this file)
   - Documentation explaining the implementation
   - Design decisions and rationale

## Usage Example

### Non-Directed Doublets

```csharp
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Sandbox;

// Create underlying directed links storage
using var memory = new HeapResizableDirectMemory();
using var innerLinks = new UnitedMemoryLinks<uint>(memory);

// Wrap with non-directed decorator
var links = new NonDirectedDoublets<uint>(innerLinks);

// Create points
var pointA = links.Create();
var pointB = links.Create();

// Create non-directed link
var link = links.Create();
link = links.Update(link, pointA, pointB);

// Both searches return the same link
var found1 = links.SearchOrDefault(pointA, pointB);  // Returns link
var found2 = links.SearchOrDefault(pointB, pointA);  // Also returns link
// found1 == found2 (same link in normalized form)
```

### Non-Directed Triplets

```csharp
using Platform.Data.Triplets;
using Platform.Sandbox;

// Assume tripletLinks is an ILinks implementation for triplets
var nonDirectedTriplets = new NonDirectedTriplets<ulong>(tripletLinks);

// Create entities
var person1 = nonDirectedTriplets.Create();
var person2 = nonDirectedTriplets.Create();
var friendshipType = nonDirectedTriplets.Create();

// Create non-directed relationship
// "person1 friend-of person2" is the same as "person2 friend-of person1"
var friendship = nonDirectedTriplets.Create();
friendship = nonDirectedTriplets.Update(friendship, person1, friendshipType, person2);
```

## Use Cases

### Suitable for Non-Directed Links:
- Social relationships (friendship, marriage, partnership)
- Physical connections (roads between cities, network cables)
- Symmetric mathematical relationships (equality, equivalence)
- Undirected graphs (network topology, molecular structures)

### Better Suited for Directed Links:
- Hierarchical relationships (parent-child, manager-employee)
- Causal relationships (cause-effect, prerequisite)
- Temporal sequences (before-after, next-previous)
- Directed graphs (dependency graphs, state machines)

## Performance Considerations

### Advantages:
- **Storage efficiency**: No duplicate storage of (A, B) and (B, A)
- **Query simplification**: Single search covers both directions
- **Semantic clarity**: Type system enforces non-directional interpretation

### Trade-offs:
- **Normalization overhead**: Extra comparison on create/update operations
- **Wrapper layer**: Additional indirection (minimal performance impact)
- **Limited to comparable types**: Requires `IComparable<TLinkAddress>`

## Future Enhancements

Potential improvements for this implementation:

1. **Bidirectional Query Support**: Add methods to query in both normalized and original order
2. **Mixed Mode**: Support for storage containing both directed and non-directed links
3. **Type Markers**: Use special linker values to distinguish directed from non-directed triplets
4. **Index Optimization**: Custom indexing strategies optimized for non-directed queries
5. **Package Separation**: Move to dedicated `Platform.Data.Doublets.NonDirected` package

## Testing

To run the examples:

```csharp
var example = new NonDirectedLinksExample();
example.RunAll();
```

This will demonstrate:
- Basic non-directed doublets operations
- Comparison with directed doublets
- Storage efficiency differences

## References

- Issue #315: Non directed links for doublets and triplets implementation
- Links Theory: doc/articles/links-theory.md (line 346)
- Platform.Data.Doublets: https://github.com/linksplatform/Data.Doublets
- Platform.Data.Triplets: https://github.com/linksplatform/Data.Triplets

## License

Same as the parent Links Platform project.
