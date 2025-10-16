using System;

namespace Platform.Examples.Triggers
{
    /// <summary>
    /// Represents a trigger that can process a group of links and pass focus to next triggers.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public interface ITrigger<TLink>
    {
        /// <summary>
        /// Processes the given context and determines if the pattern matches.
        /// </summary>
        /// <param name="context">The trigger context containing links to process and validation state.</param>
        /// <returns>True if the trigger pattern matches and processing should continue; otherwise, false.</returns>
        bool Process(TriggerContext<TLink> context);

        /// <summary>
        /// Gets the name of this trigger for debugging purposes.
        /// </summary>
        string Name { get; }
    }
}
