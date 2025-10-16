using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace Platform.CodeGeneration
{
    /// <summary>
    /// Orchestrates the transformation of Dictionary source code to UnsafeDictionary
    /// </summary>
    public class DictionaryToUnsafeDictionaryTransformer
    {
        private readonly DictionarySourceDownloader _downloader;
        private readonly UnsafeDictionaryRewriter _rewriter;

        public DictionaryToUnsafeDictionaryTransformer()
        {
            _downloader = new DictionarySourceDownloader();
            _rewriter = new UnsafeDictionaryRewriter();
        }

        public DictionaryToUnsafeDictionaryTransformer(DictionarySourceDownloader downloader, UnsafeDictionaryRewriter rewriter)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _rewriter = rewriter ?? throw new ArgumentNullException(nameof(rewriter));
        }

        /// <summary>
        /// Downloads and transforms Dictionary source code to UnsafeDictionary
        /// </summary>
        /// <returns>The transformed source code as a string</returns>
        public async Task<string> TransformAsync()
        {
            // Download the Dictionary source code
            var sourceCode = await _downloader.DownloadAsync();

            // Transform it
            return Transform(sourceCode);
        }

        /// <summary>
        /// Transforms Dictionary source code to UnsafeDictionary
        /// </summary>
        /// <param name="sourceCode">The original Dictionary source code</param>
        /// <returns>The transformed source code as a string</returns>
        public string Transform(string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentException("Source code cannot be null or empty", nameof(sourceCode));
            }

            // Parse the source code into a syntax tree
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();

            // Apply the transformation
            var newRoot = _rewriter.Visit(root);

            // Return the transformed code
            return newRoot.ToFullString();
        }

        /// <summary>
        /// Downloads and transforms Dictionary source code to UnsafeDictionary synchronously
        /// </summary>
        /// <returns>The transformed source code as a string</returns>
        public string TransformSync()
        {
            var sourceCode = _downloader.Download();
            return Transform(sourceCode);
        }
    }
}
