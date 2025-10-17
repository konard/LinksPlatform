using System;
using System.Collections.Generic;

namespace Platform.Data.LinksCloud.Models
{
    /// <summary>
    /// Status of a computational task.
    /// </summary>
    public enum TaskStatus
    {
        Pending,
        Assigned,
        Running,
        Completed,
        Failed
    }

    /// <summary>
    /// Represents a computational task that can be distributed across the network.
    /// </summary>
    public class ComputeTask
    {
        /// <summary>
        /// Unique identifier for the task.
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Name/description of the task.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Task type (e.g., "computation", "query", "transformation").
        /// </summary>
        public string TaskType { get; set; } = string.Empty;

        /// <summary>
        /// Input data for the task.
        /// </summary>
        public Dictionary<string, object> Input { get; set; } = new();

        /// <summary>
        /// Output data from the task.
        /// </summary>
        public Dictionary<string, object> Output { get; set; } = new();

        /// <summary>
        /// Current status of the task.
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// Node ID that created the task.
        /// </summary>
        public Guid CreatorNodeId { get; set; }

        /// <summary>
        /// Node ID assigned to execute the task.
        /// </summary>
        public Guid? AssignedNodeId { get; set; }

        /// <summary>
        /// Task creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Task start timestamp.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Task completion timestamp.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Error message if task failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Priority of the task (higher = more important).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Estimated computational cost.
        /// </summary>
        public int ComputeCost { get; set; }

        public ComputeTask()
        {
            TaskId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Status = TaskStatus.Pending;
            Priority = 0;
            ComputeCost = 1;
        }
    }
}
