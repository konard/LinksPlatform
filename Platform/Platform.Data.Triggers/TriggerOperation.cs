namespace Platform.Data.Triggers
{
    /// <summary>
    /// Represents the type of operation that triggered the execution.
    /// Based on the operations mentioned in issue #10 comments:
    /// (0 0) => (0 0) — ignored
    /// (0 0) => (1 1) — creation
    /// (1 1) => (2 1) — update
    /// (2 1) => (0 0) — deletion
    /// (1 1) => (1 1) — read
    /// </summary>
    public enum TriggerOperation
    {
        /// <summary>
        /// Link creation operation.
        /// </summary>
        Create,

        /// <summary>
        /// Link update operation.
        /// </summary>
        Update,

        /// <summary>
        /// Link deletion operation.
        /// </summary>
        Delete,

        /// <summary>
        /// Link read operation.
        /// </summary>
        Read,

        /// <summary>
        /// Any operation (wildcard for pattern matching).
        /// </summary>
        Any
    }
}
