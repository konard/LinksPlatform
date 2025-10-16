using System;

namespace Platform.Data.Core
{
    /// <summary>
    /// Demonstrates how traditional CRUD operations can be implemented as wrappers
    /// around the unified In/Out operations. This shows how Links simplicity
    /// influences the code by reducing all operations to a single unified approach.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier (typically ulong).</typeparam>
    public class UniLinksWrapper<TLink> : IUniLinksCRUD<TLink>, IUniLinksRW<TLink>
    {
        private readonly IUniLinksIO<TLink> _links;

        public UniLinksWrapper(IUniLinksIO<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        #region CRUD Operations (wrapper around In/Out)

        /// <summary>
        /// Creates a new link. Falls back to the unified In operation.
        /// Creation is equivalent to In(null, parts).
        /// </summary>
        public TLink Create(TLink[] parts)
        {
            // Creation: In(null, parts) - create new link from parts
            return _links.In(null, parts);
        }

        /// <summary>
        /// Updates an existing link. Falls back to the unified In operation.
        /// Update is equivalent to In(before, after).
        /// </summary>
        public TLink Update(TLink[] before, TLink[] after)
        {
            // Update: In(before, after) - transform before into after
            return _links.In(before, after);
        }

        /// <summary>
        /// Deletes a link. Falls back to the unified In operation.
        /// Deletion is equivalent to In(parts, null).
        /// </summary>
        public void Delete(TLink[] parts)
        {
            // Deletion: In(parts, null) - remove specified link
            _links.In(parts, null);
        }

        /// <summary>
        /// Reads/queries a single part of a link. Falls back to the unified Out operation.
        /// Read is replaced with Out operation.
        /// </summary>
        public TLink Read(ulong partType, TLink link)
        {
            TLink result = default;

            // Read: Out operation to extract a specific part
            _links.Out(linkParts =>
            {
                if (linkParts.Length > partType)
                {
                    result = linkParts[partType];
                }
                return false; // Stop after first match
            }, link);

            return result;
        }

        /// <summary>
        /// Reads/queries links matching a pattern. Falls back to the unified Out operation.
        /// Read is replaced with Out operation.
        /// </summary>
        public bool Read(Func<TLink, bool> handler, params TLink[] pattern)
        {
            // Read: Out operation with pattern matching
            return _links.Out(linkParts =>
            {
                if (linkParts.Length > 0)
                {
                    return handler(linkParts[0]);
                }
                return true;
            }, pattern);
        }

        #endregion

        #region Read/Write Operations (aliases)

        /// <summary>
        /// Write is an alias for the unified In operation.
        /// This demonstrates that Write and In are conceptually the same.
        /// </summary>
        public TLink Write(TLink[] before, TLink[] after)
        {
            // Write is just an alias to In
            return _links.In(before, after);
        }

        // Read methods are inherited from CRUD implementation above
        TLink IUniLinksRW<TLink>.Read(ulong partType, TLink link) => Read(partType, link);
        bool IUniLinksRW<TLink>.Read(Func<TLink, bool> handler, params TLink[] pattern) => Read(handler, pattern);

        #endregion
    }

    /// <summary>
    /// Extension methods showing convenient usage patterns of the unified In/Out operations.
    /// </summary>
    public static class UniLinksExtensions
    {
        /// <summary>
        /// Demonstrates Create operation as a semantic wrapper around In.
        /// </summary>
        public static TLink CreateLink<TLink>(this IUniLinksIO<TLink> links, TLink source, TLink target)
        {
            return links.In(null, new[] { default(TLink), source, target });
        }

        /// <summary>
        /// Demonstrates Update operation as a semantic wrapper around In.
        /// </summary>
        public static TLink UpdateLink<TLink>(this IUniLinksIO<TLink> links, TLink linkId, TLink newSource, TLink newTarget)
        {
            return links.In(new[] { linkId }, new[] { linkId, newSource, newTarget });
        }

        /// <summary>
        /// Demonstrates Delete operation as a semantic wrapper around In.
        /// </summary>
        public static void DeleteLink<TLink>(this IUniLinksIO<TLink> links, TLink linkId)
        {
            links.In(new[] { linkId }, null);
        }

        /// <summary>
        /// Demonstrates Read operation as a semantic wrapper around Out.
        /// </summary>
        public static TLink GetSource<TLink>(this IUniLinksIO<TLink> links, TLink linkId)
        {
            TLink source = default;
            links.Out(parts =>
            {
                if (parts.Length > (int)PartType.LinkSourceOrFirst)
                {
                    source = parts[(int)PartType.LinkSourceOrFirst];
                }
                return false;
            }, linkId);
            return source;
        }

        /// <summary>
        /// Demonstrates Read operation as a semantic wrapper around Out.
        /// </summary>
        public static TLink GetTarget<TLink>(this IUniLinksIO<TLink> links, TLink linkId)
        {
            TLink target = default;
            links.Out(parts =>
            {
                if (parts.Length > (int)PartType.LinkTargetOrSecond)
                {
                    target = parts[(int)PartType.LinkTargetOrSecond];
                }
                return false;
            }, linkId);
            return target;
        }
    }
}
