using System;

namespace Platform.Experiments
{
    /// <summary>
    /// Main runner for all Sequences examples demonstrating issue #24 requirements.
    ///
    /// This runner executes examples for:
    /// - Sequences CUD (Create, Update, Delete) with sequence markers
    /// - Pattern matching with AnyLink and ZeroOrMany
    /// - Sequence append/continue operations with optimization
    /// - SequencesOptions configurations
    ///
    /// These examples demonstrate the Sequences functionality requested in:
    /// https://github.com/konard/LinksPlatform/issues/24
    /// </summary>
    public class SequencesExamplesRunner
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     LinksPlatform Sequences Examples (Issue #24)           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                RunExample("Sequences CUD Operations", SequencesCUDExample.Run);
                RunExample("Pattern Matching (AnyLink)", SequencesPatternMatchingExample.Run);
                RunExample("Sequence Append/Continue", SequencesAppendExample.Run);
                RunExample("SequencesOptions Configurations", SequencesOptionsExample.Run);

                Console.WriteLine();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              All Examples Completed Successfully           ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                     Error Occurred                         ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
        }

        private static void RunExample(string name, Action exampleAction)
        {
            Console.WriteLine();
            Console.WriteLine($"┌─ Running: {name}");
            Console.WriteLine("└" + new string('─', 60));
            Console.WriteLine();

            try
            {
                exampleAction();
                Console.WriteLine();
                Console.WriteLine($"✓ {name} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"✗ {name} failed: {ex.Message}");
                throw;
            }

            Console.WriteLine(new string('─', 62));
        }
    }
}
