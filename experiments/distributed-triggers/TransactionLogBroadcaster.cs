using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Broadcasts transaction log events to all workers.
    /// Implements the distribution mechanism for transaction events.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class TransactionLogBroadcaster<TLinkAddress>
    {
        private readonly List<ITransactionLogSubscriber<TLinkAddress>> _subscribers;
        private readonly object _subscribersLock = new object();

        public TransactionLogBroadcaster()
        {
            _subscribers = new List<ITransactionLogSubscriber<TLinkAddress>>();
        }

        /// <summary>
        /// Subscribes a worker to receive transaction events.
        /// </summary>
        /// <param name="subscriber">The subscriber to add.</param>
        public void Subscribe(ITransactionLogSubscriber<TLinkAddress> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            lock (_subscribersLock)
            {
                if (!_subscribers.Contains(subscriber))
                {
                    _subscribers.Add(subscriber);
                }
            }
        }

        /// <summary>
        /// Unsubscribes a worker from receiving transaction events.
        /// </summary>
        /// <param name="subscriber">The subscriber to remove.</param>
        public void Unsubscribe(ITransactionLogSubscriber<TLinkAddress> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            lock (_subscribersLock)
            {
                _subscribers.Remove(subscriber);
            }
        }

        /// <summary>
        /// Broadcasts a transaction event to all subscribers in parallel.
        /// </summary>
        /// <param name="event">The transaction event to broadcast.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the broadcast operation.</returns>
        public async Task BroadcastAsync(TransactionEvent<TLinkAddress> @event, CancellationToken cancellationToken = default)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            ITransactionLogSubscriber<TLinkAddress>[] subscribersCopy;
            lock (_subscribersLock)
            {
                subscribersCopy = _subscribers.ToArray();
            }

            var tasks = new List<Task>(subscribersCopy.Length);
            foreach (var subscriber in subscribersCopy)
            {
                tasks.Add(Task.Run(() => subscriber.OnTransactionEvent(@event), cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the number of active subscribers.
        /// </summary>
        public int SubscriberCount
        {
            get
            {
                lock (_subscribersLock)
                {
                    return _subscribers.Count;
                }
            }
        }
    }

    /// <summary>
    /// Interface for objects that can subscribe to transaction log events.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public interface ITransactionLogSubscriber<TLinkAddress>
    {
        /// <summary>
        /// Called when a transaction event is received.
        /// </summary>
        /// <param name="event">The transaction event.</param>
        void OnTransactionEvent(TransactionEvent<TLinkAddress> @event);
    }
}
