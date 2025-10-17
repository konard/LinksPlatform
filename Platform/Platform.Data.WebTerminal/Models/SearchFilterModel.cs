using System.Collections.Generic;

namespace Platform.Data.WebTerminal.Models
{
    public class SearchFilterModel
    {
        public string PropertyName { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
    }

    public class SearchRequestModel
    {
        public List<SearchFilterModel> Filters { get; set; }

        public SearchRequestModel()
        {
            Filters = new List<SearchFilterModel>();
        }
    }

    public class SearchResultModel
    {
        public List<LinkModel> Results { get; set; }
        public int TotalCount { get; set; }
        public List<string> AvailableProperties { get; set; }

        public SearchResultModel()
        {
            Results = new List<LinkModel>();
            AvailableProperties = new List<string>();
        }
    }
}
