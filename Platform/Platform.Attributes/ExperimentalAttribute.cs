using System;

namespace Platform.Attributes
{
    /// <summary>
    /// Indicates that an API is experimental and may change or be removed in future versions.
    /// </summary>
    /// <remarks>
    /// This attribute can be applied to types, methods, properties, fields, events, and other code elements
    /// to mark them as experimental. Code that uses experimental APIs should be aware that these APIs
    /// are subject to change and may not be stable.
    ///
    /// This is a custom implementation that works with older .NET versions.
    /// For .NET 8+ and C# 12+, consider using the built-in System.Diagnostics.CodeAnalysis.ExperimentalAttribute
    /// which provides compiler warnings.
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Enum |
        AttributeTargets.Constructor |
        AttributeTargets.Method |
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Event |
        AttributeTargets.Interface |
        AttributeTargets.Delegate,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ExperimentalAttribute : Attribute
    {
        /// <summary>
        /// Gets the diagnostic ID for the experimental feature.
        /// </summary>
        public string DiagnosticId { get; }

        /// <summary>
        /// Gets the URL for more information about the experimental feature.
        /// </summary>
        public string UrlFormat { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExperimentalAttribute"/> class with the specified diagnostic ID.
        /// </summary>
        /// <param name="diagnosticId">The diagnostic ID for the experimental feature.</param>
        public ExperimentalAttribute(string diagnosticId)
        {
            DiagnosticId = diagnosticId;
        }
    }
}
