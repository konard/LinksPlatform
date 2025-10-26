using System.Collections.Generic;

namespace Platform.Transformer.Prototype
{
    /// <summary>
    /// Generates source code from Doublets representation.
    /// This is the final step in the transformation pipeline,
    /// converting the transformed graph back into compilable code.
    /// </summary>
    /// <typeparam name="TLink">The type used for link identifiers in Doublets</typeparam>
    public interface IDoubletsToCodeGenerator<TLink>
    {
        /// <summary>
        /// Generates source code files from the Doublets representation.
        /// </summary>
        /// <param name="rootLink">The root link representing the repository/project</param>
        /// <param name="targetPath">The path where generated files should be written</param>
        /// <returns>A collection of generated file paths</returns>
        IEnumerable<string> GenerateCode(TLink rootLink, string targetPath);
    }
}
