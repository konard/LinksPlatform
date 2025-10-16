using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when a link argument does not exist.
    /// </summary>
    /// <remarks>
    /// This is an internationalized version supporting multiple languages through resource files.
    /// Supported languages: English (default), Russian (ru)
    ///
    /// To add a new language:
    /// 1. Create a new resource file: DataExceptionMessages.{language-code}.resx
    /// 2. Translate all message keys
    /// 3. Users can set their preferred culture using Thread.CurrentThread.CurrentUICulture
    /// </remarks>
    /// <typeparam name="TLinkAddress">The type of link address</typeparam>
    public class ArgumentLinkDoesNotExistsException<TLinkAddress> : ArgumentException
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkDoesNotExistsException(TLinkAddress link, string argumentName)
            : base(FormatMessage(link, argumentName), argumentName) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkDoesNotExistsException(TLinkAddress link)
            : base(FormatMessage(link)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkDoesNotExistsException(string message, Exception innerException)
            : base(message, innerException) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkDoesNotExistsException(string message)
            : base(message) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArgumentLinkDoesNotExistsException() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(TLinkAddress link, string argumentName)
            => string.Format(DataExceptionMessages.ArgumentLinkDoesNotExistWithParamName, link, argumentName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(TLinkAddress link)
            => string.Format(DataExceptionMessages.ArgumentLinkDoesNotExist, link);
    }
}
