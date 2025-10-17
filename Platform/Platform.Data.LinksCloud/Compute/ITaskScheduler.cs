using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.LinksCloud.Models;

namespace Platform.Data.LinksCloud.Compute
{
    /// <summary>
    /// Interface for distributed task scheduling across the computational grid.
    /// </summary>
    public interface ITaskScheduler
    {
        /// <summary>
        /// Submits a new computational task to the grid.
        /// </summary>
        Task<Guid> SubmitTaskAsync(ComputeTask task);

        /// <summary>
        /// Gets the status of a task.
        /// </summary>
        Task<ComputeTask?> GetTaskStatusAsync(Guid taskId);

        /// <summary>
        /// Cancels a running task.
        /// </summary>
        Task<bool> CancelTaskAsync(Guid taskId);

        /// <summary>
        /// Gets all tasks submitted by the local node.
        /// </summary>
        Task<List<ComputeTask>> GetMyTasksAsync();

        /// <summary>
        /// Gets all pending tasks in the grid.
        /// </summary>
        Task<List<ComputeTask>> GetPendingTasksAsync();

        /// <summary>
        /// Starts processing tasks (as a worker node).
        /// </summary>
        Task StartWorkerAsync();

        /// <summary>
        /// Stops processing tasks.
        /// </summary>
        Task StopWorkerAsync();

        /// <summary>
        /// Event triggered when a task is completed.
        /// </summary>
        event EventHandler<ComputeTask>? TaskCompleted;

        /// <summary>
        /// Event triggered when a task fails.
        /// </summary>
        event EventHandler<ComputeTask>? TaskFailed;
    }
}
