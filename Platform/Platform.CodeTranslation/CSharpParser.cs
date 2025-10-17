using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Platform.CodeTranslation
{
    /// <summary>
    /// Parses C# source code into a Roslyn syntax tree.
    /// </summary>
    public class CSharpParser : ICodeParser<SyntaxTree>
    {
        private readonly CSharpParseOptions _parseOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpParser"/> class.
        /// </summary>
        /// <param name="parseOptions">Optional parse options for C# parsing.</param>
        public CSharpParser(CSharpParseOptions parseOptions = null)
        {
            _parseOptions = parseOptions ?? CSharpParseOptions.Default;
        }

        /// <summary>
        /// Parses the given C# source code into a syntax tree.
        /// </summary>
        /// <param name="sourceCode">The C# source code to parse.</param>
        /// <returns>The parsed syntax tree.</returns>
        public SyntaxTree Parse(string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));
            }

            return CSharpSyntaxTree.ParseText(sourceCode, _parseOptions);
        }
    }
}
