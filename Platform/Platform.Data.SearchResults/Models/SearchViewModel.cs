using System.Collections.Generic;

namespace Platform.Data.SearchResults.Models
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();
        public bool HasResults => Results != null && Results.Count > 0;
    }
}
