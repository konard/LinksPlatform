# Platform.Data.Doublets.DeepDependencies

Deep dependencies tracking for LinksPlatform's associative data storage.

## Overview

This library provides functionality to track deep dependencies in link-based associative data stores. It maintains two types of dependency sets for each link:

1. **Used By Link**: All links that are used (referenced) by a given link, recursively including transitive dependencies
2. **Referencing Link**: All links that reference a given link, recursively including all transitive referrers

These dependency sets can be represented using either `HashSet<TLink>` or `BitArray` (BitString) for memory-efficient storage.

## Motivation

Deep dependencies tracking is useful for:

- **Partial sequence search**: Quickly finding all elements that contribute to a sequence
- **Impact analysis**: Understanding what depends on a particular link before making changes
- **Graph traversal optimization**: Caching dependency information for faster repeated queries
- **Dependency visualization**: Building complete dependency graphs for analysis
- **Garbage collection**: Identifying unreferenced links that can be safely removed

## Features

- **Two implementations**:
  - `DeepDependenciesIndex<TLink>`: Uses `HashSet` for sparse link address spaces
  - `DeepDependenciesBitStringIndex<TLink>`: Uses `BitArray` for dense link address spaces
- **Recursive dependency tracking**: Follows dependencies through multiple levels
- **Cycle detection**: Handles circular references without infinite loops
- **Caching**: Computed dependencies are cached for performance
- **Cache invalidation**: Manual cache control for updated links

## Usage

### HashSet-based Implementation

```csharp
using Platform.Data.Doublets.DeepDependencies;

// Assuming you have an ILinks<ulong> instance
var links = new UnitedMemoryLinks<ulong>("db.links");

// Create the deep dependencies index
var index = new DeepDependenciesIndex<ulong>(links);

// Get all links used by a specific link
var usedLinks = index.GetUsedByLink(myLink);
foreach (var link in usedLinks)
{
    Console.WriteLine($"Link {myLink} uses link {link}");
}

// Get all links that reference a specific link
var referencingLinks = index.GetReferencingLink(myLink);
foreach (var link in referencingLinks)
{
    Console.WriteLine($"Link {link} references link {myLink}");
}

// Invalidate cache after updating a link
index.InvalidateCache(myLink);
```

### BitArray-based Implementation

```csharp
using Platform.Data.Doublets.DeepDependencies;

var links = new UnitedMemoryLinks<ulong>("db.links");

// Create the bit string index with maximum link count
var bitIndex = new DeepDependenciesBitStringIndex<ulong>(links, maxLinkIndex: 65536);

// Get BitArray of used links
var usedBitArray = bitIndex.GetUsedByLink(myLink);

// Convert to HashSet if needed
var usedLinks = bitIndex.BitArrayToSet(usedBitArray);

// Check if a specific link is used
int linkIndex = (int)someLink;
if (linkIndex < usedBitArray.Length && usedBitArray[linkIndex])
{
    Console.WriteLine($"Link {someLink} is used by {myLink}");
}
```

## Performance Considerations

### HashSet Implementation
- **Pros**: Works with any link address space, no pre-allocation needed
- **Cons**: Higher memory overhead per link, slower lookup for dense graphs
- **Best for**: Sparse link address spaces, unknown maximum link count

### BitArray Implementation
- **Pros**: Very memory-efficient for dense address spaces, O(1) lookup
- **Cons**: Requires knowing maximum link index, wastes space if sparse
- **Best for**: Dense link address spaces, known maximum link count

## Algorithm

The deep dependencies are computed using depth-first search with cycle detection:

1. Start with the target link
2. Add it to the result set
3. For "used by": recursively process Source and Target of the link
4. For "referencing": find all links with this link as Source or Target, recursively process them
5. Track visited links to avoid infinite loops in cyclic graphs

## Example

Given the following link structure:
```
Link 1 (point) -> self-referencing
Link 2 -> (Link 1, Link 1)
Link 3 -> (Link 2, Link 1)
Link 4 -> (Link 3, Link 2)
```

Deep dependencies for Link 4:
- **Used by Link 4**: {Link 4, Link 3, Link 2, Link 1}
- **Referencing Link 1**: {Link 1, Link 2, Link 3, Link 4}

## See Also

- [Issue #327](https://github.com/konard/LinksPlatform/issues/327) - Original feature request
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets) - Core doublets library
- [LinksPlatform](https://github.com/konard/LinksPlatform) - Main repository

## License

This library is distributed under the same license as the LinksPlatform project.
