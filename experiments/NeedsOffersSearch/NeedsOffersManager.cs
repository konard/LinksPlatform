using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinksPlatform.NeedsOffersSearch
{
    /// <summary>
    /// Central manager for handling needs and offers, coordinating search across multiple providers
    /// </summary>
    public class NeedsOffersManager
    {
        private readonly Dictionary<string, NeedOffer> _listings;
        private readonly List<ISearchProvider> _searchProviders;
        private readonly Dictionary<string, List<string>> _activeSubscriptions; // listingId -> subscriptionIds

        public NeedsOffersManager()
        {
            _listings = new Dictionary<string, NeedOffer>();
            _searchProviders = new List<ISearchProvider>();
            _activeSubscriptions = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Registers a search provider
        /// </summary>
        public void AddSearchProvider(ISearchProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _searchProviders.Add(provider);
        }

        /// <summary>
        /// Registers a new need or offer
        /// </summary>
        public async Task<NeedOffer> RegisterListingAsync(NeedOffer listing)
        {
            if (listing == null)
            {
                throw new ArgumentNullException(nameof(listing));
            }

            _listings[listing.Id] = listing;

            // If search is enabled, start searching automatically
            if (listing.EnableSearch)
            {
                await StartSearchingAsync(listing.Id);
            }

            return listing;
        }

        /// <summary>
        /// Updates an existing listing
        /// </summary>
        public async Task<NeedOffer> UpdateListingAsync(NeedOffer listing)
        {
            if (listing == null)
            {
                throw new ArgumentNullException(nameof(listing));
            }

            if (!_listings.ContainsKey(listing.Id))
            {
                throw new InvalidOperationException($"Listing with ID {listing.Id} not found.");
            }

            listing.UpdatedAt = DateTime.UtcNow;
            _listings[listing.Id] = listing;

            // Manage search subscriptions based on EnableSearch flag
            if (listing.EnableSearch && !_activeSubscriptions.ContainsKey(listing.Id))
            {
                await StartSearchingAsync(listing.Id);
            }
            else if (!listing.EnableSearch && _activeSubscriptions.ContainsKey(listing.Id))
            {
                await StopSearchingAsync(listing.Id);
            }

            return listing;
        }

        /// <summary>
        /// Removes a listing
        /// </summary>
        public async Task RemoveListingAsync(string listingId)
        {
            if (!_listings.ContainsKey(listingId))
            {
                throw new InvalidOperationException($"Listing with ID {listingId} not found.");
            }

            // Stop any active searches
            if (_activeSubscriptions.ContainsKey(listingId))
            {
                await StopSearchingAsync(listingId);
            }

            _listings.Remove(listingId);
        }

        /// <summary>
        /// Gets a listing by ID
        /// </summary>
        public NeedOffer GetListing(string listingId)
        {
            return _listings.TryGetValue(listingId, out var listing) ? listing : null;
        }

        /// <summary>
        /// Gets all listings
        /// </summary>
        public IEnumerable<NeedOffer> GetAllListings()
        {
            return _listings.Values.ToList();
        }

        /// <summary>
        /// Performs a one-time search for a specific listing across all providers
        /// </summary>
        public async Task<IEnumerable<SearchResult>> SearchOnceAsync(string listingId, int maxResultsPerProvider = 10)
        {
            if (!_listings.TryGetValue(listingId, out var listing))
            {
                throw new InvalidOperationException($"Listing with ID {listingId} not found.");
            }

            var allResults = new List<SearchResult>();
            var searchQuery = BuildSearchQuery(listing);

            foreach (var provider in _searchProviders.Where(p => p.IsConfigured))
            {
                try
                {
                    var results = await provider.SearchAsync(searchQuery, maxResultsPerProvider);
                    allResults.AddRange(results);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching with provider {provider.Name}: {ex.Message}");
                }
            }

            return allResults.OrderByDescending(r => r.RelevanceScore);
        }

        /// <summary>
        /// Starts continuous search for a listing
        /// </summary>
        public async Task StartSearchingAsync(string listingId)
        {
            if (!_listings.TryGetValue(listingId, out var listing))
            {
                throw new InvalidOperationException($"Listing with ID {listingId} not found.");
            }

            if (_activeSubscriptions.ContainsKey(listingId))
            {
                // Already searching
                return;
            }

            var subscriptionIds = new List<string>();
            var searchQuery = BuildSearchQuery(listing);

            foreach (var provider in _searchProviders.Where(p => p.IsConfigured))
            {
                try
                {
                    var subscriptionId = await provider.SubscribeAsync(
                        searchQuery,
                        result => OnSearchResultFound(listingId, result)
                    );
                    subscriptionIds.Add(subscriptionId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error subscribing with provider {provider.Name}: {ex.Message}");
                }
            }

            _activeSubscriptions[listingId] = subscriptionIds;
        }

        /// <summary>
        /// Stops continuous search for a listing
        /// </summary>
        public async Task StopSearchingAsync(string listingId)
        {
            if (!_activeSubscriptions.TryGetValue(listingId, out var subscriptionIds))
            {
                return;
            }

            foreach (var provider in _searchProviders)
            {
                foreach (var subscriptionId in subscriptionIds)
                {
                    try
                    {
                        await provider.UnsubscribeAsync(subscriptionId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error unsubscribing from provider {provider.Name}: {ex.Message}");
                    }
                }
            }

            _activeSubscriptions.Remove(listingId);
        }

        /// <summary>
        /// Event fired when a search result is found
        /// </summary>
        public event EventHandler<SearchResultEventArgs> SearchResultFound;

        /// <summary>
        /// Builds a search query from a listing
        /// </summary>
        private string BuildSearchQuery(NeedOffer listing)
        {
            var keywords = string.Join(" ", listing.Keywords);
            return $"{listing.Description} {keywords}".Trim();
        }

        /// <summary>
        /// Handles when a search result is found
        /// </summary>
        private void OnSearchResultFound(string listingId, SearchResult result)
        {
            SearchResultFound?.Invoke(this, new SearchResultEventArgs
            {
                ListingId = listingId,
                Result = result
            });
        }
    }

    /// <summary>
    /// Event args for search results
    /// </summary>
    public class SearchResultEventArgs : EventArgs
    {
        public string ListingId { get; set; }
        public SearchResult Result { get; set; }
    }
}
