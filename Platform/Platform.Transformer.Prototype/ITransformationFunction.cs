namespace Platform.Transformer.Prototype
{
    /// <summary>
    /// Represents a transformation function that operates on Doublets links.
    /// This replaces the regex-based substitution rules from the old architecture
    /// with more powerful function-based transformations.
    /// </summary>
    /// <typeparam name="TLink">The type used for link identifiers in Doublets</typeparam>
    public interface ITransformationFunction<TLink>
    {
        /// <summary>
        /// Gets the name of this transformation function.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether this transformation can be applied to the given link.
        /// </summary>
        /// <param name="link">The link to check</param>
        /// <returns>True if the transformation can be applied, false otherwise</returns>
        bool CanTransform(TLink link);

        /// <summary>
        /// Applies the transformation to the given link, potentially creating new links
        /// or modifying existing ones in the Doublets storage.
        /// </summary>
        /// <param name="link">The link to transform</param>
        /// <returns>The transformed link (may be the same or a new link)</returns>
        TLink Transform(TLink link);
    }
}
