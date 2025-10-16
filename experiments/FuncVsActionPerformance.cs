using System;
using System.Diagnostics;
using System.Threading;

namespace PerformanceComparison
{
    /// <summary>
    /// Performance comparison between Func&lt;T, bool&gt; and Action&lt;T&gt; with CancellationToken
    ///
    /// This experiment compares:
    /// 1. Func&lt;T, bool&gt; - a predicate function that returns bool to signal continuation
    /// 2. Action&lt;T&gt; with CancellationToken - an action that uses CancellationToken to signal cancellation
    /// </summary>
    public class FuncVsActionPerformance
    {
        private const int IterationCount = 10_000_000;
        private const int WarmupIterations = 1_000_000;

        public static void RunBenchmark()
        {
            Console.WriteLine("=== Performance Comparison: Func<T, bool> vs Action<T> with CancellationToken ===\n");
            Console.WriteLine($"Iterations: {IterationCount:N0}");
            Console.WriteLine($"Warmup iterations: {WarmupIterations:N0}\n");

            // Warmup
            Console.WriteLine("Warming up...");
            WarmupFunc();
            WarmupAction();
            Console.WriteLine("Warmup complete.\n");

            // Run benchmarks
            var funcTime = BenchmarkFunc();
            var actionTime = BenchmarkAction();

            // Display results
            Console.WriteLine("\n=== Results ===");
            Console.WriteLine($"Func<T, bool>:                       {funcTime.TotalMilliseconds:F3} ms");
            Console.WriteLine($"Action<T> with CancellationToken:    {actionTime.TotalMilliseconds:F3} ms");
            Console.WriteLine();

            var difference = actionTime.TotalMilliseconds - funcTime.TotalMilliseconds;
            var percentDifference = (difference / funcTime.TotalMilliseconds) * 100;

            if (funcTime < actionTime)
            {
                Console.WriteLine($"Func<T, bool> is faster by {Math.Abs(difference):F3} ms ({Math.Abs(percentDifference):F2}%)");
            }
            else
            {
                Console.WriteLine($"Action<T> with CancellationToken is faster by {Math.Abs(difference):F3} ms ({Math.Abs(percentDifference):F2}%)");
            }

            Console.WriteLine($"\nThroughput:");
            Console.WriteLine($"  Func<T, bool>:                     {IterationCount / funcTime.TotalSeconds:N0} ops/sec");
            Console.WriteLine($"  Action<T> with CancellationToken:  {IterationCount / actionTime.TotalSeconds:N0} ops/sec");
        }

        #region Func<T, bool> Implementation

        private static void WarmupFunc()
        {
            ProcessWithFunc(WarmupIterations, x => x < WarmupIterations);
        }

        private static TimeSpan BenchmarkFunc()
        {
            var stopwatch = Stopwatch.StartNew();
            ProcessWithFunc(IterationCount, x => x < IterationCount);
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        private static void ProcessWithFunc(int count, Func<int, bool> predicate)
        {
            for (int i = 0; i < count; i++)
            {
                if (!predicate(i))
                {
                    break;
                }
            }
        }

        #endregion

        #region Action<T> with CancellationToken Implementation

        private static void WarmupAction()
        {
            var cts = new CancellationTokenSource();
            ProcessWithAction(WarmupIterations, x =>
            {
                if (x >= WarmupIterations)
                {
                    cts.Cancel();
                }
            }, cts.Token);
        }

        private static TimeSpan BenchmarkAction()
        {
            var cts = new CancellationTokenSource();
            var stopwatch = Stopwatch.StartNew();
            ProcessWithAction(IterationCount, x =>
            {
                if (x >= IterationCount)
                {
                    cts.Cancel();
                }
            }, cts.Token);
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        private static void ProcessWithAction(int count, Action<int> action, CancellationToken cancellationToken)
        {
            for (int i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                action(i);
            }
        }

        #endregion

        public static void Main(string[] args)
        {
            RunBenchmark();
        }
    }
}
