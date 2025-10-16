using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Automatically generates and executes tests for functions.
    /// Main implementation for issue #102 - Test Everything.
    /// </summary>
    public class AutoTestGenerator
    {
        private readonly TestExecutor _executor;
        private readonly BoundaryValueAnalyzer _boundaryAnalyzer;
        private readonly bool _lazyMode;
        private readonly int _randomTestsPerFunction;

        /// <summary>
        /// Creates a new auto test generator.
        /// </summary>
        /// <param name="lazyMode">If true, only tests functions that are marked for testing. If false, tests everything.</param>
        /// <param name="randomTestsPerFunction">Number of random test cases to generate per function.</param>
        /// <param name="timeoutMs">Timeout for each test in milliseconds.</param>
        public AutoTestGenerator(bool lazyMode = false, int randomTestsPerFunction = 10, int timeoutMs = 5000)
        {
            _lazyMode = lazyMode;
            _randomTestsPerFunction = randomTestsPerFunction;
            _executor = new TestExecutor(timeoutMs);
            _boundaryAnalyzer = new BoundaryValueAnalyzer();
        }

        /// <summary>
        /// Generates and executes all tests for a function.
        /// </summary>
        public List<TestResult> GenerateAndExecuteTests(ITestableFunction function)
        {
            var results = new List<TestResult>();

            // Generate boundary value tests
            results.AddRange(GenerateBoundaryTests(function));

            // Generate random tests
            results.AddRange(GenerateRandomTests(function));

            // Generate null/invalid input tests
            results.AddRange(GenerateInvalidInputTests(function));

            return results;
        }

        /// <summary>
        /// Generates boundary value tests for all parameter combinations.
        /// </summary>
        private List<TestResult> GenerateBoundaryTests(ITestableFunction function)
        {
            var results = new List<TestResult>();
            var parameterTypes = function.ParameterTypes;

            if (parameterTypes.Length == 0)
            {
                // No parameters, just test once
                results.Add(_executor.ExecuteTest(function, Array.Empty<object>(), TestType.Normal));
                return results;
            }

            // Generate boundary values for each parameter
            var boundaryValuesByParameter = new List<List<object>>();
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var constraints = function.ParameterConstraints.ContainsKey(i)
                    ? function.ParameterConstraints[i]
                    : null;
                boundaryValuesByParameter.Add(_boundaryAnalyzer.GenerateBoundaryValues(parameterTypes[i], constraints));
            }

            // Test each parameter's boundary values while keeping others at default
            for (int paramIndex = 0; paramIndex < parameterTypes.Length; paramIndex++)
            {
                var defaultParams = GetDefaultParameters(parameterTypes);
                foreach (var boundaryValue in boundaryValuesByParameter[paramIndex])
                {
                    var testParams = (object[])defaultParams.Clone();
                    testParams[paramIndex] = boundaryValue;

                    try
                    {
                        var testType = IsMinBoundary(boundaryValue) ? TestType.BoundaryMin :
                                      IsMaxBoundary(boundaryValue) ? TestType.BoundaryMax :
                                      TestType.EdgeCase;
                        results.Add(_executor.ExecuteTest(function, testParams, testType));
                    }
                    catch (Exception ex)
                    {
                        results.Add(new TestResult
                        {
                            FunctionName = function.FunctionName,
                            InputParameters = testParams,
                            Success = false,
                            Exception = ex,
                            TestType = TestType.EdgeCase
                        });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Generates random test cases.
        /// </summary>
        private List<TestResult> GenerateRandomTests(ITestableFunction function)
        {
            var results = new List<TestResult>();
            var random = new Random();

            for (int i = 0; i < _randomTestsPerFunction; i++)
            {
                var randomParams = new object[function.ParameterTypes.Length];
                for (int j = 0; j < randomParams.Length; j++)
                {
                    randomParams[j] = _boundaryAnalyzer.GenerateRandomValue(function.ParameterTypes[j], random);
                }

                results.Add(_executor.ExecuteTest(function, randomParams, TestType.RandomInput));
            }

            return results;
        }

        /// <summary>
        /// Generates tests with null and invalid inputs.
        /// </summary>
        private List<TestResult> GenerateInvalidInputTests(ITestableFunction function)
        {
            var results = new List<TestResult>();

            // Test with all null parameters (for reference types)
            var nullParams = new object[function.ParameterTypes.Length];
            for (int i = 0; i < nullParams.Length; i++)
            {
                nullParams[i] = function.ParameterTypes[i].IsValueType ? Activator.CreateInstance(function.ParameterTypes[i]) : null;
            }

            if (nullParams.Any(p => p == null))
            {
                results.Add(_executor.ExecuteTest(function, nullParams, TestType.NullInput));
            }

            return results;
        }

        /// <summary>
        /// Generates test statistics from results.
        /// </summary>
        public TestStatistics GenerateStatistics(List<TestResult> results)
        {
            var stats = new TestStatistics
            {
                TotalTests = results.Count,
                PassedTests = results.Count(r => r.Success),
                FailedTests = results.Count(r => !r.Success),
                TotalExecutionTimeMs = results.Sum(r => r.ExecutionTimeMs)
            };

            foreach (var result in results)
            {
                if (!stats.TestsByFunction.ContainsKey(result.FunctionName))
                {
                    stats.TestsByFunction[result.FunctionName] = 0;
                    stats.FailuresByFunction[result.FunctionName] = 0;
                }
                stats.TestsByFunction[result.FunctionName]++;
                if (!result.Success)
                {
                    stats.FailuresByFunction[result.FunctionName]++;
                }
            }

            return stats;
        }

        private object[] GetDefaultParameters(Type[] parameterTypes)
        {
            var defaults = new object[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                defaults[i] = parameterTypes[i].IsValueType ? Activator.CreateInstance(parameterTypes[i]) : null;
            }
            return defaults;
        }

        private bool IsMinBoundary(object value)
        {
            if (value == null) return true;
            var type = value.GetType();
            if (type == typeof(int)) return (int)value == int.MinValue;
            if (type == typeof(long)) return (long)value == long.MinValue;
            if (type == typeof(double)) return (double)value == double.MinValue;
            if (type == typeof(float)) return (float)value == float.MinValue;
            return false;
        }

        private bool IsMaxBoundary(object value)
        {
            if (value == null) return false;
            var type = value.GetType();
            if (type == typeof(int)) return (int)value == int.MaxValue;
            if (type == typeof(long)) return (long)value == long.MaxValue;
            if (type == typeof(double)) return (double)value == double.MaxValue;
            if (type == typeof(float)) return (float)value == float.MaxValue;
            return false;
        }
    }
}
