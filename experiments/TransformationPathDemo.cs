using System;
using System.Collections.Generic;

namespace Platform.CodeGeneration.Experiments
{
    /// <summary>
    /// Demonstration program for automatic interface implementation using transformation paths.
    /// </summary>
    public class TransformationPathDemo
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Automatic Interface Implementation Demo ===");
            Console.WriteLine();

            var finder = new TransformationPathFinder();

            // Demo 1: Simple type conversion
            Console.WriteLine("--- Demo 1: Simple Type Conversion ---");
            DemoSimpleTypeConversion(finder);
            Console.WriteLine();

            // Demo 2: Multiple parameter transformations
            Console.WriteLine("--- Demo 2: Multiple Parameter Transformations ---");
            DemoMultipleParameters(finder);
            Console.WriteLine();

            // Demo 3: Return type transformation
            Console.WriteLine("--- Demo 3: Return Type Transformation ---");
            DemoReturnTypeTransformation(finder);
            Console.WriteLine();

            // Demo 4: Complex transformation chain
            Console.WriteLine("--- Demo 4: Complex Transformation Chain ---");
            DemoComplexChain(finder);
            Console.WriteLine();

            Console.WriteLine("=== Demo Complete ===");
        }

        private static void DemoSimpleTypeConversion(TransformationPathFinder finder)
        {
            // Target interface method: string GetData(int id)
            var target = new MethodSignature
            {
                Name = "GetData",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "id", Type = typeof(int) }
                },
                ReturnType = typeof(string)
            };

            // Available source method: string Fetch(long identifier)
            var source = new MethodSignature
            {
                Name = "Fetch",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "identifier", Type = typeof(long) }
                },
                ReturnType = typeof(string)
            };

            var path = finder.FindPath(source, target);
            Console.WriteLine(path);
            Console.WriteLine();
            Console.WriteLine("Generated Code:");
            Console.WriteLine(path.GenerateImplementation());
        }

        private static void DemoMultipleParameters(TransformationPathFinder finder)
        {
            // Target: void Process(int x, int y)
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

            // Source: void Execute(long a, double b)
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
            Console.WriteLine(path);
            Console.WriteLine();
            Console.WriteLine("Generated Code:");
            Console.WriteLine(path.GenerateImplementation());
        }

        private static void DemoReturnTypeTransformation(TransformationPathFinder finder)
        {
            // Target: string GetValue()
            var target = new MethodSignature
            {
                Name = "GetValue",
                Parameters = new List<Parameter>(),
                ReturnType = typeof(string)
            };

            // Source: int Calculate()
            var source = new MethodSignature
            {
                Name = "Calculate",
                Parameters = new List<Parameter>(),
                ReturnType = typeof(int)
            };

            var path = finder.FindPath(source, target);
            Console.WriteLine(path);
            Console.WriteLine();
            Console.WriteLine("Generated Code:");
            Console.WriteLine(path.GenerateImplementation());
        }

        private static void DemoComplexChain(TransformationPathFinder finder)
        {
            // Register custom async transformation
            finder.RegisterTransformation(new AsyncToSyncTransformation(typeof(System.Threading.Tasks.Task<string>), typeof(string)));

            // Target: string GetData(int id)
            var target = new MethodSignature
            {
                Name = "GetData",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "id", Type = typeof(int) }
                },
                ReturnType = typeof(string)
            };

            // Source: Task<string> FetchAsync(long key)
            var source = new MethodSignature
            {
                Name = "FetchAsync",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "key", Type = typeof(long) }
                },
                ReturnType = typeof(System.Threading.Tasks.Task<string>)
            };

            var path = finder.FindPath(source, target);
            Console.WriteLine(path);
            Console.WriteLine();
            Console.WriteLine("Generated Code:");
            Console.WriteLine(path.GenerateImplementation());
        }
    }

    // Example interfaces and implementations for demonstration

    /// <summary>
    /// Example target interface that needs implementation.
    /// </summary>
    public interface IDataService
    {
        string GetData(int id);
        void Process(int x, int y);
        string GetValue();
    }

    /// <summary>
    /// Example source class with methods that can be adapted.
    /// </summary>
    public class DataRepository
    {
        public string Fetch(long identifier)
        {
            return $"Data for ID: {identifier}";
        }

        public void Execute(long a, double b)
        {
            Console.WriteLine($"Executing with {a} and {b}");
        }

        public int Calculate()
        {
            return 42;
        }

        public System.Threading.Tasks.Task<string> FetchAsync(long key)
        {
            return System.Threading.Tasks.Task.FromResult($"Async data for key: {key}");
        }
    }

    /// <summary>
    /// Auto-generated implementation (example of what the generator would produce).
    /// </summary>
    public class DataServiceImplementation : IDataService
    {
        private readonly DataRepository _repository;

        public DataServiceImplementation(DataRepository repository)
        {
            _repository = repository;
        }

        // Generated by transformation: int → long
        public string GetData(int id)
        {
            // Auto-generated implementation
            // Source: String Fetch(Int64 identifier)

            // Prepare parameters
            var param0 = (long)id; // Int32 to Int64

            // Call source method
            var result = _repository.Fetch(param0);
            return result;
        }

        // Generated with multiple parameter transformations
        public void Process(int x, int y)
        {
            // Auto-generated implementation
            // Source: Void Execute(Int64 a, Double b)

            // Prepare parameters
            var param0 = (long)x; // Int32 to Int64
            var param1 = (double)y; // Int32 to Double

            // Call source method
            _repository.Execute(param0, param1);
        }

        // Generated with return type transformation
        public string GetValue()
        {
            // Auto-generated implementation
            // Source: Int32 Calculate()

            // Call source method
            var result = _repository.Calculate();

            // Transform return value: Int32 to String
            return result.ToString();
        }
    }
}
