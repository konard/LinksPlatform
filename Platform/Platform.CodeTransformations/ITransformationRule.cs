using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Platform.CodeTransformations
{
    /// <summary>
    /// Represents a transformation rule that can be applied to source code.
    /// </summary>
    public interface ITransformationRule
    {
        /// <summary>
        /// Gets the name of the transformation rule.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of what this transformation does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the source version this rule applies from.
        /// </summary>
        string FromVersion { get; }

        /// <summary>
        /// Gets the target version this rule applies to.
        /// </summary>
        string ToVersion { get; }

        /// <summary>
        /// Applies the transformation to the given syntax tree.
        /// </summary>
        /// <param name="tree">The syntax tree to transform.</param>
        /// <returns>The transformed syntax tree.</returns>
        SyntaxTree Transform(SyntaxTree tree);

        /// <summary>
        /// Checks if this transformation rule is applicable to the given syntax tree.
        /// </summary>
        /// <param name="tree">The syntax tree to check.</param>
        /// <returns>True if the rule is applicable, false otherwise.</returns>
        bool IsApplicable(SyntaxTree tree);
    }
}
