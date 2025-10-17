using System;
using System.Collections.Generic;
using System.Text;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates how Links Platform can be used as a database replacement for Wikipedia-like wiki applications.
    /// This example shows how to store wiki pages, revisions, links between pages, and categories using the Links storage model.
    /// </summary>
    public class WikiStorage
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;

        // Marker links for different entity types
        private readonly ulong _pageMarker;
        private readonly ulong _revisionMarker;
        private readonly ulong _categoryMarker;
        private readonly ulong _linkMarker;
        private readonly ulong _timestampMarker;
        private readonly ulong _authorMarker;
        private readonly ulong _contentMarker;
        private readonly ulong _titleMarker;

        public WikiStorage(ILinks<ulong> links, Sequences sequences)
        {
            _links = links;
            _sequences = sequences;

            // Initialize markers for different entity types
            _pageMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Page"));
            _revisionMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Revision"));
            _categoryMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Category"));
            _linkMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Link"));
            _timestampMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Timestamp"));
            _authorMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Author"));
            _contentMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Content"));
            _titleMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Title"));
        }

        /// <summary>
        /// Creates a new wiki page with a title.
        /// </summary>
        public ulong CreatePage(string title)
        {
            var titleLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(title));
            var pageLink = _links.Create();

            // Create: Page -> Title -> titleLink
            var pageTitleRelation = _links.Create();
            _links.Update(pageTitleRelation, _titleMarker, titleLink);

            // Create: pageLink -> PageMarker -> pageTitleRelation
            _links.Update(pageLink, _pageMarker, pageTitleRelation);

            return pageLink;
        }

        /// <summary>
        /// Creates a revision for a page with content, author, and timestamp.
        /// </summary>
        public ulong CreateRevision(ulong pageLink, string content, string author, DateTime timestamp)
        {
            var contentLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(content));
            var authorLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(author));
            var timestampLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(timestamp.ToString("o")));

            // Create revision metadata
            var revisionLink = _links.Create();

            // Content relation
            var contentRelation = _links.Create();
            _links.Update(contentRelation, _contentMarker, contentLink);

            // Author relation
            var authorRelation = _links.Create();
            _links.Update(authorRelation, _authorMarker, authorLink);

            // Timestamp relation
            var timestampRelation = _links.Create();
            _links.Update(timestampRelation, _timestampMarker, timestampLink);

            // Combine metadata
            var metadata1 = _links.Create();
            _links.Update(metadata1, contentRelation, authorRelation);

            var metadata2 = _links.Create();
            _links.Update(metadata2, metadata1, timestampRelation);

            // Create: revisionLink -> RevisionMarker -> metadata
            _links.Update(revisionLink, _revisionMarker, metadata2);

            // Link revision to page
            var pageRevisionLink = _links.Create();
            _links.Update(pageRevisionLink, pageLink, revisionLink);

            return revisionLink;
        }

        /// <summary>
        /// Creates a link from one page to another (like a hyperlink in wiki text).
        /// </summary>
        public ulong CreatePageLink(ulong sourcePage, ulong targetPage)
        {
            var linkRelation = _links.Create();
            _links.Update(linkRelation, sourcePage, targetPage);

            var markedLink = _links.Create();
            _links.Update(markedLink, _linkMarker, linkRelation);

            return markedLink;
        }

        /// <summary>
        /// Creates a category and assigns a page to it.
        /// </summary>
        public ulong CreateCategory(string categoryName)
        {
            var categoryNameLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(categoryName));
            var categoryLink = _links.Create();
            _links.Update(categoryLink, _categoryMarker, categoryNameLink);
            return categoryLink;
        }

        /// <summary>
        /// Adds a page to a category.
        /// </summary>
        public ulong AddPageToCategory(ulong pageLink, ulong categoryLink)
        {
            var categoryRelation = _links.Create();
            _links.Update(categoryRelation, categoryLink, pageLink);
            return categoryRelation;
        }

        /// <summary>
        /// Gets the title of a page.
        /// </summary>
        public string GetPageTitle(ulong pageLink)
        {
            var pageData = _links.GetTarget(pageLink);
            var titleLink = _links.GetTarget(pageData);
            return _sequences.FormatSequence(titleLink, AppendLinkToString, true);
        }

        private static void AppendLinkToString(StringBuilder sb, ulong link)
        {
            if (link <= char.MaxValue + 1)
            {
                sb.Append(UnicodeMap.FromLinkToChar(link));
            }
            else
            {
                sb.Append($"({link})");
            }
        }

        /// <summary>
        /// Searches for pages by title substring.
        /// </summary>
        public List<ulong> SearchPagesByTitle(string titleSubstring)
        {
            var results = new List<ulong>();

            // This is a simplified search - in production you'd use more sophisticated indexing
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _pageMarker)
                {
                    var pageTitleRelation = _links.GetTarget(linkIndex);
                    var titleLink = _links.GetTarget(pageTitleRelation);
                    var title = _sequences.FormatSequence(titleLink, AppendLinkToString, true);

                    if (title.Contains(titleSubstring))
                    {
                        results.Add(linkIndex);
                    }
                }
                return _links.Constants.Continue;
            });

            return results;
        }

        /// <summary>
        /// Gets all revisions for a page, ordered by creation (oldest first).
        /// </summary>
        public List<ulong> GetPageRevisions(ulong pageLink)
        {
            var revisions = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == pageLink)
                {
                    var target = _links.GetTarget(linkIndex);
                    // Check if target is a revision
                    if (_links.GetSource(target) == _revisionMarker)
                    {
                        revisions.Add(target);
                    }
                }
                return _links.Constants.Continue;
            });

            return revisions;
        }

        /// <summary>
        /// Gets all pages that link to a specific page (backlinks).
        /// </summary>
        public List<ulong> GetBacklinks(ulong targetPage)
        {
            var backlinks = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _linkMarker)
                {
                    var linkRelation = _links.GetTarget(linkIndex);
                    if (_links.GetTarget(linkRelation) == targetPage)
                    {
                        backlinks.Add(_links.GetSource(linkRelation));
                    }
                }
                return _links.Constants.Continue;
            });

            return backlinks;
        }

        /// <summary>
        /// Gets all pages in a category.
        /// </summary>
        public List<ulong> GetPagesInCategory(ulong categoryLink)
        {
            var pages = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == categoryLink)
                {
                    pages.Add(_links.GetTarget(linkIndex));
                }
                return _links.Constants.Continue;
            });

            return pages;
        }

        /// <summary>
        /// Prints statistics about the wiki storage.
        /// </summary>
        public void PrintStatistics()
        {
            Console.WriteLine("Wiki Storage Statistics:");
            Console.WriteLine($"Total links: {_links.Count()}");

            // Count pages
            var pageCount = 0;
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _pageMarker)
                {
                    pageCount++;
                }
                return _links.Constants.Continue;
            });
            Console.WriteLine($"Total pages: {pageCount}");
        }
    }
}
