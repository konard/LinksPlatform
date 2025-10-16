#ifndef __LINKS_PERSISTENT_MEMORY_MANAGER_H__
#define __LINKS_PERSISTENT_MEMORY_MANAGER_H__

// Public API for persistent memory management
// Публичный API для управления хранимой памятью

#include "Common.h"
#include "Link.h"

#define LINKS_DATA_SEAL_64BIT 0x810118808100180
// Binary:
// 0000100000010000
// 0001000110001000
// 0000100000010000
// 0000000110000000‬

#if defined(__cplusplus)
extern "C" {
#endif

    // Storage operations
    PREFIX_DLL signed_integer OpenLinks(char* filename);
    PREFIX_DLL signed_integer CloseLinks();

    // Link mapping operations
    PREFIX_DLL link_index GetMappedLink(signed_integer mappedIndex);
    PREFIX_DLL void SetMappedLink(signed_integer mappedIndex, link_index linkIndex);

    // Link traversal
    PREFIX_DLL void WalkThroughAllLinks(visitor visitor);
    PREFIX_DLL signed_integer WalkThroughLinks(stoppable_visitor stoppableVisitor);

    // Storage information
    PREFIX_DLL unsigned_integer GetLinksCount();

    // Link allocation (exported for tests, unsafe to use directly - use Create/Update/Delete instead)
    PREFIX_DLL link_index AllocateLink();
    PREFIX_DLL void FreeLink(link_index link);

#if defined(__cplusplus)
}
#endif

#endif
