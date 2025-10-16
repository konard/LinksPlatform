using BenchmarkDotNet.Running;

namespace Platform.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AggressiveInliningBenchmarks>();
        }
    }
}
