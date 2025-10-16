using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create a link with a value that already exists.
    /// </summary>
    /// <remarks>
    /// This is an internationalized version supporting multiple languages through resource files.
    /// Supported languages: English (default), Russian (ru)
    /// </remarks>
    public class LinkWithSameValueAlreadyExistsException : Exception
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkWithSameValueAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkWithSameValueAlreadyExistsException(string message)
            : base(message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkWithSameValueAlreadyExistsException()
            : base(DataExceptionMessages.LinkWithSameValueAlreadyExists) { }
    }
}
