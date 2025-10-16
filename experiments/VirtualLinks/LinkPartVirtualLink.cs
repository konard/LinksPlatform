using System;
using Platform.Data;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Virtual link that accesses a specific part of a link (source, target, or linker).
    /// This provides access to the nth component of a link structure.
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public class LinkPartVirtualLink<TLink> : VirtualLinkBase<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly int _partIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkPartVirtualLink{TLink}"/> class.
        /// </summary>
        /// <param name="link">The link whose part to retrieve.</param>
        /// <param name="links">The links storage to query.</param>
        /// <param name="type">The type of link part (Source, Target, or Linker).</param>
        public LinkPartVirtualLink(TLink link, ILinks<TLink> links, VirtualLinkType type)
            : base(link, type)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));

            _partIndex = type switch
            {
                VirtualLinkType.Source => Constants.SourcePart,
                VirtualLinkType.Target => Constants.TargetPart,
                VirtualLinkType.Linker => 2, // For triple links
                _ => throw new ArgumentException($"Invalid link part type: {type}", nameof(type))
            };
        }

        /// <summary>
        /// Gets the specified part of the link.
        /// </summary>
        /// <returns>The link part value.</returns>
        public override TLink GetValue()
        {
            if (!CanResolve())
            {
                throw new InvalidOperationException($"Cannot resolve {Type} for link {Link}");
            }

            var linkValue = _links.GetLink(Link);

            return _partIndex switch
            {
                Constants.SourcePart => linkValue[Constants.SourcePart],
                Constants.TargetPart => linkValue[Constants.TargetPart],
                2 => linkValue.Length > 2 ? linkValue[2] : default(TLink),
                _ => default(TLink)
            };
        }

        /// <summary>
        /// Checks if the link part can be resolved.
        /// </summary>
        /// <returns>True if the link exists and has the requested part.</returns>
        public override bool CanResolve()
        {
            if (Link == null || Link.Equals(default(TLink)))
            {
                return false;
            }

            return _links.Exists(Link);
        }
    }
}
