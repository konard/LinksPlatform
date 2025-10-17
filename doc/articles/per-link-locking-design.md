# Per-Link Locking Synchronization Design

## Overview

This document describes the design for per-link locking synchronization mechanism in LinksPlatform, as proposed in [Issue #435](https://github.com/konard/LinksPlatform/issues/435).

## Problem Statement

Current synchronization in LinksPlatform operates at a global or collection level. This approach has limitations when multiple threads need to modify different links simultaneously, as they must wait for each other even when operating on completely independent links.

## Proposed Solution

Implement a per-link locking mechanism that provides:

1. **Fine-grained locking**: Lock individual links rather than entire collections
2. **Lock marking**: Create a link that marks/indicates another link is locked
3. **Copy-on-write visibility**: Writers see an isolated copy of the link's space while readers see the locked state

## Design Components

### 1. Lock Marker Link

A special link type that represents a lock on another link:

```
[Locked Link] → [Lock Type] → [Lock Owner/Context]
```

Where:
- **Locked Link**: The link that is being locked
- **Lock Type**: A constant link indicating this is a lock marker (e.g., "IsLocked")
- **Lock Owner/Context**: Information about who holds the lock (thread ID, operation context, etc.)

### 2. Space Isolation for Writers

When a link is locked for modification:

1. **Lock Creation**: A lock marker link is created pointing to the target link
2. **Space Copy**: A private copy of the link's local space (immediate references) is created
3. **Writer View**: The writing thread operates on the copied space
4. **Reader View**: Other threads see the link as locked and cannot modify it
5. **Lock Release**: On successful completion, changes are committed atomically; on failure, changes are discarded

### 3. Visibility Model

```
┌─────────────────────────────────────────────────────┐
│  Global Links Space                                  │
│                                                      │
│  Link A ──┐                                          │
│           ↓                                          │
│  Link B (LOCKED) ←── [Lock Marker]                  │
│           ↑                                          │
│  Link C ──┘                                          │
│                                                      │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  Writer's Private Space (while lock is held)         │
│                                                      │
│  Link A' ──┐                                         │
│            ↓                                         │
│  Link B' (MODIFIED)                                  │
│            ↑                                         │
│  Link C' ──┘                                         │
│                                                      │
└─────────────────────────────────────────────────────┘
```

## Implementation Considerations

### Lock Types

The system should support different lock types:

- **Exclusive Write Lock**: Only one writer, no concurrent reads of the locked link
- **Shared Read Lock**: Multiple readers, no writers
- **Upgrade Lock**: Read lock that can be upgraded to write lock

### Deadlock Prevention

To prevent deadlocks:

1. **Lock Ordering**: If multiple links must be locked, use a consistent order (e.g., by link address)
2. **Timeout Mechanism**: Locks should have timeouts
3. **Deadlock Detection**: Monitor for circular lock dependencies

### Lock Marker Link Structure

```csharp
// Conceptual representation
Link lockMarker = Create(
    source: targetLink,        // Link being locked
    linker: LockTypeConstant,  // E.g., Constants.IsLocked
    target: lockContext        // Lock metadata (owner, timestamp, etc.)
);
```

### Space Copying Strategy

The "space around the link" includes:

- Links where the target is the Source
- Links where the target is the Linker
- Links where the target is the Target
- The target link itself

For efficiency, use **lazy copying**: Only copy what is accessed during the write operation.

## Benefits

1. **Improved Concurrency**: Multiple threads can modify different links simultaneously
2. **Isolation**: Writers work in isolation without affecting readers until commit
3. **Consistency**: Readers always see consistent state (either unlocked or locked)
4. **Flexibility**: Can implement different isolation levels

## Trade-offs

1. **Memory Overhead**: Space copying requires additional memory during write operations
2. **Complexity**: More complex than global locking
3. **Commit Cost**: Merging changes back requires validation and potential conflict resolution

## Relationship to Existing Synchronization

This design complements the existing `ISynchronizedLinks` infrastructure:

- **ISynchronizedLinks**: Provides collection-level synchronization (still useful for bulk operations)
- **Per-Link Locking**: Provides fine-grained synchronization for individual link operations

## Future Enhancements

1. **Lock Escalation**: Automatically upgrade from per-link to collection-level locks when many links are locked
2. **Optimistic Locking**: Allow reads without locks, detect conflicts on write
3. **MVCC (Multi-Version Concurrency Control)**: Maintain multiple versions of links for true lock-free reads

## References

- Issue #435: Idea for Links synchronization
- Issue #323: ISynchronizedLinks extensions
- PR #896: ISynchronizedLinksExtensions implementation
- doc/articles/links-theory.md: Links theory and structure
