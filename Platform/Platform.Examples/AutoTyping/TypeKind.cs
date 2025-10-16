namespace Platform.Examples.AutoTyping
{
    /// <summary>
    /// Represents the kind/category of an inferred type.
    /// </summary>
    public enum TypeKind
    {
        /// <summary>
        /// Type is unknown or could not be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Primitive type (numbers, strings, booleans, dates, etc.).
        /// </summary>
        Primitive,

        /// <summary>
        /// Object/record type with named properties.
        /// </summary>
        Object,

        /// <summary>
        /// Collection/array type.
        /// </summary>
        Collection,

        /// <summary>
        /// Union type (can be one of multiple types).
        /// </summary>
        Union,

        /// <summary>
        /// Null type.
        /// </summary>
        Null
    }
}
