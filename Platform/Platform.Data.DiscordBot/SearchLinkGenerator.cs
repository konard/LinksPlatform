using System;
using System.Text;
using System.Web;

namespace Platform.Data.DiscordBot
{
    /// <summary>
    /// Generates search links for error messages across multiple search engines and platforms
    /// </summary>
    public class SearchLinkGenerator
    {
        /// <summary>
        /// Generates search links for the given error message
        /// </summary>
        /// <param name="errorMessage">The error message to search for</param>
        /// <returns>A formatted string containing all search links</returns>
        public static string GenerateSearchLinks(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return "Error: Please provide an error message to search for.";
            }

            var encodedQuery = Uri.EscapeDataString(errorMessage);
            var sb = new StringBuilder();

            sb.AppendLine("🔍 **Error Search Links**");
            sb.AppendLine();
            sb.AppendLine($"**Query:** `{errorMessage}`");
            sb.AppendLine();

            // Google Search
            sb.AppendLine($"🌐 **Google:** https://www.google.com/search?q={encodedQuery}");

            // Bing Search
            sb.AppendLine($"🔷 **Bing:** https://www.bing.com/search?q={encodedQuery}");

            // GitHub Code Search
            sb.AppendLine($"💻 **GitHub Code:** https://github.com/search?type=code&q={encodedQuery}");

            // GitHub Issues Search
            sb.AppendLine($"📝 **GitHub Issues:** https://github.com/search?type=issues&q={encodedQuery}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates a compact version of search links that fits within Discord's character limit
        /// </summary>
        /// <param name="errorMessage">The error message to search for</param>
        /// <param name="maxLength">Maximum length of the response (default: 2000 for Discord)</param>
        /// <returns>A formatted string containing search links</returns>
        public static string GenerateCompactSearchLinks(string errorMessage, int maxLength = 2000)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return "Error: Please provide an error message to search for.";
            }

            // Truncate error message if it's too long
            var displayMessage = errorMessage.Length > 100
                ? errorMessage.Substring(0, 97) + "..."
                : errorMessage;

            var encodedQuery = Uri.EscapeDataString(errorMessage);
            var sb = new StringBuilder();

            sb.AppendLine("🔍 **Error Search Links**");
            sb.AppendLine($"`{displayMessage}`");
            sb.AppendLine();

            // Google
            var googleLink = $"https://www.google.com/search?q={encodedQuery}";
            sb.AppendLine($"[Google]({googleLink})");

            // Bing
            var bingLink = $"https://www.bing.com/search?q={encodedQuery}";
            sb.AppendLine($"[Bing]({bingLink})");

            // GitHub Code
            var ghCodeLink = $"https://github.com/search?type=code&q={encodedQuery}";
            sb.AppendLine($"[GitHub Code]({ghCodeLink})");

            // GitHub Issues
            var ghIssuesLink = $"https://github.com/search?type=issues&q={encodedQuery}";
            sb.AppendLine($"[GitHub Issues]({ghIssuesLink})");

            var result = sb.ToString();

            // If still too long, use ultra-compact format
            if (result.Length > maxLength)
            {
                sb.Clear();
                sb.AppendLine("🔍 Search Links:");
                sb.AppendLine($"G: {googleLink}");
                sb.AppendLine($"B: {bingLink}");
                sb.AppendLine($"GHC: {ghCodeLink}");
                sb.AppendLine($"GHI: {ghIssuesLink}");
                result = sb.ToString();
            }

            return result;
        }
    }
}
