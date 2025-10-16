using System;
using System.Collections.Generic;

namespace Platform.Examples.Triggers
{
    /// <summary>
    /// Represents the context for trigger processing, containing links to process and validation state.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class TriggerContext<TLink>
    {
        /// <summary>
        /// Gets or sets the current group of links being processed (the "focus").
        /// </summary>
        public IList<TLink> CurrentLinks { get; set; }

        /// <summary>
        /// Gets or sets the accumulated validation state.
        /// True indicates valid HTML code so far.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets additional data that can be passed between triggers.
        /// </summary>
        public IDictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets the next trigger to process.
        /// </summary>
        public ITrigger<TLink> NextTrigger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerContext{TLink}"/> class.
        /// </summary>
        public TriggerContext()
        {
            CurrentLinks = new List<TLink>();
            IsValid = true;
            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerContext{TLink}"/> class with the specified links.
        /// </summary>
        /// <param name="links">The initial links to process.</param>
        public TriggerContext(IList<TLink> links) : this()
        {
            CurrentLinks = links ?? new List<TLink>();
        }
    }
}
