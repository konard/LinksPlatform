# Minimal Links Platform for BareMetal OS

This is a minimal, portable implementation of the Links Platform core concepts designed to run on [BareMetal OS](https://github.com/ReturnInfinity/BareMetal-OS) and other bare-metal environments.

## Key Features

- **No Memory-Mapped Files**: Uses pure in-memory arrays
- **Minimal Dependencies**: Only requires basic C standard library functions (can be further reduced for bare-metal)
- **Portable**: Written in standard C99
- **Simple API**: Core operations for creating, reading, updating, and deleting links

## What is Links Platform?

Links Platform is an associative data storage system where data is represented as connections (links) between elements. Each link has:
- A unique identifier
- A source (another link or null)
- A target (another link or null)

This allows building complex data structures and relationships using a simple, uniform representation.

## Implementation Details

### Memory Model

Unlike the full Links Platform implementation which uses memory-mapped files for persistence, this implementation:
- Uses a simple array-based in-memory store
- Allocates a fixed capacity at initialization
- Uses 64-bit link identifiers for consistency with other implementations
- Tracks deleted links for potential reuse

### API

The implementation provides these core operations:

```c
// Initialize store with capacity
int links_init(links_t *links, link_t capacity);

// Create a new link
link_t links_create(links_t *links, link_t source, link_t target);

// Get link properties
link_t links_get_source(links_t *links, link_t link);
link_t links_get_target(links_t *links, link_t link);

// Update a link
int links_update(links_t *links, link_t link, link_t new_source, link_t new_target);

// Delete a link
int links_delete(links_t *links, link_t link);

// Search for links matching patterns
link_t links_search(links_t *links, link_t source, link_t target,
                    link_t *results, link_t max_results);

// Clean up
void links_free(links_t *links);
```

## Building and Running

### Standard C Environment

```bash
make
make run
```

### For BareMetal OS

To compile for BareMetal OS:

1. Install the BareMetal OS development tools
2. Modify the Makefile to use BareMetal's compiler
3. Remove or replace standard library dependencies (malloc, printf) with BareMetal equivalents
4. Build and deploy according to BareMetal OS procedures

Example modifications needed:
- Replace `malloc/free` with BareMetal's memory allocation
- Replace `printf` with BareMetal's output functions
- Use BareMetal's compilation tools instead of gcc

## Adapting for Pure Bare-Metal

For a truly bare-metal environment without any OS:

1. **Memory Allocation**: Replace `malloc/calloc/free` with a static array or custom allocator
2. **Output**: Replace `printf` with direct hardware access (serial port, screen buffer)
3. **Compilation**: Use appropriate bare-metal compiler flags and linker scripts

Example static allocation:

```c
#define MAX_LINKS 1000
static link_data_t static_memory[MAX_LINKS];

int links_init(links_t *links, link_t capacity) {
    links->data = static_memory;
    links->capacity = MAX_LINKS;
    // ... rest of initialization
}
```

## Comparison with Full Platform.Data.Doublets

This minimal implementation differs from the full Platform.Data.Doublets library:

| Feature | Minimal Implementation | Full Platform.Data.Doublets |
|---------|----------------------|---------------------------|
| Memory Model | Pure in-memory arrays | Memory-mapped files |
| Persistence | None (transient) | Automatic file persistence |
| Performance | Good for small datasets | Optimized for large datasets |
| Dependencies | Minimal (C stdlib) | .NET, Platform.Memory, etc. |
| Target | Bare-metal, embedded | Server/desktop applications |

## Use Cases

This minimal implementation is suitable for:

- **Embedded systems** with limited resources
- **Bare-metal applications** on minimal operating systems
- **Educational purposes** to understand Links Platform concepts
- **Prototyping** before integrating the full platform
- **Edge computing** where persistence is handled separately

## License

This implementation is part of the LinksPlatform project and follows the same license terms.

## References

- [LinksPlatform Organization](https://github.com/linksplatform)
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets)
- [BareMetal OS](https://github.com/ReturnInfinity/BareMetal-OS)
