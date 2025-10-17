using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Platform.CodeTransformations
{
    /// <summary>
    /// Base class for transformation rules providing common functionality.
    /// </summary>
    public abstract class TransformationRuleBase : ITransformationRule
    {
        /// <summary>
        /// Gets the name of the transformation rule.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the description of what this transformation does.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the source version this rule applies from.
        /// </summary>
        public abstract string FromVersion { get; }

        /// <summary>
        /// Gets the target version this rule applies to.
        /// </summary>
        public abstract string ToVersion { get; }

        /// <summary>
        /// Applies the transformation to the given syntax tree.
        /// </summary>
        /// <param name="tree">The syntax tree to transform.</param>
        /// <returns>The transformed syntax tree.</returns>
        public abstract SyntaxTree Transform(SyntaxTree tree);

        /// <summary>
        /// Checks if this transformation rule is applicable to the given syntax tree.
        /// </summary>
        /// <param name="tree">The syntax tree to check.</param>
        /// <returns>True if the rule is applicable, false otherwise.</returns>
        public virtual bool IsApplicable(SyntaxTree tree)
        {
            return true;
        }
    }
}
