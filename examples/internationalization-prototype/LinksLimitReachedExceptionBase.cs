using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Exceptions
{
    /// <summary>
    /// Base exception class for links limit scenarios.
    /// </summary>
    /// <remarks>
    /// This is an internationalized version supporting multiple languages through resource files.
    /// Supported languages: English (default), Russian (ru)
    /// </remarks>
    public abstract class LinksLimitReachedExceptionBase : Exception
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected LinksLimitReachedExceptionBase(string message, Exception innerException)
            : base(message, innerException) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected LinksLimitReachedExceptionBase(string message)
            : base(message) { }
    }
}
