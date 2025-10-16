# Decision: What Does 0 Mean as Amount of Links in Database

## Issue Reference
This document addresses [Issue #26](https://github.com/konard/LinksPlatform/issues/26): "Decide what means 0 (as an amount links in database)"

## Background

The Links Platform has two main implementations:
1. **Doublets (Double Links)**: Links with Source and Target
2. **Triplets (Triple Links)**: Links with Source, Linker, and Target

Both implementations include a special "null" link that serves as a fundamental element of the system.

## The Problem

There were two conflicting interpretations of database size:

### Original Interpretations:
1. **Triplets (Triple Links)**:
   - 0 is **invalid** database size
   - Minimum is 1 (because database always contains special "null" link)
   - The null link was **counted** in the total

2. **Pairs (Double Links)**:
   - 0 means there are **no "not null" links**
   - It contains special "null" link, but it is **NOT counted** as a link

## Decision

**The null link should NOT be counted in the database size.**

### Rationale:
1. **Consistency**: Both Doublets and Triplets implementations should follow the same counting convention
2. **User Perspective**: From a user's perspective, an "empty" database should show 0 links, not 1
3. **Semantics**: The null link is an internal implementation detail, not user data
4. **Maintainer Consensus**: As stated by konard (2015-11-15): "It looks like there is no need to count 'null' link. So the Triple Links implementation should be updated."

## Implementation Guidelines

### For Triplets (Triple Links):
- Database size 0 = no user-created links (only the internal null link exists)
- Database size N = N user-created links (null link excluded from count)
- The `Count` or `Size` property should return the number of links **excluding** the null link

### For Doublets (Double Links):
- Current behavior is correct: null link is not counted
- Database size 0 = no user-created links
- No changes needed

## Configuration Option

As suggested by konard (2016-01-05): "It also can be implemented as one of LinksOptions."

This could be implemented as a configurable option:
```csharp
public enum NullLinkCountingMode
{
    /// <summary>
    /// Null link is NOT counted in database size (default, recommended)
    /// </summary>
    ExcludeNullLink = 0,

    /// <summary>
    /// Null link IS counted in database size (legacy behavior)
    /// </summary>
    IncludeNullLink = 1
}
```

However, for consistency and simplicity, the **default and recommended behavior is to exclude the null link from the count**.

## Impact

### Breaking Change:
- For existing Triplets implementations that count the null link, this is a breaking change
- Database size will decrease by 1
- Migration note: If upgrading from a version that counted the null link, expect `Count - 1` after upgrade

### Benefits:
- Consistent behavior across all Links Platform implementations
- Intuitive user experience (empty database = 0 links)
- Clearer separation between internal implementation and user data

## References
- Issue #26: https://github.com/konard/LinksPlatform/issues/26
- Data.Doublets: https://github.com/linksplatform/Data.Doublets
- Data.Triplets: https://github.com/linksplatform/Data.Triplets

## Status
✅ **Decided**: Null link should NOT be counted in database size

## Next Steps
1. Update Data.Triplets implementation to exclude null link from count
2. Add tests to verify null link is not counted
3. Update documentation to reflect this decision
4. Consider adding `LinksOptions` configuration if backward compatibility is needed
