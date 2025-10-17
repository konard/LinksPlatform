using System;
using System.Collections.Generic;

namespace Platform.Data.SearchResults.Models
{
    public class SearchQuery
    {
        public ulong Id { get; set; }
        public string Query { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();
    }
}
