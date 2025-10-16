#ifndef __LINKS_LINK_INTERNAL_H__
#define __LINKS_LINK_INTERNAL_H__

// Internal implementation details for Link structure and operations
// Внутренние детали реализации структуры Link и операций над ней

#include "Link.h"

// Link structure - contains internal implementation details
// This structure should not be exposed to library users
typedef struct Link
{
    link_index          SourceIndex;            // Ссылка на начальную связь
    link_index          TargetIndex;            // Ссылка на конечную связь
    link_index          LinkerIndex;            // Ссылка на связь-связку (если разместить это поле после Source и Target, то вероятно это поможет проще конвертировать тройки в пары)
    signed_integer      Timestamp;
    /* Referers (Index, Backlinks) */
    link_index          BySourceRootIndex;      // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве начальной связи
    link_index          BySourceLeftIndex;      // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве начальной связи
    link_index          BySourceRightIndex;     // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве начальной связи
    unsigned_integer    BySourceCount;          // Количество связей ссылающихся на эту связь в качестве начальной связи (элементов в дереве)
    link_index          ByTargetRootIndex;      // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве конечной связи
    link_index          ByTargetLeftIndex;      // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве конечной связи
    link_index          ByTargetRightIndex;     // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве конечной связи
    unsigned_integer    ByTargetCount;          // Количество связей ссылающихся на эту связь в качестве конечной связи (элементов в дереве)
    link_index          ByLinkerRootIndex;      // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве связи связки
    link_index          ByLinkerLeftIndex;      // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве связи связки
    link_index          ByLinkerRightIndex;     // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве связи связки
    unsigned_integer    ByLinkerCount;          // Количество связей ссылающихся на эту связь в качестве связи связки (элементов в дереве)
} Link;

// Internal functions for link management
// These functions are used internally by the library implementation

// "Unused marker" help mark links that was deleted, but still can be reused
void AttachLinkToUnusedMarker(link_index linkIndex);
void DetachLinkFromUnusedMarker(link_index linkIndex);

// Low-level link attachment/detachment operations
void AttachLink(link_index linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
void DetachLink(link_index linkIndex);

#endif
