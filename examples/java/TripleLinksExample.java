/**
 * Example demonstrating the usage of Triple Links using JNA
 *
 * This example shows how to:
 * - Open and close a links database
 * - Create links with source, linker, and target components
 * - Update existing links
 * - Query link properties
 * - Delete links
 * - Build a simple semantic network
 */
public class TripleLinksExample {
    // Constants for self-referential links
    private static final long ITSELF = 0;
    private static final long NULL = 0;

    public static void main(String[] args) {
        TripletsLibrary lib = TripletsLibrary.INSTANCE;

        System.out.println("=== Triple Links Example ===\n");

        // Open the database
        String dbPath = "examples.db";
        System.out.println("Opening database: " + dbPath);
        long openResult = lib.OpenLinks(dbPath);

        if (openResult != 1) {
            System.err.println("Failed to open database!");
            return;
        }

        System.out.println("Database opened successfully.\n");

        try {
            // Example 1: Create a simple link
            System.out.println("--- Example 1: Creating a simple link ---");
            long simpleLink = lib.CreateLink(ITSELF, ITSELF, ITSELF);
            System.out.println("Created link with index: " + simpleLink);
            System.out.println("  Source: " + lib.GetSourceIndex(simpleLink));
            System.out.println("  Linker: " + lib.GetLinkerIndex(simpleLink));
            System.out.println("  Target: " + lib.GetTargetIndex(simpleLink));
            System.out.println();

            // Example 2: Build a semantic network (isA relationship)
            System.out.println("--- Example 2: Building a semantic network ---");

            // Create the "isA" relationship link
            long isA = lib.CreateLink(ITSELF, ITSELF, ITSELF);
            System.out.println("Created 'isA' relationship link: " + isA);

            // Create the "isNotA" relationship (referencing isA as target)
            long isNotA = lib.CreateLink(ITSELF, ITSELF, isA);
            System.out.println("Created 'isNotA' relationship link: " + isNotA);

            // Create a "link" concept
            long link = lib.CreateLink(ITSELF, isA, ITSELF);
            System.out.println("Created 'link' concept: " + link);

            // Create a "thing" concept that is not a link
            long thing = lib.CreateLink(ITSELF, isNotA, link);
            System.out.println("Created 'thing' concept: " + thing);
            System.out.println();

            // Example 3: Update a link
            System.out.println("--- Example 3: Updating a link ---");
            System.out.println("Updating 'isA' link to reference 'link' as target");
            long updatedIsA = lib.UpdateLink(isA, isA, isA, link);
            System.out.println("Updated link index: " + updatedIsA);
            System.out.println("  Source: " + lib.GetSourceIndex(updatedIsA));
            System.out.println("  Linker: " + lib.GetLinkerIndex(updatedIsA));
            System.out.println("  Target: " + lib.GetTargetIndex(updatedIsA));
            System.out.println("The minimal system core is now formed.\n");

            // Example 4: Query referers
            System.out.println("--- Example 4: Querying referers ---");
            long referersBySource = lib.GetLinkNumberOfReferersBySource(link);
            long referersByLinker = lib.GetLinkNumberOfReferersByLinker(link);
            long referersByTarget = lib.GetLinkNumberOfReferersByTarget(link);
            System.out.println("Link " + link + " has:");
            System.out.println("  " + referersBySource + " referer(s) by source");
            System.out.println("  " + referersByLinker + " referer(s) by linker");
            System.out.println("  " + referersByTarget + " referer(s) by target");
            System.out.println();

            // Example 5: Database statistics
            System.out.println("--- Example 5: Database statistics ---");
            long totalLinks = lib.GetLinksCount();
            System.out.println("Total links in database: " + totalLinks);
            System.out.println();

            // Example 6: Walk through all links
            System.out.println("--- Example 6: Walking through all links ---");
            System.out.println("All links in database:");
            lib.WalkThroughAllLinks(new TripletsLibrary.Visitor() {
                @Override
                public void invoke(long linkIndex) {
                    long source = lib.GetSourceIndex(linkIndex);
                    long linker = lib.GetLinkerIndex(linkIndex);
                    long target = lib.GetTargetIndex(linkIndex);
                    System.out.println("  Link " + linkIndex + ": [" + source + ", " + linker + ", " + target + "]");
                }
            });
            System.out.println();

            // Example 7: Search for a specific link
            System.out.println("--- Example 7: Searching for a link ---");
            long searchResult = lib.SearchLink(ITSELF, isNotA, link);
            if (searchResult != 0) {
                System.out.println("Found link with pattern [0, " + isNotA + ", " + link + "]: " + searchResult);
            } else {
                System.out.println("Link not found.");
            }
            System.out.println();

            // Example 8: Cleanup - delete links
            System.out.println("--- Example 8: Cleanup ---");
            System.out.println("Deleting links...");

            // Delete isA - this will cascade and delete dependent links
            lib.DeleteLink(isA);
            System.out.println("Deleted 'isA' link (cascade delete)");

            // Delete remaining links
            lib.DeleteLink(thing);
            System.out.println("Deleted 'thing' link");

            lib.DeleteLink(simpleLink);
            System.out.println("Deleted simple link");

            long remainingLinks = lib.GetLinksCount();
            System.out.println("Remaining links in database: " + remainingLinks);

        } catch (Exception e) {
            System.err.println("Error during execution: " + e.getMessage());
            e.printStackTrace();
        } finally {
            // Always close the database
            System.out.println("\nClosing database...");
            long closeResult = lib.CloseLinks();
            if (closeResult == 1) {
                System.out.println("Database closed successfully.");
            } else {
                System.err.println("Failed to close database!");
            }
        }

        System.out.println("\n=== Example completed ===");
    }
}
