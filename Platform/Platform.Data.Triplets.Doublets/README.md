# Platform.Data.Triplets.Doublets

An implementation of Triplets links built on top of Doublets Sequences.

## Overview

This library provides a Triplet data model (3-tuples with Source-Linker-Target semantics) using Doublets Sequences (variable-length ordered collections built from binary links) as the underlying storage engine.

## Key Features

- **Triplet Abstraction**: Work with semantic triplets (Subject-Predicate-Object) while leveraging Doublets' efficient binary link storage
- **Sequence-Based Storage**: Each triplet is stored as a sequence of exactly 3 elements
- **Efficient Querying**: Search triplets by Source, Linker, Target, or any combination
- **Compression**: Benefits from Doublets' sequence compression for repeated patterns
- **Unified Storage**: Single storage engine (Doublets) supporting both models

## Architecture

### Triplet Structure

Each triplet consists of three components:
- **Source**: The subject or starting point of the relationship
- **Linker**: The predicate, verb, or type of relationship
- **Target**: The object or endpoint of the relationship

Example: `(Alice, knows, Bob)` where:
- Source = Alice
- Linker = knows
- Target = Bob

### Implementation

Triplets are implemented as 3-element sequences in the Doublets model:

```
Triplet(S, L, T) = DoubletSequence([S, L, T])
```

This is stored as a binary tree structure:
```
Link(
    Source: S,
    Target: Link(
        Source: L,
        Target: T
    )
)
```

## Usage

### Basic Example

```csharp
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Triplets.Doublets;

// Create doublets storage
using var doublets = new UnitedMemoryLinks<uint>("db.links");

// Create triplet links wrapper
var triplets = new TripletLinks<uint>(doublets);

// Create entities
var alice = doublets.Create();
var bob = doublets.Create();
var knows = doublets.Create();

// Create a triplet: Alice knows Bob
var triplet = triplets.CreateTriplet(alice, knows, bob);

// Query triplets
triplets.SearchTriplets(alice, triplets.Constants.Any, triplets.Constants.Any, link =>
{
    var t = triplets.GetTriplet(link[0]);
    Console.WriteLine($"Source: {t[0]}, Linker: {t[1]}, Target: {t[2]}");
    return triplets.Constants.Continue;
});
```

### Semantic Web Example

```csharp
// Create ontology
var Person = doublets.Create();
var type = doublets.Create();
var worksFor = doublets.Create();

// Create instances
var john = doublets.Create();
var company = doublets.Create();

// Build knowledge graph
triplets.CreateTriplet(john, type, Person);
triplets.CreateTriplet(john, worksFor, company);

// Count relationships
var count = triplets.CountTriplets(john, triplets.Constants.Any, triplets.Constants.Any);
Console.WriteLine($"John has {count} relationships");
```

## API Reference

### TripletLinks<TLinkAddress>

Main class for working with triplets.

#### Methods

- `CreateTriplet(source, linker, target)` - Creates a new triplet
- `GetSource(triplet)` - Gets the source component
- `GetLinker(triplet)` - Gets the linker component
- `GetTarget(triplet)` - Gets the target component
- `GetTriplet(triplet)` - Gets all three components as an array
- `UpdateTriplet(triplet, newSource, newLinker, newTarget)` - Updates a triplet
- `DeleteTriplet(triplet)` - Deletes a triplet
- `IsTriplet(link)` - Checks if a link is a valid triplet
- `SearchTriplets(source, linker, target, handler)` - Searches for matching triplets
- `CountTriplets(source, linker, target)` - Counts matching triplets

#### ILinks<TLinkAddress> Implementation

Also implements standard ILinks interface:
- `Create(substitution)` - Create with [Source, Linker, Target]
- `Update(restriction, substitution)` - Update triplet
- `Delete(restriction)` - Delete triplet
- `Count(restriction)` - Count triplets
- `Each(handler, restriction)` - Iterate triplets

## Benefits

### Why Triplets on Doublets Sequences?

1. **Unified Storage**: Single storage engine for both Doublets and Triplets
2. **Compression**: Leverage sequence compression for repeated patterns
3. **Flexibility**: Can represent n-tuples of any length, not just 3
4. **Code Reuse**: Leverage existing Doublets infrastructure
5. **Migration Path**: Existing Doublets data can support Triplets view
6. **Memory Efficiency**: Shared subsequences reduce duplication

### Comparison

| Feature | Pure Triplets | Triplets on Sequences |
|---------|--------------|----------------------|
| Tuple Size | 3 (fixed) | 3 (enforced) |
| Storage | Native triples | Sequences of 3 |
| Semantic Clarity | High | High |
| Compression | None | High |
| Flexibility | Medium | High |

## Technical Details

### Sequence Representation

Sequences use a binary tree structure where:
- Each sequence is either a single element OR two subsequences
- The Source (left) contains the first part
- The Target (right) contains the second part
- Leaf nodes are individual elements

### Validation

All triplet operations validate that sequences contain exactly 3 elements.
Invalid triplets will throw `InvalidOperationException`.

### Indexing

The implementation supports efficient querying through:
- Pattern matching with wildcards (Any)
- Source/Linker/Target component access
- Full triplet iteration

## Related Projects

- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets) - Binary link storage
- [Platform.Data.Triplets](https://github.com/linksplatform/Data.Triplets) - Native triplet implementation
- [Platform.Data](https://github.com/linksplatform/Data) - Common interfaces

## License

This project is licensed under the Unlicense. See the [LICENSE](../../LICENSE) file for details.

## Contributing

Contributions are welcome! Please follow the LinksPlatform contributing guidelines.

## References

- Issue [#336](https://github.com/konard/LinksPlatform/issues/336) - Original implementation request
- [Links Theory](../../doc/articles/links-theory.md) - Theoretical foundation
- [Doublets Documentation](https://linksplatform.github.io/Data.Doublets)
