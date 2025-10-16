
BenchmarkDotNet=v0.13.5, OS=ubuntu 24.04
QEMU Virtual CPU version 2.5+, 1 CPU, 1 logical core and 1 physical core
.NET SDK=8.0.121
  [Host]     : .NET 8.0.21 (8.0.2125.47513), X64 RyuJIT SSE3
  Job-SGNNZO : .NET 8.0.21 (8.0.2125.47513), X64 RyuJIT SSE3

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Throughput  UnrollFactor=1  WarmupCount=3  

                            Method |       Mean |       Error |      StdDev |      Gen0 |   Allocated |
---------------------------------- |-----------:|------------:|------------:|----------:|------------:|
  'United: All fields in one file' | 250.179 ms |  60.1789 ms |   9.3128 ms | 1000.0000 | 14955.86 KB |
 'Split: Data + Indexes (2 files)' | 435.944 ms | 420.9422 ms | 109.3174 ms | 2000.0000 | 19153.42 KB |
     'United: Read-heavy workload' |  87.403 ms | 125.6340 ms |  19.4420 ms |         - |  2663.42 KB |
      'Split: Read-heavy workload' |  32.443 ms |  16.5034 ms |   2.5539 ms |         - |  2663.42 KB |
       'United: Sequential access' |   1.048 ms |   0.5744 ms |   0.0889 ms |         - |   311.66 KB |
        'Split: Sequential access' |   1.002 ms |   0.4150 ms |   0.1078 ms |         - |      311 KB |
