using System;

namespace Platform.Examples.ReadAccessMetrics
{
    /// <summary>
    /// Stores read access metrics for a single link.
    /// Supports both variant 1 (AverageReadAccessTime + LastReadAccessTime)
    /// and variant 2 (CreationDateTime + TotalReadAccessCount).
    /// </summary>
    public class LinkReadAccessMetrics
    {
        /// <summary>
        /// The timestamp when the link was created.
        /// Used in variant 2 formula: (Now - CreationDateTime) / TotalReadAccessCount.
        /// </summary>
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// The timestamp of the last read access.
        /// Used in variant 1 to update AverageReadAccessTime incrementally.
        /// </summary>
        public DateTime? LastReadAccessTime { get; set; }

        /// <summary>
        /// The average time between read accesses.
        /// Updated each time the link is read based on LastReadAccessTime.
        /// </summary>
        public TimeSpan? AverageReadAccessTime { get; set; }

        /// <summary>
        /// The total number of times this link has been read.
        /// Used in variant 2 formula and for calculating averages.
        /// </summary>
        public long TotalReadAccessCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the LinkReadAccessMetrics class.
        /// </summary>
        public LinkReadAccessMetrics()
        {
            CreationDateTime = DateTime.UtcNow;
            TotalReadAccessCount = 0;
            LastReadAccessTime = null;
            AverageReadAccessTime = null;
        }

        /// <summary>
        /// Records a read access event, updating all relevant metrics.
        /// Implements both variant 1 and variant 2 calculations.
        /// </summary>
        public void RecordReadAccess()
        {
            var now = DateTime.UtcNow;

            // Update variant 1: AverageReadAccessTime based on LastReadAccessTime
            if (LastReadAccessTime.HasValue && TotalReadAccessCount > 0)
            {
                var timeSinceLastRead = now - LastReadAccessTime.Value;

                if (AverageReadAccessTime.HasValue)
                {
                    // Update average using incremental formula
                    var currentAverage = AverageReadAccessTime.Value;
                    var newAverage = TimeSpan.FromTicks(
                        (currentAverage.Ticks * TotalReadAccessCount + timeSinceLastRead.Ticks) / (TotalReadAccessCount + 1)
                    );
                    AverageReadAccessTime = newAverage;
                }
                else
                {
                    // First average calculation
                    AverageReadAccessTime = timeSinceLastRead;
                }
            }

            // Update tracking fields
            LastReadAccessTime = now;
            TotalReadAccessCount++;
        }

        /// <summary>
        /// Calculates the average read access time using variant 2 formula.
        /// Formula: (Now - CreationDateTime) / TotalReadAccessCount
        /// </summary>
        /// <returns>The calculated average read access time, or null if no reads have occurred.</returns>
        public TimeSpan? CalculateAverageReadAccessTimeVariant2()
        {
            if (TotalReadAccessCount == 0)
            {
                return null;
            }

            var totalTime = DateTime.UtcNow - CreationDateTime;
            return TimeSpan.FromTicks(totalTime.Ticks / TotalReadAccessCount);
        }
    }
}
