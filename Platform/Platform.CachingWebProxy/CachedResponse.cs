using System;
using System.Collections.Generic;

namespace Platform.CachingWebProxy
{
    /// <summary>
    /// Represents a cached HTTP response with metadata.
    /// </summary>
    public class CachedResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the cached response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the response body content.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this response was cached.
        /// </summary>
        public DateTime CachedAt { get; set; }

        /// <summary>
        /// Gets or sets the content type of the response.
        /// </summary>
        public string ContentType { get; set; }

        public CachedResponse()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}
