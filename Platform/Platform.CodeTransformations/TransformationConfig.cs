using System.Collections.Generic;

namespace Platform.CodeTransformations
{
    /// <summary>
    /// Configuration for code transformations including version information and rules.
    /// </summary>
    public class TransformationConfig
    {
        /// <summary>
        /// Gets or sets the name of the library or project.
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// Gets or sets the current version.
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// Gets or sets the list of available versions.
        /// </summary>
        public List<string> Versions { get; set; }

        /// <summary>
        /// Gets or sets the transformation rules configuration.
        /// </summary>
        public List<TransformationRuleConfig> Rules { get; set; }

        public TransformationConfig()
        {
            Versions = new List<string>();
            Rules = new List<TransformationRuleConfig>();
        }
    }

    /// <summary>
    /// Configuration for a single transformation rule.
    /// </summary>
    public class TransformationRuleConfig
    {
        /// <summary>
        /// Gets or sets the type of transformation.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the source version.
        /// </summary>
        public string FromVersion { get; set; }

        /// <summary>
        /// Gets or sets the target version.
        /// </summary>
        public string ToVersion { get; set; }

        /// <summary>
        /// Gets or sets additional parameters for the transformation.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        public TransformationRuleConfig()
        {
            Parameters = new Dictionary<string, string>();
        }
    }
}
