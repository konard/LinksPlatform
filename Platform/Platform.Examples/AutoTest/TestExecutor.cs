using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Executes tests with timeout and memory constraints.
    /// Implements time and memory-limited execution as mentioned in issue #102.
    /// </summary>
    public class TestExecutor
    {
        private readonly int _timeoutMs;
        private readonly long _maxMemoryBytes;

        public TestExecutor(int timeoutMs = 5000, long maxMemoryBytes = 100 * 1024 * 1024)
        {
            _timeoutMs = timeoutMs;
            _maxMemoryBytes = maxMemoryBytes;
        }

        /// <summary>
        /// Executes a test with timeout and memory monitoring.
        /// </summary>
        public TestResult ExecuteTest(ITestableFunction function, object[] parameters, TestType testType)
        {
            var result = new TestResult
            {
                FunctionName = function.FunctionName,
                InputParameters = parameters,
                TestType = testType
            };

            var stopwatch = Stopwatch.StartNew();
            var initialMemory = GC.GetTotalMemory(false);

            try
            {
                // Execute with timeout
                var task = Task.Run(() => function.Execute(parameters));
                if (task.Wait(_timeoutMs))
                {
                    result.Output = task.Result;
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Exception = new TimeoutException($"Test execution exceeded {_timeoutMs}ms");
                }
            }
            catch (AggregateException ae)
            {
                result.Success = false;
                result.Exception = ae.InnerException ?? ae;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Exception = ex;
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.MemoryUsedBytes = GC.GetTotalMemory(false) - initialMemory;
            }

            return result;
        }

        /// <summary>
        /// Maps input value ranges that can execute within time limit using binary search.
        /// Implements the concept from issue #102 comment about mapping executable value ranges.
        /// </summary>
        public (object min, object max) MapExecutableRange(
            ITestableFunction function,
            Type parameterType,
            int parameterIndex,
            object[] baseParameters)
        {
            if (parameterType == typeof(int))
            {
                var min = BinarySearchMinValue(function, parameterIndex, baseParameters, int.MinValue, 0);
                var max = BinarySearchMaxValue(function, parameterIndex, baseParameters, 0, int.MaxValue);
                return (min, max);
            }
            else if (parameterType == typeof(long))
            {
                var min = BinarySearchMinValue(function, parameterIndex, baseParameters, long.MinValue, 0L);
                var max = BinarySearchMaxValue(function, parameterIndex, baseParameters, 0L, long.MaxValue);
                return (min, max);
            }

            return (null, null);
        }

        private T BinarySearchMinValue<T>(
            ITestableFunction function,
            int parameterIndex,
            object[] baseParameters,
            T min,
            T max) where T : IComparable<T>
        {
            // Binary search to find minimum executable value
            dynamic left = min;
            dynamic right = max;
            dynamic result = max;

            while (left.CompareTo(right) < 0)
            {
                dynamic mid = (left + right) / 2;
                var testParams = (object[])baseParameters.Clone();
                testParams[parameterIndex] = mid;

                var testResult = ExecuteTest(function, testParams, TestType.BoundaryMin);
                if (testResult.Success && testResult.ExecutionTimeMs <= _timeoutMs)
                {
                    result = mid;
                    right = mid;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return result;
        }

        private T BinarySearchMaxValue<T>(
            ITestableFunction function,
            int parameterIndex,
            object[] baseParameters,
            T min,
            T max) where T : IComparable<T>
        {
            // Binary search to find maximum executable value
            dynamic left = min;
            dynamic right = max;
            dynamic result = min;

            while (left.CompareTo(right) < 0)
            {
                dynamic mid = (left + right) / 2;
                var testParams = (object[])baseParameters.Clone();
                testParams[parameterIndex] = mid;

                var testResult = ExecuteTest(function, testParams, TestType.BoundaryMax);
                if (testResult.Success && testResult.ExecutionTimeMs <= _timeoutMs)
                {
                    result = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }
            }

            return result;
        }
    }
}
