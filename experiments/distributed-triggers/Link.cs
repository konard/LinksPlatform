namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Represents a link with its source and target.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public struct Link<TLinkAddress>
    {
        /// <summary>
        /// Gets or sets the link address.
        /// </summary>
        public TLinkAddress Address { get; set; }

        /// <summary>
        /// Gets or sets the source of the link.
        /// </summary>
        public TLinkAddress Source { get; set; }

        /// <summary>
        /// Gets or sets the target of the link.
        /// </summary>
        public TLinkAddress Target { get; set; }

        public Link(TLinkAddress address, TLinkAddress source, TLinkAddress target)
        {
            Address = address;
            Source = source;
            Target = target;
        }
    }
}
