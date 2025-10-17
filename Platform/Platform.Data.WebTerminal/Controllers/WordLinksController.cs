using Microsoft.AspNetCore.Mvc;
using Platform.Data.WebTerminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Platform.Data.WebTerminal.Controllers
{
    public class WordLinksController : Controller
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // GET: /WordLinks/{word}
        public async Task<IActionResult> Index(string word = "link")
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                word = "link";
            }

            var model = new WordLinksModel
            {
                Word = word,
                OccurrencesOnInternet = await FindWordOccurrencesAsync(word)
            };

            return View("Index", model);
        }

        private Task<List<WordOccurrence>> FindWordOccurrencesAsync(string word)
        {
            var occurrences = new List<WordOccurrence>();

            try
            {
                // Search for the word on various platforms
                var searchEngines = new[]
                {
                    new { Name = "Wikipedia", Url = $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(word)}" },
                    new { Name = "Wiktionary", Url = $"https://en.wiktionary.org/wiki/{Uri.EscapeDataString(word)}" },
                    new { Name = "GitHub", Url = $"https://github.com/search?q={Uri.EscapeDataString(word)}" },
                    new { Name = "Stack Overflow", Url = $"https://stackoverflow.com/search?q={Uri.EscapeDataString(word)}" },
                    new { Name = "Reddit", Url = $"https://www.reddit.com/search/?q={Uri.EscapeDataString(word)}" },
                    new { Name = "Google", Url = $"https://www.google.com/search?q={Uri.EscapeDataString(word)}" },
                    new { Name = "DuckDuckGo", Url = $"https://duckduckgo.com/?q={Uri.EscapeDataString(word)}" },
                    new { Name = "Bing", Url = $"https://www.bing.com/search?q={Uri.EscapeDataString(word)}" }
                };

                foreach (var engine in searchEngines)
                {
                    occurrences.Add(new WordOccurrence
                    {
                        Platform = engine.Name,
                        Url = engine.Url,
                        Description = $"Search results for '{word}' on {engine.Name}"
                    });
                }

                // Add domain-specific resources
                occurrences.Add(new WordOccurrence
                {
                    Platform = "Dictionary.com",
                    Url = $"https://www.dictionary.com/browse/{Uri.EscapeDataString(word)}",
                    Description = $"Definition of '{word}'"
                });

                occurrences.Add(new WordOccurrence
                {
                    Platform = "Thesaurus.com",
                    Url = $"https://www.thesaurus.com/browse/{Uri.EscapeDataString(word)}",
                    Description = $"Synonyms for '{word}'"
                });
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                Console.WriteLine($"Error finding word occurrences: {ex.Message}");
            }

            return Task.FromResult(occurrences);
        }
    }
}
