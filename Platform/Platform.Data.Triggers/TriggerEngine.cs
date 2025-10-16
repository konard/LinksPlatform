using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.Triggers
{
    /// <summary>
    /// The trigger execution engine that manages and executes triggers.
    /// Inspired by Markov algorithm execution model: iteratively apply rules until no more matches.
    /// </summary>
    public class TriggerEngine
    {
        private readonly List<ITrigger> _triggers;
        private readonly ILinkStorage _storage;
        private bool _enabled;

        /// <summary>
        /// Gets or sets whether the trigger engine is enabled.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of iterations for Markov-style execution.
        /// Prevents infinite loops in trigger chains.
        /// </summary>
        public int MaxIterations { get; set; }

        public TriggerEngine(ILinkStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _triggers = new List<ITrigger>();
            _enabled = true;
            MaxIterations = 1000; // Default limit to prevent infinite loops
        }

        /// <summary>
        /// Registers a new trigger.
        /// </summary>
        public void RegisterTrigger(ITrigger trigger)
        {
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            _triggers.Add(trigger);

            // Sort by priority (descending)
            _triggers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Unregisters a trigger by name.
        /// </summary>
        public bool UnregisterTrigger(string name)
        {
            var trigger = _triggers.FirstOrDefault(t => t.Name == name);
            if (trigger != null)
            {
                _triggers.Remove(trigger);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Executes triggers for a link operation.
        /// Uses Markov algorithm-style execution: apply rules in order until terminal rule or no matches.
        /// </summary>
        /// <param name="source">The source link.</param>
        /// <param name="linker">The linker link.</param>
        /// <param name="target">The target link.</param>
        /// <param name="operation">The operation type.</param>
        /// <returns>True if operation should proceed; false if cancelled.</returns>
        public bool ExecuteTriggers(object source, object linker, object target, TriggerOperation operation)
        {
            if (!_enabled)
                return true;

            var context = new TriggerContext(source, linker, target, operation, _storage);

            foreach (var trigger in _triggers)
            {
                if (trigger.Matches(context))
                {
                    trigger.Execute(context);

                    // Check if execution should stop (terminal rule)
                    if (context.StopExecution)
                        break;

                    // Check if operation was cancelled
                    if (context.Cancel)
                        return false;
                }
            }

            return !context.Cancel;
        }

        /// <summary>
        /// Executes triggers in Markov algorithm style: iteratively apply rules until no matches or terminal rule.
        /// This is useful for implementing computational systems within the link database.
        /// </summary>
        /// <param name="source">Initial source link.</param>
        /// <param name="linker">Initial linker link.</param>
        /// <param name="target">Initial target link.</param>
        /// <param name="operation">The operation type.</param>
        /// <returns>The final transformation result.</returns>
        public TriggerContext ExecuteMarkovStyle(object source, object linker, object target, TriggerOperation operation)
        {
            if (!_enabled)
                return new TriggerContext(source, linker, target, operation, _storage);

            var context = new TriggerContext(source, linker, target, operation, _storage);
            int iterations = 0;
            bool anyMatched;

            do
            {
                anyMatched = false;
                iterations++;

                if (iterations > MaxIterations)
                {
                    throw new InvalidOperationException(
                        $"Trigger execution exceeded maximum iterations ({MaxIterations}). Possible infinite loop detected.");
                }

                foreach (var trigger in _triggers)
                {
                    if (trigger.Matches(context))
                    {
                        trigger.Execute(context);
                        anyMatched = true;

                        // Terminal rule stops execution completely
                        if (context.StopExecution)
                            return context;

                        // If cancelled, stop
                        if (context.Cancel)
                            return context;

                        // After a match, restart from the beginning (Markov algorithm behavior)
                        break;
                    }
                }
            }
            while (anyMatched);

            return context;
        }

        /// <summary>
        /// Gets all registered triggers.
        /// </summary>
        public IReadOnlyList<ITrigger> GetTriggers()
        {
            return _triggers.AsReadOnly();
        }

        /// <summary>
        /// Clears all registered triggers.
        /// </summary>
        public void ClearTriggers()
        {
            _triggers.Clear();
        }
    }
}
