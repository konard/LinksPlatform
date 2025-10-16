#ifndef PAIR_LINK_H
#define PAIR_LINK_H

#include <stddef.h>
#include <stdbool.h>

/* Forward declaration */
typedef struct pair_link pair_link;

/* Pair link structure - represents a directed link between two links */
struct pair_link {
    /* The source and target of this pair link */
    pair_link *source;
    pair_link *target;

    /* Linked list of links that reference this link as source */
    pair_link *first_referer_by_source;
    /* Linked list of links that reference this link as target */
    pair_link *first_referer_by_target;

    /* Sibling pointers for linked list navigation */
    pair_link *next_sibling_referer_by_source;
    pair_link *next_sibling_referer_by_target;
};

/* Create a new pair link with given source and target
 * If a link with the same source and target already exists, returns that link
 * Returns NULL on allocation failure
 */
pair_link* pair_link_create(pair_link *source, pair_link *target);

/* Create a self-referencing point (source and target point to itself) */
pair_link* pair_link_create_point(void);

/* Create an outgoing self-link (source points to itself, target is specified) */
pair_link* pair_link_create_outgoing_selflink(pair_link *target);

/* Create an incoming self-link (source is specified, target points to itself) */
pair_link* pair_link_create_incoming_selflink(pair_link *source);

/* Delete a pair link and all links that reference it */
void pair_link_delete(pair_link *link);

/* Check if a link has been deleted (has no references) */
bool pair_link_is_deleted(pair_link *link);

/* Try to find an existing link with the given source and target
 * Returns NULL if not found
 */
pair_link* pair_link_find(pair_link *source, pair_link *target);

/* Set the source of a link (updates the referer lists) */
void pair_link_set_source(pair_link *link, pair_link *new_source);

/* Set the target of a link (updates the referer lists) */
void pair_link_set_target(pair_link *link, pair_link *new_target);

/* Iterator callback type for traversing referers */
typedef void (*pair_link_referer_callback)(pair_link *referer, void *user_data);

/* Iterate over all links that reference this link as source */
void pair_link_foreach_referer_by_source(pair_link *link, pair_link_referer_callback callback, void *user_data);

/* Iterate over all links that reference this link as target */
void pair_link_foreach_referer_by_target(pair_link *link, pair_link_referer_callback callback, void *user_data);

#endif /* PAIR_LINK_H */
