# Proposal: Index Structure Type Marker for Platform.Data.Kernel

**Issue:** #15 - Allow to switch Platform.Data.Kernel.dll between linked-list and tree

**Status:** Awaiting clarification

**Author:** AI Issue Solver

**Date:** 2025-10-16

## Background

The Platform.Data.Kernel library (now maintained as [Data.Triplets.Kernel](https://github.com/linksplatform/Data.Triplets.Kernel)) is a native C library that provides persistent storage for link data structures. Currently, it uses Size-Balanced Trees for indexing connections between links.

Issue #15 requests the ability to distinguish between different index implementations (specifically linked-list and tree) to prevent database corruption when opening a database file with an incompatible implementation.

## Current Implementation

### File Format

The database file uses a service block header structure:

```
Service Block (2 * System Page Size):
┌─────────────────────────────────────┐
│ DataSeal            (64-bit)        │  0x810118808100180
│ LinkIndexSize       (64-bit)        │
│ MappingLinksMaxSize (64-bit)        │
│ LinksMaxSize        (link_index)    │
│ LinksActualSize     (link_index)    │
│ Base/Mapped Link indices            │
└─────────────────────────────────────┘
Links Block:
┌─────────────────────────────────────┐
│ Actual Link Structures              │
└─────────────────────────────────────┘
```

### Link Structure

Each link contains:
- Core fields: `SourceIndex`, `TargetIndex`, `LinkerIndex`, `Timestamp`
- Tree index fields for each reference type (Source, Target, Linker):
  - Root, Left, Right indices
  - Count (size of subtree)

Reference: [Link.h](https://github.com/linksplatform/Data.Triplets.Kernel/blob/master/Platform.Data.Triplets.Kernel/Link.h)

### Current Validation

The `SetStorageFileMemoryMapping()` function in `PersistentMemoryManager.c` performs validation:

```c
if (*pointerToDataSeal == LINKS_DATA_SEAL_64BIT) {
    // Existing database - validate compatibility
    if (*pointerToLinkIndexSize != sizeof(link_index)) {
        ERROR_MESSAGE("Opening storage file with different link index size is not supported yet.");
        return ResetStorageFileMapping() & CloseStorageFile();
    }
    // ... other checks
}
```

Reference: [PersistentMemoryManager.c:428](https://github.com/linksplatform/Data.Triplets.Kernel/blob/master/Platform.Data.Triplets.Kernel/PersistentMemoryManager.c#L428)

## Problem Statement

The current implementation:
1. Only supports one index structure type (Size-Balanced Tree)
2. Has no mechanism to identify which index structure a database file uses
3. Cannot prevent corruption if a database is opened with an incompatible implementation

## Proposed Solution

### Option 1: Extend Service Block Header (Recommended)

Add an `IndexStructureType` field to the service block header:

```
Service Block (2 * System Page Size):
┌─────────────────────────────────────┐
│ DataSeal            (64-bit)        │  0x810118808100180
│ IndexStructureType  (64-bit)        │  NEW FIELD
│ LinkIndexSize       (64-bit)        │
│ MappingLinksMaxSize (64-bit)        │
│ LinksMaxSize        (link_index)    │
│ LinksActualSize     (link_index)    │
│ Base/Mapped Link indices            │
└─────────────────────────────────────┘
```

#### Constants Definition

```c
// Index structure type identifiers
#define INDEX_STRUCTURE_UNKNOWN         0x00
#define INDEX_STRUCTURE_LINKED_LIST     0x01
#define INDEX_STRUCTURE_SIZE_BALANCED_TREE  0x02
#define INDEX_STRUCTURE_AVL_TREE        0x03
#define INDEX_STRUCTURE_B_TREE          0x04
#define INDEX_STRUCTURE_SKIP_LIST       0x05
#define INDEX_STRUCTURE_HASHTABLE       0x06
#define INDEX_STRUCTURE_MATRIX          0x07
// Reserve 0x08-0xFF for future implementations
```

#### Implementation Changes

**1. Update PersistentMemoryManager.h:**

```c
#define LINKS_DATA_SEAL_64BIT 0x810118808100180

// Define the index structure type used in this build
#ifndef INDEX_STRUCTURE_TYPE
#define INDEX_STRUCTURE_TYPE INDEX_STRUCTURE_SIZE_BALANCED_TREE
#endif
```

**2. Update PersistentMemoryManager.c:**

Add pointer to index structure type field:

```c
uint64_t* pointerToIndexStructureType;  // New field
```

Update `SetStorageFileMemoryMapping()`:

```c
void* pointers[8] = {  // Changed from 7 to 8
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 0, // DataSeal
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 1, // IndexStructureType (NEW)
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 2, // LinkIndexSize
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 3, // MappingLinksMaxSize
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 4 + sizeof(link_index) * 0, // LinksMaxSize
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 4 + sizeof(link_index) * 1, // LinksSize
    (char*)pointerToMappedRegion + sizeof(uint64_t) * 4 + sizeof(link_index) * 2, // MappingLinks
    (char*)pointerToMappedRegion + serviceBlockSizeInBytes // Links Block
};

pointerToDataSeal = (uint64_t*)pointers[0];
pointerToIndexStructureType = (uint64_t*)pointers[1];  // NEW
pointerToLinkIndexSize = (uint64_t*)pointers[2];
// ... etc
```

Add validation logic:

```c
if (*pointerToDataSeal == LINKS_DATA_SEAL_64BIT) {
    // Opening existing database
    DEBUG_MESSAGE("Storage file opened.");

    // Validate index structure type
    if (*pointerToIndexStructureType != INDEX_STRUCTURE_TYPE) {
        char errorMsg[256];
        sprintf(errorMsg,
            "Incompatible index structure. Database uses type 0x%02" PRIX64
            ", but this build expects type 0x%02" PRIX64 ".",
            *pointerToIndexStructureType,
            (uint64_t)INDEX_STRUCTURE_TYPE);
        ERROR_MESSAGE(errorMsg);
        return ResetStorageFileMapping() & CloseStorageFile();
    }

    // ... existing validations
} else {
    // Creating new database
    DEBUG_MESSAGE("Storage file created.");

    *pointerToIndexStructureType = INDEX_STRUCTURE_TYPE;  // NEW
    // ... existing initialization
}
```

**3. Update baseLinksSizeInBytes calculation:**

```c
void InitPersistentMemoryManager() {
    currentMemoryPageSizeInBytes = GetCurrentSystemPageSize();
    serviceBlockSizeInBytes = currentMemoryPageSizeInBytes * 2;

    // Updated: now 4 uint64_t fields instead of 3
    baseLinksSizeInBytes = serviceBlockSizeInBytes
                          - sizeof(uint64_t) * 4  // Changed from 3
                          - sizeof(link_index) * 2;
    // ... rest unchanged
}
```

### Option 2: Incorporate into DataSeal (Alternative)

Encode the index structure type into the DataSeal itself:

```c
// Base seal value
#define LINKS_DATA_SEAL_BASE        0x810118808100000

// Structure-specific seals
#define LINKS_DATA_SEAL_LINKED_LIST 0x810118808100001
#define LINKS_DATA_SEAL_TREE        0x810118808100002
#define LINKS_DATA_SEAL_AVL_TREE    0x810118808100003
// etc.

// Current implementation uses tree
#define LINKS_DATA_SEAL_64BIT LINKS_DATA_SEAL_TREE
```

**Pros:** No header format change required
**Cons:** Less flexible, harder to decode, limits to 255 structure types

### Option 3: Version-based Approach

Introduce a format version number:

```c
#define LINKS_FORMAT_VERSION_1  0x01  // Original tree-only
#define LINKS_FORMAT_VERSION_2  0x02  // With IndexStructureType field
```

This allows for graceful migration and backwards compatibility detection.

## Implementation Considerations

### 1. Linked-List Implementation Requirements

If a linked-list implementation is to be created, the Link structure would need to be modified:

```c
#ifdef INDEX_STRUCTURE_TYPE == INDEX_STRUCTURE_LINKED_LIST
typedef struct Link {
    link_index SourceIndex;
    link_index TargetIndex;
    link_index LinkerIndex;
    signed_integer Timestamp;

    // Linked-list fields (much simpler than tree)
    link_index BySourceFirstIndex;    // First link in source list
    link_index BySourceNextIndex;     // Next link in source list
    link_index ByTargetFirstIndex;    // First link in target list
    link_index ByTargetNextIndex;     // Next link in target list
    link_index ByLinkerFirstIndex;    // First link in linker list
    link_index ByLinkerNextIndex;     // Next link in linker list
} Link;
#else
// Existing tree-based Link structure
// ...
#endif
```

**Note:** Linked-list uses significantly less memory per link (6 index fields vs 12 in tree), but has O(n) search complexity vs O(log n) for trees.

### 2. Compile-Time vs Runtime Selection

**Compile-time (Recommended):**
- Different builds for different structures
- `-DINDEX_STRUCTURE_TYPE=INDEX_STRUCTURE_LINKED_LIST`
- Simpler, no runtime overhead
- Current approach for x86/x64 builds

**Runtime:**
- Single binary supports multiple structures
- More complex implementation
- Useful for tools that need to open any database type

### 3. Migration Path

For existing databases without the IndexStructureType field:

```c
if (*pointerToDataSeal == OLD_LINKS_DATA_SEAL_64BIT) {
    // Assume tree structure (only implementation that existed)
    *pointerToIndexStructureType = INDEX_STRUCTURE_SIZE_BALANCED_TREE;
    // Update seal to new format
    *pointerToDataSeal = NEW_LINKS_DATA_SEAL_64BIT;
}
```

### 4. Testing Strategy

1. Create database with tree implementation
2. Verify it can be opened with tree build
3. Verify it CANNOT be opened with linked-list build (proper error)
4. Create database with linked-list implementation
5. Verify it can be opened with linked-list build
6. Verify it CANNOT be opened with tree build (proper error)

## Advantages

1. **Prevents Data Corruption:** Cannot accidentally open database with wrong implementation
2. **Future-Proof:** Supports any number of index structure types
3. **Clear Error Messages:** User knows exactly what went wrong
4. **Backward Compatible:** Can detect and migrate old format databases
5. **Enables Experimentation:** Easy to try different index structures (as explored in issue #5)

## Disadvantages

1. **File Format Change:** Requires migration for existing databases
2. **Increased Complexity:** More validation code
3. **Service Block Size:** Must ensure enough space (currently 2 pages should be sufficient)

## Related Issues

- Issue #5: Test alternative forms of index (lists of connections between links)
- Issue #432: Move Platform.Data.Kernel to separate repository
- Issue #136: Split Platform/Platform.Data.Kernel/Link.h into Public and Internal parts

## Next Steps

1. **Await clarification** on whether linked-list implementation exists
2. **Get approval** on proposed approach (Option 1 recommended)
3. **Implement** in Data.Triplets.Kernel repository
4. **Create** reference linked-list implementation (if needed)
5. **Add** comprehensive tests
6. **Document** in Data.Triplets.Kernel README

## References

- [Data.Triplets.Kernel Repository](https://github.com/linksplatform/Data.Triplets.Kernel)
- [Link.h](https://github.com/linksplatform/Data.Triplets.Kernel/blob/master/Platform.Data.Triplets.Kernel/Link.h)
- [PersistentMemoryManager.h](https://github.com/linksplatform/Data.Triplets.Kernel/blob/master/Platform.Data.Triplets.Kernel/PersistentMemoryManager.h)
- [PersistentMemoryManager.c](https://github.com/linksplatform/Data.Triplets.Kernel/blob/master/Platform.Data.Triplets.Kernel/PersistentMemoryManager.c)
- [SizeBalancedTree.h](https://github.com/linksplatform/Data.Triplets.Kernel/blob/master/Platform.Data.Triplets.Kernel/SizeBalancedTree.h)
