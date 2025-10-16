using System.ComponentModel.DataAnnotations;

namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Data transfer object for a Link (triplet)
    /// </summary>
    public class LinkDto
    {
        /// <summary>
        /// Unique identifier representing a specific link
        /// </summary>
        [Required]
        public long Id { get; set; }

        /// <summary>
        /// Identifier of Source (beginning, subject) link
        /// </summary>
        [Required]
        public long Source { get; set; }

        /// <summary>
        /// Identifier of Linker (type of connection, verb, predicate, action, operator, transition) link
        /// </summary>
        [Required]
        public long Linker { get; set; }

        /// <summary>
        /// Identifier of Target (end, object) link
        /// </summary>
        [Required]
        public long Target { get; set; }
    }
}
