using System;

namespace Platform.BlackBox
{
    /// <summary>
    /// Represents a single activity event captured by the black box recorder.
    /// </summary>
    public class ActivityEvent
    {
        /// <summary>
        /// Timestamp when the event occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Type of the activity (e.g., "UserAction", "SystemEvent", "Error", "Exception").
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Source component or module that generated the event.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Detailed description of the event.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity level (e.g., "Info", "Warning", "Error", "Critical").
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Additional metadata as key-value pairs (serialized as JSON).
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Stack trace if this event is an exception.
        /// </summary>
        public string StackTrace { get; set; }

        public ActivityEvent()
        {
            Timestamp = DateTime.UtcNow;
            Severity = "Info";
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Severity}] [{EventType}] {Source}: {Description}";
        }
    }
}
