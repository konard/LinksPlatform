using System;

namespace Platform.CodeTranslation
{
    /// <summary>
    /// Represents a code generator that can generate source code from an abstract syntax tree (AST).
    /// </summary>
    /// <typeparam name="TAst">The type representing the abstract syntax tree.</typeparam>
    public interface ICodeGenerator<TAst>
    {
        /// <summary>
        /// Generates source code from the given AST.
        /// </summary>
        /// <param name="ast">The abstract syntax tree to generate code from.</param>
        /// <returns>The generated source code.</returns>
        string Generate(TAst ast);
    }
}
