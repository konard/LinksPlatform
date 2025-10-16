using System;

namespace Platform.Examples.Triggers
{
    /// <summary>
    /// Base class for triggers providing common functionality.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public abstract class TriggerBase<TLink> : ITrigger<TLink>
    {
        /// <summary>
        /// Gets the name of this trigger.
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Processes the given context and determines if the pattern matches.
        /// </summary>
        /// <param name="context">The trigger context containing links to process and validation state.</param>
        /// <returns>True if the trigger pattern matches and processing should continue; otherwise, false.</returns>
        public abstract bool Process(TriggerContext<TLink> context);

        /// <summary>
        /// Transfers processing focus to the next trigger if it exists.
        /// </summary>
        /// <param name="context">The trigger context.</param>
        /// <returns>True if there was a next trigger and it was processed successfully; otherwise, false.</returns>
        protected bool TransferToNext(TriggerContext<TLink> context)
        {
            if (context.NextTrigger != null)
            {
                return context.NextTrigger.Process(context);
            }
            return context.IsValid;
        }
    }
}
