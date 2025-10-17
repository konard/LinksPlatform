using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Platform.BlackBox
{
    /// <summary>
    /// Activity recorder that maintains a circular buffer of events for the past N hours.
    /// This allows capturing context leading up to an issue without storing unlimited history.
    /// </summary>
    public class CircularBufferActivityRecorder : IActivityRecorder
    {
        private readonly object _lock = new object();
        private readonly LinkedList<ActivityEvent> _events = new LinkedList<ActivityEvent>();
        private readonly TimeSpan _retentionPeriod;
        private readonly int _maxEvents;
        private long _totalRecorded = 0;

        /// <summary>
        /// Creates a new circular buffer activity recorder.
        /// </summary>
        /// <param name="retentionPeriod">How long to keep events (default: 2 hours).</param>
        /// <param name="maxEvents">Maximum number of events to store (default: 100000).</param>
        public CircularBufferActivityRecorder(TimeSpan? retentionPeriod = null, int maxEvents = 100000)
        {
            _retentionPeriod = retentionPeriod ?? TimeSpan.FromHours(2);
            _maxEvents = maxEvents;
        }

        public void Record(ActivityEvent activityEvent)
        {
            if (activityEvent == null) throw new ArgumentNullException(nameof(activityEvent));

            lock (_lock)
            {
                _events.AddLast(activityEvent);
                _totalRecorded++;

                // Remove old events based on time
                CleanupOldEvents();

                // Enforce max events limit
                while (_events.Count > _maxEvents)
                {
                    _events.RemoveFirst();
                }
            }
        }

        public void Record(string eventType, string source, string description, string severity = "Info")
        {
            var activityEvent = new ActivityEvent
            {
                EventType = eventType,
                Source = source,
                Description = description,
                Severity = severity
            };
            Record(activityEvent);
        }

        public void RecordException(Exception exception, string source, string additionalContext = null)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            var description = additionalContext != null
                ? $"{additionalContext}: {exception.Message}"
                : exception.Message;

            var activityEvent = new ActivityEvent
            {
                EventType = "Exception",
                Source = source,
                Description = description,
                Severity = "Error",
                StackTrace = exception.StackTrace,
                Metadata = exception.GetType().FullName
            };
            Record(activityEvent);
        }

        public IEnumerable<ActivityEvent> GetEvents()
        {
            lock (_lock)
            {
                CleanupOldEvents();
                return _events.ToList();
            }
        }

        public IEnumerable<ActivityEvent> GetEvents(DateTime startTime, DateTime endTime)
        {
            lock (_lock)
            {
                return _events
                    .Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime)
                    .ToList();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _events.Clear();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    CleanupOldEvents();
                    return _events.Count;
                }
            }
        }

        /// <summary>
        /// Gets the total number of events recorded since creation (including expired ones).
        /// </summary>
        public long TotalRecorded
        {
            get
            {
                lock (_lock)
                {
                    return _totalRecorded;
                }
            }
        }

        private void CleanupOldEvents()
        {
            var cutoffTime = DateTime.UtcNow - _retentionPeriod;
            while (_events.Count > 0 && _events.First.Value.Timestamp < cutoffTime)
            {
                _events.RemoveFirst();
            }
        }
    }
}
