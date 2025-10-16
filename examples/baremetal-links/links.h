/*
 * Minimal Links Platform Implementation for BareMetal OS
 *
 * This is a pure memory implementation without dependencies on:
 * - Memory-mapped files
 * - Standard library file I/O
 * - Complex OS features
 *
 * Suitable for bare-metal environments with minimal OS support.
 */

#ifndef LINKS_H
#define LINKS_H

#include <stdint.h>
#include <stddef.h>

/* Use 64-bit link identifiers for consistency with C# implementation */
typedef uint64_t link_t;

/* Link structure: each link has source and target */
typedef struct {
    link_t source;
    link_t target;
} link_data_t;

/* Links store structure */
typedef struct {
    link_data_t *data;     /* Array of links */
    link_t capacity;       /* Total capacity */
    link_t count;          /* Number of links (including deleted) */
    link_t first_free;     /* Index of first free slot */
} links_t;

/* Special constants */
#define LINK_NULL       0ULL
#define LINK_ANY        0xFFFFFFFFFFFFFFFFULL

/* Error codes */
#define LINKS_OK        0
#define LINKS_ERROR     -1
#define LINKS_NOMEM     -2
#define LINKS_NOTFOUND  -3

/* Initialize links store with given capacity */
int links_init(links_t *links, link_t capacity);

/* Free links store */
void links_free(links_t *links);

/* Create a new link */
link_t links_create(links_t *links, link_t source, link_t target);

/* Get source of a link */
link_t links_get_source(links_t *links, link_t link);

/* Get target of a link */
link_t links_get_target(links_t *links, link_t link);

/* Update a link */
int links_update(links_t *links, link_t link, link_t new_source, link_t new_target);

/* Delete a link */
int links_delete(links_t *links, link_t link);

/* Check if link exists */
int links_exists(links_t *links, link_t link);

/* Get total count of links */
link_t links_count(links_t *links);

/* Search for links matching source and target patterns */
/* Use LINK_ANY for wildcard matching */
link_t links_search(links_t *links, link_t source, link_t target,
                    link_t *results, link_t max_results);

#endif /* LINKS_H */
