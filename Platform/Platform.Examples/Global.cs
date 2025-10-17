using System.Threading;

namespace Platform.Examples
{
    /// <summary>
    /// Provides access to the current execution context for the calling thread.
    /// </summary>
    public static class Global
    {
        private static readonly AsyncLocal<ExecutionContext> _currentContext = new AsyncLocal<ExecutionContext>();

        /// <summary>
        /// Gets the current execution context for the calling thread.
        /// If no context exists, a new one is created.
        /// </summary>
        /// <returns>The current execution context.</returns>
        public static ExecutionContext GetCurrentContext()
        {
            if (_currentContext.Value == null)
            {
                _currentContext.Value = new ExecutionContext();
            }
            return _currentContext.Value;
        }

        /// <summary>
        /// Sets the current execution context for the calling thread.
        /// </summary>
        /// <param name="context">The execution context to set as current.</param>
        public static void SetCurrentContext(ExecutionContext context)
        {
            _currentContext.Value = context;
        }

        /// <summary>
        /// Clears the current execution context for the calling thread.
        /// </summary>
        public static void ClearCurrentContext()
        {
            _currentContext.Value = null;
        }
    }
}
