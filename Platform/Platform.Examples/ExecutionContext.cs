using System.Collections.Generic;
using System.Threading;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a thread-safe execution context that stores key-value pairs for use by extension methods.
    /// </summary>
    public class ExecutionContext
    {
        private readonly Dictionary<string, object> _contextData = new Dictionary<string, object>();
        private readonly object _lock = new object();

        /// <summary>
        /// Sets a value in the context by key.
        /// </summary>
        /// <param name="key">The key to store the value under.</param>
        /// <param name="value">The value to store.</param>
        public void Set(string key, object value)
        {
            lock (_lock)
            {
                _contextData[key] = value;
            }
        }

        /// <summary>
        /// Gets a value from the context by key.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <returns>The value associated with the key, or null if not found.</returns>
        public object Get(string key)
        {
            lock (_lock)
            {
                return _contextData.TryGetValue(key, out var value) ? value : null;
            }
        }

        /// <summary>
        /// Tries to get a value from the context by key.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="value">The value associated with the key, or null if not found.</param>
        /// <returns>True if the key exists in the context, false otherwise.</returns>
        public bool TryGet(string key, out object value)
        {
            lock (_lock)
            {
                return _contextData.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Clears all values from the context.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _contextData.Clear();
            }
        }
    }
}
