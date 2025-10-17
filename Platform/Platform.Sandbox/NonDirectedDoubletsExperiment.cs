using System;
using System.Collections.Generic;
using Platform.Data;

namespace Platform.Sandbox
{
    /// <summary>
    /// <para>
    /// Experimental implementation of non-directed doublets.
    /// </para>
    /// <para>
    /// In non-directed doublets, both references (Source and Target) are equivalent
    /// and carry no additional meaning beyond connecting two elements.
    /// </para>
    /// <para>
    /// Key differences from directed doublets:
    /// - Query (A, B) returns both (A, B) and (B, A) representations
    /// - Creating (A, B) automatically considers (B, A) as the same link
    /// - Links are stored in normalized form (e.g., min value first) to avoid duplicates
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class NonDirectedDoublets<TLinkAddress>
        where TLinkAddress : struct, IComparable<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress, LinksConstants<TLinkAddress>> _innerLinks;
        private readonly IComparer<TLinkAddress> _comparer;

        /// <summary>
        /// <para>
        /// Gets the constants for this links storage.
        /// </para>
        /// </summary>
        public LinksConstants<TLinkAddress> Constants => _innerLinks.Constants;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="NonDirectedDoublets{TLinkAddress}"/> class.
        /// </para>
        /// </summary>
        /// <param name="innerLinks">The underlying directed links implementation.</param>
        public NonDirectedDoublets(ILinks<TLinkAddress, LinksConstants<TLinkAddress>> innerLinks)
        {
            _innerLinks = innerLinks ?? throw new ArgumentNullException(nameof(innerLinks));
            _comparer = Comparer<TLinkAddress>.Default;
        }

        /// <summary>
        /// <para>
        /// Normalizes a link by ensuring the smaller address comes first.
        /// This ensures (A, B) and (B, A) are treated as the same link.
        /// </para>
        /// </summary>
        private (TLinkAddress first, TLinkAddress second) Normalize(TLinkAddress source, TLinkAddress target)
        {
            // For non-directed links, we store them in normalized form
            // where the smaller address is always first
            if (_comparer.Compare(source, target) <= 0)
            {
                return (source, target);
            }
            else
            {
                return (target, source);
            }
        }

        /// <summary>
        /// <para>
        /// Creates a new non-directed link.
        /// </para>
        /// </summary>
        public TLinkAddress Create()
        {
            return _innerLinks.Create();
        }

        /// <summary>
        /// <para>
        /// Creates or updates a non-directed link between two addresses.
        /// The link is stored in normalized form (smaller address first).
        /// </para>
        /// </summary>
        public TLinkAddress Update(TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget)
        {
            var (first, second) = Normalize(newSource, newTarget);
            return _innerLinks.Update(link, first, second);
        }

        /// <summary>
        /// <para>
        /// Deletes a non-directed link.
        /// </para>
        /// </summary>
        public void Delete(TLinkAddress link)
        {
            _innerLinks.Delete(link);
        }

        /// <summary>
        /// <para>
        /// Searches for links matching the specified restrictions.
        /// For non-directed links, when source and target are specified,
        /// this searches for both (source, target) and (target, source).
        /// </para>
        /// </summary>
        public TLinkAddress SearchOrDefault(TLinkAddress source, TLinkAddress target)
        {
            var any = Constants.Any;

            // If both source and target are specified
            if (!EqualityComparer<TLinkAddress>.Default.Equals(source, any) &&
                !EqualityComparer<TLinkAddress>.Default.Equals(target, any))
            {
                // First try normalized form
                var (first, second) = Normalize(source, target);
                var result = _innerLinks.SearchOrDefault(first, second);

                // If not found and source != target, the normalized form is the only form
                // No need to search reverse as we store normalized
                return result;
            }
            else
            {
                // For partial queries, delegate to inner implementation
                return _innerLinks.SearchOrDefault(source, target);
            }
        }

        /// <summary>
        /// <para>
        /// Enumerates all links or links matching the specified handler.
        /// For non-directed links, each link is reported in its stored (normalized) form.
        /// </para>
        /// </summary>
        public TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, IList<TLinkAddress> restrictions)
        {
            // For non-directed doublets, we can delegate to the inner implementation
            // since links are already stored in normalized form
            return _innerLinks.Each(handler, restrictions);
        }

        /// <summary>
        /// <para>
        /// Counts the number of links.
        /// </para>
        /// </summary>
        public TLinkAddress Count(IList<TLinkAddress> restrictions)
        {
            return _innerLinks.Count(restrictions);
        }
    }
}
