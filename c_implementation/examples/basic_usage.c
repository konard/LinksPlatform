#include "../pair_link.h"
#include <stdio.h>
#include <stdlib.h>

/* Counter for referers */
typedef struct {
    int count;
} referer_counter;

/* Callback to count referers */
static void count_referer(pair_link *referer, void *user_data) {
    referer_counter *counter = (referer_counter*)user_data;
    counter->count++;
}

int main(void) {
    printf("=== Pair Link Implementation Example ===\n\n");

    /* Example 1: Create simple points */
    printf("1. Creating points (self-referencing links):\n");
    pair_link *point1 = pair_link_create_point();
    pair_link *point2 = pair_link_create_point();
    printf("   Created point1 at %p\n", (void*)point1);
    printf("   Created point2 at %p\n", (void*)point2);
    printf("   point1->source == point1: %s\n", point1->source == point1 ? "true" : "false");
    printf("   point1->target == point1: %s\n\n", point1->target == point1 ? "true" : "false");

    /* Example 2: Create a link between two points */
    printf("2. Creating a link between point1 and point2:\n");
    pair_link *link1 = pair_link_create(point1, point2);
    printf("   Created link1: %p -> %p\n", (void*)link1->source, (void*)link1->target);
    printf("   link1->source == point1: %s\n", link1->source == point1 ? "true" : "false");
    printf("   link1->target == point2: %s\n\n", link1->target == point2 ? "true" : "false");

    /* Example 3: Try to create duplicate link */
    printf("3. Attempting to create duplicate link:\n");
    pair_link *link1_duplicate = pair_link_create(point1, point2);
    printf("   Same link returned: %s\n", link1 == link1_duplicate ? "true" : "false");
    printf("   link1 address: %p\n", (void*)link1);
    printf("   link1_duplicate address: %p\n\n", (void*)link1_duplicate);

    /* Example 4: Create more links */
    printf("4. Creating additional links:\n");
    pair_link *link2 = pair_link_create(point2, point1);
    pair_link *link3 = pair_link_create(link1, link2);
    printf("   link2: %p -> %p (reverse of link1)\n", (void*)link2->source, (void*)link2->target);
    printf("   link3: %p -> %p (links can reference other links)\n\n", (void*)link3->source, (void*)link3->target);

    /* Example 5: Count referers */
    printf("5. Counting referers:\n");
    referer_counter counter = {0};
    pair_link_foreach_referer_by_source(point1, count_referer, &counter);
    printf("   point1 has %d referers by source\n", counter.count);

    counter.count = 0;
    pair_link_foreach_referer_by_target(point1, count_referer, &counter);
    printf("   point1 has %d referers by target\n", counter.count);

    counter.count = 0;
    pair_link_foreach_referer_by_source(link1, count_referer, &counter);
    printf("   link1 has %d referers by source\n\n", counter.count);

    /* Example 6: Create outgoing self-link */
    printf("6. Creating outgoing self-link:\n");
    pair_link *selflink = pair_link_create_outgoing_selflink(point1);
    printf("   selflink->source == selflink: %s\n", selflink->source == selflink ? "true" : "false");
    printf("   selflink->target == point1: %s\n\n", selflink->target == point1 ? "true" : "false");

    /* Example 7: Find existing link */
    printf("7. Finding existing link:\n");
    pair_link *found = pair_link_find(point1, point2);
    printf("   Found link: %p\n", (void*)found);
    printf("   Is link1: %s\n\n", found == link1 ? "true" : "false");

    /* Example 8: Check if deleted */
    printf("8. Checking deletion status:\n");
    printf("   point1 is deleted: %s\n", pair_link_is_deleted(point1) ? "true" : "false");
    printf("   NULL is deleted: %s\n\n", pair_link_is_deleted(NULL) ? "true" : "false");

    /* Example 9: Delete links */
    printf("9. Deleting links:\n");
    printf("   Deleting selflink...\n");
    pair_link_delete(selflink);
    printf("   Selflink deleted\n\n");

    /* Clean up remaining links */
    printf("10. Cleaning up remaining links:\n");
    printf("    Deleting link3 (will cascade to link1 and link2)...\n");
    pair_link_delete(link3);
    printf("    Deleting point1 and point2...\n");
    pair_link_delete(point1);
    pair_link_delete(point2);
    printf("    All links cleaned up\n\n");

    printf("=== Example completed successfully ===\n");
    return 0;
}
