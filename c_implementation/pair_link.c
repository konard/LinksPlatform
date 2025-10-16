#include "pair_link.h"
#include <stdlib.h>
#include <string.h>

/* Helper function to remove link from source referer list */
static void remove_from_source_referer_list(pair_link *link) {
    if (link->source == NULL) {
        return;
    }

    pair_link *source = link->source;

    if (source->first_referer_by_source == link) {
        source->first_referer_by_source = link->next_sibling_referer_by_source;
    } else {
        pair_link *prev = source->first_referer_by_source;
        while (prev != NULL && prev->next_sibling_referer_by_source != link) {
            prev = prev->next_sibling_referer_by_source;
        }
        if (prev != NULL) {
            prev->next_sibling_referer_by_source = link->next_sibling_referer_by_source;
        }
    }
}

/* Helper function to remove link from target referer list */
static void remove_from_target_referer_list(pair_link *link) {
    if (link->target == NULL) {
        return;
    }

    pair_link *target = link->target;

    if (target->first_referer_by_target == link) {
        target->first_referer_by_target = link->next_sibling_referer_by_target;
    } else {
        pair_link *prev = target->first_referer_by_target;
        while (prev != NULL && prev->next_sibling_referer_by_target != link) {
            prev = prev->next_sibling_referer_by_target;
        }
        if (prev != NULL) {
            prev->next_sibling_referer_by_target = link->next_sibling_referer_by_target;
        }
    }
}

/* Helper function to add link to source referer list */
static void add_to_source_referer_list(pair_link *link, pair_link *new_source) {
    if (new_source != NULL) {
        link->next_sibling_referer_by_source = new_source->first_referer_by_source;
        new_source->first_referer_by_source = link;
    } else {
        link->next_sibling_referer_by_source = NULL;
    }
}

/* Helper function to add link to target referer list */
static void add_to_target_referer_list(pair_link *link, pair_link *new_target) {
    if (new_target != NULL) {
        link->next_sibling_referer_by_target = new_target->first_referer_by_target;
        new_target->first_referer_by_target = link;
    } else {
        link->next_sibling_referer_by_target = NULL;
    }
}

void pair_link_set_source(pair_link *link, pair_link *new_source) {
    if (link == NULL) {
        return;
    }

    pair_link *previous_source = link->source;

    if (previous_source != new_source) {
        remove_from_source_referer_list(link);
        add_to_source_referer_list(link, new_source);
        link->source = new_source;
    }
}

void pair_link_set_target(pair_link *link, pair_link *new_target) {
    if (link == NULL) {
        return;
    }

    pair_link *previous_target = link->target;

    if (previous_target != new_target) {
        remove_from_target_referer_list(link);
        add_to_target_referer_list(link, new_target);
        link->target = new_target;
    }
}

pair_link* pair_link_find(pair_link *source, pair_link *target) {
    if (source == NULL || target == NULL) {
        return NULL;
    }

    /* Search through target's referers (assuming this is faster on average) */
    pair_link *referer = target->first_referer_by_target;
    while (referer != NULL) {
        if (referer->source == source && referer->target == target) {
            return referer;
        }
        referer = referer->next_sibling_referer_by_target;
    }

    return NULL;
}

pair_link* pair_link_create(pair_link *source, pair_link *target) {
    if (source == NULL || target == NULL) {
        return NULL;
    }

    /* Check if link already exists */
    pair_link *existing = pair_link_find(source, target);
    if (existing != NULL) {
        return existing;
    }

    /* Allocate new link */
    pair_link *link = (pair_link*)calloc(1, sizeof(pair_link));
    if (link == NULL) {
        return NULL;
    }

    /* Initialize all fields to NULL (calloc already does this, but being explicit) */
    link->source = NULL;
    link->target = NULL;
    link->first_referer_by_source = NULL;
    link->first_referer_by_target = NULL;
    link->next_sibling_referer_by_source = NULL;
    link->next_sibling_referer_by_target = NULL;

    /* Set source and target using the setter functions to update referer lists */
    pair_link_set_source(link, source);
    pair_link_set_target(link, target);

    return link;
}

pair_link* pair_link_create_point(void) {
    pair_link *link = (pair_link*)calloc(1, sizeof(pair_link));
    if (link == NULL) {
        return NULL;
    }

    /* Point to itself */
    link->source = link;
    link->target = link;

    /* Add to own referer lists */
    link->first_referer_by_source = NULL;
    link->first_referer_by_target = NULL;
    link->next_sibling_referer_by_source = NULL;
    link->next_sibling_referer_by_target = NULL;

    return link;
}

pair_link* pair_link_create_outgoing_selflink(pair_link *target) {
    if (target == NULL) {
        return NULL;
    }

    pair_link *link = (pair_link*)calloc(1, sizeof(pair_link));
    if (link == NULL) {
        return NULL;
    }

    /* Source points to itself, target is specified */
    link->source = link;
    pair_link_set_target(link, target);

    return link;
}

pair_link* pair_link_create_incoming_selflink(pair_link *source) {
    if (source == NULL) {
        return NULL;
    }

    pair_link *link = (pair_link*)calloc(1, sizeof(pair_link));
    if (link == NULL) {
        return NULL;
    }

    /* Target points to itself, source is specified */
    pair_link_set_source(link, source);
    link->target = link;

    return link;
}

bool pair_link_is_deleted(pair_link *link) {
    if (link == NULL) {
        return true;
    }

    return link->first_referer_by_source == NULL &&
           link->first_referer_by_target == NULL;
}

void pair_link_delete(pair_link *link) {
    if (link == NULL) {
        return;
    }

    /* Remove from source and target referer lists */
    pair_link_set_source(link, NULL);
    pair_link_set_target(link, NULL);

    /* Delete all links that reference this link as source */
    while (link->first_referer_by_source != NULL) {
        pair_link_delete(link->first_referer_by_source);
    }

    /* Delete all links that reference this link as target */
    while (link->first_referer_by_target != NULL) {
        pair_link_delete(link->first_referer_by_target);
    }

    /* Free the link itself */
    free(link);
}

void pair_link_foreach_referer_by_source(pair_link *link, pair_link_referer_callback callback, void *user_data) {
    if (link == NULL || callback == NULL) {
        return;
    }

    pair_link *referer = link->first_referer_by_source;
    while (referer != NULL) {
        /* Save next pointer before callback (in case callback deletes the referer) */
        pair_link *next = referer->next_sibling_referer_by_source;
        callback(referer, user_data);
        referer = next;
    }
}

void pair_link_foreach_referer_by_target(pair_link *link, pair_link_referer_callback callback, void *user_data) {
    if (link == NULL || callback == NULL) {
        return;
    }

    pair_link *referer = link->first_referer_by_target;
    while (referer != NULL) {
        /* Save next pointer before callback (in case callback deletes the referer) */
        pair_link *next = referer->next_sibling_referer_by_target;
        callback(referer, user_data);
        referer = next;
    }
}
