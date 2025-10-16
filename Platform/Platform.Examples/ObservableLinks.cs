using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// Represents a wrapper for ILinks that implements the IObservable pattern for link change events.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
    public class ObservableLinks<TLinkAddress> : IObservable<LinkChange<TLinkAddress>>
    {
        private readonly ILinks<TLinkAddress> _links;
        private readonly List<IObserver<LinkChange<TLinkAddress>>> _observers;
        private readonly object _observersLock = new object();

        /// <summary>
        /// <para>
        /// Gets the underlying ILinks implementation.
        /// </para>
        /// <para></para>
        /// </summary>
        public ILinks<TLinkAddress> Links => _links;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="ObservableLinks{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">The underlying ILinks implementation to wrap.</param>
        public ObservableLinks(ILinks<TLinkAddress> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _observers = new List<IObserver<LinkChange<TLinkAddress>>>();
        }

        /// <summary>
        /// <para>
        /// Creates a link and notifies all observers.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">The source of the link.</param>
        /// <param name="target">The target of the link.</param>
        /// <returns>The address of the created link.</returns>
        public TLinkAddress Create(TLinkAddress source, TLinkAddress target)
        {
            // Use GetOrCreate extension method to create the link
            var result = _links.GetOrCreate(source, target);

            // Create the "after" state - a link with [index, source, target]
            var after = new TLinkAddress[] { result, source, target };

            // Notify observers about the create operation
            NotifyObservers(new LinkChange<TLinkAddress>(null, after, LinkChangeType.Create));

            return result;
        }

        /// <summary>
        /// <para>
        /// Creates a point link and notifies all observers.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>The address of the created link.</returns>
        public TLinkAddress Create()
        {
            // Create a point link (a link that points to itself)
            var result = _links.GetOrCreate(default(TLinkAddress), default(TLinkAddress));

            // Create the "after" state
            var after = new TLinkAddress[] { result, _links.GetSource(result), _links.GetTarget(result) };

            // Notify observers about the create operation
            NotifyObservers(new LinkChange<TLinkAddress>(null, after, LinkChangeType.Create));

            return result;
        }

        /// <summary>
        /// <para>
        /// Updates a link and notifies all observers.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">The address of the link to update.</param>
        /// <param name="newSource">The new source of the link.</param>
        /// <param name="newTarget">The new target of the link.</param>
        /// <returns>The address of the updated link.</returns>
        public TLinkAddress Update(TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget)
        {
            // Get the current state before update
            var oldSource = _links.GetSource(link);
            var oldTarget = _links.GetTarget(link);
            var before = new TLinkAddress[] { link, oldSource, oldTarget };

            // Use Update extension method to update the link
            var result = _links.Update(link, newSource, newTarget);

            // Create the "after" state
            var after = new TLinkAddress[] { result, newSource, newTarget };

            // Notify observers about the update operation
            NotifyObservers(new LinkChange<TLinkAddress>(before, after, LinkChangeType.Update));

            return result;
        }

        /// <summary>
        /// <para>
        /// Deletes a link and notifies all observers.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">The address of the link to delete.</param>
        public void Delete(TLinkAddress link)
        {
            // Get the current state before deletion
            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);
            var before = new TLinkAddress[] { link, source, target };

            // Delete the link using the Delete extension method
            _links.Delete(link);

            // Notify observers about the delete operation
            NotifyObservers(new LinkChange<TLinkAddress>(before, null, LinkChangeType.Delete));
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<LinkChange<TLinkAddress>> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (_observersLock)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
            }

            return new Unsubscriber(_observers, observer, _observersLock);
        }

        private void NotifyObservers(LinkChange<TLinkAddress> change)
        {
            List<IObserver<LinkChange<TLinkAddress>>> observersCopy;
            lock (_observersLock)
            {
                observersCopy = new List<IObserver<LinkChange<TLinkAddress>>>(_observers);
            }

            foreach (var observer in observersCopy)
            {
                try
                {
                    observer.OnNext(change);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<LinkChange<TLinkAddress>>> _observers;
            private readonly IObserver<LinkChange<TLinkAddress>> _observer;
            private readonly object _lock;

            public Unsubscriber(List<IObserver<LinkChange<TLinkAddress>>> observers, IObserver<LinkChange<TLinkAddress>> observer, object lockObject)
            {
                _observers = observers;
                _observer = observer;
                _lock = lockObject;
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    if (_observer != null && _observers.Contains(_observer))
                        _observers.Remove(_observer);
                }
            }
        }
    }
}
