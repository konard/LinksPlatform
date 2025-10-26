namespace Platform.Examples
{
    /// <summary>
    /// Defines an interface for storing Lino (Links Notation) data structures.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public interface ILinoStorage<TLink>
    {
        /// <summary>
        /// Creates a link from parsed Lino notation.
        /// </summary>
        /// <param name="id">Optional identifier for the link.</param>
        /// <param name="values">Array of values that constitute the link.</param>
        /// <returns>The created link identifier.</returns>
        TLink CreateLink(string id, params string[] values);

        /// <summary>
        /// Creates a reference link that points to an existing link.
        /// </summary>
        /// <param name="id">The identifier of the reference.</param>
        /// <param name="target">The target link identifier.</param>
        /// <returns>The created reference link identifier.</returns>
        TLink CreateReference(string id, TLink target);

        /// <summary>
        /// Gets an existing link by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the link.</param>
        /// <returns>The link identifier, or default if not found.</returns>
        TLink GetLink(string id);
    }
}
