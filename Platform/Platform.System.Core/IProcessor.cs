using System;

namespace Platform.System.Core
{
    /// <summary>
    /// Represents the processor abstraction for symmetric multiprocessing support
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Gets the number of available processor cores
        /// </summary>
        int CoreCount { get; }

        /// <summary>
        /// Gets the current processor utilization percentage (0-100)
        /// </summary>
        double Utilization { get; }

        /// <summary>
        /// Executes a task on a specific core
        /// </summary>
        /// <param name="coreId">The core identifier</param>
        /// <param name="task">The task to execute</param>
        void ExecuteOnCore(int coreId, Action task);
    }
}
