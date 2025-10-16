using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when the storage links limit has been reached.
    /// </summary>
    /// <remarks>
    /// This is an internationalized version supporting multiple languages through resource files.
    /// Supported languages: English (default), Russian (ru)
    /// </remarks>
    /// <typeparam name="TLinkAddress">The type of link address</typeparam>
    public class LinksLimitReachedException<TLinkAddress> : LinksLimitReachedExceptionBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksLimitReachedException(TLinkAddress limit)
            : this(FormatMessage(limit)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksLimitReachedException(string message, Exception innerException)
            : base(message, innerException) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksLimitReachedException(string message)
            : base(message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksLimitReachedException()
            : base(DataExceptionMessages.LinksLimitReached) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(TLinkAddress limit)
            => string.Format(DataExceptionMessages.LinksLimitReachedWithValue, limit);
    }
}
