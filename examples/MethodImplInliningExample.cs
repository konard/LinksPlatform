using System;
using System.IO;

namespace Platform.Examples.MethodImplInliningTransformer
{
    /// <summary>
    /// <para>
    /// Example demonstrating how to use the MethodImplAggressiveInliningTransformer
    /// to add or remove [MethodImpl(MethodImplOptions.AggressiveInlining)] attributes
    /// to/from C# source files.
    /// </para>
    /// </summary>
    public class MethodImplInliningExample
    {
        /// <summary>
        /// <para>
        /// Main entry point for the example
        /// </para>
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();
            var filePath = args[1];

            try
            {
                switch (command)
                {
                    case "add":
                        AddMethodImplToFile(filePath);
                        break;

                    case "remove":
                        RemoveMethodImplFromFile(filePath);
                        break;

                    case "test":
                        RunTests();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// <para>
        /// Adds MethodImpl attributes to all methods in a file
        /// </para>
        /// </summary>
        private static void AddMethodImplToFile(string filePath)
        {
            Console.WriteLine($"Adding [MethodImpl(MethodImplOptions.AggressiveInlining)] to: {filePath}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                Environment.Exit(1);
                return;
            }

            var backupPath = filePath + ".backup";
            File.Copy(filePath, backupPath, overwrite: true);
            Console.WriteLine($"Backup created: {backupPath}");

            MethodImplAggressiveInliningTransformer.TransformFileAdd(filePath);

            Console.WriteLine("Transformation complete!");
            Console.WriteLine($"To undo, restore from: {backupPath}");
        }

        /// <summary>
        /// <para>
        /// Removes MethodImpl attributes from all methods in a file
        /// </para>
        /// </summary>
        private static void RemoveMethodImplFromFile(string filePath)
        {
            Console.WriteLine($"Removing [MethodImpl(MethodImplOptions.AggressiveInlining)] from: {filePath}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                Environment.Exit(1);
                return;
            }

            var backupPath = filePath + ".backup";
            File.Copy(filePath, backupPath, overwrite: true);
            Console.WriteLine($"Backup created: {backupPath}");

            MethodImplAggressiveInliningTransformer.TransformFileRemove(filePath);

            Console.WriteLine("Transformation complete!");
            Console.WriteLine($"To undo, restore from: {backupPath}");
        }

        /// <summary>
        /// <para>
        /// Runs example tests to demonstrate the transformer capabilities
        /// </para>
        /// </summary>
        private static void RunTests()
        {
            Console.WriteLine("Running MethodImplInliningTransformer tests...");
            Console.WriteLine();

            // Test 1: Simple method
            var test1Input = @"
public class Example
{
    public void SimpleMethod()
    {
        Console.WriteLine(""Hello"");
    }
}";

            var test1Output = MethodImplAggressiveInliningTransformer.AddMethodImplAttributes(test1Input);
            Console.WriteLine("Test 1: Simple method");
            Console.WriteLine("Input:");
            Console.WriteLine(test1Input);
            Console.WriteLine("\nOutput:");
            Console.WriteLine(test1Output);
            Console.WriteLine("\n" + new string('-', 80) + "\n");

            // Test 2: Property with getter/setter
            var test2Input = @"
public class Example
{
    public string Name { get; set; }
}";

            var test2Output = MethodImplAggressiveInliningTransformer.AddMethodImplAttributes(test2Input);
            Console.WriteLine("Test 2: Property with auto-implemented getter/setter");
            Console.WriteLine("Input:");
            Console.WriteLine(test2Input);
            Console.WriteLine("\nOutput:");
            Console.WriteLine(test2Output);
            Console.WriteLine("\n" + new string('-', 80) + "\n");

            // Test 3: Generic method
            var test3Input = @"
public class Example<T>
{
    public T GetValue<TKey>(TKey key) where TKey : IComparable
    {
        return default(T);
    }
}";

            var test3Output = MethodImplAggressiveInliningTransformer.AddMethodImplAttributes(test3Input);
            Console.WriteLine("Test 3: Generic method with constraints");
            Console.WriteLine("Input:");
            Console.WriteLine(test3Input);
            Console.WriteLine("\nOutput:");
            Console.WriteLine(test3Output);
            Console.WriteLine("\n" + new string('-', 80) + "\n");

            // Test 4: Remove attributes
            var test4Input = @"using System.Runtime.CompilerServices;

public class Example
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Method1()
    {
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Method2()
    {
    }
}";

            var test4Output = MethodImplAggressiveInliningTransformer.RemoveMethodImplAttributes(test4Input);
            Console.WriteLine("Test 4: Remove MethodImpl attributes");
            Console.WriteLine("Input:");
            Console.WriteLine(test4Input);
            Console.WriteLine("\nOutput:");
            Console.WriteLine(test4Output);
            Console.WriteLine("\n" + new string('-', 80) + "\n");

            Console.WriteLine("All tests completed!");
        }

        /// <summary>
        /// <para>
        /// Prints usage information
        /// </para>
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("MethodImplInliningTransformer - Add/Remove MethodImpl AggressiveInlining attributes");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  MethodImplInliningExample add <file.cs>     - Add MethodImpl attributes to all methods");
            Console.WriteLine("  MethodImplInliningExample remove <file.cs>  - Remove MethodImpl attributes from all methods");
            Console.WriteLine("  MethodImplInliningExample test              - Run example tests");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  MethodImplInliningExample add MyClass.cs");
            Console.WriteLine("  MethodImplInliningExample remove MyClass.cs");
            Console.WriteLine("  MethodImplInliningExample test");
            Console.WriteLine();
            Console.WriteLine("Note: A backup file (.backup) is created before transformation.");
        }
    }
}
