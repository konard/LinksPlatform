using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Manages snapshots of the links store state.
    /// After each event, a snapshot is created and distributed to all workers.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class SnapshotManager<TLinkAddress>
    {
        private Dictionary<TLinkAddress, Link<TLinkAddress>> _currentSnapshot;
        private readonly object _snapshotLock = new object();
        private long _snapshotVersion;

        public SnapshotManager()
        {
            _currentSnapshot = new Dictionary<TLinkAddress, Link<TLinkAddress>>();
            _snapshotVersion = 0;
        }

        /// <summary>
        /// Gets the current snapshot version.
        /// </summary>
        public long SnapshotVersion
        {
            get
            {
                lock (_snapshotLock)
                {
                    return _snapshotVersion;
                }
            }
        }

        /// <summary>
        /// Creates a new snapshot by applying transaction operations to the current snapshot.
        /// </summary>
        /// <param name="operations">The operations to apply.</param>
        /// <returns>The new snapshot.</returns>
        public IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> CreateSnapshot(IEnumerable<TransactionOperation<TLinkAddress>> operations)
        {
            if (operations == null)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            lock (_snapshotLock)
            {
                var newSnapshot = new Dictionary<TLinkAddress, Link<TLinkAddress>>(_currentSnapshot);

                foreach (var operation in operations)
                {
                    switch (operation.OperationType)
                    {
                        case OperationType.Create:
                            newSnapshot[operation.LinkAddress] = new Link<TLinkAddress>(
                                operation.LinkAddress,
                                operation.Source,
                                operation.Target);
                            break;

                        case OperationType.Update:
                            if (newSnapshot.ContainsKey(operation.LinkAddress))
                            {
                                newSnapshot[operation.LinkAddress] = new Link<TLinkAddress>(
                                    operation.LinkAddress,
                                    operation.Source,
                                    operation.Target);
                            }
                            break;

                        case OperationType.Delete:
                            newSnapshot.Remove(operation.LinkAddress);
                            break;
                    }
                }

                _currentSnapshot = newSnapshot;
                _snapshotVersion++;

                return new Dictionary<TLinkAddress, Link<TLinkAddress>>(newSnapshot);
            }
        }

        /// <summary>
        /// Gets the current snapshot without creating a new one.
        /// </summary>
        /// <returns>A read-only view of the current snapshot.</returns>
        public IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> GetCurrentSnapshot()
        {
            lock (_snapshotLock)
            {
                return new Dictionary<TLinkAddress, Link<TLinkAddress>>(_currentSnapshot);
            }
        }

        /// <summary>
        /// Initializes the snapshot from an existing links store.
        /// </summary>
        /// <param name="links">The links to initialize from.</param>
        public void InitializeFromLinks(IEnumerable<Link<TLinkAddress>> links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            lock (_snapshotLock)
            {
                _currentSnapshot = links.ToDictionary(link => link.Address, link => link);
                _snapshotVersion = 1;
            }
        }

        /// <summary>
        /// Clears the snapshot.
        /// </summary>
        public void Clear()
        {
            lock (_snapshotLock)
            {
                _currentSnapshot.Clear();
                _snapshotVersion = 0;
            }
        }

        /// <summary>
        /// Gets the number of links in the current snapshot.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_snapshotLock)
                {
                    return _currentSnapshot.Count;
                }
            }
        }
    }
}
