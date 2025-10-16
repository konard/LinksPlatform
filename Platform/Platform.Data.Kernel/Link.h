#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

// Public API for high-level link operations
// Публичный API для высокоуровневой работы со связями

#include "Common.h"

#define null    0LL
#define itself  0LL

// Callback types
typedef signed_integer(*stoppable_visitor)(link_index); // Stoppable visitor callback (Останавливаемый обработчик для прохода по связям)
typedef void(*visitor)(link_index); // Visitor callback (Неостанавливаемый обработчик для прохода по связям)

#if defined(__cplusplus)
extern "C" {
#endif

    // Link property getters
    PREFIX_DLL link_index GetSourceIndex(link_index linkIndex);
    PREFIX_DLL link_index GetLinkerIndex(link_index linkIndex);
    PREFIX_DLL link_index GetTargetIndex(link_index linkIndex);
    PREFIX_DLL signed_integer GetTime(link_index linkIndex);

    // Link CRUD operations
    PREFIX_DLL link_index CreateLink(link_index sourceIndex, link_index linkerIndex, link_index targetIndex);
    PREFIX_DLL link_index SearchLink(link_index sourceIndex, link_index linkerIndex, link_index targetIndex);
    PREFIX_DLL link_index ReplaceLink(link_index linkIndex, link_index replacementIndex);
    PREFIX_DLL link_index UpdateLink(link_index linkIndex, link_index sourceIndex, link_index linkerIndex, link_index targetIndex);
    PREFIX_DLL void DeleteLink(link_index linkIndex);

    // Referers access
    PREFIX_DLL link_index GetFirstRefererBySourceIndex(link_index linkIndex);
    PREFIX_DLL link_index GetFirstRefererByLinkerIndex(link_index linkIndex);
    PREFIX_DLL link_index GetFirstRefererByTargetIndex(link_index linkIndex);

    // Referers count
    PREFIX_DLL unsigned_integer GetLinkNumberOfReferersBySource(link_index linkIndex);
    PREFIX_DLL unsigned_integer GetLinkNumberOfReferersByLinker(link_index linkIndex);
    PREFIX_DLL unsigned_integer GetLinkNumberOfReferersByTarget(link_index linkIndex);

    // Referers traversal
    PREFIX_DLL void WalkThroughAllReferersBySource(link_index rootIndex, visitor);
    PREFIX_DLL signed_integer WalkThroughReferersBySource(link_index rootIndex, stoppable_visitor stoppableVisitor);

    PREFIX_DLL void WalkThroughAllReferersByLinker(link_index rootIndex, visitor);
    PREFIX_DLL signed_integer WalkThroughReferersByLinker(link_index rootIndex, stoppable_visitor stoppableVisitor);

    PREFIX_DLL void WalkThroughAllReferersByTarget(link_index rootIndex, visitor);
    PREFIX_DLL signed_integer WalkThroughReferersByTarget(link_index rootIndex, stoppable_visitor stoppableVisitor);

#if defined(__cplusplus)
}
#endif

#endif
