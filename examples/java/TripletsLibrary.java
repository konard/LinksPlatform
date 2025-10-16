import com.sun.jna.Library;
import com.sun.jna.Native;
import com.sun.jna.Callback;

/**
 * JNA interface for Platform.Data.Triplets.Kernel native library
 *
 * This interface provides access to the triple links data structure,
 * where each link consists of three parts: source, linker, and target.
 */
public interface TripletsLibrary extends Library {
    // Load the appropriate library based on the platform
    TripletsLibrary INSTANCE = (TripletsLibrary) Native.load(
        System.getProperty("os.name").toLowerCase().contains("win")
            ? "Platform_Data_Triplets_Kernel"
            : "Platform_Data_Triplets_Kernel",
        TripletsLibrary.class
    );

    /**
     * Visitor callback interface for walking through links
     */
    interface Visitor extends Callback {
        void invoke(long linkIndex);
    }

    /**
     * Stoppable visitor callback interface for walking through links with stop capability
     * @return 0 to stop, 1 to continue
     */
    interface StoppableVisitor extends Callback {
        long invoke(long linkIndex);
    }

    // Memory management functions
    /**
     * Opens a links database file
     * @param filename Path to the database file
     * @return 1 on success, 0 on failure
     */
    long OpenLinks(String filename);

    /**
     * Closes the currently open links database
     * @return 1 on success, 0 on failure
     */
    long CloseLinks();

    /**
     * Allocates a new link (low-level, use CreateLink instead)
     * @return Index of the allocated link
     */
    long AllocateLink();

    /**
     * Frees an allocated link (low-level)
     * @param linkIndex Index of the link to free
     */
    void FreeLink(long linkIndex);

    // Link manipulation functions
    /**
     * Creates a new link with given source, linker, and target
     * @param sourceIndex Index of the source link (use 0 for itself)
     * @param linkerIndex Index of the linker link (use 0 for itself)
     * @param targetIndex Index of the target link (use 0 for itself)
     * @return Index of the created link
     */
    long CreateLink(long sourceIndex, long linkerIndex, long targetIndex);

    /**
     * Searches for a link with given source, linker, and target
     * @param sourceIndex Index of the source link
     * @param linkerIndex Index of the linker link
     * @param targetIndex Index of the target link
     * @return Index of the found link, or 0 if not found
     */
    long SearchLink(long sourceIndex, long linkerIndex, long targetIndex);

    /**
     * Updates an existing link with new source, linker, and target
     * @param linkIndex Index of the link to update
     * @param sourceIndex New source index
     * @param linkerIndex New linker index
     * @param targetIndex New target index
     * @return Index of the updated link
     */
    long UpdateLink(long linkIndex, long sourceIndex, long linkerIndex, long targetIndex);

    /**
     * Replaces one link with another
     * @param linkIndex Index of the link to replace
     * @param replacementIndex Index of the replacement link
     * @return Index of the replacement link
     */
    long ReplaceLink(long linkIndex, long replacementIndex);

    /**
     * Deletes a link
     * @param linkIndex Index of the link to delete
     */
    void DeleteLink(long linkIndex);

    // Link property getters
    /**
     * Gets the source index of a link
     * @param linkIndex Index of the link
     * @return Source index
     */
    long GetSourceIndex(long linkIndex);

    /**
     * Gets the linker index of a link
     * @param linkIndex Index of the link
     * @return Linker index
     */
    long GetLinkerIndex(long linkIndex);

    /**
     * Gets the target index of a link
     * @param linkIndex Index of the link
     * @return Target index
     */
    long GetTargetIndex(long linkIndex);

    /**
     * Gets the timestamp of a link
     * @param linkIndex Index of the link
     * @return Timestamp as signed long
     */
    long GetTime(long linkIndex);

    // Referer functions
    /**
     * Gets the first link that references this link as source
     * @param linkIndex Index of the link
     * @return Index of the first referer by source
     */
    long GetFirstRefererBySourceIndex(long linkIndex);

    /**
     * Gets the first link that references this link as linker
     * @param linkIndex Index of the link
     * @return Index of the first referer by linker
     */
    long GetFirstRefererByLinkerIndex(long linkIndex);

    /**
     * Gets the first link that references this link as target
     * @param linkIndex Index of the link
     * @return Index of the first referer by target
     */
    long GetFirstRefererByTargetIndex(long linkIndex);

    /**
     * Gets the number of links that reference this link as source
     * @param linkIndex Index of the link
     * @return Number of referers by source
     */
    long GetLinkNumberOfReferersBySource(long linkIndex);

    /**
     * Gets the number of links that reference this link as linker
     * @param linkIndex Index of the link
     * @return Number of referers by linker
     */
    long GetLinkNumberOfReferersByLinker(long linkIndex);

    /**
     * Gets the number of links that reference this link as target
     * @param linkIndex Index of the link
     * @return Number of referers by target
     */
    long GetLinkNumberOfReferersByTarget(long linkIndex);

    // Walking through referers
    /**
     * Walks through all links that reference this link as source
     * @param linkIndex Index of the link
     * @param visitor Callback to invoke for each referer
     */
    void WalkThroughAllReferersBySource(long linkIndex, Visitor visitor);

    /**
     * Walks through links that reference this link as source (stoppable)
     * @param linkIndex Index of the link
     * @param visitor Callback to invoke for each referer (return 0 to stop)
     * @return Result of the walk
     */
    long WalkThroughReferersBySource(long linkIndex, StoppableVisitor visitor);

    /**
     * Walks through all links that reference this link as linker
     * @param linkIndex Index of the link
     * @param visitor Callback to invoke for each referer
     */
    void WalkThroughAllReferersByLinker(long linkIndex, Visitor visitor);

    /**
     * Walks through links that reference this link as linker (stoppable)
     * @param linkIndex Index of the link
     * @param visitor Callback to invoke for each referer (return 0 to stop)
     * @return Result of the walk
     */
    long WalkThroughReferersByLinker(long linkIndex, StoppableVisitor visitor);

    /**
     * Walks through all links that reference this link as target
     * @param linkIndex Index of the link
     * @param visitor Callback to invoke for each referer
     */
    void WalkThroughAllReferersByTarget(long linkIndex, Visitor visitor);

    /**
     * Walks through links that reference this link as target (stoppable)
     * @param linkIndex Index of the link
     * @param visitor Callback to invoke for each referer (return 0 to stop)
     * @return Result of the walk
     */
    long WalkThroughReferersByTarget(long linkIndex, StoppableVisitor visitor);

    // Mapped links
    /**
     * Gets a link by its mapped index
     * @param mappedIndex Mapped index
     * @return Link index
     */
    long GetMappedLink(long mappedIndex);

    /**
     * Sets a mapped index to point to a link
     * @param mappedIndex Mapped index
     * @param linkIndex Link index to map
     */
    void SetMappedLink(long mappedIndex, long linkIndex);

    // Database statistics
    /**
     * Gets the total number of links in the database
     * @return Number of links
     */
    long GetLinksCount();

    /**
     * Walks through all links in the database
     * @param visitor Callback to invoke for each link
     */
    void WalkThroughAllLinks(Visitor visitor);

    /**
     * Walks through all links in the database (stoppable)
     * @param visitor Callback to invoke for each link (return 0 to stop)
     * @return Result of the walk
     */
    long WalkThroughLinks(StoppableVisitor visitor);
}
