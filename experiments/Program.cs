using BenchmarkDotNet.Running;

namespace MemoryStorageBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StorageStrategyBenchmarks>();
        }
    }
}
