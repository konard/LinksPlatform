using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.SearchResults.Models;

namespace Platform.Data.SearchResults.Controllers
{
    public class SearchController : Controller
    {
        // In-memory storage for demonstration purposes
        // In a real implementation, this would use Platform.Data.Doublets for persistent storage
        private static readonly Dictionary<string, List<SearchResult>> _searchData = new Dictionary<string, List<SearchResult>>();
        private static ulong _nextId = 1;

        public IActionResult Index(string q)
        {
            var model = new SearchViewModel { Query = q };

            if (!string.IsNullOrWhiteSpace(q))
            {
                model.Results = GetSearchResults(q);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AddResult(string query, string title, string content, string url, string imageUrl, string codeSnippet, string resultType)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query is required");
            }

            CreateSearchResult(query, title, content, url, imageUrl, codeSnippet, resultType);

            return RedirectToAction("Index", new { q = query });
        }

        [HttpPost]
        public IActionResult Vote(ulong resultId, bool upvote)
        {
            UpdateVote(resultId, upvote);
            return Ok();
        }

        private List<SearchResult> GetSearchResults(string query)
        {
            if (!_searchData.ContainsKey(query.ToLower()))
            {
                return new List<SearchResult>();
            }

            return _searchData[query.ToLower()].OrderByDescending(r => r.Score).ToList();
        }

        private void CreateSearchResult(string query, string title, string content, string url, string imageUrl, string codeSnippet, string resultType)
        {
            var queryKey = query.ToLower();
            if (!_searchData.ContainsKey(queryKey))
            {
                _searchData[queryKey] = new List<SearchResult>();
            }

            var result = new SearchResult
            {
                Id = _nextId++,
                QueryId = 0,
                Title = title,
                Content = content,
                Url = url,
                ImageUrl = imageUrl,
                CodeSnippet = codeSnippet,
                Type = Enum.TryParse<ResultType>(resultType, out var type) ? type : ResultType.Link,
                Upvotes = 0,
                Downvotes = 0,
                CreatedAt = DateTime.UtcNow
            };

            _searchData[queryKey].Add(result);
        }

        private void UpdateVote(ulong resultId, bool upvote)
        {
            foreach (var results in _searchData.Values)
            {
                var result = results.FirstOrDefault(r => r.Id == resultId);
                if (result != null)
                {
                    if (upvote)
                    {
                        result.Upvotes++;
                    }
                    else
                    {
                        result.Downvotes++;
                    }
                    break;
                }
            }
        }
    }
}
