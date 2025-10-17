using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.CodeGeneration.Experiments.Tests
{
    /// <summary>
    /// Tests for the TransformationPathFinder to verify it correctly constructs
    /// transformation paths from method arguments to return values.
    /// </summary>
    public class TransformationPathTests
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Running TransformationPathFinder Tests ===");
            Console.WriteLine();

            int passed = 0;
            int failed = 0;

            // Test 1: Direct type match (no transformation needed)
            if (TestDirectTypeMatch())
            {
                Console.WriteLine("✓ Test 1: Direct type match - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 1: Direct type match - FAILED");
                failed++;
            }

            // Test 2: Single type cast transformation
            if (TestSingleTypeCast())
            {
                Console.WriteLine("✓ Test 2: Single type cast - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 2: Single type cast - FAILED");
                failed++;
            }

            // Test 3: Return type transformation
            if (TestReturnTypeTransformation())
            {
                Console.WriteLine("✓ Test 3: Return type transformation - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 3: Return type transformation - FAILED");
                failed++;
            }

            // Test 4: Multiple parameter transformations
            if (TestMultipleParameters())
            {
                Console.WriteLine("✓ Test 4: Multiple parameters - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 4: Multiple parameters - FAILED");
                failed++;
            }

            // Test 5: Missing parameter uses default
            if (TestMissingParameterDefault())
            {
                Console.WriteLine("✓ Test 5: Missing parameter default - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 5: Missing parameter default - FAILED");
                failed++;
            }

            // Test 6: Parameter matching by name
            if (TestParameterNameMatching())
            {
                Console.WriteLine("✓ Test 6: Parameter name matching - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 6: Parameter name matching - FAILED");
                failed++;
            }

            // Test 7: Cost calculation
            if (TestCostCalculation())
            {
                Console.WriteLine("✓ Test 7: Cost calculation - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 7: Cost calculation - FAILED");
                failed++;
            }

            // Test 8: Code generation
            if (TestCodeGeneration())
            {
                Console.WriteLine("✓ Test 8: Code generation - PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 8: Code generation - FAILED");
                failed++;
            }

            Console.WriteLine();
            Console.WriteLine($"=== Test Results: {passed} passed, {failed} failed ===");

            if (failed == 0)
            {
                Console.WriteLine("All tests passed! ✓");
            }
            else
            {
                Console.WriteLine($"Some tests failed. Please review the output above.");
            }
        }

        private static bool TestDirectTypeMatch()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "Process",
                Parameters = new List<Parameter> { new Parameter { Name = "value", Type = typeof(int) } },
                ReturnType = typeof(string)
            };

            var source = new MethodSignature
            {
                Name = "Execute",
                Parameters = new List<Parameter> { new Parameter { Name = "value", Type = typeof(int) } },
                ReturnType = typeof(string)
            };

            var path = finder.FindPath(source, target);

            // Should have no transformations for matching types
            return path != null && path.ParameterTransformations.Count == 1 &&
                   path.ParameterTransformations[0].Description.Contains("Direct match");
        }

        private static bool TestSingleTypeCast()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "Process",
                Parameters = new List<Parameter> { new Parameter { Name = "id", Type = typeof(int) } },
                ReturnType = typeof(string)
            };

            var source = new MethodSignature
            {
                Name = "Fetch",
                Parameters = new List<Parameter> { new Parameter { Name = "id", Type = typeof(long) } },
                ReturnType = typeof(string)
            };

            var path = finder.FindPath(source, target);

            // Should have transformation from long to int
            return path != null && path.Transformations.Any() &&
                   path.ParameterTransformations[0].GeneratedCode.Contains("(Int32)");
        }

        private static bool TestReturnTypeTransformation()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "GetValue",
                Parameters = new List<Parameter>(),
                ReturnType = typeof(string)
            };

            var source = new MethodSignature
            {
                Name = "Calculate",
                Parameters = new List<Parameter>(),
                ReturnType = typeof(int)
            };

            var path = finder.FindPath(source, target);

            // Should have return type transformation from int to string
            return path != null &&
                   !string.IsNullOrEmpty(path.ReturnTransformationCode) &&
                   path.ReturnTransformationCode.Contains("ToString()");
        }

        private static bool TestMultipleParameters()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "Process",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "x", Type = typeof(int) },
                    new Parameter { Name = "y", Type = typeof(int) }
                },
                ReturnType = typeof(void)
            };

            var source = new MethodSignature
            {
                Name = "Execute",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "a", Type = typeof(long) },
                    new Parameter { Name = "b", Type = typeof(double) }
                },
                ReturnType = typeof(void)
            };

            var path = finder.FindPath(source, target);

            // Should have transformations for both parameters
            return path != null &&
                   path.ParameterTransformations.Count == 2 &&
                   path.ParameterTransformations[0].GeneratedCode.Contains("Int32") &&
                   path.ParameterTransformations[1].GeneratedCode.Contains("Int32");
        }

        private static bool TestMissingParameterDefault()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "Process",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "x", Type = typeof(int) },
                    new Parameter { Name = "y", Type = typeof(int) }
                },
                ReturnType = typeof(void)
            };

            var source = new MethodSignature
            {
                Name = "Execute",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "a", Type = typeof(int) }
                },
                ReturnType = typeof(void)
            };

            var path = finder.FindPath(source, target);

            // Second parameter should use default value
            return path != null &&
                   path.ParameterTransformations.Count == 2 &&
                   path.ParameterTransformations[1].GeneratedCode.Contains("default");
        }

        private static bool TestParameterNameMatching()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "Process",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "userId", Type = typeof(int) },
                    new Parameter { Name = "userName", Type = typeof(string) }
                },
                ReturnType = typeof(void)
            };

            var source = new MethodSignature
            {
                Name = "Execute",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "userName", Type = typeof(string) },
                    new Parameter { Name = "userId", Type = typeof(int) }
                },
                ReturnType = typeof(void)
            };

            var path = finder.FindPath(source, target);

            // Should match by name, not position
            // userId parameter should reference userName source parameter
            return path != null &&
                   path.ParameterTransformations.Count == 2 &&
                   path.ParameterTransformations[0].SourceParameter == "userId" &&
                   path.ParameterTransformations[1].SourceParameter == "userName";
        }

        private static bool TestCostCalculation()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "Process",
                Parameters = new List<Parameter> { new Parameter { Name = "x", Type = typeof(int) } },
                ReturnType = typeof(string)
            };

            var source1 = new MethodSignature
            {
                Name = "Execute1",
                Parameters = new List<Parameter> { new Parameter { Name = "x", Type = typeof(long) } },
                ReturnType = typeof(int)
            };

            var path1 = finder.FindPath(source1, target);

            // Should calculate cost based on number and type of transformations
            // One type cast + one ToString = at least 2
            return path1 != null && path1.TotalCost >= 2;
        }

        private static bool TestCodeGeneration()
        {
            var finder = new TransformationPathFinder();

            var target = new MethodSignature
            {
                Name = "GetData",
                Parameters = new List<Parameter> { new Parameter { Name = "id", Type = typeof(int) } },
                ReturnType = typeof(string)
            };

            var source = new MethodSignature
            {
                Name = "Fetch",
                Parameters = new List<Parameter> { new Parameter { Name = "identifier", Type = typeof(long) } },
                ReturnType = typeof(string)
            };

            var path = finder.FindPath(source, target);
            var code = path.GenerateImplementation();

            // Generated code should contain method signature and implementation
            return !string.IsNullOrEmpty(code) &&
                   code.Contains("public") &&
                   code.Contains("GetData") &&
                   code.Contains("Int32 id") &&
                   code.Contains("param0") &&
                   code.Contains("Fetch");
        }
    }
}
