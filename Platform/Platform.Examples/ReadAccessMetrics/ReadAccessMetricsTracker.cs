using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Platform.Examples.ReadAccessMetrics
{
    /// <summary>
    /// Tracks read access frequency metrics for links.
    /// Implements both variant 1 (incremental average) and variant 2 (formula-based average).
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link addresses.</typeparam>
    public class ReadAccessMetricsTracker<TLinkAddress> : IReadAccessMetrics<TLinkAddress>
    {
        private readonly ConcurrentDictionary<TLinkAddress, LinkReadAccessMetrics> _metricsStorage;

        /// <summary>
        /// Initializes a new instance of the ReadAccessMetricsTracker class.
        /// </summary>
        public ReadAccessMetricsTracker()
        {
            _metricsStorage = new ConcurrentDictionary<TLinkAddress, LinkReadAccessMetrics>();
        }

        /// <summary>
        /// Records a read access event for a specific link.
        /// </summary>
        /// <param name="linkAddress">The address of the link that was read.</param>
        public void RecordReadAccess(TLinkAddress linkAddress)
        {
            var metrics = _metricsStorage.GetOrAdd(linkAddress, _ => new LinkReadAccessMetrics());
            metrics.RecordReadAccess();
        }

        /// <summary>
        /// Records multiple read access events.
        /// </summary>
        /// <param name="linkAddresses">The addresses of the links that were read.</param>
        public void RecordReadAccess(IEnumerable<TLinkAddress> linkAddresses)
        {
            foreach (var linkAddress in linkAddresses)
            {
                RecordReadAccess(linkAddress);
            }
        }

        /// <summary>
        /// Registers a newly created link with the tracker.
        /// </summary>
        /// <param name="linkAddress">The address of the newly created link.</param>
        public void RegisterCreatedLink(TLinkAddress linkAddress)
        {
            _metricsStorage.TryAdd(linkAddress, new LinkReadAccessMetrics());
        }

        /// <summary>
        /// Removes metrics for a deleted link.
        /// </summary>
        /// <param name="linkAddress">The address of the deleted link.</param>
        public void UnregisterDeletedLink(TLinkAddress linkAddress)
        {
            _metricsStorage.TryRemove(linkAddress, out _);
        }

        // IReadAccessMetrics implementation

        /// <summary>
        /// Gets the average read access time using variant 1 (incremental calculation).
        /// </summary>
        public TimeSpan? GetAverageReadAccessTime(TLinkAddress linkAddress)
        {
            if (_metricsStorage.TryGetValue(linkAddress, out var metrics))
            {
                return metrics.AverageReadAccessTime;
            }
            return null;
        }

        /// <summary>
        /// Gets the last read access time.
        /// </summary>
        public DateTime? GetLastReadAccessTime(TLinkAddress linkAddress)
        {
            if (_metricsStorage.TryGetValue(linkAddress, out var metrics))
            {
                return metrics.LastReadAccessTime;
            }
            return null;
        }

        /// <summary>
        /// Gets the creation date and time.
        /// </summary>
        public DateTime? GetCreationDateTime(TLinkAddress linkAddress)
        {
            if (_metricsStorage.TryGetValue(linkAddress, out var metrics))
            {
                return metrics.CreationDateTime;
            }
            return null;
        }

        /// <summary>
        /// Gets the total read access count.
        /// </summary>
        public long GetTotalReadAccessCount(TLinkAddress linkAddress)
        {
            if (_metricsStorage.TryGetValue(linkAddress, out var metrics))
            {
                return metrics.TotalReadAccessCount;
            }
            return 0;
        }

        /// <summary>
        /// Calculates the average read access time using variant 2 formula.
        /// Formula: (Now - CreationDateTime) / TotalReadAccessCount
        /// </summary>
        public TimeSpan? GetAverageReadAccessTimeVariant2(TLinkAddress linkAddress)
        {
            if (_metricsStorage.TryGetValue(linkAddress, out var metrics))
            {
                return metrics.CalculateAverageReadAccessTimeVariant2();
            }
            return null;
        }

        /// <summary>
        /// Gets all tracked link addresses.
        /// </summary>
        public IEnumerable<TLinkAddress> GetTrackedLinks()
        {
            return _metricsStorage.Keys;
        }

        /// <summary>
        /// Clears all metrics.
        /// </summary>
        public void Clear()
        {
            _metricsStorage.Clear();
        }
    }
}
