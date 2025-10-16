using System;
using System.Linq;
using Platform.Data;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Virtual link that computes the count of references to a link.
    /// This is computed on-demand and not stored.
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public class ReferenceCountVirtualLink<TLink> : VirtualLinkBase<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly int? _asPartIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceCountVirtualLink{TLink}"/> class.
        /// </summary>
        /// <param name="link">The link whose references to count.</param>
        /// <param name="links">The links storage to query.</param>
        /// <param name="type">The type of reference count (total or specific part).</param>
        public ReferenceCountVirtualLink(TLink link, ILinks<TLink> links, VirtualLinkType type = VirtualLinkType.ReferenceCount)
            : base(link, type)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));

            _asPartIndex = type switch
            {
                VirtualLinkType.ReferenceCount => null, // Count all references
                VirtualLinkType.SourceReferencesCount => Constants.SourcePart,
                VirtualLinkType.TargetReferencesCount => Constants.TargetPart,
                VirtualLinkType.LinkerReferencesCount => 2,
                _ => throw new ArgumentException($"Invalid reference count type: {type}", nameof(type))
            };
        }

        /// <summary>
        /// Computes the count of references to the link.
        /// </summary>
        /// <returns>The reference count as a TLink value.</returns>
        public override TLink GetValue()
        {
            if (!CanResolve())
            {
                return default(TLink);
            }

            long count = 0;

            if (_asPartIndex.HasValue)
            {
                // Count references where this link appears in a specific position
                count = CountReferencesAtIndex(_asPartIndex.Value);
            }
            else
            {
                // Count all references to this link
                count = CountReferencesAtIndex(Constants.SourcePart) +
                        CountReferencesAtIndex(Constants.TargetPart);
            }

            // Convert count to TLink type
            return (TLink)Convert.ChangeType(count, typeof(TLink));
        }

        private long CountReferencesAtIndex(int index)
        {
            long count = 0;
            var any = _links.Constants.Any;

            // Create a query pattern based on which part we're counting
            var query = index switch
            {
                Constants.SourcePart => new Link<TLink>(Link, any, any),
                Constants.TargetPart => new Link<TLink>(any, Link, Link),
                2 => new Link<TLink>(any, any, Link), // Linker position
                _ => new Link<TLink>(any, any, any)
            };

            _links.Each(link =>
            {
                count++;
                return _links.Constants.Continue;
            }, query);

            return count;
        }

        /// <summary>
        /// Checks if the reference count can be computed.
        /// </summary>
        /// <returns>True if the link exists.</returns>
        public override bool CanResolve()
        {
            return Link != null && !Link.Equals(default(TLink)) && _links.Exists(Link);
        }
    }
}
