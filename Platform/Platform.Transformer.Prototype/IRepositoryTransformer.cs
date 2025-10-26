using System.Threading.Tasks;

namespace Platform.Transformer.Prototype
{
    /// <summary>
    /// Represents a transformer that operates on entire repositories/projects
    /// rather than individual files. This is the core interface for the new
    /// translator architecture that uses AST and Doublets as intermediate storage.
    /// </summary>
    /// <remarks>
    /// The transformation process:
    /// 1. Parse entire repository into AST
    /// 2. Convert AST to Doublets (intermediate storage)
    /// 3. Apply transformation functions on Doublets
    /// 4. Generate code from transformed Doublets
    /// </remarks>
    public interface IRepositoryTransformer
    {
        /// <summary>
        /// Transforms an entire repository from source to target format.
        /// </summary>
        /// <param name="sourceRepositoryPath">Path to the source repository</param>
        /// <param name="targetRepositoryPath">Path to the target repository</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task TransformAsync(string sourceRepositoryPath, string targetRepositoryPath);
    }
}
