using System;

namespace Platform.CodeTranslation
{
    /// <summary>
    /// Represents a translator that can translate source code from one language to another.
    /// </summary>
    public interface ICodeTranslator
    {
        /// <summary>
        /// Gets the name of the source language.
        /// </summary>
        string SourceLanguage { get; }

        /// <summary>
        /// Gets the name of the target language.
        /// </summary>
        string TargetLanguage { get; }

        /// <summary>
        /// Translates source code from the source language to the target language.
        /// </summary>
        /// <param name="sourceCode">The source code to translate.</param>
        /// <returns>The translated source code.</returns>
        string Translate(string sourceCode);
    }
}
