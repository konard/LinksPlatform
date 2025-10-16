using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// Represents a change event for a link.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
    public class LinkChange<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the state of the link before the change (null for create operations).
        /// </para>
        /// <para></para>
        /// </summary>
        public IList<TLinkAddress> Before { get; }

        /// <summary>
        /// <para>
        /// Gets the state of the link after the change (null for delete operations).
        /// </para>
        /// <para></para>
        /// </summary>
        public IList<TLinkAddress> After { get; }

        /// <summary>
        /// <para>
        /// Gets the type of change operation.
        /// </para>
        /// <para></para>
        /// </summary>
        public LinkChangeType ChangeType { get; }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="LinkChange{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="before">The state of the link before the change.</param>
        /// <param name="after">The state of the link after the change.</param>
        /// <param name="changeType">The type of change operation.</param>
        public LinkChange(IList<TLinkAddress> before, IList<TLinkAddress> after, LinkChangeType changeType)
        {
            Before = before;
            After = after;
            ChangeType = changeType;
        }
    }

    /// <summary>
    /// <para>
    /// Defines the type of change operation on a link.
    /// </para>
    /// <para></para>
    /// </summary>
    public enum LinkChangeType
    {
        /// <summary>
        /// <para>
        /// A new link was created.
        /// </para>
        /// <para></para>
        /// </summary>
        Create,

        /// <summary>
        /// <para>
        /// An existing link was updated.
        /// </para>
        /// <para></para>
        /// </summary>
        Update,

        /// <summary>
        /// <para>
        /// A link was deleted.
        /// </para>
        /// <para></para>
        /// </summary>
        Delete
    }
}
