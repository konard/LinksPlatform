using System.Collections.Generic;

namespace Platform.Data.AutocompleteService.Models
{
    public class AutocompleteResponse
    {
        public List<string> Suggestions { get; set; }
        public int Count { get; set; }
        public string Prefix { get; set; }
    }
}
