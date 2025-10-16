using System;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Virtual link that returns the index/ID of a link.
    /// This is a computed value that represents the link's address itself.
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public class IndexVirtualLink<TLink> : VirtualLinkBase<TLink>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexVirtualLink{TLink}"/> class.
        /// </summary>
        /// <param name="link">The link whose index to retrieve.</param>
        public IndexVirtualLink(TLink link) : base(link, VirtualLinkType.Index)
        {
        }

        /// <summary>
        /// Gets the index/ID of the link.
        /// For a link, the index is the link address itself.
        /// </summary>
        /// <returns>The link's index/ID.</returns>
        public override TLink GetValue()
        {
            return Link;
        }

        /// <summary>
        /// Checks if this virtual link can be resolved.
        /// Index is always available for any link.
        /// </summary>
        /// <returns>Always returns true.</returns>
        public override bool CanResolve()
        {
            return Link != null && !Link.Equals(default(TLink));
        }
    }
}
