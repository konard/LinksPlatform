using System;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Represents a virtual link - a method to access computed or metadata information about a link
    /// without physically storing it in the Links Platform storage.
    ///
    /// Virtual links can be used to:
    /// 1. Access link index/ID (computed, not stored)
    /// 2. Access link parts (source, target, linker)
    /// 3. Count references to a link
    /// 4. Access nth reference or child
    /// 5. Get link weight
    /// 6. Get creation/update timestamps
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public interface IVirtualLink<TLink>
    {
        /// <summary>
        /// Gets the actual link this virtual link represents.
        /// </summary>
        TLink Link { get; }

        /// <summary>
        /// Gets the virtual link type identifier.
        /// </summary>
        VirtualLinkType Type { get; }

        /// <summary>
        /// Computes and returns the value of this virtual link.
        /// </summary>
        /// <returns>The computed value.</returns>
        TLink GetValue();

        /// <summary>
        /// Checks if this virtual link can be resolved for the given link.
        /// </summary>
        /// <returns>True if the virtual link can be resolved, false otherwise.</returns>
        bool CanResolve();
    }

    /// <summary>
    /// Defines the types of virtual links available in the system.
    /// </summary>
    public enum VirtualLinkType
    {
        /// <summary>Index or ID of the link.</summary>
        Index = 0,

        /// <summary>Source part of the link.</summary>
        Source = 1,

        /// <summary>Target part of the link.</summary>
        Target = 2,

        /// <summary>Linker part of the link (for triple links).</summary>
        Linker = 3,

        /// <summary>Count of direct references to the link.</summary>
        ReferenceCount = 4,

        /// <summary>Nth reference to the link.</summary>
        NthReference = 5,

        /// <summary>Weight of the link.</summary>
        Weight = 6,

        /// <summary>Creation timestamp.</summary>
        CreationTime = 7,

        /// <summary>Last update timestamp.</summary>
        UpdateTime = 8,

        /// <summary>Count of children using this link as source.</summary>
        SourceReferencesCount = 9,

        /// <summary>Count of children using this link as target.</summary>
        TargetReferencesCount = 10,

        /// <summary>Count of children using this link as linker.</summary>
        LinkerReferencesCount = 11
    }
}
