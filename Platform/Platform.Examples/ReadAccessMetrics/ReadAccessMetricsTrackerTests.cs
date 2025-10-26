using System;
using System.Threading;
using Xunit;

namespace Platform.Examples.ReadAccessMetrics
{
    /// <summary>
    /// Tests for read access frequency metrics tracker.
    /// </summary>
    public class ReadAccessMetricsTrackerTests
    {
        [Fact]
        public void NewLink_HasNoReadMetrics()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);

            Assert.Null(tracker.GetAverageReadAccessTime(1));
            Assert.Null(tracker.GetLastReadAccessTime(1));
            Assert.Equal(0, tracker.GetTotalReadAccessCount(1));
            Assert.NotNull(tracker.GetCreationDateTime(1));
        }

        [Fact]
        public void ReadAccess_IncrementsReadCount()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);

            // Perform read access
            tracker.RecordReadAccess(1);

            Assert.Equal(1, tracker.GetTotalReadAccessCount(1));
            Assert.NotNull(tracker.GetLastReadAccessTime(1));
        }

        [Fact]
        public void MultipleReads_UpdatesAverageReadAccessTime()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);

            // First read
            tracker.RecordReadAccess(1);
            Thread.Sleep(50);

            // Second read
            tracker.RecordReadAccess(1);

            Assert.Equal(2, tracker.GetTotalReadAccessCount(1));
            Assert.NotNull(tracker.GetAverageReadAccessTime(1));

            // Average should be approximately 50ms (with some tolerance)
            var avgTime = tracker.GetAverageReadAccessTime(1);
            Assert.True(avgTime.Value.TotalMilliseconds >= 40 && avgTime.Value.TotalMilliseconds <= 100);
        }

        [Fact]
        public void Variant2Calculation_ReturnsCorrectAverage()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);

            // Wait a bit and perform reads
            Thread.Sleep(100);
            tracker.RecordReadAccess(1);
            tracker.RecordReadAccess(1);

            var avgTimeV2 = tracker.GetAverageReadAccessTimeVariant2(1);

            Assert.NotNull(avgTimeV2);
            // Should be at least 50ms per read (100ms / 2 reads)
            Assert.True(avgTimeV2.Value.TotalMilliseconds >= 50);
        }

        [Fact]
        public void UnregisterLink_RemovesMetrics()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);
            tracker.RecordReadAccess(1);

            // Unregister the link
            tracker.UnregisterDeletedLink(1);

            // Metrics should be removed
            Assert.Equal(0, tracker.GetTotalReadAccessCount(1));
            Assert.Null(tracker.GetLastReadAccessTime(1));
        }

        [Fact]
        public void RecordMultipleLinks_TracksIndependently()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);
            tracker.RegisterCreatedLink(2);

            tracker.RecordReadAccess(1);
            tracker.RecordReadAccess(1);
            tracker.RecordReadAccess(2);

            Assert.Equal(2, tracker.GetTotalReadAccessCount(1));
            Assert.Equal(1, tracker.GetTotalReadAccessCount(2));
        }

        [Fact]
        public void RecordReadAccessBatch_TrackMultipleLinks()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);
            tracker.RegisterCreatedLink(2);
            tracker.RegisterCreatedLink(3);

            tracker.RecordReadAccess(new uint[] { 1, 2, 3 });

            Assert.Equal(1, tracker.GetTotalReadAccessCount(1));
            Assert.Equal(1, tracker.GetTotalReadAccessCount(2));
            Assert.Equal(1, tracker.GetTotalReadAccessCount(3));
        }

        [Fact]
        public void Clear_RemovesAllMetrics()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();
            tracker.RegisterCreatedLink(1);
            tracker.RegisterCreatedLink(2);
            tracker.RecordReadAccess(1);
            tracker.RecordReadAccess(2);

            tracker.Clear();

            Assert.Equal(0, tracker.GetTotalReadAccessCount(1));
            Assert.Equal(0, tracker.GetTotalReadAccessCount(2));
        }

        [Fact]
        public void UnregisteredLink_ReturnsDefaultMetrics()
        {
            var tracker = new ReadAccessMetricsTracker<uint>();

            // Link 999 was never registered
            Assert.Null(tracker.GetAverageReadAccessTime(999));
            Assert.Null(tracker.GetLastReadAccessTime(999));
            Assert.Null(tracker.GetCreationDateTime(999));
            Assert.Equal(0, tracker.GetTotalReadAccessCount(999));
        }
    }
}
