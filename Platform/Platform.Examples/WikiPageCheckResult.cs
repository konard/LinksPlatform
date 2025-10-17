using System;

namespace Platform.Examples
{
    /// <summary>
    /// Represents the result of checking for a wiki page.
    /// </summary>
    public class WikiPageCheckResult
    {
        public string Query { get; set; }
        public WikiType WikiType { get; set; }
        public string Language { get; set; }
        public bool Exists { get; set; }
        public string Url { get; set; }
        public DateTime CheckedAt { get; set; }
        public string Error { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Error))
            {
                return $"{WikiType} ({Language}): Error checking '{Query}' - {Error}";
            }
            return Exists
                ? $"{WikiType} ({Language}): '{Query}' exists at {Url}"
                : $"{WikiType} ({Language}): '{Query}' does not exist";
        }
    }
}
