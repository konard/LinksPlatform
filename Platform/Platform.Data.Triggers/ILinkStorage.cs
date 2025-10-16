using System;
using System.Collections.Generic;

namespace Platform.Data.Triggers
{
    /// <summary>
    /// Abstraction for link storage operations.
    /// Allows triggers to interact with the link database.
    /// </summary>
    public interface ILinkStorage
    {
        /// <summary>
        /// Creates a new link with the specified source, linker, and target.
        /// </summary>
        object CreateLink(object source, object linker, object target);

        /// <summary>
        /// Updates an existing link.
        /// </summary>
        void UpdateLink(object link, object newSource, object newLinker, object newTarget);

        /// <summary>
        /// Deletes a link.
        /// </summary>
        void DeleteLink(object link);

        /// <summary>
        /// Searches for links matching the given pattern.
        /// Null values act as wildcards.
        /// </summary>
        IEnumerable<object> SearchLinks(object source, object linker, object target);

        /// <summary>
        /// Gets the source of a link.
        /// </summary>
        object GetSource(object link);

        /// <summary>
        /// Gets the linker of a link.
        /// </summary>
        object GetLinker(object link);

        /// <summary>
        /// Gets the target of a link.
        /// </summary>
        object GetTarget(object link);
    }
}
