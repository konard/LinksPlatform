namespace Platform.Examples
{
    /// <summary>
    /// Represents a virtual links structure that can be traversed like links
    /// but is not physically stored in the links space.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    /// <remarks>
    /// Virtual links structures allow traditional table data (like strings, numbers, etc.)
    /// to be accessed through the links interface without physically storing them as links.
    /// This enables uniform access patterns while maintaining efficient storage for different data types.
    /// </remarks>
    public interface IVirtualLinksStructure<TLink>
    {
        /// <summary>
        /// Gets the source of a virtual link with the specified identifier.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <returns>The source of the virtual link, or default if not applicable.</returns>
        TLink GetSource(TLink link);

        /// <summary>
        /// Gets the target of a virtual link with the specified identifier.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <returns>The target of the virtual link, or default if not applicable.</returns>
        TLink GetTarget(TLink link);

        /// <summary>
        /// Determines whether the specified link identifier represents a virtual link in this structure.
        /// </summary>
        /// <param name="link">The link identifier to check.</param>
        /// <returns>true if the link is part of this virtual structure; otherwise, false.</returns>
        bool Contains(TLink link);

        /// <summary>
        /// Gets the count of virtual links in this structure.
        /// </summary>
        long Count { get; }
    }
}
