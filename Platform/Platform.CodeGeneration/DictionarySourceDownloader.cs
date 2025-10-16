using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Platform.CodeGeneration
{
    /// <summary>
    /// Downloads the latest Dictionary source code from .NET runtime GitHub repository
    /// </summary>
    public class DictionarySourceDownloader
    {
        private const string DictionarySourceUrl = "https://raw.githubusercontent.com/dotnet/runtime/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Dictionary.cs";

        private readonly HttpClient _httpClient;

        public DictionarySourceDownloader()
        {
            _httpClient = new HttpClient();
        }

        public DictionarySourceDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Downloads the Dictionary source code from GitHub
        /// </summary>
        /// <returns>The source code as a string</returns>
        public async Task<string> DownloadAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(DictionarySourceUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to download Dictionary source code from {DictionarySourceUrl}", ex);
            }
        }

        /// <summary>
        /// Downloads the Dictionary source code synchronously
        /// </summary>
        /// <returns>The source code as a string</returns>
        public string Download()
        {
            return DownloadAsync().GetAwaiter().GetResult();
        }
    }
}
