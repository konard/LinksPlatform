# Needs & Offers Search System

## Overview

This prototype implementation demonstrates the concept described in [Issue #597](https://github.com/konard/LinksPlatform/issues/597): using advertising APIs (Google Ads and Yandex Direct) to search for people who can satisfy user needs or who need what users offer.

## Concept

When a user registers a **need** in the system, the system can automatically begin searching for people who **offer** matching services using advertising APIs. Similarly, when a user registers an **offer**, the system searches for people who **need** that service.

This approach leverages the extensive audience data and keyword matching capabilities of advertising platforms to connect people with matching needs and offers.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  User Application                        │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│           NeedsOffersManager                             │
│  - Register needs/offers                                 │
│  - Manage search subscriptions                           │
│  - Coordinate multiple providers                         │
└───┬──────────────────────┬──────────────────────────────┘
    │                      │
    ▼                      ▼
┌──────────────┐    ┌──────────────┐
│   Google Ads │    │Yandex Direct │
│   Provider   │    │   Provider   │
└──────┬───────┘    └──────┬───────┘
       │                   │
       ▼                   ▼
┌──────────────────────────────────┐
│     Advertising APIs              │
│  - Google Ads API                 │
│  - Yandex Direct API              │
└───────────────────────────────────┘
```

## Components

### 1. **ISearchProvider** (`ISearchProvider.cs`)
Core interface for search providers. Any advertising platform can be integrated by implementing this interface.

Key methods:
- `SearchAsync()` - Perform one-time search
- `SubscribeAsync()` - Set up continuous monitoring
- `UnsubscribeAsync()` - Cancel monitoring

### 2. **GoogleAdsSearchProvider** (`GoogleAdsSearchProvider.cs`)
Implementation for Google Ads API integration.

Features:
- Keyword-based search
- Audience targeting
- Low-frequency monitoring for cost efficiency

### 3. **YandexAdsSearchProvider** (`YandexAdsSearchProvider.cs`)
Implementation for Yandex Direct API integration.

Features:
- Russian market focus
- Keyword research
- Regional targeting

### 4. **NeedsOffersManager** (`NeedsOffersManager.cs`)
Central manager coordinating all operations.

Capabilities:
- Register/update/remove listings
- Start/stop searches
- Event-driven notifications
- Multi-provider coordination

### 5. **Configuration System** (`SearchConfig.cs`)
Manages API credentials and settings.

Includes:
- API key management
- Provider-specific settings
- Search parameters

## Usage

### Basic Example

```csharp
// 1. Initialize manager
var manager = new NeedsOffersManager();

// 2. Add search providers
var googleProvider = new GoogleAdsSearchProvider(apiKey, customerId);
var yandexProvider = new YandexAdsSearchProvider(token, login);
manager.AddSearchProvider(googleProvider);
manager.AddSearchProvider(yandexProvider);

// 3. Create a need
var need = new NeedOffer
{
    UserId = "user123",
    Type = ListingType.Need,
    Description = "Need web developer for SaaS project",
    Keywords = new[] { "web developer", "react", "node.js" },
    EnableSearch = true
};

// 4. Register and start searching
await manager.RegisterListingAsync(need);

// 5. Handle results
manager.SearchResultFound += (sender, e) =>
{
    Console.WriteLine($"Match found: {e.Result.ContactInfo}");
};
```

### Running the Example

```bash
cd experiments/NeedsOffersSearch
dotnet run
```

The example will:
1. Create a default configuration file (`search-config.json`)
2. Demonstrate one-time search
3. Show continuous monitoring
4. Display results as they arrive

### Configuration

Edit `search-config.json`:

```json
{
  "GoogleAds": {
    "ApiKey": "YOUR_GOOGLE_ADS_API_KEY",
    "CustomerId": "YOUR_CUSTOMER_ID",
    "Enabled": true
  },
  "YandexDirect": {
    "Token": "YOUR_YANDEX_TOKEN",
    "Login": "YOUR_LOGIN",
    "Enabled": true
  },
  "Settings": {
    "DefaultMaxResults": 10,
    "DefaultSearchInterval": 30,
    "EnableLogging": true
  }
}
```

## Cost Management

Low-frequency requests are cheap in advertising APIs. The system is designed to:
- Use periodic checks (default: 30 minutes)
- Limit results per query
- Allow budget caps per listing
- Support manual triggering

## Implementation Status

### ✅ Completed (Prototype)
- Core interfaces and abstractions
- Provider pattern for multiple APIs
- Configuration management
- Event-driven architecture
- Basic example code

### 🚧 To Be Implemented (Production)
- [ ] Actual Google Ads API integration
- [ ] Actual Yandex Direct API integration
- [ ] Persistent storage (database)
- [ ] Rate limiting and quotas
- [ ] Cost tracking and budgeting
- [ ] Authentication and authorization
- [ ] Web UI for managing listings
- [ ] Notification system (email, SMS, push)
- [ ] Analytics and reporting
- [ ] Machine learning for better matching

## API References

### Google Ads API
- Documentation: https://developers.google.com/google-ads/api/docs
- Libraries: https://developers.google.com/google-ads/api/docs/client-libs
- Keyword Planner: https://developers.google.com/google-ads/api/docs/keyword-planning

### Yandex Direct API
- Documentation: https://yandex.ru/dev/direct/doc/
- OAuth: https://yandex.ru/dev/oauth/doc/
- Keyword Research: https://yandex.ru/dev/direct/doc/ref-v5/

## Security Considerations

1. **API Credentials**: Store securely, never commit to version control
2. **Rate Limiting**: Respect API quotas
3. **Cost Control**: Implement budget caps
4. **Data Privacy**: Handle user data responsibly
5. **Authentication**: Implement proper user authentication

## Future Enhancements

1. **AI Matching**: Use machine learning to improve match quality
2. **Multi-platform**: Add more advertising platforms (Facebook, LinkedIn)
3. **Blockchain**: Consider decentralized identity and reputation
4. **Smart Contracts**: Automate agreements between parties
5. **Integration**: Connect with LinksPlatform's associative storage

## Contributing

This is a prototype for Issue #597. Contributions are welcome:
- Improve matching algorithms
- Add more search providers
- Enhance cost management
- Build web interface
- Write tests

## License

This code is part of the LinksPlatform project and follows the same license terms.

## Contact

For questions or discussions, refer to Issue #597 on the main repository.
