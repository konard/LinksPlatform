using System;
using Platform.Sandbox;

namespace Experiments
{
    /// <summary>
    /// Demonstrates the lazy event loop pattern with transaction log.
    /// Shows how events notify about their time and position, enabling:
    /// - Fast access using position
    /// - Binary search using time
    /// - Lazy iteration (request next event only when needed)
    /// </summary>
    public static class LazyEventLoopExample
    {
        public static void Main()
        {
            Console.WriteLine("=== Lazy Event Loop with Transaction Log ===\n");

            // Example 1: Lazy iteration through log
            Console.WriteLine("Example 1: Lazy iteration through events");
            IterateEventsLazily();

            Console.WriteLine("\n---\n");

            // Example 2: Find event by time using binary search
            Console.WriteLine("Example 2: Find event by time (binary search)");
            FindEventByTimeExample();

            Console.WriteLine("\n---\n");

            // Example 3: Get next event from specific position
            Console.WriteLine("Example 3: Get next event from position");
            GetNextEventExample();
        }

        private static void IterateEventsLazily()
        {
            // Start from the beginning of the log
            long position = Marshal.SizeOf<Transactions.TransactionsState>();

            int eventCount = 0;
            while (true)
            {
                var logEvent = Transactions.GetNextEvent(position);
                if (logEvent == null)
                {
                    Console.WriteLine($"Reached end of log. Processed {eventCount} events.");
                    break;
                }

                Console.WriteLine($"Event at position {logEvent.Value.Position}: Time={logEvent.Value.Time:O}");

                // Get next position for lazy iteration
                position = Transactions.GetNextEventPosition(position);
                if (position == -1)
                {
                    Console.WriteLine($"No more events. Total: {eventCount + 1}");
                    break;
                }

                eventCount++;

                // Lazy: we can stop at any point and continue later
                // No matter how long it takes to handle the event
                if (eventCount >= 5) // Limit for demo
                {
                    Console.WriteLine($"Stopped after {eventCount} events (demo limit).");
                    break;
                }
            }
        }

        private static void FindEventByTimeExample()
        {
            DateTime targetTime = DateTime.UtcNow.AddHours(-1);
            var logEvent = Transactions.FindEventByTime(targetTime);

            if (logEvent == null)
            {
                Console.WriteLine("No events found in log.");
            }
            else
            {
                Console.WriteLine($"Found event at position {logEvent.Value.Position}");
                Console.WriteLine($"Event time: {logEvent.Value.Time:O}");
                Console.WriteLine($"Binary search allows efficient time-based lookup.");
            }
        }

        private static void GetNextEventExample()
        {
            // Assume we have a position from a previous event
            long somePosition = Marshal.SizeOf<Transactions.TransactionsState>();

            var currentEvent = Transactions.GetNextEvent(somePosition);
            if (currentEvent == null)
            {
                Console.WriteLine("No event at this position.");
                return;
            }

            Console.WriteLine($"Current event: Position={currentEvent.Value.Position}, Time={currentEvent.Value.Time:O}");

            // Get next event position
            long nextPosition = Transactions.GetNextEventPosition(somePosition);
            if (nextPosition != -1)
            {
                var nextEvent = Transactions.GetNextEvent(nextPosition);
                if (nextEvent != null)
                {
                    Console.WriteLine($"Next event: Position={nextEvent.Value.Position}, Time={nextEvent.Value.Time:O}");
                    Console.WriteLine("Position allows fast direct access to next entry.");
                }
            }
            else
            {
                Console.WriteLine("No next event available.");
            }
        }
    }
}
