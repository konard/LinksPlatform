using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Platform.Experiments
{
    /// <summary>
    /// Simple demonstration of C# generic vs specialized performance difference.
    /// This simplified example shows the concept behind comparing UInt64Links vs Links&lt;ulong&gt;.
    /// </summary>
    public static class SimpleGenericVsSpecializedDemo
    {
        private const int Iterations = 1_000_000;

        public static void Run()
        {
            Console.WriteLine("=== Generic vs Specialized Performance Demo ===");
            Console.WriteLine($"Testing {Iterations:N0} iterations of mathematical operations");
            Console.WriteLine();

            // Warmup
            var _ = TestGenericMath(1000);
            var __ = TestSpecializedMath(1000);

            // Actual benchmarks
            var genericTime = TestGenericMath(Iterations);
            var specializedTime = TestSpecializedMath(Iterations);

            Console.WriteLine($"Generic<ulong>:     {genericTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Specialized UInt64: {specializedTime.TotalMilliseconds:F2} ms");
            Console.WriteLine();

            var difference = ((genericTime - specializedTime).TotalMilliseconds / specializedTime.TotalMilliseconds) * 100;
            Console.WriteLine($"Performance difference: {difference:F1}% slower for generic");
            Console.WriteLine();

            Console.WriteLine("Key Insights:");
            Console.WriteLine("- Generic code has runtime type checking overhead");
            Console.WriteLine("- Specialized code can be more aggressively optimized by JIT");
            Console.WriteLine("- C++ templates would show even greater benefits (compile-time specialization)");
            Console.WriteLine("- Real-world difference depends on operation complexity");
        }

        private static TimeSpan TestGenericMath(int iterations)
        {
            var container = new GenericContainer<ulong>();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                container.Add((ulong)i);
                var result = container.Multiply(2);
                var sum = container.Sum();
                container.Reset();
            }

            sw.Stop();
            return sw.Elapsed;
        }

        private static TimeSpan TestSpecializedMath(int iterations)
        {
            var container = new SpecializedUInt64Container();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                container.Add((ulong)i);
                var result = container.Multiply(2);
                var sum = container.Sum();
                container.Reset();
            }

            sw.Stop();
            return sw.Elapsed;
        }
    }

    // Generic version (like Links<ulong>)
    public class GenericContainer<T> where T : struct, IComparable<T>
    {
        private T[] _data = new T[10];
        private int _count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            if (_count < _data.Length)
                _data[_count++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Multiply(int multiplier)
        {
            if (_count == 0) return default;
            // Generic math requires conversion (using dynamic for demonstration)
            try
            {
                dynamic result = _data[_count - 1];
                dynamic mult = (dynamic)multiplier;
                return (T)(result * mult);
            }
            catch
            {
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Sum()
        {
            try
            {
                dynamic sum = default(T);
                for (int i = 0; i < _count; i++)
                {
                    sum = (dynamic)sum + (dynamic)_data[i];
                }
                return (T)sum;
            }
            catch
            {
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _count = 0;
    }

    // Specialized version (like hypothetical UInt64Links)
    public class SpecializedUInt64Container
    {
        private ulong[] _data = new ulong[10];
        private int _count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong value)
        {
            if (_count < _data.Length)
                _data[_count++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Multiply(int multiplier)
        {
            if (_count == 0) return 0;
            // Direct ulong math - no conversion needed
            return _data[_count - 1] * (ulong)multiplier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Sum()
        {
            ulong sum = 0;
            for (int i = 0; i < _count; i++)
            {
                sum += _data[i];
            }
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _count = 0;
    }
}
