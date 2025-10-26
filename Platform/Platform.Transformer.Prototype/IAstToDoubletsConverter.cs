using Microsoft.CodeAnalysis;

namespace Platform.Transformer.Prototype
{
    /// <summary>
    /// Converts Abstract Syntax Trees (AST) into Doublets representation.
    /// This allows storing the parsed code structure in a graph database
    /// where transformations can be applied more flexibly.
    /// </summary>
    /// <typeparam name="TLink">The type used for link identifiers in Doublets</typeparam>
    public interface IAstToDoubletsConverter<TLink>
    {
        /// <summary>
        /// Converts a Roslyn syntax tree into Doublets links.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to convert</param>
        /// <param name="filePath">The file path associated with this syntax tree</param>
        /// <returns>The root link representing this syntax tree in Doublets</returns>
        TLink ConvertSyntaxTree(SyntaxTree syntaxTree, string filePath);

        /// <summary>
        /// Converts a syntax node and its children into Doublets links recursively.
        /// </summary>
        /// <param name="node">The syntax node to convert</param>
        /// <returns>The link representing this node in Doublets</returns>
        TLink ConvertNode(SyntaxNode node);
    }
}
