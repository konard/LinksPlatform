# Non-Directed Links Design

## Overview

This document describes the conceptual design for non-directed (undirected) links for both doublets and triplets, addressing issue #315.

**Note**: This is a design document and conceptual specification. Actual implementation would require updating to compatible Platform.Data package versions.

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

## Design Files

### Documentation

1. **NON_DIRECTED_LINKS.md** (this file)
   - Design specification and conceptual overview
   - Implementation approach and rationale
   - Design decisions and use cases

## Conceptual Usage Example

### Non-Directed Doublets (Pseudocode)

```csharp
// Conceptual example - actual implementation would depend on
// specific Platform.Data.Doublets version API

// Create underlying directed links storage
var innerLinks = CreateDoubletsStorage();

// Wrap with non-directed decorator
var links = new NonDirectedDoublets(innerLinks);

// Create points
var pointA = links.CreatePoint();
var pointB = links.CreatePoint();

// Create non-directed link between A and B
// Stored as (min(A,B), max(A,B))
var link = links.CreateLink(pointA, pointB);

// Both searches would return the same link
var found1 = links.Search(pointA, pointB);  // Returns link
var found2 = links.Search(pointB, pointA);  // Also returns link
// found1 == found2 (same link in normalized form)
```

### Non-Directed Triplets (Pseudocode)

```csharp
// Conceptual example for non-directed triplets

var tripletLinks = CreateTripletsStorage();
var nonDirectedTriplets = new NonDirectedTriplets(tripletLinks);

// Create entities
var person1 = tripletLinks.CreatePoint();
var person2 = tripletLinks.CreatePoint();
var friendshipType = tripletLinks.CreatePoint();

// Create non-directed relationship
// Stored as (min(person1,person2), friendshipType, max(person1,person2))
// "person1 friend-of person2" == "person2 friend-of person1"
var friendship = nonDirectedTriplets.CreateTriplet(person1, friendshipType, person2);
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

## Implementation Roadmap

To implement this design:

1. **API Analysis**: Study the current Platform.Data.Doublets and Platform.Data.Triplets APIs
2. **Decorator Implementation**: Create decorator classes following the normalization strategy
3. **Extension Methods**: Implement convenience methods for common operations
4. **Testing**: Comprehensive test suite covering all use cases
5. **Documentation**: Examples and migration guides
6. **Package**: Consider separate NuGet package for non-directed link support

## Future Enhancements

Potential improvements:

1. **Bidirectional Query Support**: Methods to query in both normalized and original order
2. **Mixed Mode**: Storage containing both directed and non-directed links
3. **Type Markers**: Special linker values to distinguish directed from non-directed triplets
4. **Index Optimization**: Custom indexing strategies optimized for non-directed queries
5. **Package Separation**: Dedicated `Platform.Data.Doublets.NonDirected` NuGet package
6. **Query Language Extensions**: Support for non-directed semantics in query languages

## Implementation Considerations

### Package Compatibility

Actual implementation requires:
- Compatible Platform.Data.Doublets version (with current ILinks API)
- Compatible Platform.Data.Triplets version
- Understanding of the specific API surface for the target version

### Testing Strategy

A complete implementation would include:
- Unit tests for normalization logic
- Integration tests with actual link storage
- Performance benchmarks comparing directed vs non-directed
- Edge case tests (self-loops, duplicate prevention)

## References

- Issue #315: Non directed links for doublets and triplets implementation
- Links Theory: doc/articles/links-theory.md (line 346)
- Platform.Data.Doublets: https://github.com/linksplatform/Data.Doublets
- Platform.Data.Triplets: https://github.com/linksplatform/Data.Triplets

## License

Same as the parent Links Platform project.
