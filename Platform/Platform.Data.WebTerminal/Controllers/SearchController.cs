using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    public class SearchController : Controller
    {
        // GET: /Search/
        public IActionResult Index()
        {
            var model = new SearchResultModel
            {
                AvailableProperties = GetAvailableProperties()
            };
            return View(model);
        }

        // POST: /Search/
        [HttpPost]
        public IActionResult Search([FromBody] SearchRequestModel request)
        {
            try
            {
                var results = PerformSearch(request.Filters);
                var model = new SearchResultModel
                {
                    Results = results,
                    TotalCount = results.Count,
                    AvailableProperties = GetAvailableProperties()
                };
                return Json(model);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private List<LinkModel> PerformSearch(List<SearchFilterModel> filters)
        {
            var results = new List<LinkModel>();

            // Get all links or a reasonable subset
            var allLinks = GetAllLinks();

            foreach (var link in allLinks)
            {
                if (MatchesFilters(link, filters))
                {
                    var model = LinkModel.CreateLinkModel(link, nestingLevel: 1);
                    results.Add(model);
                }
            }

            return results;
        }

        private bool MatchesFilters(Link link, List<SearchFilterModel> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return true;
            }

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.PropertyName))
                {
                    continue;
                }

                var value = GetPropertyValue(link, filter.PropertyName);

                if (!MatchesFilter(value, filter.MinValue, filter.MaxValue))
                {
                    return false;
                }
            }

            return true;
        }

        private bool MatchesFilter(object value, string minValue, string maxValue)
        {
            if (value == null)
            {
                return false;
            }

            var valueStr = value.ToString();

            // Try numeric comparison
            if (long.TryParse(valueStr, out long numericValue))
            {
                if (!string.IsNullOrWhiteSpace(minValue) && long.TryParse(minValue, out long min))
                {
                    if (numericValue < min)
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(maxValue) && long.TryParse(maxValue, out long max))
                {
                    if (numericValue > max)
                    {
                        return false;
                    }
                }
            }
            else
            {
                // String comparison
                if (!string.IsNullOrWhiteSpace(minValue) && string.Compare(valueStr, minValue, StringComparison.Ordinal) < 0)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(maxValue) && string.Compare(valueStr, maxValue, StringComparison.Ordinal) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private object GetPropertyValue(Link link, string propertyName)
        {
            switch (propertyName.ToLower())
            {
                case "id":
                    return link.ToInt();
                case "source":
                    return link.Source?.ToInt();
                case "target":
                    return link.Target?.ToInt();
                case "linker":
                    return link.Linker?.ToInt();
                case "totalreferers":
                    return link.TotalReferers;
                case "timestamp":
                    return link.Timestamp;
                default:
                    return null;
            }
        }

        private List<Link> GetAllLinks()
        {
            var links = new List<Link>();
            var startId = Net.Link;
            var maxLinks = 1000; // Limit for performance

            try
            {
                for (long i = startId; i < startId + maxLinks && Link.Exists(i); i++)
                {
                    var link = Link.Restore(i);
                    if (link != null)
                    {
                        links.Add(link);
                    }
                }
            }
            catch
            {
                // Handle any exceptions during link retrieval
            }

            return links;
        }

        private List<string> GetAvailableProperties()
        {
            return new List<string>
            {
                "Id",
                "Source",
                "Target",
                "Linker",
                "TotalReferers",
                "Timestamp"
            };
        }
    }
}
