# Matrix Indices for Links - Technical Documentation

## Overview

This document explains the two-matrix index structure for links as proposed in issue #526.

## Problem Statement

Traditional link implementations (as seen in the old `Link.cs` file) use linked-list structures for indexing:
- `m_FirstRefererBySource` and `m_NextSiblingRefererBySource`
- `m_FirstRefererByTarget` and `m_NextSiblingRefererByTarget`

This approach requires **O(n) time complexity** for traversing to find all links with a specific source or target.

## Proposed Solution: Two-Matrix Index Structure

### Matrix 1: Address → Source
Maps each link's address to its source link address.

```
| Link Address | Source Address |
|--------------|----------------|
| 1            | 1              |
| 2            | 2              |
| 3            | 1              |
| 4            | 2              |
| 5            | 3              |
```

### Matrix 2: Address → Target
Maps each link's address to its target link address.

```
| Link Address | Target Address |
|--------------|----------------|
| 1            | 1              |
| 2            | 2              |
| 3            | 2              |
| 4            | 1              |
| 5            | 4              |
```

## Key Advantages

### 1. O(1) Direct Lookup
Given a link address, finding its source or target is a constant-time operation:
```csharp
ulong source = addressToSource[linkAddress];
ulong target = addressToTarget[linkAddress];
```

### 2. Memory Efficiency
- Uses two hash tables (dictionaries) instead of linked-list pointers in every link object
- For N links: 2N entries total across both matrices
- No overhead for sibling/traversal pointers

### 3. Simplicity
- Clear separation of concerns
- Easier to understand and maintain
- No complex pointer manipulation

### 4. Flexibility
- Easy to add additional indices if needed (e.g., reverse indices)
- Can be extended to support matrix operations

## Trade-offs

### Advantages vs Linked-List Approach:
- ✅ **O(1) lookup** for source/target by address (vs O(n) traversal)
- ✅ **Simpler code** (no complex pointer manipulation)
- ✅ **Better for random access** patterns

### Disadvantages vs Linked-List Approach:
- ❌ **O(n) reverse lookup** (finding all links with specific source/target)
  - Can be mitigated with additional reverse indices if needed
- ❌ **No inherent ordering** of links by source/target
  - Linked lists maintain insertion order naturally

## Implementation Details

### Data Structures

```csharp
// Using Dictionary for sparse matrix representation
private Dictionary<ulong, ulong> AddressToSource;
private Dictionary<ulong, ulong> AddressToTarget;
```

**Why Dictionary (Hash Table)?**
- Links are created and deleted dynamically
- Addresses may not be contiguous
- Hash table provides O(1) average-case performance
- Memory efficient for sparse data

**Alternative: Dense Array**
If link addresses are guaranteed to be contiguous (1, 2, 3, ...), could use:
```csharp
private ulong[] AddressToSource;
private ulong[] AddressToTarget;
```

### Core Operations

#### Create Link
```csharp
public ulong CreateLink(ulong source, ulong target)
{
    ulong address = NextAddress++;
    AddressToSource[address] = source;
    AddressToTarget[address] = target;
    return address;
}
```

#### Get Source (O(1))
```csharp
public ulong GetSource(ulong address)
{
    return AddressToSource.TryGetValue(address, out ulong source) ? source : 0;
}
```

#### Get Target (O(1))
```csharp
public ulong GetTarget(ulong address)
{
    return AddressToTarget.TryGetValue(address, out ulong target) ? target : 0;
}
```

#### Update Operations (O(1))
```csharp
public bool UpdateSource(ulong address, ulong newSource)
{
    if (AddressToSource.ContainsKey(address))
    {
        AddressToSource[address] = newSource;
        return true;
    }
    return false;
}
```

#### Delete Link (O(1))
```csharp
public bool DeleteLink(ulong address)
{
    bool sourceRemoved = AddressToSource.Remove(address);
    bool targetRemoved = AddressToTarget.Remove(address);
    return sourceRemoved && targetRemoved;
}
```

#### Reverse Lookup (O(n))
```csharp
public List<ulong> FindBySource(ulong source)
{
    var result = new List<ulong>();
    foreach (var kvp in AddressToSource)
    {
        if (kvp.Value == source)
            result.Add(kvp.Key);
    }
    return result;
}
```

## Optional: Adding Reverse Indices

For applications that frequently need to find all links with a specific source or target, we can add reverse indices:

```csharp
// Reverse indices (optional, for O(1) reverse lookups)
private Dictionary<ulong, HashSet<ulong>> SourceToAddresses;
private Dictionary<ulong, HashSet<ulong>> TargetToAddresses;
```

This creates a **four-matrix system**:
1. Address → Source (original)
2. Address → Target (original)
3. Source → Addresses (reverse)
4. Target → Addresses (reverse)

Trade-off: 2x memory usage, but O(1) reverse lookups.

## Comparison with Original Implementation

### Original (Linked-List Based)
```csharp
// In Link.cs:
private Link m_Source;
private Link m_Target;
private Link m_FirstRefererBySource;
private Link m_NextSiblingRefererBySource;
private Link m_FirstRefererByTarget;
private Link m_NextSiblingRefererByTarget;
```

Each link object stores 6 references. For N links = 6N references total.

Traversing all referrers by source:
```csharp
Link referer = m_FirstRefererBySource;
while (referer != null)
{
    yield return referer;
    referer = referer.m_NextSiblingRefererBySource;
}
```

### Matrix-Based Approach
```csharp
// Two shared dictionaries (not per-link overhead)
private Dictionary<ulong, ulong> AddressToSource;
private Dictionary<ulong, ulong> AddressToTarget;
```

For N links = 2N dictionary entries total.

Getting source/target:
```csharp
ulong source = AddressToSource[address]; // O(1)
ulong target = AddressToTarget[address]; // O(1)
```

## Use Cases

### Best suited for:
- **Random access patterns**: Frequently looking up source/target by address
- **Large link networks**: Where linked-list traversal becomes expensive
- **Read-heavy workloads**: More reads than writes
- **Clean data model**: When you want separation between link identity and relationships

### Less suitable for:
- **Sequential traversal**: When you primarily iterate through all referrers
- **Ordered access**: When insertion order of referrers matters
- **Memory-constrained**: When you can't afford hash table overhead

## Example Usage

```csharp
var indices = new MatrixIndicesDemo();

// Create self-referential point
ulong point = indices.CreateLink(0, 0);
indices.UpdateSource(point, point);
indices.UpdateTarget(point, point);

// Create link between points
ulong link1 = indices.CreateLink(point, point);

// O(1) lookup
ulong source = indices.GetSource(link1); // Returns: point
ulong target = indices.GetTarget(link1); // Returns: point

// Find all links with specific source
var linksFromPoint = indices.FindBySource(point);
```

## Conclusion

The two-matrix index structure provides a clean, efficient alternative to linked-list based indexing for associative memory systems. It excels at direct lookups by address and simplifies the codebase at the cost of slower reverse lookups (which can be mitigated with additional indices if needed).

This approach aligns with the principles of associative data models while providing practical performance benefits for common access patterns.

## References

- Issue #526: https://github.com/konard/LinksPlatform/issues/526
- Original Link.cs implementation: `/28.03.2010-04.11.2010/Net/Net/Link.cs`
- Demonstration code: `/experiments/MatrixIndicesDemo.cs`
