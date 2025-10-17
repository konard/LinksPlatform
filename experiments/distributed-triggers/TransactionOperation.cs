namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Represents a single operation in a transaction (create, update, or delete).
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class TransactionOperation<TLinkAddress>
    {
        /// <summary>
        /// Gets or sets the type of operation.
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets the link address affected by this operation.
        /// </summary>
        public TLinkAddress LinkAddress { get; set; }

        /// <summary>
        /// Gets or sets the source of the link (for create and update operations).
        /// </summary>
        public TLinkAddress Source { get; set; }

        /// <summary>
        /// Gets or sets the target of the link (for create and update operations).
        /// </summary>
        public TLinkAddress Target { get; set; }

        /// <summary>
        /// Gets or sets the previous source (for update operations).
        /// </summary>
        public TLinkAddress PreviousSource { get; set; }

        /// <summary>
        /// Gets or sets the previous target (for update operations).
        /// </summary>
        public TLinkAddress PreviousTarget { get; set; }

        public TransactionOperation() { }

        public TransactionOperation(OperationType operationType, TLinkAddress linkAddress, TLinkAddress source, TLinkAddress target)
        {
            OperationType = operationType;
            LinkAddress = linkAddress;
            Source = source;
            Target = target;
        }
    }

    /// <summary>
    /// Defines the types of operations that can be performed on links.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// A new link was created.
        /// </summary>
        Create,

        /// <summary>
        /// An existing link was updated.
        /// </summary>
        Update,

        /// <summary>
        /// An existing link was deleted.
        /// </summary>
        Delete
    }
}
