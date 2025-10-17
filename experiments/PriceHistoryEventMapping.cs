using System;
using System.Collections.Generic;

namespace LinksPlatform.Experiments
{
    /// <summary>
    /// Demonstrates how to map price history with events using Links Platform's associative data model.
    /// This can be useful for trading decisions by creating relationships between price movements and events.
    /// </summary>
    public class PriceHistoryEventMapping
    {
        // Simplified representation of Links using generic types
        // In a real implementation, this would use Platform.Data.Doublets
        public class Link
        {
            public ulong Id { get; set; }
            public ulong Source { get; set; }
            public ulong Target { get; set; }
            public string Description { get; set; }

            public override string ToString() => $"Link[{Id}]: {Source} -> {Target} ({Description})";
        }

        private Dictionary<ulong, Link> _links = new Dictionary<ulong, Link>();
        private ulong _nextId = 1;

        // Semantic markers (point links)
        private ulong _pricePointType;
        private ulong _eventType;
        private ulong _timestampType;
        private ulong _associationType;
        private ulong _priceValueType;
        private ulong _eventDescriptionType;

        public PriceHistoryEventMapping()
        {
            InitializeSemanticMarkers();
        }

        private void InitializeSemanticMarkers()
        {
            _pricePointType = CreatePointLink("PricePoint");
            _eventType = CreatePointLink("Event");
            _timestampType = CreatePointLink("Timestamp");
            _associationType = CreatePointLink("Association");
            _priceValueType = CreatePointLink("PriceValue");
            _eventDescriptionType = CreatePointLink("EventDescription");
        }

        private ulong CreatePointLink(string description)
        {
            var id = _nextId++;
            var link = new Link { Id = id, Source = id, Target = id, Description = description };
            _links[id] = link;
            return id;
        }

        private ulong CreateLink(ulong source, ulong target, string description = "")
        {
            var id = _nextId++;
            var link = new Link { Id = id, Source = source, Target = target, Description = description };
            _links[id] = link;
            return id;
        }

        /// <summary>
        /// Records a price point at a specific timestamp
        /// </summary>
        public ulong RecordPricePoint(DateTime timestamp, decimal price)
        {
            // Create a link for the timestamp (in real implementation, would encode the actual timestamp value)
            var timestampLink = CreateLink(_timestampType, _timestampType, $"Timestamp:{timestamp:yyyy-MM-dd HH:mm:ss}");

            // Create a link for the price value
            var priceValueLink = CreateLink(_priceValueType, _priceValueType, $"Price:{price}");

            // Create a price point that associates timestamp with price
            var pricePoint = CreateLink(timestampLink, priceValueLink, "PricePoint");

            // Mark this as a PricePoint type
            CreateLink(_pricePointType, pricePoint, "TypeOf");

            return pricePoint;
        }

        /// <summary>
        /// Records an event at a specific timestamp
        /// </summary>
        public ulong RecordEvent(DateTime timestamp, string eventDescription)
        {
            // Create a link for the timestamp
            var timestampLink = CreateLink(_timestampType, _timestampType, $"Timestamp:{timestamp:yyyy-MM-dd HH:mm:ss}");

            // Create a link for the event description
            var eventDescLink = CreateLink(_eventDescriptionType, _eventDescriptionType, $"Event:{eventDescription}");

            // Create an event that associates timestamp with description
            var eventLink = CreateLink(timestampLink, eventDescLink, "Event");

            // Mark this as an Event type
            CreateLink(_eventType, eventLink, "TypeOf");

            return eventLink;
        }

        /// <summary>
        /// Creates an association between a price point and an event
        /// This is the core of mapping price history with events
        /// </summary>
        public ulong AssociatePriceWithEvent(ulong pricePointId, ulong eventId, string relationshipType = "CorrelatedWith")
        {
            var association = CreateLink(pricePointId, eventId, relationshipType);
            CreateLink(_associationType, association, "TypeOf");
            return association;
        }

