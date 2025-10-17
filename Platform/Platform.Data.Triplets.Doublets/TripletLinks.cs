#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Data.Triplets.Doublets
{
    /// <summary>
    /// Represents a Triplet links implementation built on top of Doublets Sequences.
    /// Each triplet (Source, Linker, Target) is stored as a sequence of exactly 3 elements.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type used for link addresses.</typeparam>
    public class TripletLinks<TLinkAddress> : ILinks<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _doublets;
        private const int TripletSize = 3;

        /// <summary>
        /// Gets the constants used by the underlying doublets storage.
        /// </summary>
        public LinksConstants<TLinkAddress> Constants => _doublets.Constants;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripletLinks{TLinkAddress}"/> class.
        /// </summary>
        /// <param name="doublets">The underlying doublets storage.</param>
        public TripletLinks(ILinks<TLinkAddress> doublets)
        {
            _doublets = doublets ?? throw new ArgumentNullException(nameof(doublets));
        }

        /// <summary>
        /// Creates a new triplet with the specified source, linker, and target.
        /// </summary>
        /// <param name="source">The source link.</param>
        /// <param name="linker">The linker (predicate/verb) link.</param>
        /// <param name="target">The target link.</param>
        /// <returns>The address of the created triplet.</returns>
        public TLinkAddress CreateTriplet(TLinkAddress source, TLinkAddress linker, TLinkAddress target)
        {
            // Triplet structure: (Source, (Linker, Target))
            // This creates a binary tree representing the 3-element sequence
            var rightPair = _doublets.GetOrCreate(linker, target);
            return _doublets.GetOrCreate(source, rightPair);
        }

        /// <summary>
        /// Gets the source component of a triplet.
        /// </summary>
        /// <param name="triplet">The triplet address.</param>
        /// <returns>The source link address.</returns>
        public TLinkAddress GetSource(TLinkAddress triplet)
        {
            var link = _doublets.GetLink(triplet);
            if (link == null)
            {
                throw new ArgumentException($"Link {triplet} does not exist.", nameof(triplet));
            }
            return link[Constants.SourcePart];
        }

        /// <summary>
        /// Gets the linker (predicate) component of a triplet.
        /// </summary>
        /// <param name="triplet">The triplet address.</param>
        /// <returns>The linker link address.</returns>
        public TLinkAddress GetLinker(TLinkAddress triplet)
        {
            var link = _doublets.GetLink(triplet);
            if (link == null)
            {
                throw new ArgumentException($"Link {triplet} does not exist.", nameof(triplet));
            }

            var rightPair = link[Constants.TargetPart];
            var rightLink = _doublets.GetLink(rightPair);
            if (rightLink == null)
            {
                throw new InvalidOperationException($"Invalid triplet structure at {triplet}.");
            }

            return rightLink[Constants.SourcePart];
        }

        /// <summary>
        /// Gets the target component of a triplet.
        /// </summary>
        /// <param name="triplet">The triplet address.</param>
        /// <returns>The target link address.</returns>
        public TLinkAddress GetTarget(TLinkAddress triplet)
        {
            var link = _doublets.GetLink(triplet);
            if (link == null)
            {
                throw new ArgumentException($"Link {triplet} does not exist.", nameof(triplet));
            }

            var rightPair = link[Constants.TargetPart];
            var rightLink = _doublets.GetLink(rightPair);
            if (rightLink == null)
            {
                throw new InvalidOperationException($"Invalid triplet structure at {triplet}.");
            }

            return rightLink[Constants.TargetPart];
        }

        /// <summary>
        /// Gets the triplet as a structured array [Source, Linker, Target].
        /// </summary>
        /// <param name="triplet">The triplet address.</param>
        /// <returns>Array containing [Source, Linker, Target].</returns>
        public TLinkAddress[] GetTriplet(TLinkAddress triplet)
        {
            var link = _doublets.GetLink(triplet);
            if (link == null)
            {
                throw new ArgumentException($"Link {triplet} does not exist.", nameof(triplet));
            }

            var source = link[Constants.SourcePart];
            var rightPair = link[Constants.TargetPart];

            var rightLink = _doublets.GetLink(rightPair);
            if (rightLink == null)
            {
                throw new InvalidOperationException($"Invalid triplet structure at {triplet}.");
            }

            var linker = rightLink[Constants.SourcePart];
            var target = rightLink[Constants.TargetPart];

            return new TLinkAddress[] { source, linker, target };
        }

        /// <summary>
        /// Updates an existing triplet.
        /// </summary>
        /// <param name="triplet">The triplet address to update.</param>
        /// <param name="newSource">The new source link.</param>
        /// <param name="newLinker">The new linker link.</param>
        /// <param name="newTarget">The new target link.</param>
        /// <returns>The address of the updated triplet.</returns>
        public TLinkAddress UpdateTriplet(TLinkAddress triplet, TLinkAddress newSource, TLinkAddress newLinker, TLinkAddress newTarget)
        {
            // Delete the old triplet and create a new one
            // This approach maintains sequence integrity
            DeleteTriplet(triplet);
            return CreateTriplet(newSource, newLinker, newTarget);
        }

        /// <summary>
        /// Deletes a triplet.
        /// </summary>
        /// <param name="triplet">The triplet address to delete.</param>
        public void DeleteTriplet(TLinkAddress triplet)
        {
            var link = _doublets.GetLink(triplet);
            if (link != null)
            {
                var rightPair = link[Constants.TargetPart];
                _doublets.Delete(triplet);
                // Try to delete the right pair if it's not used elsewhere
                try
                {
                    _doublets.Delete(rightPair);
                }
                catch
                {
                    // Ignore if still referenced
                }
            }
        }

        /// <summary>
        /// Checks if a given link is a valid triplet (has correct structure).
        /// </summary>
        /// <param name="link">The link to check.</param>
        /// <returns>True if the link is a valid triplet, false otherwise.</returns>
        public bool IsTriplet(TLinkAddress link)
        {
            try
            {
                var linkData = _doublets.GetLink(link);
                if (linkData == null)
                {
                    return false;
                }

                var rightPair = linkData[Constants.TargetPart];
                var rightLink = _doublets.GetLink(rightPair);

                // A triplet has a valid right pair structure
                return rightLink != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Searches for triplets matching the specified pattern.
        /// </summary>
        /// <param name="source">The source to match, or Any for wildcard.</param>
        /// <param name="linker">The linker to match, or Any for wildcard.</param>
        /// <param name="target">The target to match, or Any for wildcard.</param>
        /// <param name="handler">Handler to process each matching triplet.</param>
        /// <returns>The result of the search operation.</returns>
        public TLinkAddress SearchTriplets(TLinkAddress source, TLinkAddress linker, TLinkAddress target, Func<IList<TLinkAddress>, TLinkAddress> handler)
        {
            var any = Constants.Any;
            TLinkAddress result = Constants.Continue;

            _doublets.Each(link =>
            {
                try
                {
                    var triplet = GetTriplet(link[Constants.IndexPart]);

                    bool sourceMatches = EqualityComparer<TLinkAddress>.Default.Equals(source, any) ||
                                         EqualityComparer<TLinkAddress>.Default.Equals(triplet[0], source);
                    bool linkerMatches = EqualityComparer<TLinkAddress>.Default.Equals(linker, any) ||
                                         EqualityComparer<TLinkAddress>.Default.Equals(triplet[1], linker);
                    bool targetMatches = EqualityComparer<TLinkAddress>.Default.Equals(target, any) ||
                                         EqualityComparer<TLinkAddress>.Default.Equals(triplet[2], target);

                    if (sourceMatches && linkerMatches && targetMatches)
                    {
                        result = handler(new TLinkAddress[] { link[Constants.IndexPart], triplet[0], triplet[1], triplet[2] });
                        if (EqualityComparer<TLinkAddress>.Default.Equals(result, Constants.Break))
                        {
                            return Constants.Break;
                        }
                    }
                }
                catch
                {
                    // Not a valid triplet, skip
                }
                return Constants.Continue;
            }, any);

            return result;
        }

        /// <summary>
        /// Counts triplets matching the specified pattern.
        /// </summary>
        /// <param name="source">The source to match, or Any for wildcard.</param>
        /// <param name="linker">The linker to match, or Any for wildcard.</param>
        /// <param name="target">The target to match, or Any for wildcard.</param>
        /// <returns>The count of matching triplets.</returns>
        public long CountTriplets(TLinkAddress source, TLinkAddress linker, TLinkAddress target)
        {
            long count = 0;
            SearchTriplets(source, linker, target, _ => { count++; return Constants.Continue; });
            return count;
        }

        #region ILinks<TLinkAddress> Implementation

        /// <summary>
        /// Creates a new link (triplet) with the specified substitution.
        /// </summary>
        /// <param name="substitution">The substitution containing [Source, Linker, Target].</param>
        /// <returns>The address of the created triplet.</returns>
        public TLinkAddress Create(IList<TLinkAddress>? substitution = null)
        {
            if (substitution == null || substitution.Count != TripletSize)
            {
                throw new ArgumentException($"Substitution must contain exactly {TripletSize} elements [Source, Linker, Target].", nameof(substitution));
            }

            return CreateTriplet(substitution[0], substitution[1], substitution[2]);
        }

        /// <summary>
        /// Updates a link (triplet) matching the restriction with new values from substitution.
        /// </summary>
        /// <param name="restriction">The restriction to match the triplet.</param>
        /// <param name="substitution">The new values [Source, Linker, Target].</param>
        /// <returns>The address of the updated triplet.</returns>
        public TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution)
        {
            if (restriction == null || restriction.Count == 0)
            {
                throw new ArgumentException("Restriction must be provided.", nameof(restriction));
            }

            if (substitution == null || substitution.Count != TripletSize)
            {
                throw new ArgumentException($"Substitution must contain exactly {TripletSize} elements [Source, Linker, Target].", nameof(substitution));
            }

            var tripletAddress = restriction[0];
            return UpdateTriplet(tripletAddress, substitution[0], substitution[1], substitution[2]);
        }

        /// <summary>
        /// Deletes a link (triplet) matching the restriction.
        /// </summary>
        /// <param name="restriction">The restriction to match the triplet.</param>
        public void Delete(IList<TLinkAddress>? restriction)
        {
            if (restriction == null || restriction.Count == 0)
            {
                throw new ArgumentException("Restriction must be provided.", nameof(restriction));
            }

            DeleteTriplet(restriction[0]);
        }

        /// <summary>
        /// Counts links (triplets) matching the restriction.
        /// </summary>
        /// <param name="restriction">The restriction pattern [Index/Any, Source/Any, Linker/Any, Target/Any].</param>
        /// <returns>The count of matching triplets.</returns>
        public TLinkAddress Count(IList<TLinkAddress>? restriction = null)
        {
            if (restriction == null || restriction.Count == 0)
            {
                // Count all triplets
                long count = 0;
                _doublets.Each(link =>
                {
                    if (IsTriplet(link[Constants.IndexPart]))
                    {
                        count++;
                    }
                    return Constants.Continue;
                }, Constants.Any);

                return (TLinkAddress)Convert.ChangeType(count, typeof(TLinkAddress));
            }

            var source = restriction.Count > 1 ? restriction[1] : Constants.Any;
            var linker = restriction.Count > 2 ? restriction[2] : Constants.Any;
            var target = restriction.Count > 3 ? restriction[3] : Constants.Any;

            var tripletCount = CountTriplets(source, linker, target);
            return (TLinkAddress)Convert.ChangeType(tripletCount, typeof(TLinkAddress));
        }

        /// <summary>
        /// Iterates through links (triplets) matching the restriction.
        /// </summary>
        /// <param name="restriction">The restriction pattern.</param>
        /// <param name="handler">Handler to process each matching triplet.</param>
        /// <returns>The result of the iteration.</returns>
        public TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, IList<TLinkAddress>? restriction = null)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (restriction == null || restriction.Count == 0)
            {
                // Iterate all triplets
                return _doublets.Each(link =>
                {
                    if (IsTriplet(link[Constants.IndexPart]))
                    {
                        try
                        {
                            var triplet = GetTriplet(link[Constants.IndexPart]);
                            return handler(new TLinkAddress[] { link[Constants.IndexPart], triplet[0], triplet[1], triplet[2] });
                        }
                        catch
                        {
                            // Skip invalid triplets
                        }
                    }
                    return Constants.Continue;
                }, Constants.Any);
            }

            var source = restriction.Count > 1 ? restriction[1] : Constants.Any;
            var linker = restriction.Count > 2 ? restriction[2] : Constants.Any;
            var target = restriction.Count > 3 ? restriction[3] : Constants.Any;

            return SearchTriplets(source, linker, target, handler);
        }

        #endregion
    }
}
