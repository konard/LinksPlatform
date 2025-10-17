using System;

namespace Platform.Data.SearchResults.Models
{
    public class SearchResult
    {
        public ulong Id { get; set; }
        public ulong QueryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string CodeSnippet { get; set; }
        public ulong? LinkedQueryId { get; set; }
        public ResultType Type { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public int Score => Upvotes - Downvotes;
    }

    public enum ResultType
    {
        Link,
        LinkWithQuote,
        Image,
        CodeSnippet,
        LinkedQuery
    }
}
