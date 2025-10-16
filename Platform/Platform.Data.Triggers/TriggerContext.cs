using System;
using System.Collections.Generic;

namespace Platform.Data.Triggers
{
    /// <summary>
    /// Represents the execution context for a trigger.
    /// Contains information about the link operation and allows transformation.
    /// </summary>
    public class TriggerContext
    {
        /// <summary>
        /// Gets or sets the source link reference.
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// Gets or sets the linker link reference (the relation type).
        /// </summary>
        public object Linker { get; set; }

        /// <summary>
        /// Gets or sets the target link reference.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Gets the operation type that triggered this context.
        /// </summary>
        public TriggerOperation Operation { get; }

        /// <summary>
        /// Gets the link storage for creating/reading links.
        /// </summary>
        public ILinkStorage Storage { get; }

        /// <summary>
        /// Gets or sets additional context data for trigger execution.
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets whether the trigger execution should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets whether to stop executing further triggers.
        /// </summary>
        public bool StopExecution { get; set; }

        public TriggerContext(object source, object linker, object target, TriggerOperation operation, ILinkStorage storage)
        {
            Source = source;
            Linker = linker;
            Target = target;
            Operation = operation;
            Storage = storage;
            Data = new Dictionary<string, object>();
            Cancel = false;
            StopExecution = false;
        }
    }
}
