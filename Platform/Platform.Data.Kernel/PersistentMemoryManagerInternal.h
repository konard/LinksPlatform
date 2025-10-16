#ifndef __LINKS_PERSISTENT_MEMORY_MANAGER_INTERNAL_H__
#define __LINKS_PERSISTENT_MEMORY_MANAGER_INTERNAL_H__

// Internal implementation details for persistent memory management
// Внутренние детали реализации управления хранимой памятью

#include "PersistentMemoryManager.h"
#include "LinkInternal.h"

// Internal initialization and file operations
// These functions are used internally by the library implementation

void InitPersistentMemoryManager();

signed_integer OpenStorageFile(char* filename);
signed_integer CloseStorageFile();
signed_integer EnlargeStorageFile();
signed_integer ShrinkStorageFile();
signed_integer SetStorageFileMemoryMapping();
signed_integer ResetStorageFileMemoryMapping();

// Internal link access functions
Link* GetLink(link_index linkIndex);
link_index GetLinkIndex(Link* link);

#endif
