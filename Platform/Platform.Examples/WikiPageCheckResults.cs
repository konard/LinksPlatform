namespace Platform.Examples
{
    /// <summary>
    /// Represents the combined results for both Wikipedia and Wiktionary.
    /// </summary>
    public class WikiPageCheckResults
    {
        public WikiPageCheckResult Wikipedia { get; set; }
        public WikiPageCheckResult Wiktionary { get; set; }

        public override string ToString()
        {
            return $"{Wikipedia}\n{Wiktionary}";
        }
    }
}
