using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Manages continuous computation and execution of research queries.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    public class DynamicResearchSystem<TLink>
    {
        private readonly SynchronizedLinks<TLink> _links;
        private readonly ResearchDatabase<TLink> _database;
        private readonly Dictionary<TLink, Task> _continuousTasks;
        private readonly Dictionary<TLink, CancellationTokenSource> _cancellationTokens;
        private readonly Dictionary<TLink, string> _lastResults;

        /// <summary>
        /// Gets whether the system is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Event raised when a research query completes execution.
        /// </summary>
        public event EventHandler<ResearchExecutedEventArgs<TLink>> ResearchExecuted;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicResearchSystem{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        /// <param name="database">The research database.</param>
        public DynamicResearchSystem(SynchronizedLinks<TLink> links, ResearchDatabase<TLink> database)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _continuousTasks = new Dictionary<TLink, Task>();
            _cancellationTokens = new Dictionary<TLink, CancellationTokenSource>();
            _lastResults = new Dictionary<TLink, string>();
        }

        /// <summary>
        /// Starts the dynamic research system, executing continuous research queries.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;

            foreach (var research in _database.Researches.Values.Where(r => r.IsContinuous))
            {
                StartContinuousResearch(research);
            }
        }

        /// <summary>
        /// Stops the dynamic research system and all continuous computations.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;

            foreach (var cts in _cancellationTokens.Values)
            {
                cts.Cancel();
            }

            Task.WaitAll(_continuousTasks.Values.ToArray());

            _continuousTasks.Clear();
            _cancellationTokens.Clear();
        }

        /// <summary>
        /// Starts continuous execution of a specific research query.
        /// </summary>
        /// <param name="research">The research query to execute continuously.</param>
        public void StartContinuousResearch(ResearchQuery<TLink> research)
        {
            if (research == null || !research.IsContinuous)
            {
                return;
            }

            if (_continuousTasks.ContainsKey(research.Id))
            {
                return; // Already running
            }

            var cts = new CancellationTokenSource();
            _cancellationTokens[research.Id] = cts;

            var task = Task.Run(() => ContinuousExecutionLoop(research, cts.Token), cts.Token);
            _continuousTasks[research.Id] = task;
        }

        /// <summary>
        /// Stops continuous execution of a specific research query.
        /// </summary>
        /// <param name="researchId">The research ID to stop.</param>
        public void StopContinuousResearch(TLink researchId)
        {
            if (_cancellationTokens.TryGetValue(researchId, out var cts))
            {
                cts.Cancel();
                _cancellationTokens.Remove(researchId);
            }

            if (_continuousTasks.TryGetValue(researchId, out var task))
            {
                task.Wait();
                _continuousTasks.Remove(researchId);
            }
        }

        /// <summary>
        /// Executes a research query once and returns the result.
        /// </summary>
        /// <param name="researchId">The research ID to execute.</param>
        /// <returns>The execution result.</returns>
        public string ExecuteResearch(TLink researchId)
        {
            var research = _database.GetResearch(researchId);
            if (research == null)
            {
                return $"Research with ID {researchId} not found.";
            }

            var result = research.Execute(_links);
            _lastResults[researchId] = result;

            OnResearchExecuted(new ResearchExecutedEventArgs<TLink>
            {
                ResearchId = researchId,
                ResearchName = research.Name,
                Result = result,
                ExecutedAt = DateTime.UtcNow
            });

            return result;
        }

        /// <summary>
        /// Gets the last execution result for a research query.
        /// </summary>
        /// <param name="researchId">The research ID.</param>
        /// <returns>The last result, or null if never executed.</returns>
        public string GetLastResult(TLink researchId)
        {
            return _lastResults.TryGetValue(researchId, out var result) ? result : null;
        }

        /// <summary>
        /// Gets a summary of all research execution statuses.
        /// </summary>
        /// <returns>A formatted string with execution statuses.</returns>
        public string GetExecutionStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Dynamic Research System Status ===");
            sb.AppendLine($"System Running: {IsRunning}");
            sb.AppendLine($"Continuous Tasks: {_continuousTasks.Count}");
            sb.AppendLine();

            foreach (var research in _database.Researches.Values)
            {
                sb.AppendLine($"[{research.Id}] {research.Name}");
                sb.AppendLine($"  Type: {(research.IsContinuous ? "Continuous" : "On-Demand")}");

                if (_continuousTasks.ContainsKey(research.Id))
                {
                    sb.AppendLine($"  Status: Running");
                }
                else if (research.IsContinuous)
                {
                    sb.AppendLine($"  Status: Stopped");
                }

                if (research.LastExecutedAt.HasValue)
                {
                    sb.AppendLine($"  Last Executed: {research.LastExecutedAt.Value:yyyy-MM-dd HH:mm:ss}");
                }

                if (_lastResults.TryGetValue(research.Id, out var lastResult))
                {
                    var preview = lastResult.Length > 100 ? lastResult.Substring(0, 100) + "..." : lastResult;
                    sb.AppendLine($"  Last Result: {preview}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void ContinuousExecutionLoop(ResearchQuery<TLink> research, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = research.Execute(_links);
                    _lastResults[research.Id] = result;

                    OnResearchExecuted(new ResearchExecutedEventArgs<TLink>
                    {
                        ResearchId = research.Id,
                        ResearchName = research.Name,
                        Result = result,
                        ExecutedAt = DateTime.UtcNow
                    });

                    // Wait before next execution (default 1 second)
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    // Log error but continue execution
                    _lastResults[research.Id] = $"Error: {ex.Message}";
                }
            }
        }

        private void OnResearchExecuted(ResearchExecutedEventArgs<TLink> e)
        {
            ResearchExecuted?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Event args for research execution completion.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifiers.</typeparam>
    public class ResearchExecutedEventArgs<TLink> : EventArgs
    {
        public TLink ResearchId { get; set; }
        public string ResearchName { get; set; }
        public string Result { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
