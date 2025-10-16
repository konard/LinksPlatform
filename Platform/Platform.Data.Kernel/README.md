# Platform.Data.Kernel Header Split

This directory contains the split header structure for Platform.Data.Kernel as requested in issue #136.

## Structure

The headers have been split into **Public** and **Internal** parts following best practices for C library design:

### Link Headers

- **Link.h** - Public API
  - Contains only the public API functions that library users should call
  - Includes callback type definitions (`visitor`, `stoppable_visitor`)
  - Exports functions marked with `PREFIX_DLL` for library interface
  - Includes CRUD operations, traversal functions, and property getters

- **LinkInternal.h** - Internal Implementation Details
  - Contains the `Link` structure definition with all internal fields
  - Includes internal helper functions for link management
  - Should only be included by library implementation files (.c files)
  - Not exposed to library users

### PersistentMemoryManager Headers

- **PersistentMemoryManager.h** - Public API
  - Contains public functions for storage operations (OpenLinks, CloseLinks)
  - Includes link traversal and mapping functions
  - Exports memory allocation functions (AllocateLink, FreeLink) for testing

- **PersistentMemoryManagerInternal.h** - Internal Implementation Details
  - Contains internal storage file operations
  - Includes low-level functions for file mapping and resizing
  - Contains internal link access functions (GetLink, GetLinkIndex)
  - Should only be included by implementation files

## Benefits of This Split

1. **Clean API Surface**: Library users only see the public API, making it easier to understand what functions they should use
2. **Encapsulation**: Internal implementation details are hidden from users, allowing for changes without breaking the API
3. **Better Documentation**: The public headers serve as clear documentation of the library interface
4. **Reduced Compilation Dependencies**: Users don't need to recompile when internal structures change
5. **Binary Compatibility**: Internal structure changes don't break binary compatibility as long as the public API remains stable

## Usage

### For Library Users (Application Code)
```c
#include "Link.h"
#include "PersistentMemoryManager.h"

// Use public API functions
int main() {
    OpenLinks("db.links");
    link_index link = CreateLink(itself, itself, itself);
    DeleteLink(link);
    CloseLinks();
    return 0;
}
```

### For Library Implementation (Library .c Files)
```c
#include "LinkInternal.h"
#include "PersistentMemoryManagerInternal.h"

// Can access internal structures and functions
Link* GetLink(link_index linkIndex) {
    // Implementation has access to Link structure
    // ...
}
```

## Test File

The included `test.c` demonstrates that the public API remains unchanged and existing code continues to work with the split headers.

## Notes

This is a reference implementation showing how the headers from the Data.Triplets.Kernel repository (originally Platform.Data.Kernel) should be split. The actual implementation is now maintained at https://github.com/linksplatform/Data.Triplets.Kernel.
