using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a dynamic object interface for Links-based structures.
    /// Allows interpretation of Links as dynamic objects similar to JSON or XML.
    /// </summary>
    public interface IDynamicObject<TLink>
    {
        /// <summary>
        /// Gets the underlying link that represents this object.
        /// </summary>
        TLink Link { get; }

        /// <summary>
        /// Gets or sets a property value by property name.
        /// </summary>
        TLink this[string propertyName] { get; set; }

        /// <summary>
        /// Gets all properties of this object.
        /// </summary>
        IDictionary<string, TLink> GetProperties();

        /// <summary>
        /// Checks if a property exists.
        /// </summary>
        bool HasProperty(string propertyName);

        /// <summary>
        /// Gets the type of this object, if any.
        /// </summary>
        TLink GetType();

        /// <summary>
        /// Sets the type of this object.
        /// </summary>
        void SetType(TLink typeMarker);
    }
}
