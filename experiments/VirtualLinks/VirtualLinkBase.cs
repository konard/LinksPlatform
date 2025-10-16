using System;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Base class for virtual link implementations.
    /// Provides common functionality for all virtual links.
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public abstract class VirtualLinkBase<TLink> : IVirtualLink<TLink>
    {
        /// <summary>
        /// Gets the actual link this virtual link represents.
        /// </summary>
        public TLink Link { get; }

        /// <summary>
        /// Gets the virtual link type identifier.
        /// </summary>
        public VirtualLinkType Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualLinkBase{TLink}"/> class.
        /// </summary>
        /// <param name="link">The link this virtual link represents.</param>
        /// <param name="type">The type of virtual link.</param>
        protected VirtualLinkBase(TLink link, VirtualLinkType type)
        {
            Link = link;
            Type = type;
        }

        /// <summary>
        /// Computes and returns the value of this virtual link.
        /// </summary>
        /// <returns>The computed value.</returns>
        public abstract TLink GetValue();

        /// <summary>
        /// Checks if this virtual link can be resolved for the given link.
        /// </summary>
        /// <returns>True if the virtual link can be resolved, false otherwise.</returns>
        public abstract bool CanResolve();

        /// <summary>
        /// Returns a string representation of this virtual link.
        /// </summary>
        public override string ToString()
        {
            return $"VirtualLink[{Type}]({Link})";
        }
    }
}
