using System;

namespace Platform.CodeTranslation
{
    /// <summary>
    /// Represents a parser that can parse source code into an abstract syntax tree (AST).
    /// </summary>
    /// <typeparam name="TAst">The type representing the abstract syntax tree.</typeparam>
    public interface ICodeParser<TAst>
    {
        /// <summary>
        /// Parses the given source code string into an AST.
        /// </summary>
        /// <param name="sourceCode">The source code to parse.</param>
        /// <returns>The parsed abstract syntax tree.</returns>
        TAst Parse(string sourceCode);
    }
}
