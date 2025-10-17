using System;
using System.Globalization;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Provides extension methods for object conversion with support for context-based configuration.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts an object to DateTime using the specified format or the format from the current context.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="format">Optional format string. If not provided, uses "ToDateTime.format" from the current context.</param>
        /// <returns>A DateTime representation of the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when obj is null.</exception>
        /// <exception cref="FormatException">Thrown when the object cannot be converted to DateTime.</exception>
        public static DateTime ToDateTime(this object obj, string format = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            // If no format is provided, try to get it from context
            if (format == null)
            {
                format = Global.GetCurrentContext().Get("ToDateTime.format") as string;
            }

            // If obj is already a DateTime, return it
            if (obj is DateTime dateTime)
            {
                return dateTime;
            }

            // Convert to string
            var stringValue = obj.ToString();

            // If we have a format, use it
            if (!string.IsNullOrEmpty(format))
            {
                return DateTime.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
            }

            // Otherwise, try default parsing
            return DateTime.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts an object to a Link using the specified Links instance or the instance from the current context.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="links">Optional Links instance. If not provided, uses "ToLink.links" from the current context.</param>
        /// <returns>A link representation of the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when obj is null or links instance is not available.</exception>
        public static ulong ToLink(this object obj, ILinks<ulong> links = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            // If no links instance is provided, try to get it from context
            if (links == null)
            {
                links = Global.GetCurrentContext().Get("ToLink.links") as ILinks<ulong>;
            }

            if (links == null)
            {
                throw new ArgumentNullException(nameof(links), "Links instance not provided and not found in context.");
            }

            // If obj is already a link (ulong), return it
            if (obj is ulong linkValue)
            {
                return linkValue;
            }

            // If obj can be converted to ulong, do so
            if (obj is long || obj is int || obj is short || obj is byte)
            {
                return Convert.ToUInt64(obj);
            }

            // For string objects, create a link that represents the string
            if (obj is string stringValue)
            {
                // This is a simplified example - in real implementation you might want to
                // create a sequence of links representing the string
                return links.GetOrCreate<ulong>(0, (ulong)Math.Abs(stringValue.GetHashCode()));
            }

            // For other types, create a link based on the object's hash code
            return links.GetOrCreate<ulong>(0, (ulong)Math.Abs(obj.GetHashCode()));
        }
    }
}
