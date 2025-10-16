using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Implements persistent search and goal completion assist functionality.
    /// Allows users to register search requests that continue until the goal is achieved.
    /// </summary>
    /// <remarks>
    /// Based on issue #122: Constant search concept or goal completion assist.
    /// This class provides a subscription-based search system where:
    /// - Searches persist until explicitly cancelled or satisfied
    /// - Users are notified when new results appear
    /// - System assists in goal completion through continuous monitoring
    /// </remarks>
    public class PersistentSearch<TLinkAddress> where TLinkAddress : struct, IEquatable<TLinkAddress>, IComparable<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _links;
        private readonly Dictionary<TLinkAddress, SearchSubscription> _subscriptions;
        private readonly object _lock = new object();

        public PersistentSearch(ILinks<TLinkAddress> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _subscriptions = new Dictionary<TLinkAddress, SearchSubscription>();
        }

        /// <summary>
        /// Subscribe to continuous search results for a given query.
        /// </summary>
        /// <param name="userId">The user ID registering the search</param>
        /// <param name="searchQuery">The search query/pattern</param>
        /// <param name="onResultFound">Callback when new results are found</param>
        /// <param name="onGoalComplete">Optional callback when search goal is completed</param>
        /// <returns>Subscription ID that can be used to cancel the search</returns>
        public TLinkAddress Subscribe(TLinkAddress userId, TLinkAddress[] searchQuery,
            Action<TLinkAddress> onResultFound, Action onGoalComplete = null)
        {
            if (searchQuery == null || searchQuery.Length == 0)
                throw new ArgumentException("Search query cannot be empty", nameof(searchQuery));

            lock (_lock)
            {
                // Create a subscription link in the database
                // Structure: [User] -> [HasSubscription] -> [SearchQuery]
                var subscriptionId = CreateSubscriptionLink(userId, searchQuery);

                var subscription = new SearchSubscription
                {
                    SubscriptionId = subscriptionId,
                    UserId = userId,
                    SearchQuery = searchQuery,
                    OnResultFound = onResultFound,
                    OnGoalComplete = onGoalComplete,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    FoundResults = new HashSet<TLinkAddress>()
                };

                _subscriptions[subscriptionId] = subscription;
                return subscriptionId;
            }
        }

        /// <summary>
        /// Unsubscribe from a persistent search.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to cancel</param>
        /// <returns>True if successfully unsubscribed, false otherwise</returns>
        public bool Unsubscribe(TLinkAddress subscriptionId)
        {
            lock (_lock)
            {
                if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
                {
                    subscription.IsActive = false;
                    _subscriptions.Remove(subscriptionId);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Mark a search as satisfied/complete (user confirms goal is achieved).
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to mark as satisfied</param>
        /// <param name="satisfiedWith">Optional result link that satisfied the user</param>
        public void MarkAsSatisfied(TLinkAddress subscriptionId, TLinkAddress? satisfiedWith = null)
        {
            lock (_lock)
            {
                if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
                {
                    subscription.IsActive = false;
                    subscription.SatisfiedAt = DateTime.UtcNow;
                    subscription.SatisfiedWith = satisfiedWith;

                    subscription.OnGoalComplete?.Invoke();
                    _subscriptions.Remove(subscriptionId);
                }
            }
        }

        /// <summary>
        /// Check all active subscriptions against newly added/modified links.
        /// Call this when database changes occur.
        /// </summary>
        /// <param name="modifiedLink">The link that was added or modified</param>
        public void CheckSubscriptions(TLinkAddress modifiedLink)
        {
            lock (_lock)
            {
                foreach (var subscription in _subscriptions.Values.Where(s => s.IsActive).ToList())
                {
                    // Check if the modified link matches the search criteria
                    if (MatchesQuery(modifiedLink, subscription.SearchQuery))
                    {
                        // Only notify if this is a new result
                        if (!subscription.FoundResults.Contains(modifiedLink))
                        {
                            subscription.FoundResults.Add(modifiedLink);
                            subscription.OnResultFound?.Invoke(modifiedLink);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all active subscriptions for a user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of active subscription IDs</returns>
        public IReadOnlyList<TLinkAddress> GetUserSubscriptions(TLinkAddress userId)
        {
            lock (_lock)
            {
                return _subscriptions.Values
                    .Where(s => s.IsActive && s.UserId.Equals(userId))
                    .Select(s => s.SubscriptionId)
                    .ToList();
            }
        }

        /// <summary>
        /// Get information about a specific subscription.
        /// </summary>
        public SearchSubscription GetSubscriptionInfo(TLinkAddress subscriptionId)
        {
            lock (_lock)
            {
                return _subscriptions.TryGetValue(subscriptionId, out var subscription)
                    ? subscription
                    : null;
            }
        }

        private TLinkAddress CreateSubscriptionLink(TLinkAddress userId, TLinkAddress[] searchQuery)
        {
            // Create a link representing the subscription
            // This is a simplified implementation - real implementation would use proper sequence creation
            var querySequence = _links.Create();
            var subscription = _links.Create();
            return subscription;
        }

        private bool MatchesQuery(TLinkAddress link, TLinkAddress[] query)
        {
            // Simplified matching - in real implementation this would use
            // proper sequence matching similar to MasterServer.Search
            try
            {
                var source = _links.GetSource(link);
                var target = _links.GetTarget(link);

                // Check if any query element matches source or target
                foreach (var queryElement in query)
                {
                    if (queryElement.Equals(source) || queryElement.Equals(target))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Link might not exist or be invalid
                return false;
            }

            return false;
        }

        /// <summary>
        /// Represents an active search subscription.
        /// </summary>
        public class SearchSubscription
        {
            public TLinkAddress SubscriptionId { get; set; }
            public TLinkAddress UserId { get; set; }
            public TLinkAddress[] SearchQuery { get; set; }
            public Action<TLinkAddress> OnResultFound { get; set; }
            public Action OnGoalComplete { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? SatisfiedAt { get; set; }
            public TLinkAddress? SatisfiedWith { get; set; }
            public bool IsActive { get; set; }
            public HashSet<TLinkAddress> FoundResults { get; set; }
        }
    }
}
