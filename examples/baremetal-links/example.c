/*
 * Example program demonstrating minimal Links Platform
 * for BareMetal OS or other bare-metal environments
 *
 * This example shows basic operations without dependencies on:
 * - Memory-mapped files
 * - Complex file I/O
 * - Heavy OS features
 */

#include "links.h"
#include <stdio.h>

/* Simple print helper for demonstration */
static void print_link(links_t *links, link_t link) {
    if (!links_exists(links, link)) {
        printf("  Link %llu: [DELETED]\n", (unsigned long long)link);
        return;
    }

    link_t source = links_get_source(links, link);
    link_t target = links_get_target(links, link);
    printf("  Link %llu: %llu -> %llu\n",
           (unsigned long long)link,
           (unsigned long long)source,
           (unsigned long long)target);
}

int main(void) {
    links_t store;
    int result;

    printf("=== Minimal Links Platform Example ===\n");
    printf("Portable implementation for BareMetal OS\n\n");

    /* Initialize links store with capacity for 100 links */
    result = links_init(&store, 100);
    if (result != LINKS_OK) {
        printf("Error: Failed to initialize links store\n");
        return 1;
    }
    printf("Initialized links store with capacity: 100\n\n");

    /* Create some links */
    printf("Creating links...\n");
    link_t link1 = links_create(&store, LINK_NULL, LINK_NULL);
    printf("  Created link %llu (self-referential placeholder)\n", (unsigned long long)link1);

    link_t link2 = links_create(&store, link1, link1);
    printf("  Created link %llu: %llu -> %llu\n",
           (unsigned long long)link2,
           (unsigned long long)link1,
           (unsigned long long)link1);

    link_t link3 = links_create(&store, link1, link2);
    printf("  Created link %llu: %llu -> %llu\n",
           (unsigned long long)link3,
           (unsigned long long)link1,
           (unsigned long long)link2);

    link_t link4 = links_create(&store, link2, link3);
    printf("  Created link %llu: %llu -> %llu\n",
           (unsigned long long)link4,
           (unsigned long long)link2,
           (unsigned long long)link3);

    printf("\nTotal links: %llu\n\n", (unsigned long long)links_count(&store));

    /* Display all links */
    printf("Current links:\n");
    for (link_t i = 1; i <= 4; i++) {
        print_link(&store, i);
    }

    /* Update a link */
    printf("\nUpdating link %llu...\n", (unsigned long long)link2);
    result = links_update(&store, link2, link3, link4);
    if (result == LINKS_OK) {
        printf("  Updated successfully\n");
        print_link(&store, link2);
    }

    /* Search for links */
    printf("\nSearching for all links with source = %llu...\n", (unsigned long long)link1);
    link_t results[10];
    link_t found = links_search(&store, link1, LINK_ANY, results, 10);
    printf("  Found %llu link(s):\n", (unsigned long long)found);
    for (link_t i = 0; i < found; i++) {
        printf("    ");
        print_link(&store, results[i]);
    }

    /* Delete a link */
    printf("\nDeleting link %llu...\n", (unsigned long long)link3);
    result = links_delete(&store, link3);
    if (result == LINKS_OK) {
        printf("  Deleted successfully\n");
    }

    printf("\nTotal links after deletion: %llu\n\n", (unsigned long long)links_count(&store));

    /* Display remaining links */
    printf("Remaining links:\n");
    for (link_t i = 1; i <= 4; i++) {
        print_link(&store, i);
    }

    /* Demonstrate self-referential link (common in Links Platform) */
    printf("\nCreating self-referential link...\n");
    link_t link5 = links_create(&store, LINK_NULL, LINK_NULL);
    links_update(&store, link5, link5, link5);
    printf("  Created link %llu pointing to itself\n", (unsigned long long)link5);
    print_link(&store, link5);

    /* Clean up */
    printf("\nCleaning up...\n");
    links_free(&store);
    printf("Done!\n");

    return 0;
}
