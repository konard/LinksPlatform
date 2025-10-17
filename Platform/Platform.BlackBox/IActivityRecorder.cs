using System;
using System.Collections.Generic;

namespace Platform.BlackBox
{
    /// <summary>
    /// Interface for activity recording implementations.
    /// </summary>
    public interface IActivityRecorder
    {
        /// <summary>
        /// Records a new activity event.
        /// </summary>
        void Record(ActivityEvent activityEvent);

        /// <summary>
        /// Records a simple event with automatic timestamp.
        /// </summary>
        void Record(string eventType, string source, string description, string severity = "Info");

        /// <summary>
        /// Records an exception event.
        /// </summary>
        void RecordException(Exception exception, string source, string additionalContext = null);

        /// <summary>
        /// Gets all recorded events within the buffer.
        /// </summary>
        IEnumerable<ActivityEvent> GetEvents();

        /// <summary>
        /// Gets events within a specific time range.
        /// </summary>
        IEnumerable<ActivityEvent> GetEvents(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Clears all recorded events.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the current count of recorded events.
        /// </summary>
        int Count { get; }
    }
}
