using System;

namespace Platform.Data.Triggers
{
    /// <summary>
    /// Represents a Markov algorithm-style trigger that performs pattern matching
    /// and transformation on link triplets.
    /// </summary>
    public class MarkovTrigger : ITrigger
    {
        private readonly object _patternSource;
        private readonly object _patternLinker;
        private readonly object _patternTarget;
        private readonly TriggerOperation _operation;
        private readonly Action<TriggerContext> _action;

        public string Name { get; }
        public int Priority { get; }
        public bool IsTerminal { get; }

        /// <summary>
        /// Creates a new Markov-style trigger.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="patternSource">The pattern to match for source (use PatternMatcher.Wildcard for any).</param>
        /// <param name="patternLinker">The pattern to match for linker (use PatternMatcher.Wildcard for any).</param>
        /// <param name="patternTarget">The pattern to match for target (use PatternMatcher.Wildcard for any).</param>
        /// <param name="operation">The operation type to match.</param>
        /// <param name="action">The action to execute when matched.</param>
        /// <param name="priority">The priority (higher executes first).</param>
        /// <param name="isTerminal">Whether this is a terminal rule.</param>
        public MarkovTrigger(string name,
                            object patternSource,
                            object patternLinker,
                            object patternTarget,
                            TriggerOperation operation,
                            Action<TriggerContext> action,
                            int priority = 0,
                            bool isTerminal = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _patternSource = patternSource;
            _patternLinker = patternLinker;
            _patternTarget = patternTarget;
            _operation = operation;
            _action = action ?? throw new ArgumentNullException(nameof(action));
            Priority = priority;
            IsTerminal = isTerminal;
        }

        public bool Matches(TriggerContext context)
        {
            if (context == null)
                return false;

            // Check if operation matches
            if (_operation != TriggerOperation.Any && context.Operation != _operation)
                return false;

            // Check if pattern matches
            return PatternMatcher.Matches(
                context.Source, context.Linker, context.Target,
                _patternSource, _patternLinker, _patternTarget);
        }

        public void Execute(TriggerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _action(context);

            if (IsTerminal)
            {
                context.StopExecution = true;
            }
        }
    }
}
