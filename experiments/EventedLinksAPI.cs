using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.EventedIO
{
    /// <summary>
    /// Represents the type of CRUD operation
    /// </summary>
    public enum OperationType
    {
        Create,
        Read,
        Update,
        Delete
    }

    /// <summary>
    /// Represents a single operation/transition in the queue
    /// </summary>
    public class LinkOperation<TLink>
    {
        public OperationType Type { get; set; }
        public TLink? Source { get; set; }
        public TLink? Target { get; set; }
        public TLink? LinkId { get; set; }
        public TLink? NewSource { get; set; }
        public TLink? NewTarget { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid TransactionId { get; set; }

        public LinkOperation()
        {
            Timestamp = DateTime.UtcNow;
            TransactionId = Guid.NewGuid();
        }
    }

    /// <summary>
    /// Event arguments for CRUD operations
    /// </summary>
    public class LinkEventArgs<TLink> : EventArgs
    {
        public LinkOperation<TLink> Operation { get; set; }
        public bool Cancel { get; set; }
        public string? ValidationError { get; set; }

        public LinkEventArgs(LinkOperation<TLink> operation)
        {
            Operation = operation;
            Cancel = false;
        }
    }

    /// <summary>
    /// Represents a transaction containing multiple operations
    /// </summary>
    public class LinkTransaction<TLink>
    {
        public Guid TransactionId { get; set; }
        public List<LinkOperation<TLink>> Operations { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsCommitted { get; set; }

        public LinkTransaction()
        {
            TransactionId = Guid.NewGuid();
            Operations = new List<LinkOperation<TLink>>();
            StartTime = DateTime.UtcNow;
            IsCommitted = false;
        }
    }

    /// <summary>
    /// Event-driven wrapper for ILinks interface with operation queue and native triggers
    /// </summary>
    public interface ILinksEvented<TLink>
    {
        // Event handlers (native triggers)
        event EventHandler<LinkEventArgs<TLink>>? BeforeCreate;
        event EventHandler<LinkEventArgs<TLink>>? AfterCreate;
        event EventHandler<LinkEventArgs<TLink>>? BeforeRead;
        event EventHandler<LinkEventArgs<TLink>>? AfterRead;
        event EventHandler<LinkEventArgs<TLink>>? BeforeUpdate;
        event EventHandler<LinkEventArgs<TLink>>? AfterUpdate;
        event EventHandler<LinkEventArgs<TLink>>? BeforeDelete;
        event EventHandler<LinkEventArgs<TLink>>? AfterDelete;
        event EventHandler<LinkEventArgs<TLink>>? OnConflict;
        event EventHandler<LinkEventArgs<TLink>>? OnValidationError;

        // Operation queue methods
        void EnqueueOperation(LinkOperation<TLink> operation);
        IEnumerable<LinkOperation<TLink>> GetPendingOperations();
        void ClearQueue();

        // Transaction methods
        LinkTransaction<TLink> BeginTransaction();
        bool CommitTransaction(Guid transactionId);
        void RollbackTransaction(Guid transactionId);
        IEnumerable<LinkTransaction<TLink>> GetPendingTransactions();

        // Validation methods
        bool ValidateOperation(LinkOperation<TLink> operation, out string? error);
        bool CheckConflicts(LinkOperation<TLink> operation, out IEnumerable<LinkOperation<TLink>> conflicts);
        bool CheckConsistency(IEnumerable<LinkOperation<TLink>> operations, out string? error);

        // CRUD operations (queued)
        TLink CreateAsync(TLink source, TLink target);
        TLink? ReadAsync(TLink linkId);
        bool UpdateAsync(TLink linkId, TLink newSource, TLink newTarget);
        bool DeleteAsync(TLink linkId);

        // Execute pending operations
        void ProcessQueue();
    }

    /// <summary>
    /// Implementation of event-driven Links API
    /// </summary>
    public class LinksEvented<TLink> : ILinksEvented<TLink>
    {
        private readonly Queue<LinkOperation<TLink>> _operationQueue;
        private readonly Dictionary<Guid, LinkTransaction<TLink>> _transactions;
        private readonly Dictionary<TLink, (TLink Source, TLink Target)> _linkStore;

        // Events
        public event EventHandler<LinkEventArgs<TLink>>? BeforeCreate;
        public event EventHandler<LinkEventArgs<TLink>>? AfterCreate;
        public event EventHandler<LinkEventArgs<TLink>>? BeforeRead;
        public event EventHandler<LinkEventArgs<TLink>>? AfterRead;
        public event EventHandler<LinkEventArgs<TLink>>? BeforeUpdate;
        public event EventHandler<LinkEventArgs<TLink>>? AfterUpdate;
        public event EventHandler<LinkEventArgs<TLink>>? BeforeDelete;
        public event EventHandler<LinkEventArgs<TLink>>? AfterDelete;
        public event EventHandler<LinkEventArgs<TLink>>? OnConflict;
        public event EventHandler<LinkEventArgs<TLink>>? OnValidationError;

        public LinksEvented()
        {
            _operationQueue = new Queue<LinkOperation<TLink>>();
            _transactions = new Dictionary<Guid, LinkTransaction<TLink>>();
            _linkStore = new Dictionary<TLink, (TLink, TLink)>();
        }

        public void EnqueueOperation(LinkOperation<TLink> operation)
        {
            if (!ValidateOperation(operation, out var error))
            {
                var eventArgs = new LinkEventArgs<TLink>(operation) { ValidationError = error };
                OnValidationError?.Invoke(this, eventArgs);
                if (!eventArgs.Cancel)
                    throw new InvalidOperationException($"Operation validation failed: {error}");
                return;
            }

            if (CheckConflicts(operation, out var conflicts))
            {
                var eventArgs = new LinkEventArgs<TLink>(operation);
                OnConflict?.Invoke(this, eventArgs);
                if (!eventArgs.Cancel)
                    throw new InvalidOperationException($"Operation conflicts with {conflicts.Count()} pending operation(s)");
                return;
            }

            _operationQueue.Enqueue(operation);
        }

        public IEnumerable<LinkOperation<TLink>> GetPendingOperations()
        {
            return _operationQueue.ToList();
        }

        public void ClearQueue()
        {
            _operationQueue.Clear();
        }

        public LinkTransaction<TLink> BeginTransaction()
        {
            var transaction = new LinkTransaction<TLink>();
            _transactions[transaction.TransactionId] = transaction;
            return transaction;
        }

        public bool CommitTransaction(Guid transactionId)
        {
            if (!_transactions.TryGetValue(transactionId, out var transaction))
                return false;

            if (!CheckConsistency(transaction.Operations, out var error))
            {
                throw new InvalidOperationException($"Transaction consistency check failed: {error}");
            }

            foreach (var operation in transaction.Operations)
            {
                EnqueueOperation(operation);
            }

            transaction.IsCommitted = true;
            ProcessQueue();

            return true;
        }

        public void RollbackTransaction(Guid transactionId)
        {
            _transactions.Remove(transactionId);
        }

        public IEnumerable<LinkTransaction<TLink>> GetPendingTransactions()
        {
            return _transactions.Values.Where(t => !t.IsCommitted).ToList();
        }

        public bool ValidateOperation(LinkOperation<TLink> operation, out string? error)
        {
            error = null;

            switch (operation.Type)
            {
                case OperationType.Create:
                    if (operation.Source == null || operation.Target == null)
                    {
                        error = "Create operation requires both Source and Target";
                        return false;
                    }
                    break;

                case OperationType.Read:
                    if (operation.LinkId == null)
                    {
                        error = "Read operation requires LinkId";
                        return false;
                    }
                    break;

                case OperationType.Update:
                    if (operation.LinkId == null || operation.NewSource == null || operation.NewTarget == null)
                    {
                        error = "Update operation requires LinkId, NewSource and NewTarget";
                        return false;
                    }
                    break;

                case OperationType.Delete:
                    if (operation.LinkId == null)
                    {
                        error = "Delete operation requires LinkId";
                        return false;
                    }
                    break;
            }

            return true;
        }

        public bool CheckConflicts(LinkOperation<TLink> operation, out IEnumerable<LinkOperation<TLink>> conflicts)
        {
            // Check for conflicting operations in the queue
            var conflictingOps = _operationQueue.Where(op =>
            {
                // Operations on the same link
                if (op.LinkId != null && op.LinkId.Equals(operation.LinkId))
                {
                    // Update after delete
                    if (op.Type == OperationType.Delete && operation.Type == OperationType.Update)
                        return true;
                    // Delete after delete
                    if (op.Type == OperationType.Delete && operation.Type == OperationType.Delete)
                        return true;
                }
                return false;
            }).ToList();

            conflicts = conflictingOps;
            return conflictingOps.Any();
        }

        public bool CheckConsistency(IEnumerable<LinkOperation<TLink>> operations, out string? error)
        {
            error = null;
            var opList = operations.ToList();

            // Check that operations are in a valid sequence
            var linkStates = new Dictionary<TLink, bool>(); // true = exists, false = deleted

            foreach (var op in opList)
            {
                switch (op.Type)
                {
                    case OperationType.Create:
                        if (op.LinkId != null && linkStates.ContainsKey(op.LinkId) && linkStates[op.LinkId])
                        {
                            error = $"Cannot create link {op.LinkId} - already exists";
                            return false;
                        }
                        if (op.LinkId != null)
                            linkStates[op.LinkId] = true;
                        break;

                    case OperationType.Update:
                    case OperationType.Delete:
                        if (op.LinkId != null && (!linkStates.ContainsKey(op.LinkId) || !linkStates[op.LinkId]))
                        {
                            error = $"Cannot {op.Type} link {op.LinkId} - does not exist";
                            return false;
                        }
                        if (op.Type == OperationType.Delete && op.LinkId != null)
                            linkStates[op.LinkId] = false;
                        break;
                }
            }

            return true;
        }

        public TLink CreateAsync(TLink source, TLink target)
        {
            var operation = new LinkOperation<TLink>
            {
                Type = OperationType.Create,
                Source = source,
                Target = target,
                LinkId = GenerateNewLinkId()
            };

            EnqueueOperation(operation);
            return operation.LinkId;
        }

        public TLink? ReadAsync(TLink linkId)
        {
            var operation = new LinkOperation<TLink>
            {
                Type = OperationType.Read,
                LinkId = linkId
            };

            EnqueueOperation(operation);
            return linkId;
        }

        public bool UpdateAsync(TLink linkId, TLink newSource, TLink newTarget)
        {
            var operation = new LinkOperation<TLink>
            {
                Type = OperationType.Update,
                LinkId = linkId,
                NewSource = newSource,
                NewTarget = newTarget
            };

            EnqueueOperation(operation);
            return true;
        }

        public bool DeleteAsync(TLink linkId)
        {
            var operation = new LinkOperation<TLink>
            {
                Type = OperationType.Delete,
                LinkId = linkId
            };

            EnqueueOperation(operation);
            return true;
        }

        public void ProcessQueue()
        {
            while (_operationQueue.Count > 0)
            {
                var operation = _operationQueue.Dequeue();
                ExecuteOperation(operation);
            }
        }

        private void ExecuteOperation(LinkOperation<TLink> operation)
        {
            switch (operation.Type)
            {
                case OperationType.Create:
                    ExecuteCreate(operation);
                    break;
                case OperationType.Read:
                    ExecuteRead(operation);
                    break;
                case OperationType.Update:
                    ExecuteUpdate(operation);
                    break;
                case OperationType.Delete:
                    ExecuteDelete(operation);
                    break;
            }
        }

        private void ExecuteCreate(LinkOperation<TLink> operation)
        {
            var eventArgs = new LinkEventArgs<TLink>(operation);
            BeforeCreate?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel && operation.LinkId != null && operation.Source != null && operation.Target != null)
            {
                _linkStore[operation.LinkId] = (operation.Source, operation.Target);
                AfterCreate?.Invoke(this, eventArgs);
            }
        }

        private void ExecuteRead(LinkOperation<TLink> operation)
        {
            var eventArgs = new LinkEventArgs<TLink>(operation);
            BeforeRead?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                // Read logic here
                AfterRead?.Invoke(this, eventArgs);
            }
        }

        private void ExecuteUpdate(LinkOperation<TLink> operation)
        {
            var eventArgs = new LinkEventArgs<TLink>(operation);
            BeforeUpdate?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel && operation.LinkId != null && operation.NewSource != null && operation.NewTarget != null)
            {
                if (_linkStore.ContainsKey(operation.LinkId))
                {
                    _linkStore[operation.LinkId] = (operation.NewSource, operation.NewTarget);
                    AfterUpdate?.Invoke(this, eventArgs);
                }
            }
        }

        private void ExecuteDelete(LinkOperation<TLink> operation)
        {
            var eventArgs = new LinkEventArgs<TLink>(operation);
            BeforeDelete?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel && operation.LinkId != null)
            {
                _linkStore.Remove(operation.LinkId);
                AfterDelete?.Invoke(this, eventArgs);
            }
        }

        private TLink GenerateNewLinkId()
        {
            // Simple implementation - in real usage, this would integrate with actual link storage
            return default(TLink)!;
        }
    }
}
