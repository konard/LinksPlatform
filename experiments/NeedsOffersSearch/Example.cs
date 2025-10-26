using System;
using System.Threading.Tasks;

namespace LinksPlatform.NeedsOffersSearch
{
    /// <summary>
    /// Example demonstrating the Needs and Offers search system
    /// </summary>
    public class Example
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== LinksPlatform Needs & Offers Search - Example ===\n");

            // Step 1: Load or create configuration
            SearchConfig config;
            string configPath = "search-config.json";

            try
            {
                config = SearchConfig.LoadFromFile(configPath);
                Console.WriteLine($"Loaded configuration from {configPath}");
            }
            catch
            {
                config = SearchConfig.CreateDefault();
                config.SaveToFile(configPath);
                Console.WriteLine($"Created default configuration file at {configPath}");
                Console.WriteLine("Please update the configuration with your API credentials and set Enabled=true\n");
            }

            // Step 2: Initialize the manager
            var manager = new NeedsOffersManager();

            // Step 3: Add search providers
            if (config.GoogleAds.Enabled)
            {
                var googleProvider = new GoogleAdsSearchProvider(
                    config.GoogleAds.ApiKey,
                    config.GoogleAds.CustomerId
                );
                manager.AddSearchProvider(googleProvider);
                Console.WriteLine("Added Google Ads search provider");
            }

            if (config.YandexDirect.Enabled)
            {
                var yandexProvider = new YandexAdsSearchProvider(
                    config.YandexDirect.Token,
                    config.YandexDirect.Login
                );
                manager.AddSearchProvider(yandexProvider);
                Console.WriteLine("Added Yandex Direct search provider");
            }

            // For demonstration, add mock providers even if not configured
            if (!config.GoogleAds.Enabled && !config.YandexDirect.Enabled)
            {
                Console.WriteLine("\nNote: No providers configured. Using mock providers for demonstration.\n");
                manager.AddSearchProvider(new GoogleAdsSearchProvider("mock-key", "mock-customer"));
                manager.AddSearchProvider(new YandexAdsSearchProvider("mock-token", "mock-login"));
            }

            // Step 4: Subscribe to search results
            manager.SearchResultFound += (sender, e) =>
            {
                Console.WriteLine($"\n*** New Match Found for Listing {e.ListingId} ***");
                Console.WriteLine($"  Source: {e.Result.Source}");
                Console.WriteLine($"  Contact: {e.Result.ContactInfo}");
                Console.WriteLine($"  Description: {e.Result.Description}");
                Console.WriteLine($"  Relevance: {e.Result.RelevanceScore:P0}");
                Console.WriteLine($"  Timestamp: {e.Result.Timestamp}");
            };

            // Step 5: Create example listings
            Console.WriteLine("\n=== Creating Example Listings ===\n");

            // Example 1: User needs a web developer
            var need1 = new NeedOffer
            {
                UserId = "user123",
                Type = ListingType.Need,
                Description = "Need experienced full-stack web developer for SaaS project",
                Keywords = new[] { "web developer", "full-stack", "react", "node.js", "saas" },
                EnableSearch = false, // We'll enable it manually
                SearchBudget = 100,
                ContactInfo = "contact@example.com"
            };

            await manager.RegisterListingAsync(need1);
            Console.WriteLine($"Created Need: {need1.Description}");

            // Example 2: User offers graphic design services
            var offer1 = new NeedOffer
            {
                UserId = "user456",
                Type = ListingType.Offer,
                Description = "Professional graphic designer offering logo and branding services",
                Keywords = new[] { "graphic design", "logo design", "branding", "photoshop", "illustrator" },
                EnableSearch = false,
                SearchBudget = 50,
                ContactInfo = "designer@example.com"
            };

            await manager.RegisterListingAsync(offer1);
            Console.WriteLine($"Created Offer: {offer1.Description}");

            // Step 6: Perform one-time search
            Console.WriteLine("\n=== Performing One-Time Search for Need ===\n");
            var results = await manager.SearchOnceAsync(need1.Id, maxResultsPerProvider: 3);

            Console.WriteLine($"Found {((System.Collections.Generic.List<SearchResult>)results).Count} potential matches:");
            foreach (var result in results)
            {
                Console.WriteLine($"\n  - {result.ContactInfo}");
                Console.WriteLine($"    Source: {result.Source}");
                Console.WriteLine($"    Relevance: {result.RelevanceScore:P0}");
                Console.WriteLine($"    Description: {result.Description}");
            }

            // Step 7: Enable continuous search
            Console.WriteLine("\n=== Starting Continuous Search ===\n");
            need1.EnableSearch = true;
            await manager.UpdateListingAsync(need1);
            Console.WriteLine("Continuous search started for need listing");
            Console.WriteLine("The system will now periodically check for new matches...");

            // Step 8: Demonstrate subscription management
            Console.WriteLine("\n=== Subscription Management ===\n");
            await manager.StartSearchingAsync(offer1.Id);
            Console.WriteLine("Started search for offer listing");

            // Wait a bit to simulate monitoring
            Console.WriteLine("\nMonitoring for results (waiting 5 seconds)...");
            await Task.Delay(5000);

            // Clean up
            Console.WriteLine("\n=== Cleanup ===\n");
            await manager.StopSearchingAsync(need1.Id);
            await manager.StopSearchingAsync(offer1.Id);
            Console.WriteLine("Stopped all searches");

            // Step 9: Display summary
            Console.WriteLine("\n=== Summary ===\n");
            var allListings = manager.GetAllListings();
            Console.WriteLine($"Total listings: {System.Linq.Enumerable.Count(allListings)}");

            Console.WriteLine("\n=== Example Complete ===");
            Console.WriteLine("\nThis prototype demonstrates:");
            Console.WriteLine("1. Configuration management for API credentials");
            Console.WriteLine("2. Multiple search provider support (Google Ads, Yandex Direct)");
            Console.WriteLine("3. One-time and continuous search capabilities");
            Console.WriteLine("4. Event-driven notification of new matches");
            Console.WriteLine("5. Listing management (create, update, delete)");
            Console.WriteLine("\nNext steps for production:");
            Console.WriteLine("- Implement actual API integration with Google Ads API");
            Console.WriteLine("- Implement actual API integration with Yandex Direct API");
            Console.WriteLine("- Add persistent storage for listings and results");
            Console.WriteLine("- Implement rate limiting and cost management");
            Console.WriteLine("- Add authentication and authorization");
            Console.WriteLine("- Create web UI for managing listings");
        }
    }
}