        /// <summary>
        /// Queries all events associated with a specific price point
        /// </summary>
        public List<Link> GetEventsForPricePoint(ulong pricePointId)
        {
            var results = new List<Link>();
            foreach (var link in _links.Values)
            {
                if (link.Source == pricePointId)
                {
                    // Check if this is an association
                    foreach (var typeLink in _links.Values)
                    {
                        if (typeLink.Source == _associationType && typeLink.Target == link.Id)
                        {
                            results.Add(link);
                            break;
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Queries all price points associated with a specific event
        /// </summary>
        public List<Link> GetPricePointsForEvent(ulong eventId)
        {
            var results = new List<Link>();
            foreach (var link in _links.Values)
            {
                if (link.Target == eventId)
                {
                    // Check if this is an association
                    foreach (var typeLink in _links.Values)
                    {
                        if (typeLink.Source == _associationType && typeLink.Target == link.Id)
                        {
                            results.Add(link);
                            break;
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Example usage demonstrating the mapping of price history with events
        /// </summary>
        public static void RunExample()
        {
            Console.WriteLine("=== Price History Event Mapping Example ===\n");

            var mapper = new PriceHistoryEventMapping();

            // Record some price points
            var price1 = mapper.RecordPricePoint(new DateTime(2025, 1, 15, 9, 0, 0), 100.50m);
            var price2 = mapper.RecordPricePoint(new DateTime(2025, 1, 15, 10, 0, 0), 102.75m);
            var price3 = mapper.RecordPricePoint(new DateTime(2025, 1, 15, 11, 0, 0), 98.25m);
            var price4 = mapper.RecordPricePoint(new DateTime(2025, 1, 15, 12, 0, 0), 101.00m);

            Console.WriteLine("Recorded price points:");
            Console.WriteLine($"  {mapper._links[price1]}");
            Console.WriteLine($"  {mapper._links[price2]}");
            Console.WriteLine($"  {mapper._links[price3]}");
            Console.WriteLine($"  {mapper._links[price4]}");
            Console.WriteLine();

            // Record some events
            var event1 = mapper.RecordEvent(new DateTime(2025, 1, 15, 9, 30, 0), "CEO announces quarterly earnings beat");
            var event2 = mapper.RecordEvent(new DateTime(2025, 1, 15, 10, 45, 0), "Federal Reserve announces interest rate decision");
            var event3 = mapper.RecordEvent(new DateTime(2025, 1, 15, 11, 30, 0), "Major competitor files for bankruptcy");

            Console.WriteLine("Recorded events:");
            Console.WriteLine($"  {mapper._links[event1]}");
            Console.WriteLine($"  {mapper._links[event2]}");
            Console.WriteLine($"  {mapper._links[event3]}");
            Console.WriteLine();

            // Associate price movements with events
            mapper.AssociatePriceWithEvent(price1, event1, "PrecededBy");
            mapper.AssociatePriceWithEvent(price2, event1, "FollowedBy");
            mapper.AssociatePriceWithEvent(price3, event2, "InfluencedBy");
            mapper.AssociatePriceWithEvent(price3, event3, "PrecededBy");
            mapper.AssociatePriceWithEvent(price4, event3, "RecoveryAfter");

            Console.WriteLine("Created associations between price points and events");
            Console.WriteLine();

            // Query: What events are associated with price3 (the drop to 98.25)?
            Console.WriteLine($"Events associated with price drop at {mapper._links[price3].Description}:");
            var eventsForPrice3 = mapper.GetEventsForPricePoint(price3);
            foreach (var assoc in eventsForPrice3)
            {
                Console.WriteLine($"  -> {assoc.Description}: {mapper._links[assoc.Target].Description}");
            }
            Console.WriteLine();

            // Query: What price points are associated with event2 (Fed decision)?
            Console.WriteLine($"Price points associated with {mapper._links[event2].Description}:");
            var pricesForEvent2 = mapper.GetPricePointsForEvent(event2);
            foreach (var assoc in pricesForEvent2)
            {
                Console.WriteLine($"  -> {assoc.Description}: {mapper._links[assoc.Source].Description}");
            }
            Console.WriteLine();

            Console.WriteLine("Total links created: " + mapper._links.Count);
            Console.WriteLine("\n=== Trading Decision Insights ===");
            Console.WriteLine("By mapping price history with events, traders can:");
            Console.WriteLine("  1. Identify which events historically correlate with price movements");
            Console.WriteLine("  2. Build predictive models based on event-price relationships");
            Console.WriteLine("  3. Quickly query all relevant context when analyzing a price change");
            Console.WriteLine("  4. Track cause-and-effect relationships in the market");
        }
    }
}
