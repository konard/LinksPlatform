using System;
using System.Collections.Generic;
using Platform.Converters;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// Represents a simple observer that logs link change events to the console.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
    public class LinksObserver<TLinkAddress> : IObserver<LinkChange<TLinkAddress>>
    {
        private static readonly UncheckedConverter<TLinkAddress, long> _addressToInt64Converter = UncheckedConverter<TLinkAddress, long>.Default;
        private readonly string _name;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="LinksObserver{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="name">The name of the observer (used for identification in console output).</param>
        public LinksObserver(string name)
        {
            _name = name ?? "Observer";
        }

        /// <inheritdoc/>
        public void OnNext(LinkChange<TLinkAddress> change)
        {
            switch (change.ChangeType)
            {
                case LinkChangeType.Create:
                    OnLinkCreated(change.After);
                    break;
                case LinkChangeType.Update:
                    OnLinkUpdated(change.Before, change.After);
                    break;
                case LinkChangeType.Delete:
                    OnLinkDeleted(change.Before);
                    break;
            }
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            Console.WriteLine($"[{_name}] Error: {error.Message}");
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
            Console.WriteLine($"[{_name}] Observation completed.");
        }

        /// <summary>
        /// <para>
        /// Called when a new link is created.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">The created link.</param>
        protected virtual void OnLinkCreated(IList<TLinkAddress> link)
        {
            if (link != null)
            {
                Console.WriteLine($"[{_name}] Link created: {FormatLink(link)}");
            }
        }

        /// <summary>
        /// <para>
        /// Called when a link is updated.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="before">The link state before update.</param>
        /// <param name="after">The link state after update.</param>
        protected virtual void OnLinkUpdated(IList<TLinkAddress> before, IList<TLinkAddress> after)
        {
            if (before != null && after != null)
            {
                Console.WriteLine($"[{_name}] Link updated: {FormatLink(before)} -> {FormatLink(after)}");
            }
        }

        /// <summary>
        /// <para>
        /// Called when a link is deleted.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">The deleted link.</param>
        protected virtual void OnLinkDeleted(IList<TLinkAddress> link)
        {
            if (link != null)
            {
                Console.WriteLine($"[{_name}] Link deleted: {FormatLink(link)}");
            }
        }

        private string FormatLink(IList<TLinkAddress> link)
        {
            if (link.Count >= 3)
            {
                return $"({_addressToInt64Converter.Convert(link[0])}: {_addressToInt64Converter.Convert(link[1])} -> {_addressToInt64Converter.Convert(link[2])})";
            }
            return string.Join(", ", link);
        }
    }
}
