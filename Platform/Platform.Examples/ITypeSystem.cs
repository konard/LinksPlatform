using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a type system interface for Links-based objects.
    /// Provides foundation for static and dynamic type interpretation.
    /// </summary>
    public interface ITypeSystem<TLink>
    {
        /// <summary>
        /// Gets or creates a type marker for the specified type name.
        /// </summary>
        TLink GetOrCreateType(string typeName);

        /// <summary>
        /// Gets or creates a property marker for the specified property name.
        /// </summary>
        TLink GetOrCreateProperty(string propertyName);

        /// <summary>
        /// Checks if a link represents a type marker.
        /// </summary>
        bool IsType(TLink link);

        /// <summary>
        /// Checks if a link represents a property marker.
        /// </summary>
        bool IsProperty(TLink link);

        /// <summary>
        /// Gets the type name for a type marker link.
        /// </summary>
        string GetTypeName(TLink typeMarker);

        /// <summary>
        /// Gets the property name for a property marker link.
        /// </summary>
        string GetPropertyName(TLink propertyMarker);
    }
}
