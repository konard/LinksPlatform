using System;
using System.Collections.Generic;

namespace BreakingChangesMigrator
{
    /// <summary>
    /// Represents a migration rule for handling breaking changes.
    /// </summary>
    public class MigrationRule
    {
        /// <summary>
        /// The version from which this rule applies.
        /// </summary>
        public string FromVersion { get; set; } = string.Empty;

        /// <summary>
        /// The version to which this rule applies.
        /// </summary>
        public string ToVersion { get; set; } = string.Empty;

        /// <summary>
        /// Description of the breaking change.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The type of change (e.g., "MethodRename", "ParameterChange", "TypeChange").
        /// </summary>
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// Old pattern to match (e.g., old method name, old type name).
        /// </summary>
        public PatternMatch OldPattern { get; set; } = new PatternMatch();

        /// <summary>
        /// New pattern to replace with.
        /// </summary>
        public PatternMatch NewPattern { get; set; } = new PatternMatch();
    }

    /// <summary>
    /// Represents a pattern to match or replace in code.
    /// </summary>
    public class PatternMatch
    {
        /// <summary>
        /// Full type name (e.g., "Platform.Data.Doublets.Memory.United.Generic.LinksOperator").
        /// </summary>
        public string? TypeName { get; set; }

        /// <summary>
        /// Method or property name.
        /// </summary>
        public string? MemberName { get; set; }

        /// <summary>
        /// Parameter types for method matching.
        /// </summary>
        public List<string> ParameterTypes { get; set; } = new List<string>();

        /// <summary>
        /// Replacement template with placeholders.
        /// </summary>
        public string? ReplacementTemplate { get; set; }
    }
}
