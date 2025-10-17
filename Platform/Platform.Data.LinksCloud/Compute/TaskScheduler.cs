using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Platform.Data.LinksCloud.Models;
using Platform.Data.LinksCloud.Network;
using TaskStatus = Platform.Data.LinksCloud.Models.ComputeTaskStatus;

namespace Platform.Data.LinksCloud.Compute
{
    /// <summary>
    /// Distributed task scheduler for the computational grid.
    /// Distributes tasks across available nodes based on capacity and load.
    /// </summary>
    public class TaskScheduler : ITaskScheduler
    {
        private readonly IP2PNetworkManager _networkManager;
        private readonly Dictionary<Guid, ComputeTask> _tasks;
        private readonly object _tasksLock = new();
        private CancellationTokenSource? _workerCts;
        private bool _isWorkerRunning;

        public event EventHandler<ComputeTask>? TaskCompleted;
        public event EventHandler<ComputeTask>? TaskFailed;

        public TaskScheduler(IP2PNetworkManager networkManager)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _tasks = new Dictionary<Guid, ComputeTask>();

            // Subscribe to network events
            _networkManager.MessageReceived += OnNetworkMessageReceived;
        }

        public async Task<Guid> SubmitTaskAsync(ComputeTask task)
        {
            task.CreatorNodeId = _networkManager.LocalNode.NodeId;
            task.Status = TaskStatus.Pending;

            lock (_tasksLock)
            {
                _tasks[task.TaskId] = task;
            }

            // Broadcast task to the network
            await _networkManager.BroadcastMessageAsync("TASK_SUBMIT", task);

            // Try to find a suitable node for the task
            await AssignTaskToNodeAsync(task);

            return task.TaskId;
        }

        public Task<ComputeTask?> GetTaskStatusAsync(Guid taskId)
        {
            lock (_tasksLock)
            {
                return Task.FromResult(_tasks.TryGetValue(taskId, out var task) ? task : null);
            }
        }

        public async Task<bool> CancelTaskAsync(Guid taskId)
        {
            ComputeTask? task;
            lock (_tasksLock)
            {
                if (!_tasks.TryGetValue(taskId, out task))
                {
                    return false;
                }

                if (task.Status == TaskStatus.Completed || task.Status == TaskStatus.Failed)
                {
                    return false;
                }

                task.Status = TaskStatus.Failed;
                task.ErrorMessage = "Cancelled by user";
            }

            // Broadcast cancellation
            await _networkManager.BroadcastMessageAsync("TASK_CANCEL", taskId);

            return true;
        }

        public Task<List<ComputeTask>> GetMyTasksAsync()
        {
            lock (_tasksLock)
            {
                var myTasks = _tasks.Values
                    .Where(t => t.CreatorNodeId == _networkManager.LocalNode.NodeId)
                    .ToList();
                return Task.FromResult(myTasks);
            }
        }

        public Task<List<ComputeTask>> GetPendingTasksAsync()
        {
            lock (_tasksLock)
            {
                var pendingTasks = _tasks.Values
                    .Where(t => t.Status == TaskStatus.Pending)
                    .OrderByDescending(t => t.Priority)
                    .ToList();
                return Task.FromResult(pendingTasks);
            }
        }

        public async Task StartWorkerAsync()
        {
            if (_isWorkerRunning)
            {
                return;
            }

            _workerCts = new CancellationTokenSource();
            _isWorkerRunning = true;

            // Start worker loop
            _ = Task.Run(() => WorkerLoopAsync(_workerCts.Token));

            await Task.CompletedTask;
        }

        public async Task StopWorkerAsync()
        {
            if (!_isWorkerRunning)
            {
                return;
            }

            _workerCts?.Cancel();
            _isWorkerRunning = false;

            await Task.CompletedTask;
        }

        private async Task WorkerLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check for pending tasks
                    var pendingTasks = await GetPendingTasksAsync();

