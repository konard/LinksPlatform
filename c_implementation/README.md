# Pair Link Implementation in C

This directory contains a C implementation of the pair (double) links concept from the LinksPlatform project.

## Overview

A **pair link** is a fundamental data structure that represents a directed connection between two links. Each pair link has:
- **Source**: The origin link
- **Target**: The destination link

The key feature of this implementation is that links can reference other links, creating arbitrary graph structures. This allows building complex data representations using only these simple pair relationships.

## Structure

The implementation is based on the original C# implementation found in `28.03.2010-04.11.2010/Net/Net/Link.cs`, but simplified to focus on the core pair link concept (as discussed in issue #63).

### Main Components

- `pair_link.h` - Header file with public API
- `pair_link.c` - Implementation
- `examples/basic_usage.c` - Example demonstrating the API

### Key Features

1. **Automatic duplicate prevention**: Creating a link with the same source and target returns the existing link
2. **Reference tracking**: Each link maintains lists of all links that reference it (as source or target)
3. **Cascading deletion**: Deleting a link also deletes all links that reference it
4. **Self-referencing links**: Links can point to themselves, useful for creating "points" or "atoms"

## Building

```bash
make
```

This builds the example program. To run it:

```bash
make run
```

To clean build artifacts:

```bash
make clean
```

## API Reference

### Creating Links

```c
/* Create a regular pair link */
pair_link* pair_link_create(pair_link *source, pair_link *target);

/* Create a point (self-referencing on both source and target) */
pair_link* pair_link_create_point(void);

/* Create outgoing self-link (source = self, target = specified) */
pair_link* pair_link_create_outgoing_selflink(pair_link *target);

/* Create incoming self-link (source = specified, target = self) */
pair_link* pair_link_create_incoming_selflink(pair_link *source);
```

### Querying Links

```c
/* Find an existing link with given source and target */
pair_link* pair_link_find(pair_link *source, pair_link *target);

/* Check if a link has been deleted */
bool pair_link_is_deleted(pair_link *link);
```

### Modifying Links

```c
/* Change the source or target of a link */
void pair_link_set_source(pair_link *link, pair_link *new_source);
void pair_link_set_target(pair_link *link, pair_link *new_target);
```

### Deleting Links

```c
/* Delete a link and all links that reference it (cascading) */
void pair_link_delete(pair_link *link);
```

### Traversing References

```c
/* Iterate over all links that reference this link as source */
void pair_link_foreach_referer_by_source(pair_link *link,
                                          pair_link_referer_callback callback,
                                          void *user_data);

/* Iterate over all links that reference this link as target */
void pair_link_foreach_referer_by_target(pair_link *link,
                                          pair_link_referer_callback callback,
                                          void *user_data);
```

## Example Usage

```c
#include "pair_link.h"

/* Create two points */
pair_link *point1 = pair_link_create_point();
pair_link *point2 = pair_link_create_point();

/* Create a link from point1 to point2 */
pair_link *link = pair_link_create(point1, point2);

/* Attempting to create the same link returns the existing one */
pair_link *duplicate = pair_link_create(point1, point2);
assert(link == duplicate);

/* Links can reference other links */
pair_link *meta_link = pair_link_create(link, point1);

/* Clean up */
pair_link_delete(meta_link);
pair_link_delete(point1);
pair_link_delete(point2);
```

## Differences from C# Implementation

The original C# implementation (Link.cs) includes three references per link:
- Source
- Linker (type/verb)
- Target

This C implementation simplifies to just Source and Target (pair links), as discussed in issue #63. This corresponds to the "PAIR = ID + Source + Target" structure mentioned in the issue comments.

The triplet structure (TRIPLET = ID + Source + Linker + Target) can be implemented as a future enhancement if needed.

## Theory

For more information about the theory behind link structures, see:
- `doc/articles/links-theory.md` - Theory of Links (in Russian)
- Issue #63 discussion on GitHub

## License

See the LICENSE file in the repository root.
