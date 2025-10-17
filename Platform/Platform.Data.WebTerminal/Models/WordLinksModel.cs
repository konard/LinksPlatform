using System.Collections.Generic;

namespace Platform.Data.WebTerminal.Models
{
    public class WordLinksModel
    {
        public string Word { get; set; }
        public List<WordOccurrence> OccurrencesOnInternet { get; set; }
    }

    public class WordOccurrence
    {
        public string Platform { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
