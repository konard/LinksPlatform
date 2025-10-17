using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Represents an independent code block that can be enabled or disabled for testing.
    /// </summary>
    public class CodeBlock
    {
        /// <summary>
        /// Unique identifier for the code block.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description of what this code block does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// File path where this code block is located.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Starting line number in the file.
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        /// Ending line number in the file.
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// List of code block IDs that this block depends on.
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// Whether this code block is currently enabled for testing.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The actual code content.
        /// </summary>
        public string Content { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Description} ({FilePath}:{StartLine}-{EndLine})";
        }
    }
}
