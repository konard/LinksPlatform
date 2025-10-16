# API Bridge: Practical Examples

This document provides practical examples of how the API Bridge architecture can be used to interact with various APIs through different modalities.

## Table of Contents

- [Example 1: Text-Based Weather Query](#example-1-text-based-weather-query)
- [Example 2: Voice-Controlled Database Operations](#example-2-voice-controlled-database-operations)
- [Example 3: Gesture-Based Application Control](#example-3-gesture-based-application-control)
- [Example 4: Multi-API Composition](#example-4-multi-api-composition)
- [Example 5: Links Platform Integration](#example-5-links-platform-integration)

## Example 1: Text-Based Weather Query

### User Input

```
"What's the weather like in Tokyo?"
```

### Processing Flow

```
Input: Text → "What's the weather like in Tokyo?"
    ↓
Intent Recognition:
    Intent: GetWeather
    Entity: Location = "Tokyo"
    ↓
API Discovery:
    Found: WeatherAPI v1.0
    Endpoint: GET /current?location={location}
    ↓
API Translation:
    URL: https://api.weather.com/v1/current?location=Tokyo
    Headers: X-API-Key: {api_key}
    ↓
API Execution:
    HTTP GET Request
    Response: {
        "temperature": 22,
        "conditions": "Partly Cloudy",
        "humidity": 65,
        "wind_speed": 15
    }
    ↓
Response Formatting:
    "The weather in Tokyo is currently 22°C and partly cloudy,
     with 65% humidity and winds at 15 km/h."
    ↓
Output: Text
```

### Implementation Example (Pseudocode)

```csharp
public class WeatherQueryHandler : IIntentHandler
{
    private readonly IApiRegistry _apiRegistry;
    private readonly IApiExecutor _apiExecutor;

    public async Task<Response> HandleAsync(Intent intent)
    {
        // Extract parameters
        var location = intent.GetEntity("location");

        // Discover API
        var api = await _apiRegistry.FindAsync("WeatherAPI");
        var endpoint = api.GetEndpoint("GetCurrent");

        // Translate to API call
        var request = new ApiRequest
        {
            Url = endpoint.BuildUrl(new { location }),
            Method = HttpMethod.Get,
            Headers = endpoint.GetAuthHeaders()
        };

        // Execute
        var response = await _apiExecutor.ExecuteAsync(request);

        // Format response
        return FormatWeatherResponse(response.Data);
    }
}
```

## Example 2: Voice-Controlled Database Operations

### User Input (Voice)

```
*User speaks*: "Show me all orders from last week"
```

### Processing Flow

```
Input: Voice → Audio Stream
    ↓
Speech-to-Text:
    "Show me all orders from last week"
    ↓
Intent Recognition:
    Intent: DatabaseQuery
    Entity: Table = "orders"
    Constraint: TimeRange = "last week"
    ↓
API Discovery:
    Found: OrdersDatabase API
    Method: QueryOrders
    ↓
API Translation:
    Query: SELECT * FROM orders
           WHERE created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
    ↓
API Execution:
    Database Query Execution
    Results: [
        {id: 1001, customer: "John", total: 150.00, date: "2024-10-10"},
        {id: 1002, customer: "Jane", total: 200.00, date: "2024-10-12"},
        ...
    ]
    ↓
Response Formatting:
    "I found 15 orders from last week.
     The total value is $2,450.00."
    +
    [Table with order details]
    ↓
Output: Voice (TTS) + Visual (Table)
```

### Implementation Example

```csharp
public class DatabaseQueryHandler : IIntentHandler
{
    private readonly ISpeechRecognizer _speechRecognizer;
    private readonly IDatabaseConnector _database;
    private readonly ITextToSpeech _tts;

    public async Task<Response> HandleAsync(AudioStream audio)
    {
        // Convert speech to text
        var text = await _speechRecognizer.RecognizeAsync(audio);

        // Recognize intent
        var intent = await RecognizeIntent(text);

        // Build query
        var query = BuildSqlQuery(intent);

        // Execute
        var results = await _database.QueryAsync(query);

        // Format response
        var summary = $"I found {results.Count} orders from last week. " +
                     $"The total value is {CalculateTotal(results):C}.";

        // Generate voice output
        var audioResponse = await _tts.SynthesizeAsync(summary);

        return new Response
        {
            Audio = audioResponse,
            Visual = FormatAsTable(results),
            Data = results
        };
    }
}
```

## Example 3: Gesture-Based Application Control

### User Input (Gesture)

```
*User performs a "swipe right" gesture*
```

### Processing Flow

```
Input: Gesture → Swipe Right (camera/sensor data)
    ↓
Gesture Recognition:
    Gesture: SwipeRight
    Context: ApplicationManagement
    ↓
Intent Mapping:
    Intent: DeployApplication
    Target: CurrentProject
    ↓
API Discovery:
    Found: DeploymentService API
    Method: TriggerDeploy
    ↓
API Translation:
    URL: https://deploy.service.com/v1/deploy
    Body: {
        "project_id": "current",
        "environment": "production",
        "triggered_by": "gesture"
    }
    ↓
API Execution:
    HTTP POST Request
    Response: {
        "deployment_id": "dep_12345",
        "status": "started",
        "eta": "5 minutes"
    }
    ↓
Response Formatting:
    "Deployment started successfully.
     Estimated completion: 5 minutes."
    +
    [Progress indicator]
    ↓
Output: Visual + Haptic Feedback
```

### Gesture Mapping Configuration

```json
{
  "gesture_mappings": [
    {
      "gesture": "swipe_right",
      "context": "application_management",
      "intent": "deploy_application",
      "confirmation_required": true,
      "parameters": {
        "environment": "production"
      }
    },
    {
      "gesture": "swipe_left",
      "context": "application_management",
      "intent": "rollback_deployment",
      "confirmation_required": true
    },
    {
      "gesture": "circle_clockwise",
      "context": "application_management",
      "intent": "restart_service",
      "confirmation_required": false
    }
  ]
}
```

## Example 4: Multi-API Composition

### User Input

```
"Find restaurants near me that are open now and have good reviews"
```

### Processing Flow

```
Input: Text
    ↓
Intent Recognition:
    Primary Intent: FindRestaurants
    Constraints:
        - Location: "near me" (requires geolocation)
        - Status: "open now" (requires current time + hours API)
        - Quality: "good reviews" (requires ratings API)
    ↓
API Discovery (Multiple APIs needed):
    1. GeolocationAPI - Get current location
    2. RestaurantAPI - Search restaurants
    3. ReviewsAPI - Get ratings/reviews
    4. BusinessHoursAPI - Check if open
    ↓
API Execution Plan (Sequential + Parallel):

    Step 1: Get Location
        GET https://geo.api.com/v1/location
        → {lat: 35.6762, lon: 139.6503}

    Step 2: Find Restaurants (Parallel Execution)
        a) GET https://restaurant.api.com/v1/search?lat=35.6762&lon=139.6503
           → [restaurant_ids: 101, 102, 103, ...]

        b) For each restaurant in parallel:
           - GET https://reviews.api.com/v1/ratings?restaurant_id={id}
           - GET https://hours.api.com/v1/check?business_id={id}&time=now

    Step 3: Filter and Rank
        - Keep only restaurants open now
        - Keep only restaurants with rating >= 4.0
        - Sort by rating
    ↓
Response Aggregation:
    Results: [
        {
            name: "Sushi Place",
            rating: 4.5,
            distance: "500m",
            status: "Open until 10 PM"
        },
        {
            name: "Ramen House",
            rating: 4.3,
            distance: "800m",
            status: "Open 24 hours"
        },
        ...
    ]
    ↓
Response Formatting:
    "I found 5 restaurants near you that are open now with good reviews:

     1. Sushi Place (4.5 stars, 500m away)
        Open until 10 PM

     2. Ramen House (4.3 stars, 800m away)
        Open 24 hours

     ..."
    ↓
Output: Text + Map View
```

### Implementation Example

```csharp
public class RestaurantFinderHandler : IIntentHandler
{
    private readonly IApiOrchestrator _orchestrator;

    public async Task<Response> HandleAsync(Intent intent)
    {
        // Step 1: Get current location
        var location = await _orchestrator.CallAsync(
            "GeolocationAPI",
            "GetCurrentLocation"
        );

        // Step 2: Find restaurants
        var restaurants = await _orchestrator.CallAsync(
            "RestaurantAPI",
            "Search",
            new { lat = location.Latitude, lon = location.Longitude }
        );

        // Step 3: Get reviews and hours in parallel
        var enrichedData = await _orchestrator.CallManyInParallelAsync(
            restaurants.Select(r => new[]
            {
                new ApiCall("ReviewsAPI", "GetRating", new { restaurant_id = r.Id }),
                new ApiCall("BusinessHoursAPI", "CheckOpen", new { business_id = r.Id })
            })
        );

        // Step 4: Filter and combine
        var results = restaurants
            .Zip(enrichedData, (restaurant, data) => new
            {
                Restaurant = restaurant,
                Rating = data[0],
                Hours = data[1]
            })
            .Where(x => x.Hours.IsOpen && x.Rating.Score >= 4.0)
            .OrderByDescending(x => x.Rating.Score)
            .Take(5)
            .ToList();

        return FormatRestaurantList(results);
    }
}
```

## Example 5: Links Platform Integration

### Scenario: Building an API Knowledge Graph

In the Links Platform, APIs can be represented as interconnected links, creating a semantic network of API capabilities.

### Links Representation

```
# API Definition Links
[WeatherAPI] → [hasEndpoint] → [WeatherEndpoint_Current]
[WeatherAPI] → [hasEndpoint] → [WeatherEndpoint_Forecast]
[WeatherAPI] → [requiresAuth] → [APIKey]

# Endpoint Details
[WeatherEndpoint_Current] → [hasPath] → ["/current"]
[WeatherEndpoint_Current] → [hasMethod] → [GET]
[WeatherEndpoint_Current] → [hasParameter] → [LocationParameter]

# Parameter Details
[LocationParameter] → [hasName] → ["location"]
[LocationParameter] → [hasType] → [String]
[LocationParameter] → [isRequired] → [True]

# Intent Mapping
[Intent_GetWeather] → [mapsToAPI] → [WeatherAPI]
[Intent_GetWeather] → [mapsToEndpoint] → [WeatherEndpoint_Current]
[Intent_GetWeather] → [requiresEntity] → [Location]

# Entity Extraction
[UserInput_"weather_in_Tokyo"] → [containsIntent] → [Intent_GetWeather]
[UserInput_"weather_in_Tokyo"] → [containsEntity] → [Location_Tokyo]
[Location_Tokyo] → [mapsToParameter] → [LocationParameter]
```

### Query Example Using Links

```csharp
// Find API for weather intent
var weatherIntent = links.Search(
    source: any,
    linker: links.GetLink("mapsToAPI"),
    target: any
).Where(link =>
    links.GetMeaning(link.Source).Contains("weather")
).First();

var apiId = links.GetTarget(weatherIntent);

// Get endpoint details
var endpoint = links.Search(
    source: apiId,
    linker: links.GetLink("hasEndpoint"),
    target: any
).First().Target;

// Build API call from links
var apiCall = BuildApiCallFromLinks(endpoint);
```

### Dynamic API Composition with Links

```csharp
public class LinksBasedApiComposer
{
    private readonly ILinks _links;

    public async Task<Response> ComposeAndExecute(string userInput)
    {
        // 1. Find matching intent
        var intent = FindIntent(userInput);

        // 2. Traverse links to find required APIs
        var requiredApis = _links.Each(
            (link) =>
            {
                if (links.GetSource(link) == intent &&
                    links.GetLinker(link) == _links.GetLink("requiresAPI"))
                {
                    return links.GetTarget(link);
                }
                return null;
            }
        );

        // 3. Build execution plan from links
        var executionPlan = BuildExecutionPlanFromLinks(requiredApis);

        // 4. Execute
        return await ExecutePlan(executionPlan);
    }

    private ExecutionPlan BuildExecutionPlanFromLinks(IEnumerable<Link> apis)
    {
        var plan = new ExecutionPlan();

        foreach (var api in apis)
        {
            // Check dependencies
            var dependencies = _links.Each(
                (link) =>
                {
                    if (links.GetSource(link) == api &&
                        links.GetLinker(link) == _links.GetLink("dependsOn"))
                    {
                        return links.GetTarget(link);
                    }
                    return null;
                }
            );

            // Add to plan with dependency info
            plan.AddStep(api, dependencies);
        }

        return plan.Optimize(); // Parallelize independent steps
    }
}
```

## Example 6: Cross-Modal Interaction

### Scenario: Voice Input → Multiple Output Modalities

```
User speaks: "Show me sales data for last quarter"
    ↓
Processing:
    - Speech to Text: "Show me sales data for last quarter"
    - Intent: GetSalesReport, Period: Q3-2024
    - API Call: SalesDatabase.GetQuarterlySales(2024, 3)
    ↓
Response Generated:
    1. Voice: "Q3 sales totaled $1.2 million, up 15% from Q2"
    2. Visual: Chart showing sales trend
    3. Text: Detailed table with breakdown
    4. Haptic: Vibration pattern indicating positive trend
    ↓
Delivery:
    - Voice output via speaker
    - Chart displayed on screen
    - Table sent to user's email
    - Haptic feedback via smartwatch
```

### Configuration

```yaml
response_modalities:
  voice:
    enabled: true
    provider: "azure_tts"
    voice: "en-US-neural"

  visual:
    enabled: true
    formats:
      - charts
      - tables
      - maps

  haptic:
    enabled: true
    device: "smartwatch"
    patterns:
      positive_trend: "gentle_pulse"
      negative_trend: "alert_pattern"

  text:
    enabled: true
    delivery:
      - screen
      - email
      - sms
```

## Conclusion

These examples demonstrate the versatility and power of the API Bridge architecture. By supporting multiple input modalities, intelligent API discovery and composition, and flexible output formatting, the system enables natural and intuitive interaction with any API, regardless of its original implementation.

The integration with the Links Platform provides a powerful semantic layer that enables dynamic API discovery, intelligent composition, and context-aware API selection based on user intent and available resources.
