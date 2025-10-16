using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples.Triggers
{
    /// <summary>
    /// Represents a chain of triggers that can be executed sequentially.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class TriggerChain<TLink> : TriggerBase<TLink>
    {
        private readonly IList<ITrigger<TLink>> _triggers;

        /// <summary>
        /// Gets the name of this trigger chain.
        /// </summary>
        public override string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerChain{TLink}"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger chain.</param>
        /// <param name="triggers">The triggers in the chain.</param>
        public TriggerChain(string name, params ITrigger<TLink>[] triggers)
        {
            Name = name;
            _triggers = triggers?.ToList() ?? new List<ITrigger<TLink>>();
        }

        /// <summary>
        /// Processes the context by executing each trigger in the chain sequentially.
        /// </summary>
        /// <param name="context">The trigger context.</param>
        /// <returns>True if all triggers succeeded and the state is valid; otherwise, false.</returns>
        public override bool Process(TriggerContext<TLink> context)
        {
            foreach (var trigger in _triggers)
            {
                if (!context.IsValid)
                {
                    return false;
                }

                if (!trigger.Process(context))
                {
                    return false;
                }
            }

            return context.IsValid;
        }
    }
}