                    foreach (var task in pendingTasks)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        // Try to claim this task
                        if (await TryClaimTaskAsync(task))
                        {
                            // Execute the task
                            await ExecuteTaskAsync(task);
                        }
                    }

                    // Wait before checking for new tasks
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Log error and continue
                }
            }
        }

        private async Task<bool> TryClaimTaskAsync(ComputeTask task)
        {
            lock (_tasksLock)
            {
                if (task.Status != TaskStatus.Pending)
                {
                    return false;
                }

                // Check if we have enough capacity
                if (_networkManager.LocalNode.ComputeCapacity < task.ComputeCost)
                {
                    return false;
                }

                task.Status = TaskStatus.Assigned;
                task.AssignedNodeId = _networkManager.LocalNode.NodeId;
            }

            // Broadcast task assignment
            await _networkManager.BroadcastMessageAsync("TASK_ASSIGNED", task);

            return true;
        }

        private async Task ExecuteTaskAsync(ComputeTask task)
        {
            lock (_tasksLock)
            {
                task.Status = TaskStatus.Running;
                task.StartedAt = DateTime.UtcNow;
            }

            await _networkManager.BroadcastMessageAsync("TASK_RUNNING", task);

            try
            {
                // Execute the task based on its type
                // This is a simplified implementation - in production, you'd have pluggable task executors
                await Task.Delay(1000); // Simulate work

                lock (_tasksLock)
                {
                    task.Status = TaskStatus.Completed;
                    task.CompletedAt = DateTime.UtcNow;
                    task.Output["result"] = "Task completed successfully";
                }

                await _networkManager.BroadcastMessageAsync("TASK_COMPLETED", task);
                TaskCompleted?.Invoke(this, task);
            }
            catch (Exception ex)
            {
                lock (_tasksLock)
                {
                    task.Status = TaskStatus.Failed;
                    task.CompletedAt = DateTime.UtcNow;
                    task.ErrorMessage = ex.Message;
                }

                await _networkManager.BroadcastMessageAsync("TASK_FAILED", task);
                TaskFailed?.Invoke(this, task);
            }
        }

        private async Task AssignTaskToNodeAsync(ComputeTask task)
        {
            // Simple load balancing: find the node with most available capacity
            var peers = _networkManager.Peers;
            var bestNode = peers
                .Where(p => p.IsOnline && p.ComputeCapacity >= task.ComputeCost)
                .OrderByDescending(p => p.ComputeCapacity)
                .FirstOrDefault();

            if (bestNode != null)
            {
                task.AssignedNodeId = bestNode.NodeId;
                task.Status = TaskStatus.Assigned;

                await _networkManager.SendMessageAsync(bestNode.NodeId, "TASK_ASSIGN", task);
            }
        }

        private void OnNetworkMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            switch (e.MessageType)
            {
                case "TASK_SUBMIT":
                    if (e.Data is ComputeTask submitTask)
                    {
                        lock (_tasksLock)
                        {
                            if (!_tasks.ContainsKey(submitTask.TaskId))
                            {
                                _tasks[submitTask.TaskId] = submitTask;
                            }
                        }
                    }
                    break;

                case "TASK_ASSIGNED":
                case "TASK_RUNNING":
                case "TASK_COMPLETED":
                case "TASK_FAILED":
                    if (e.Data is ComputeTask statusTask)
                    {
                        lock (_tasksLock)
                        {
                            _tasks[statusTask.TaskId] = statusTask;
                        }

                        if (e.MessageType == "TASK_COMPLETED")
                        {
                            TaskCompleted?.Invoke(this, statusTask);
                        }
                        else if (e.MessageType == "TASK_FAILED")
                        {
                            TaskFailed?.Invoke(this, statusTask);
                        }
                    }
                    break;

                case "TASK_CANCEL":
                    if (e.Data is Guid cancelTaskId)
                    {
                        lock (_tasksLock)
                        {
                            if (_tasks.TryGetValue(cancelTaskId, out var task))
                            {
                                task.Status = TaskStatus.Failed;
                                task.ErrorMessage = "Cancelled";
                            }
                        }
                    }
                    break;
            }
        }
    }
}
