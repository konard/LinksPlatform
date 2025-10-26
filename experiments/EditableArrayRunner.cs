using System;

namespace Platform.Experiments
{
    /// <summary>
    /// Runner for EditableArray tests and examples.
    /// </summary>
    public static class EditableArrayRunner
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("EditableArray Implementation - Issue #591");
            Console.WriteLine("==========================================\n");

            if (args.Length > 0 && args[0] == "tests")
            {
                EditableArrayTests.RunAllTests();
            }
            else if (args.Length > 0 && args[0] == "examples")
            {
                Platform.Examples.EditableArrayExample.RunAll();
            }
            else
            {
                Console.WriteLine("Running tests...\n");
                EditableArrayTests.RunAllTests();

                Console.WriteLine("\n\n");
                Console.WriteLine("Running examples...\n");
                Platform.Examples.EditableArrayExample.RunAll();
            }

            Console.WriteLine("\nDone!");
        }
    }
}
