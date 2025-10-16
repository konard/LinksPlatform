namespace Platform.Examples
{
    /// <summary>
    /// Represents a converter interface for importing and exporting objects.
    /// </summary>
    /// <typeparam name="TLink">The type of link used in the Links platform.</typeparam>
    /// <typeparam name="TFormat">The format type (e.g., string for JSON, XmlDocument for XML).</typeparam>
    public interface IObjectConverter<TLink, TFormat>
    {
        /// <summary>
        /// Imports an object from the specified format into Links structure.
        /// </summary>
        TLink Import(TFormat source);

        /// <summary>
        /// Exports a Links structure to the specified format.
        /// </summary>
        TFormat Export(TLink objectLink);
    }
}
