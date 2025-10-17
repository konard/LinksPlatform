using System;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Represents a reference (fact/link) supporting a response
    /// </summary>
    public class ReferenceModel
    {
        public long Id { get; set; }
        public long ResponseId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public ReferenceType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public Link Link { get; set; }

        public ReferenceModel()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public ReferenceModel(long id, long responseId, string title, string url, ReferenceType type) : this()
        {
            Id = id;
            ResponseId = responseId;
            Title = title;
            Url = url;
            Type = type;
        }
    }

    public enum ReferenceType
    {
        Book,
        Website,
        Article,
        Paper,
        Fact,
        Other
    }
}
