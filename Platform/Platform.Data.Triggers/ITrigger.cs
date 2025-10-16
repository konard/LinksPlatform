using System;

namespace Platform.Data.Triggers
{
    /// <summary>
    /// Represents a trigger that can be executed on link operations.
    /// Inspired by Markov algorithm substitution rules.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Gets the name of the trigger for identification and debugging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether this trigger matches the given link pattern.
        /// </summary>
        /// <param name="context">The trigger execution context containing link information.</param>
        /// <returns>True if the trigger matches and should execute; otherwise, false.</returns>
        bool Matches(TriggerContext context);

        /// <summary>
        /// Executes the trigger action when a match is found.
        /// </summary>
        /// <param name="context">The trigger execution context.</param>
        void Execute(TriggerContext context);

        /// <summary>
        /// Gets the priority of this trigger. Higher priority triggers execute first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets whether this is a terminal trigger (execution stops after this trigger if it matches).
        /// This corresponds to the Markov algorithm concept of terminal rules (ending with dot).
        /// </summary>
        bool IsTerminal { get; }
    }
}
