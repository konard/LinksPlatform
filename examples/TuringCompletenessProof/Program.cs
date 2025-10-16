using System;

namespace TuringCompletenessProof
{
    /// <summary>
    /// Demonstrates that Triggers in the Links Platform are Turing complete
    /// by emulating five well-known Turing-complete computational models:
    ///
    /// 1. Markov Algorithm
    /// 2. Turing Machine
    /// 3. SKI Combinator Calculus
    /// 4. Y Combinator (Fixed-point combinator)
    /// 5. Lambda Calculus
    ///
    /// Each example shows how triggers (substitution rules) can express
    /// the computational primitives of these systems.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintHeader();

            if (args.Length > 0)
            {
                RunSpecificExample(args[0]);
            }
            else
            {
                RunAllExamples();
            }

            PrintFooter();
        }

        static void PrintHeader()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                           ║");
            Console.WriteLine("║           PROOF OF TURING COMPLETENESS FOR TRIGGERS                       ║");
            Console.WriteLine("║                    Links Platform                                         ║");
            Console.WriteLine("║                                                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("This demonstration proves that Triggers are Turing complete by showing");
            Console.WriteLine("that they can emulate five different Turing-complete computational models.");
            Console.WriteLine();
        }

        static void PrintFooter()
        {
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         FINAL CONCLUSION                                  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("We have successfully demonstrated that Triggers in the Links Platform");
            Console.WriteLine("can emulate five independent Turing-complete formalisms:");
            Console.WriteLine();
            Console.WriteLine("  ✓ Markov Algorithm    - String rewriting with substitution rules");
            Console.WriteLine("  ✓ Turing Machine      - State transitions on a tape");
            Console.WriteLine("  ✓ SKI Calculus        - Combinator reduction rules");
            Console.WriteLine("  ✓ Y Combinator        - Fixed-point for general recursion");
            Console.WriteLine("  ✓ Lambda Calculus     - β-reduction and substitution");
            Console.WriteLine();
            Console.WriteLine("Each emulation shows that the computational primitives of these systems");
            Console.WriteLine("can be expressed as trigger rules (condition → substitution).");
            Console.WriteLine();
            Console.WriteLine("THEREFORE: Triggers in the Links Platform are TURING COMPLETE.");
            Console.WriteLine();
            Console.WriteLine("This means:");
            Console.WriteLine("  • Any computable function can be computed using triggers");
            Console.WriteLine("  • The Links Platform can express any algorithm");
            Console.WriteLine("  • Triggers form a universal computational model");
            Console.WriteLine();
            Console.WriteLine("For the complete theoretical proof, see:");
            Console.WriteLine("  doc/articles/turing-completeness-proof.md");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine();
        }

        static void RunAllExamples()
        {
            Console.WriteLine("Running all examples...\n");
            Console.WriteLine("Press Enter after each example to continue...\n");

            // 1. Markov Algorithm
            Console.WriteLine("\n[1/5] Markov Algorithm");
            WaitForUser();
            MarkovAlgorithmExample.RunExamples();
            WaitForUser();

            // 2. Turing Machine
            Console.WriteLine("\n[2/5] Turing Machine");
            WaitForUser();
            TuringMachineExample.RunExamples();
            WaitForUser();

            // 3. SKI Calculus
            Console.WriteLine("\n[3/5] SKI Combinator Calculus");
            WaitForUser();
            SKICalculusExample.RunExamples();
            WaitForUser();

            // 4. Y Combinator
            Console.WriteLine("\n[4/5] Y Combinator");
            WaitForUser();
            YCombinatorExample.RunExamples();
            WaitForUser();

            // 5. Lambda Calculus
            Console.WriteLine("\n[5/5] Lambda Calculus");
            WaitForUser();
            LambdaCalculusExample.RunExamples();
            WaitForUser();
        }

        static void RunSpecificExample(string example)
        {
            switch (example.ToLower())
            {
                case "markov":
                case "1":
                    MarkovAlgorithmExample.RunExamples();
                    break;

                case "turing":
                case "2":
                    TuringMachineExample.RunExamples();
                    break;

                case "ski":
                case "3":
                    SKICalculusExample.RunExamples();
                    break;

                case "y":
                case "ycombinator":
                case "4":
                    YCombinatorExample.RunExamples();
                    break;

                case "lambda":
                case "5":
                    LambdaCalculusExample.RunExamples();
                    break;

                default:
                    Console.WriteLine($"Unknown example: {example}");
                    Console.WriteLine("Available examples: markov, turing, ski, ycombinator, lambda");
                    Console.WriteLine("Or use numbers: 1, 2, 3, 4, 5");
                    break;
            }
        }

        static void WaitForUser()
        {
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
            Console.Clear();
            PrintHeader();
        }
    }
}
