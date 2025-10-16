using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when a link has dependencies that prevent modification.
    /// </summary>
    /// <remarks>
    /// This is an internationalized version supporting multiple languages through resource files.
    /// Supported languages: English (default), Russian (ru)
    /// </remarks>
    /// <typeparam name="TLinkAddress">The type of link address</typeparam>
    public class ArgumentLinkHasDependenciesException<TLinkAddress> : ArgumentException
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkHasDependenciesException(TLinkAddress link, string paramName)
            : base(FormatMessage(link, paramName), paramName) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkHasDependenciesException(TLinkAddress link)
            : base(FormatMessage(link)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkHasDependenciesException(string message, Exception innerException)
            : base(message, innerException) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkHasDependenciesException(string message)
            : base(message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkHasDependenciesException() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(TLinkAddress link, string paramName)
            => string.Format(DataExceptionMessages.ArgumentLinkHasDependenciesWithParamName, link, paramName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(TLinkAddress link)
            => string.Format(DataExceptionMessages.ArgumentLinkHasDependencies, link);
    }
}
