using System;
using Platform.Data;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Factory for creating virtual links.
    /// Provides methods to create different types of virtual links for accessing
    /// computed or metadata information about links.
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public class VirtualLinkFactory<TLink>
    {
        private readonly ILinks<TLink> _links;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualLinkFactory{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        public VirtualLinkFactory(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// Creates a virtual link for accessing the index/ID of a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>An index virtual link.</returns>
        public IVirtualLink<TLink> CreateIndexLink(TLink link)
        {
            return new IndexVirtualLink<TLink>(link);
        }

        /// <summary>
        /// Creates a virtual link for accessing the source of a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A source virtual link.</returns>
        public IVirtualLink<TLink> CreateSourceLink(TLink link)
        {
            return new LinkPartVirtualLink<TLink>(link, _links, VirtualLinkType.Source);
        }

        /// <summary>
        /// Creates a virtual link for accessing the target of a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A target virtual link.</returns>
        public IVirtualLink<TLink> CreateTargetLink(TLink link)
        {
            return new LinkPartVirtualLink<TLink>(link, _links, VirtualLinkType.Target);
        }

        /// <summary>
        /// Creates a virtual link for accessing the linker of a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A linker virtual link.</returns>
        public IVirtualLink<TLink> CreateLinkerLink(TLink link)
        {
            return new LinkPartVirtualLink<TLink>(link, _links, VirtualLinkType.Linker);
        }

        /// <summary>
        /// Creates a virtual link for counting all references to a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A reference count virtual link.</returns>
        public IVirtualLink<TLink> CreateReferenceCountLink(TLink link)
        {
            return new ReferenceCountVirtualLink<TLink>(link, _links);
        }

        /// <summary>
        /// Creates a virtual link for counting source references to a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A source references count virtual link.</returns>
        public IVirtualLink<TLink> CreateSourceReferencesCountLink(TLink link)
        {
            return new ReferenceCountVirtualLink<TLink>(link, _links, VirtualLinkType.SourceReferencesCount);
        }

        /// <summary>
        /// Creates a virtual link for counting target references to a link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A target references count virtual link.</returns>
        public IVirtualLink<TLink> CreateTargetReferencesCountLink(TLink link)
        {
            return new ReferenceCountVirtualLink<TLink>(link, _links, VirtualLinkType.TargetReferencesCount);
        }

        /// <summary>
        /// Creates a virtual link of the specified type.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="type">The type of virtual link to create.</param>
        /// <returns>A virtual link of the specified type.</returns>
        public IVirtualLink<TLink> CreateVirtualLink(TLink link, VirtualLinkType type)
        {
            return type switch
            {
                VirtualLinkType.Index => CreateIndexLink(link),
                VirtualLinkType.Source => CreateSourceLink(link),
                VirtualLinkType.Target => CreateTargetLink(link),
                VirtualLinkType.Linker => CreateLinkerLink(link),
                VirtualLinkType.ReferenceCount => CreateReferenceCountLink(link),
                VirtualLinkType.SourceReferencesCount => CreateSourceReferencesCountLink(link),
                VirtualLinkType.TargetReferencesCount => CreateTargetReferencesCountLink(link),
                _ => throw new NotSupportedException($"Virtual link type {type} is not yet implemented.")
            };
        }
    }
}
