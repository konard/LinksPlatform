using System;
using System.Linq;

namespace Platform.Data.Triplets.Memory
{
    public partial class Link
    {
        /// <summary>
        /// Creates or retrieves an existing link with the specified source, linker, and target.
        /// If a link with these components already exists, returns the existing link.
        /// </summary>
        /// <param name="source">The source link (required).</param>
        /// <param name="linker">The linker link (required).</param>
        /// <param name="target">The target link (required).</param>
        /// <returns>A link with the specified components.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public static Link Create(Link source, Link linker, Link target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Source link must be specified.");
            if (linker == null)
                throw new ArgumentNullException(nameof(linker), "Linker link must be specified.");
            if (target == null)
                throw new ArgumentNullException(nameof(target), "Target link must be specified.");

            var existing = TryFindExistingLink(source, linker, target);
            if (existing != null)
                return existing;

            return new Link
            {
                Source = source,
                Linker = linker,
                Target = target
            };
        }

        /// <summary>
        /// Creates an outgoing self-link where the source references itself.
        /// Pattern: (link, linker, target)
        /// </summary>
        /// <param name="linker">The linker link.</param>
        /// <param name="target">The target link.</param>
        /// <returns>A new link that references itself as source.</returns>
        public static Link CreateOutcomingSelfLink(Link linker, Link? target)
        {
            var link = new Link
            {
                Linker = linker,
                Target = target
            };
            link.Source = link;
            return link;
        }

        /// <summary>
        /// Creates an outgoing self-linker where the linker references itself.
        /// Pattern: (link, link, target)
        /// </summary>
        /// <param name="target">The target link (can be null for initialization).</param>
        /// <returns>A new link that references itself as both source and linker.</returns>
        public static Link CreateOutcomingSelfLinker(Link? target)
        {
            var link = new Link
            {
                Target = target
            };
            link.Source = link;
            link.Linker = link;
            return link;
        }

        /// <summary>
        /// Creates an incoming self-link where the target references itself.
        /// Pattern: (source, linker, link)
        /// </summary>
        /// <param name="source">The source link.</param>
        /// <param name="linker">The linker link.</param>
        /// <returns>A new link that references itself as target.</returns>
        public static Link CreateIncomingSelfLink(Link source, Link linker)
        {
            var link = new Link
            {
                Source = source,
                Linker = linker
            };
            link.Target = link;
            return link;
        }

        /// <summary>
        /// Creates a self-linker link where the linker references itself.
        /// Pattern: (source, link, target)
        /// </summary>
        /// <param name="source">The source link.</param>
        /// <param name="target">The target link.</param>
        /// <returns>A new link that references itself as linker.</returns>
        public static Link CreateSelfLinker(Link source, Link target)
        {
            var link = new Link
            {
                Source = source,
                Target = target
            };
            link.Linker = link;
            return link;
        }

        /// <summary>
        /// Creates a cycle self-link where both source and target reference the link itself.
        /// Pattern: (link, linker, link)
        /// </summary>
        /// <param name="linker">The linker link.</param>
        /// <returns>A new link that references itself as both source and target.</returns>
        public static Link CreateCycleSelfLink(Link linker)
        {
            var link = new Link
            {
                Linker = linker
            };
            link.Source = link;
            link.Target = link;
            return link;
        }

        /// <summary>
        /// Creates a link that references itself in all three positions.
        /// Pattern: (link, link, link)
        /// </summary>
        /// <returns>A new link that references itself as source, linker, and target.</returns>
        public static Link CreateLinkLinkingItself()
        {
            var link = new Link();
            link.Source = link;
            link.Linker = link;
            link.Target = link;
            return link;
        }

        /// <summary>
        /// Tries to find an existing link with the specified components.
        /// Searches through the target's referers for efficiency.
        /// </summary>
        /// <param name="source">The source link.</param>
        /// <param name="linker">The linker link.</param>
        /// <param name="target">The target link.</param>
        /// <returns>An existing link if found, otherwise null.</returns>
        private static Link? TryFindExistingLink(Link source, Link linker, Link target)
        {
            bool IsEqual(Link link) =>
                link.Source == source &&
                link.Linker == linker &&
                link.Target == target;

            return target.ReferersByTarget.FirstOrDefault(IsEqual);
        }
    }
}
