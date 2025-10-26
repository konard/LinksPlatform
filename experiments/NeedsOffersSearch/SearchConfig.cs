using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LinksPlatform.NeedsOffersSearch
{
    /// <summary>
    /// Configuration for search providers
    /// </summary>
    public class SearchConfig
    {
        /// <summary>
        /// Google Ads API configuration
        /// </summary>
        public GoogleAdsConfig GoogleAds { get; set; } = new GoogleAdsConfig();

        /// <summary>
        /// Yandex Direct API configuration
        /// </summary>
        public YandexAdsConfig YandexDirect { get; set; } = new YandexAdsConfig();

        /// <summary>
        /// General search settings
        /// </summary>
        public SearchSettings Settings { get; set; } = new SearchSettings();

        /// <summary>
        /// Loads configuration from a JSON file
        /// </summary>
        public static SearchConfig LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            return JsonSerializer.Deserialize<SearchConfig>(json, options);
        }

        /// <summary>
        /// Saves configuration to a JSON file
        /// </summary>
        public void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Creates a default configuration template
        /// </summary>
        public static SearchConfig CreateDefault()
        {
            return new SearchConfig
            {
                GoogleAds = new GoogleAdsConfig
                {
                    ApiKey = "YOUR_GOOGLE_ADS_API_KEY",
                    CustomerId = "YOUR_GOOGLE_ADS_CUSTOMER_ID",
                    Enabled = false
                },
                YandexDirect = new YandexAdsConfig
                {
                    Token = "YOUR_YANDEX_DIRECT_TOKEN",
                    Login = "YOUR_YANDEX_LOGIN",
                    Enabled = false
                },
                Settings = new SearchSettings
                {
                    DefaultMaxResults = 10,
                    DefaultSearchInterval = 30,
                    EnableLogging = true
                }
            };
        }
    }

    /// <summary>
    /// Google Ads API configuration
    /// </summary>
    public class GoogleAdsConfig
    {
        /// <summary>
        /// Google Ads API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Google Ads Customer ID
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Whether this provider is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Additional settings
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Yandex Direct API configuration
    /// </summary>
    public class YandexAdsConfig
    {
        /// <summary>
        /// Yandex Direct OAuth Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Yandex Login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Whether this provider is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Additional settings
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// General search settings
    /// </summary>
    public class SearchSettings
    {
        /// <summary>
        /// Default maximum number of results per search
        /// </summary>
        public int DefaultMaxResults { get; set; } = 10;

        /// <summary>
        /// Default search interval in minutes for subscriptions
        /// </summary>
        public int DefaultSearchInterval { get; set; } = 30;

        /// <summary>
        /// Whether to enable logging
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Maximum concurrent searches
        /// </summary>
        public int MaxConcurrentSearches { get; set; } = 5;
    }
}
