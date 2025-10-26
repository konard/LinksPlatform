using System;
using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a virtual links structure for string data stored in a table.
    /// Each string can be accessed as if it were a link, but is stored efficiently in a traditional table.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    /// <remarks>
    /// This class demonstrates how traditional table data (strings) can be represented
    /// as a virtual links structure. Each string entry has:
    /// - A unique identifier (link ID)
    /// - Virtual source: points to individual characters
    /// - Virtual target: points to metadata or next character
    /// The actual string data is stored in a Dictionary (simulating a table),
    /// not in the physical links space.
    /// </remarks>
    public class VirtualStringTable<TLink> : IVirtualLinksStructure<TLink>
    {
        private readonly Dictionary<TLink, string> _stringTable;
        private readonly TLink _baseId;
        private readonly Func<TLink, TLink> _increment;
        private readonly Func<TLink, TLink, bool> _greaterThanOrEqual;
        private readonly Func<TLink, TLink, bool> _lessThan;
        private readonly Func<TLink, TLink, TLink> _subtract;
        private readonly Func<TLink, int, TLink> _add;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualStringTable{TLink}"/> class.
        /// </summary>
        /// <param name="baseId">The base identifier for virtual links in this table.</param>
        /// <param name="increment">Function to increment a link value.</param>
        /// <param name="greaterThanOrEqual">Function to compare if first >= second.</param>
        /// <param name="lessThan">Function to compare if first &lt; second.</param>
        /// <param name="subtract">Function to subtract two link values.</param>
        /// <param name="add">Function to add an integer to a link value.</param>
        public VirtualStringTable(
            TLink baseId,
            Func<TLink, TLink> increment,
            Func<TLink, TLink, bool> greaterThanOrEqual,
            Func<TLink, TLink, bool> lessThan,
            Func<TLink, TLink, TLink> subtract,
            Func<TLink, int, TLink> add)
        {
            _stringTable = new Dictionary<TLink, string>();
            _baseId = baseId;
            _increment = increment;
            _greaterThanOrEqual = greaterThanOrEqual;
            _lessThan = lessThan;
            _subtract = subtract;
            _add = add;
        }

        /// <summary>
        /// Gets the count of strings in the virtual table.
        /// </summary>
        public long Count => _stringTable.Count;

        /// <summary>
        /// Adds a string to the virtual table and returns its virtual link identifier.
        /// </summary>
        /// <param name="value">The string value to add.</param>
        /// <returns>The virtual link identifier for the added string.</returns>
        public TLink Add(string value)
        {
            var id = _add(_baseId, _stringTable.Count);
            _stringTable[id] = value;
            return id;
        }

        /// <summary>
        /// Gets the string value associated with a virtual link identifier.
        /// </summary>
        /// <param name="link">The virtual link identifier.</param>
        /// <returns>The string value, or null if not found.</returns>
        public string GetValue(TLink link)
        {
            return _stringTable.TryGetValue(link, out var value) ? value : null;
        }

        /// <summary>
        /// Gets the source of a virtual link.
        /// For strings, this returns a reference to the first character (as a virtual link).
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <returns>The source of the virtual link (first character position).</returns>
        public TLink GetSource(TLink link)
        {
            if (!Contains(link))
            {
                return default;
            }

            // Virtual source: represents the beginning of the string
            // In a real implementation, this could point to character links
            return link;
        }

        /// <summary>
        /// Gets the target of a virtual link.
        /// For strings, this represents the end or metadata about the string.
        /// </summary>
        /// <param name="link">The link identifier.</param>
        /// <returns>The target of the virtual link.</returns>
        public TLink GetTarget(TLink link)
        {
            if (!Contains(link))
            {
                return default;
            }

            var value = _stringTable[link];
            // Virtual target: could represent string length, last character, or metadata
            // Here we use the next link ID to represent the string's end marker
            return _increment(link);
        }

        /// <summary>
        /// Determines whether the specified link identifier is part of this virtual structure.
        /// </summary>
        /// <param name="link">The link identifier to check.</param>
        /// <returns>true if the link is in the virtual table range; otherwise, false.</returns>
        public bool Contains(TLink link)
        {
            if (_lessThan(link, _baseId))
            {
                return false;
            }

            var maxId = _add(_baseId, _stringTable.Count);
            return _lessThan(link, maxId);
        }

        /// <summary>
        /// Traverses the virtual structure starting from a specific link.
        /// This demonstrates that virtual structures can be traversed like real links.
        /// </summary>
        /// <param name="link">The starting link.</param>
        /// <param name="handler">Action to perform on each link during traversal.</param>
        public void Traverse(TLink link, Action<TLink, TLink, TLink> handler)
        {
            if (!Contains(link))
            {
                return;
            }

            var source = GetSource(link);
            var target = GetTarget(link);
            handler(link, source, target);
        }
    }
}
