using System.Collections.Generic;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal.Models
{
    public class FactModel
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public List<string> References { get; set; }
        public bool IsDraft => References == null || References.Count == 0;

        public FactModel()
        {
            References = new List<string>();
        }

        public FactModel(long id, string content, List<string> references)
        {
            Id = id;
            Content = content;
            References = references ?? new List<string>();
        }
    }
}
