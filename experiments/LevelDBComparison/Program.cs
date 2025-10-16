using BenchmarkDotNet.Running;

namespace LevelDBComparison
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DatabaseBenchmarks>();
        }
    }
}
