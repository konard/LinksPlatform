/*
 * Minimal Links Platform Implementation for BareMetal OS
 *
 * Pure memory implementation without external dependencies.
 */

#include "links.h"
#include <stdlib.h>
#include <string.h>

/* Internal helper: check if link index is valid */
static int is_valid_index(links_t *links, link_t index) {
    if (index == LINK_NULL || index > links->count) {
        return 0;
    }
    /* Check if link is not deleted (deleted links point to themselves as markers) */
    link_data_t *link = &links->data[index - 1];
    return !(link->source == index && link->target == LINK_NULL);
}

/* Initialize links store */
int links_init(links_t *links, link_t capacity) {
    if (links == NULL || capacity == 0) {
        return LINKS_ERROR;
    }

    links->data = (link_data_t *)calloc(capacity, sizeof(link_data_t));
    if (links->data == NULL) {
        return LINKS_NOMEM;
    }

    links->capacity = capacity;
    links->count = 0;
    links->first_free = 1; /* Links are 1-indexed */

    return LINKS_OK;
}

/* Free links store */
void links_free(links_t *links) {
    if (links != NULL && links->data != NULL) {
        free(links->data);
        links->data = NULL;
        links->capacity = 0;
        links->count = 0;
        links->first_free = 1;
    }
}

/* Create a new link */
link_t links_create(links_t *links, link_t source, link_t target) {
    if (links == NULL) {
        return LINK_NULL;
    }

    /* Check if we have space */
    if (links->count >= links->capacity) {
        return LINK_NULL;
    }

    /* Allocate new link */
    link_t new_link = links->count + 1;
    links->count = new_link;

    /* Set link data */
    link_data_t *link = &links->data[new_link - 1];
    link->source = source;
    link->target = target;

    return new_link;
}

/* Get source of a link */
link_t links_get_source(links_t *links, link_t link) {
    if (links == NULL || !is_valid_index(links, link)) {
        return LINK_NULL;
    }
    return links->data[link - 1].source;
}

/* Get target of a link */
link_t links_get_target(links_t *links, link_t link) {
    if (links == NULL || !is_valid_index(links, link)) {
        return LINK_NULL;
    }
    return links->data[link - 1].target;
}

/* Update a link */
int links_update(links_t *links, link_t link, link_t new_source, link_t new_target) {
    if (links == NULL || !is_valid_index(links, link)) {
        return LINKS_ERROR;
    }

    link_data_t *data = &links->data[link - 1];
    data->source = new_source;
    data->target = new_target;

    return LINKS_OK;
}

/* Delete a link */
int links_delete(links_t *links, link_t link) {
    if (links == NULL || !is_valid_index(links, link)) {
        return LINKS_ERROR;
    }

    /* Mark as deleted by setting a special pattern */
    link_data_t *data = &links->data[link - 1];
    data->source = link;
    data->target = LINK_NULL;

    return LINKS_OK;
}

/* Check if link exists */
int links_exists(links_t *links, link_t link) {
    if (links == NULL) {
        return 0;
    }
    return is_valid_index(links, link);
}

/* Get total count of valid links */
link_t links_count(links_t *links) {
    if (links == NULL) {
        return 0;
    }

    link_t valid_count = 0;
    for (link_t i = 1; i <= links->count; i++) {
        if (is_valid_index(links, i)) {
            valid_count++;
        }
    }

    return valid_count;
}

/* Search for links matching patterns */
link_t links_search(links_t *links, link_t source, link_t target,
                    link_t *results, link_t max_results) {
    if (links == NULL || results == NULL || max_results == 0) {
        return 0;
    }

    link_t found = 0;
    for (link_t i = 1; i <= links->count && found < max_results; i++) {
        if (!is_valid_index(links, i)) {
            continue;
        }

        link_data_t *link = &links->data[i - 1];

        /* Check if this link matches the search criteria */
        int source_match = (source == LINK_ANY) || (link->source == source);
        int target_match = (target == LINK_ANY) || (link->target == target);

        if (source_match && target_match) {
            results[found++] = i;
        }
    }

    return found;
}
