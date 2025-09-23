using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    public class SimpleEventStore : IDisposable
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;
        private readonly UnicodeMap _unicodeMap;
        private readonly ulong _eventTypeLink;
        private readonly ulong _timestampLink;
        private readonly ulong _processIdLink;
        private readonly ulong _messageLink;
        private readonly ulong _uuidLink;
        private readonly object _lock = new object();
        private volatile bool _disposed = false;

        public SimpleEventStore(ILinks<ulong> links, Sequences sequences, UnicodeMap unicodeMap)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
            _unicodeMap = unicodeMap ?? throw new ArgumentNullException(nameof(unicodeMap));

            // Initialize system links for event structure
            _eventTypeLink = _unicodeMap.FromString("EventType");
            _timestampLink = _unicodeMap.FromString("Timestamp");
            _processIdLink = _unicodeMap.FromString("ProcessId");
            _messageLink = _unicodeMap.FromString("Message");
            _uuidLink = _unicodeMap.FromString("UUID");
        }

        public ulong StoreEvent(string eventType, string message, string processId = null, Guid? eventId = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SimpleEventStore));

            lock (_lock)
            {
                var uuid = eventId ?? CreateUUIDv7();
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var currentProcessId = processId ?? Environment.ProcessId.ToString();

                // Create links for event data
                var eventTypeValue = _unicodeMap.FromString(eventType);
                var messageValue = _unicodeMap.FromString(message);
                var processIdValue = _unicodeMap.FromString(currentProcessId);
                var timestampValue = _unicodeMap.FromString(timestamp.ToString());
                var uuidValue = _unicodeMap.FromString(uuid.ToString());

                // Create event structure as a sequence of links
                var eventStructure = new[]
                {
                    _links.Create(_eventTypeLink, eventTypeValue),
                    _links.Create(_timestampLink, timestampValue),
                    _links.Create(_processIdLink, processIdValue),
                    _links.Create(_messageLink, messageValue),
                    _links.Create(_uuidLink, uuidValue)
                };

                // Store as a sequence
                return _sequences.Create(eventStructure);
            }
        }

        public IEnumerable<SimpleEventRecord> GetEvents(string eventType = null, string processId = null, DateTimeOffset? fromTime = null, DateTimeOffset? toTime = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SimpleEventStore));

            var events = new List<SimpleEventRecord>();

            // Get all sequences and check which ones are events
            try
            {
                var count = _links.Count();
                for (ulong i = 1; i <= count; i++)
                {
                    var eventRecord = TryParseEventSequence(i);
                    if (eventRecord != null)
                    {
                        // Apply filters
                        if (eventType != null && eventRecord.EventType != eventType)
                            continue;
                        if (processId != null && eventRecord.ProcessId != processId)
                            continue;
                        if (fromTime.HasValue && eventRecord.Timestamp < fromTime.Value)
                            continue;
                        if (toTime.HasValue && eventRecord.Timestamp > toTime.Value)
                            continue;

                        events.Add(eventRecord);
                    }
                }
            }
            catch
            {
                // Ignore errors during parsing
            }

            // Sort by timestamp (UUIDv7 contains timestamp so can also sort by UUID)
            return events.OrderBy(e => e.Timestamp);
        }

        public IEnumerable<SimpleEventRecord> GetEventsByTimeRange(DateTimeOffset from, DateTimeOffset to)
        {
            return GetEvents(fromTime: from, toTime: to);
        }

        public IEnumerable<SimpleEventRecord> GetEventsByProcess(string processId)
        {
            return GetEvents(processId: processId);
        }

        public IEnumerable<SimpleEventRecord> GetEventsByType(string eventType)
        {
            return GetEvents(eventType: eventType);
        }

        private SimpleEventRecord TryParseEventSequence(ulong sequenceLink)
        {
            try
            {
                if (_links.Exists(sequenceLink))
                {
                    var source = _links.GetSource(sequenceLink);
                    var target = _links.GetTarget(sequenceLink);

                    var eventRecord = new SimpleEventRecord();
                    bool foundEventType = false;

                    // Try to parse the link as event data
                    if (source == _eventTypeLink)
                    {
                        eventRecord.EventType = _unicodeMap.ToStringOrDefault(target);
                        foundEventType = true;
                    }
                    else if (source == _timestampLink)
                    {
                        var timestampStr = _unicodeMap.ToStringOrDefault(target);
                        if (long.TryParse(timestampStr, out var timestamp))
                        {
                            eventRecord.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                        }
                    }
                    else if (source == _processIdLink)
                    {
                        eventRecord.ProcessId = _unicodeMap.ToStringOrDefault(target);
                    }
                    else if (source == _messageLink)
                    {
                        eventRecord.Message = _unicodeMap.ToStringOrDefault(target);
                    }
                    else if (source == _uuidLink)
                    {
                        var uuidStr = _unicodeMap.ToStringOrDefault(target);
                        if (Guid.TryParse(uuidStr, out var guid))
                        {
                            eventRecord.EventId = guid;
                        }
                    }

                    return foundEventType && eventRecord.IsValid() ? eventRecord : null;
                }
            }
            catch
            {
                // Ignore parse errors
            }

            return null;
        }

        private static Guid CreateUUIDv7()
        {
            // UUIDv7 implementation - timestamp-based UUID
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timestampBytes = BitConverter.GetBytes(timestamp);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timestampBytes, 0, 6); // Take first 6 bytes for 48-bit timestamp

            var randomBytes = new byte[10];
            new Random().NextBytes(randomBytes);

            var uuidBytes = new byte[16];

            // Copy timestamp (48 bits)
            Array.Copy(timestampBytes, 0, uuidBytes, 0, 6);

            // Copy random data
            Array.Copy(randomBytes, 0, uuidBytes, 6, 10);

            // Set version (7) and variant bits
            uuidBytes[6] = (byte)((uuidBytes[6] & 0x0F) | 0x70); // Version 7
            uuidBytes[8] = (byte)((uuidBytes[8] & 0x3F) | 0x80); // Variant 10

            return new Guid(uuidBytes);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }

    public class SimpleEventRecord
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
        public string ProcessId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(EventType) &&
                   !string.IsNullOrEmpty(Message) &&
                   !string.IsNullOrEmpty(ProcessId);
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{ProcessId}] [{EventType}] {Message} (ID: {EventId})";
        }
    }
}