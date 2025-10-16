using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.CachingWebProxy
{
    /// <summary>
    /// A caching HTTP proxy server that intercepts requests and caches responses.
    /// </summary>
    public class CachingProxyServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly HttpClient _httpClient;
        private readonly IResponseCache _cache;
        private readonly string _prefix;
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;
        private long _totalRequests;
        private long _cacheHits;
        private long _cacheMisses;

        /// <summary>
        /// Gets the total number of requests processed.
        /// </summary>
        public long TotalRequests => _totalRequests;

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        public long CacheHits => _cacheHits;

        /// <summary>
        /// Gets the number of cache misses.
        /// </summary>
        public long CacheMisses => _cacheMisses;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingProxyServer"/> class.
        /// </summary>
        /// <param name="prefix">The URL prefix to listen on (e.g., "http://localhost:8080/").</param>
        /// <param name="cache">The response cache implementation to use.</param>
        public CachingProxyServer(string prefix, IResponseCache cache = null)
        {
            _prefix = prefix;
            _cache = cache ?? new MemoryResponseCache();
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Starts the proxy server.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _listener.Start();
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine($"Caching proxy server started on {_prefix}");
            Console.WriteLine("Configure your browser to use this proxy.");
            Console.WriteLine("Press Ctrl+C to stop the server.");

            Task.Run(() => ProcessRequestsAsync(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Stops the proxy server.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _listener.Stop();
            _isRunning = false;

            Console.WriteLine("Proxy server stopped.");
            PrintStatistics();
        }

        private async Task ProcessRequestsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context), cancellationToken);
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            Interlocked.Increment(ref _totalRequests);

            var request = context.Request;
            var response = context.Response;

            try
            {
                // For proxy requests, the URL should be in the request URL
                var targetUrl = request.Url.ToString();

                // Simple proxy mode: extract target from query parameter or path
                if (request.QueryString["url"] != null)
                {
                    targetUrl = request.QueryString["url"];
                }

                Console.WriteLine($"[{_totalRequests}] {request.HttpMethod} {targetUrl}");

                // Check cache first
                if (request.HttpMethod == "GET" && _cache.TryGet(targetUrl, out var cachedResponse))
                {
                    Interlocked.Increment(ref _cacheHits);
                    Console.WriteLine($"  -> Cache HIT");
                    await WriteResponseAsync(response, cachedResponse);
                }
                else
                {
                    Interlocked.Increment(ref _cacheMisses);
                    Console.WriteLine($"  -> Cache MISS, fetching from origin...");

                    // Fetch from origin
                    var fetchedResponse = await FetchFromOriginAsync(targetUrl, request);

                    // Cache GET requests
                    if (request.HttpMethod == "GET" && fetchedResponse.StatusCode == 200)
                    {
                        _cache.Set(targetUrl, fetchedResponse);
                    }

                    await WriteResponseAsync(response, fetchedResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  -> Error: {ex.Message}");
                response.StatusCode = 500;
                var errorBytes = Encoding.UTF8.GetBytes($"Proxy error: {ex.Message}");
                response.ContentLength64 = errorBytes.Length;
                await response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
            finally
            {
                response.Close();
            }
        }

        private async Task<CachedResponse> FetchFromOriginAsync(string url, HttpListenerRequest originalRequest)
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(originalRequest.HttpMethod), url);

            // Copy headers (except Host and Connection)
            foreach (var headerName in originalRequest.Headers.AllKeys)
            {
                if (headerName.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    headerName.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    requestMessage.Headers.TryAddWithoutValidation(headerName, originalRequest.Headers[headerName]);
                }
                catch
                {
                    // Skip problematic headers
                }
            }

            var httpResponse = await _httpClient.SendAsync(requestMessage);
            var content = await httpResponse.Content.ReadAsByteArrayAsync();

            var cachedResponse = new CachedResponse
            {
                StatusCode = (int)httpResponse.StatusCode,
                Content = content,
                ContentType = httpResponse.Content.Headers.ContentType?.ToString()
            };

            // Copy response headers
            foreach (var header in httpResponse.Headers)
            {
                cachedResponse.Headers[header.Key] = string.Join(", ", header.Value);
            }

            return cachedResponse;
        }

        private async Task WriteResponseAsync(HttpListenerResponse response, CachedResponse cachedResponse)
        {
            response.StatusCode = cachedResponse.StatusCode;

            if (!string.IsNullOrEmpty(cachedResponse.ContentType))
            {
                response.ContentType = cachedResponse.ContentType;
            }

            foreach (var header in cachedResponse.Headers)
            {
                try
                {
                    response.Headers[header.Key] = header.Value;
                }
                catch
                {
                    // Skip headers that can't be set
                }
            }

            if (cachedResponse.Content != null && cachedResponse.Content.Length > 0)
            {
                response.ContentLength64 = cachedResponse.Content.Length;
                await response.OutputStream.WriteAsync(cachedResponse.Content, 0, cachedResponse.Content.Length);
            }
        }

        private void PrintStatistics()
        {
            Console.WriteLine("\n=== Proxy Statistics ===");
            Console.WriteLine($"Total Requests: {TotalRequests}");
            Console.WriteLine($"Cache Hits: {CacheHits}");
            Console.WriteLine($"Cache Misses: {CacheMisses}");
            if (TotalRequests > 0)
            {
                var hitRate = (double)CacheHits / TotalRequests * 100;
                Console.WriteLine($"Cache Hit Rate: {hitRate:F2}%");
            }
        }

        public void Dispose()
        {
            Stop();
            _httpClient?.Dispose();
            _listener?.Close();
            _cancellationTokenSource?.Dispose();
        }
    }
}
