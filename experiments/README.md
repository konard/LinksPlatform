# Price History Event Mapping Example

This directory contains an experimental implementation demonstrating how to map price history with events using the Links Platform's associative data model.

## Overview

The example shows how to create relationships between price points and market events, which can be useful for trading decisions by:

1. Recording price points at specific timestamps
2. Recording events at specific timestamps
3. Creating associations between price movements and events
4. Querying relationships to understand market behavior

## Conceptual Model

The implementation uses the associative data model (doublets/pairs) to represent:

- **Price Points**: Links connecting timestamps to price values
- **Events**: Links connecting timestamps to event descriptions
- **Associations**: Links connecting price points to events with relationship types

### Example Structure

```
PricePoint = Timestamp -> PriceValue
Event = Timestamp -> EventDescription
Association = PricePoint -> Event (with relationship type)
```

## Usage

To run the example:

```bash
cd experiments
csc PriceHistoryEventMapping.cs RunPriceHistoryExample.cs
./RunPriceHistoryExample
```

Or compile with:

```bash
dotnet build
```

## Example Output

The example demonstrates:

1. Recording several price points throughout a trading day
2. Recording market events (earnings announcements, Fed decisions, etc.)
3. Creating associations with meaningful relationship types:
   - "PrecededBy" - event occurred before price change
   - "FollowedBy" - price change followed the event
   - "InfluencedBy" - direct causal relationship
   - "RecoveryAfter" - price recovery after event

4. Querying:
   - All events associated with a price drop
   - All price points affected by a specific event

## Trading Decision Applications

By mapping price history with events in an associative data model, traders can:

1. **Pattern Recognition**: Identify which types of events historically correlate with specific price movements
2. **Predictive Modeling**: Build models based on event-price relationships to predict future movements
3. **Context Analysis**: Quickly query all relevant context when analyzing a price change
4. **Cause-Effect Tracking**: Track and visualize cause-and-effect relationships in the market
5. **Event Impact Assessment**: Measure the historical impact of similar events on price

## Implementation Notes

This is a simplified demonstration using in-memory data structures. A production implementation would:

- Use Platform.Data.Doublets for actual link storage
- Support persistence to disk
- Handle time-series data efficiently
- Support complex queries using the doublets API
- Integrate with real market data feeds
- Support multiple assets/symbols
- Include data validation and error handling

## Future Enhancements

Possible extensions to this concept:

1. **Multi-asset tracking**: Map events across multiple securities
2. **Technical indicators**: Include technical analysis indicators as additional event types
3. **Sentiment analysis**: Incorporate news sentiment as weighted associations
4. **Time-window queries**: Find all events within a time window before/after price movements
5. **Pattern matching**: Identify recurring event-price patterns
6. **Backtesting**: Use historical event-price mappings to test trading strategies

## Related Concepts

- Associative memory
- Knowledge graphs for financial data
- Event-driven trading systems
- Market microstructure analysis
- Causal inference in financial markets
