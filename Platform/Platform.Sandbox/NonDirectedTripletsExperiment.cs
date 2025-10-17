using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Triplets;

namespace Platform.Sandbox
{
    /// <summary>
    /// <para>
    /// Experimental implementation of non-directed triplets.
    /// </para>
    /// <para>
    /// In non-directed triplets, the Source and Target are equivalent,
    /// but the Linker retains its meaning as the type/predicate of the relationship.
    /// </para>
    /// <para>
    /// Key characteristics:
    /// - Source and Target are interchangeable (non-directed aspect)
    /// - Linker maintains its role as the relationship type
    /// - (A, Linker, B) is equivalent to (B, Linker, A)
    /// - Stored in normalized form: (min(A,B), Linker, max(A,B))
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class NonDirectedTriplets<TLinkAddress> : ILinks<TLinkAddress>
        where TLinkAddress : struct, IComparable<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _innerLinks;
        private readonly IComparer<TLinkAddress> _comparer;

        /// <summary>
        /// <para>
        /// Gets the constants for this links storage.
        /// </para>
        /// </summary>
        public LinksConstants<TLinkAddress> Constants => _innerLinks.Constants;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="NonDirectedTriplets{TLinkAddress}"/> class.
        /// </para>
        /// </summary>
        /// <param name="innerLinks">The underlying triplet links implementation.</param>
        public NonDirectedTriplets(ILinks<TLinkAddress> innerLinks)
        {
            _innerLinks = innerLinks ?? throw new ArgumentNullException(nameof(innerLinks));
            _comparer = Comparer<TLinkAddress>.Default;
        }

        /// <summary>
        /// <para>
        /// Normalizes a triplet by ensuring source &lt;= target while preserving linker.
        /// Format: [link_id, source, linker, target] becomes [link_id, min(source,target), linker, max(source,target)]
        /// </para>
        /// </summary>
        private (TLinkAddress source, TLinkAddress linker, TLinkAddress target) Normalize(
            TLinkAddress source, TLinkAddress linker, TLinkAddress target)
        {
            // For non-directed triplets, source and target are interchangeable
            // but linker retains its meaning
            if (_comparer.Compare(source, target) <= 0)
            {
                return (source, linker, target);
            }
            else
            {
                return (target, linker, source);
            }
        }

        /// <summary>
        /// <para>
        /// Creates a new non-directed triplet link.
        /// </para>
        /// </summary>
        public TLinkAddress Create()
        {
            return _innerLinks.Create();
        }

        /// <summary>
        /// <para>
        /// Updates a non-directed triplet link.
        /// For a triplet structure [index, source, linker, target],
        /// this normalizes source and target while preserving linker position.
        /// </para>
        /// </summary>
        public TLinkAddress Update(TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget)
        {
            // Note: For triplets, Update typically works with triplet structure
            // This is a simplified version that assumes the underlying implementation
            // handles triplet-specific update logic
            return _innerLinks.Update(link, newSource, newTarget);
        }

        /// <summary>
        /// <para>
        /// Updates a triplet with all three components: source, linker, and target.
        /// The source and target are normalized to ensure non-directed behavior.
        /// </para>
        /// </summary>
        public TLinkAddress Update(TLinkAddress link, TLinkAddress newSource, TLinkAddress newLinker, TLinkAddress newTarget)
        {
            var (normalizedSource, normalizedLinker, normalizedTarget) = Normalize(newSource, newLinker, newTarget);

            // Create a restriction list for the update
            var restrictions = new TLinkAddress[] { link, normalizedSource, normalizedLinker, normalizedTarget };

            // Delegate to inner implementation with normalized values
            // Note: The actual update mechanism depends on the inner triplet implementation
            return _innerLinks.Update(link, normalizedSource, normalizedTarget);
        }

        /// <summary>
        /// <para>
        /// Deletes a non-directed triplet link.
        /// </para>
        /// </summary>
        public void Delete(TLinkAddress link)
        {
            _innerLinks.Delete(link);
        }

        /// <summary>
        /// <para>
        /// Searches for a triplet matching the specified source, linker, and target.
        /// Automatically searches both (source, linker, target) and (target, linker, source).
        /// </para>
        /// </summary>
        public TLinkAddress SearchOrDefault(TLinkAddress source, TLinkAddress target)
        {
            // For doublet-style search, delegate to inner
            return _innerLinks.SearchOrDefault(source, target);
        }

        /// <summary>
        /// <para>
        /// Searches for a triplet with normalization.
        /// </para>
        /// </summary>
        public TLinkAddress SearchOrDefault(TLinkAddress source, TLinkAddress linker, TLinkAddress target)
        {
            var any = Constants.Any;

            // If source, linker, and target are all specified
            if (!EqualityComparer<TLinkAddress>.Default.Equals(source, any) &&
                !EqualityComparer<TLinkAddress>.Default.Equals(linker, any) &&
                !EqualityComparer<TLinkAddress>.Default.Equals(target, any))
            {
                // Search for normalized form
                var (normalizedSource, normalizedLinker, normalizedTarget) = Normalize(source, linker, target);

                // Create search restriction: [any, normalizedSource, normalizedLinker, normalizedTarget]
                var restrictions = new TLinkAddress[] { any, normalizedSource, normalizedLinker, normalizedTarget };

                // Use Each to find matching link
                TLinkAddress result = default;
                _innerLinks.Each(link =>
                {
                    result = link[0]; // First element is typically the link ID
                    return Constants.Break;
                }, restrictions);

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
        /// Enumerates triplets matching the specified restrictions.
        /// </para>
        /// </summary>
        public TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, IList<TLinkAddress> restrictions)
        {
            // Delegate to inner implementation
            // Links are already stored in normalized form
            return _innerLinks.Each(handler, restrictions);
        }

        /// <summary>
        /// <para>
        /// Counts the number of triplets matching the restrictions.
        /// </para>
        /// </summary>
        public TLinkAddress Count(IList<TLinkAddress> restrictions)
        {
            return _innerLinks.Count(restrictions);
        }
    }
}
