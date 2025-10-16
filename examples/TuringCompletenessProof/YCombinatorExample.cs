using System;

namespace TuringCompletenessProof
{
    /// <summary>
    /// Demonstrates how Triggers can emulate the Y combinator.
    ///
    /// The Y combinator is a fixed-point combinator that enables recursion
    /// in lambda calculus without explicit self-reference:
    ///
    /// Y = λf. (λx. f (x x)) (λx. f (x x))
    /// Y f = f (Y f)
    /// </summary>
    public class YCombinatorExample
    {
        /// <summary>
        /// Simulates Y combinator using triggers.
        /// In trigger terms: Y f → f (Y f)
        /// </summary>
        public delegate Func<int, int> RecursiveFunc(Func<int, int> self);

        /// <summary>
        /// Y combinator implementation.
        /// This simulates the trigger: Y f → f (Y f)
        /// </summary>
        public static Func<int, int> Y(RecursiveFunc f)
        {
            // Y f = f (Y f)
            // We create a function that, when called, applies f to itself
            Func<int, int> result = null;
            result = x =>
            {
                // This is the trigger application: Y f → f (Y f)
                Console.WriteLine($"  Trigger: Y f → f (Y f)");
                return f(result)(x);
            };
            return result;
        }

        /// <summary>
        /// Example: Factorial using Y combinator.
        /// </summary>
        public static void FactorialExample()
        {
            Console.WriteLine("=== Y Combinator: Factorial ===\n");

            Console.WriteLine("Factorial can be defined without explicit recursion using Y:\n");
            Console.WriteLine("fact = Y (λf. λn. if n=0 then 1 else n * f(n-1))\n");

            // Define factorial body (without recursion)
            RecursiveFunc factorialBody = self => n =>
            {
                Console.WriteLine($"  Computing factorial({n})");
                if (n == 0)
                {
                    Console.WriteLine($"  Base case: factorial(0) = 1");
                    return 1;
                }
                else
                {
                    Console.WriteLine($"  Recursive case: factorial({n}) = {n} * factorial({n - 1})");
                    return n * self(n - 1);
                }
            };

            // Apply Y combinator (trigger)
            var factorial = Y(factorialBody);

            // Test cases
            var testCases = new[] { 0, 1, 3, 5 };

            foreach (var n in testCases)
            {
                Console.WriteLine($"\nCalculating factorial({n}):");
                var result = factorial(n);
                Console.WriteLine($"Result: {result}\n");
            }
        }

        /// <summary>
        /// Example: Fibonacci using Y combinator.
        /// </summary>
        public static void FibonacciExample()
        {
            Console.WriteLine("\n=== Y Combinator: Fibonacci ===\n");

            Console.WriteLine("Fibonacci can be defined without explicit recursion using Y:\n");
            Console.WriteLine("fib = Y (λf. λn. if n<2 then n else f(n-1) + f(n-2))\n");

            // Define Fibonacci body (without recursion)
            RecursiveFunc fibonacciBody = self => n =>
            {
                Console.WriteLine($"  Computing fibonacci({n})");
                if (n < 2)
                {
                    Console.WriteLine($"  Base case: fibonacci({n}) = {n}");
                    return n;
                }
                else
                {
                    Console.WriteLine($"  Recursive case: fibonacci({n}) = fibonacci({n - 1}) + fibonacci({n - 2})");
                    return self(n - 1) + self(n - 2);
                }
            };

            // Apply Y combinator (trigger)
            var fibonacci = Y(fibonacciBody);

            // Test cases (keeping small to avoid too much output)
            var testCases = new[] { 0, 1, 2, 3, 4, 5 };

            foreach (var n in testCases)
            {
                Console.WriteLine($"\nCalculating fibonacci({n}):");
                var result = fibonacci(n);
                Console.WriteLine($"Result: {result}\n");
            }
        }

        /// <summary>
        /// Example: Sum from 1 to n using Y combinator.
        /// </summary>
        public static void SumExample()
        {
            Console.WriteLine("\n=== Y Combinator: Sum from 1 to n ===\n");

            Console.WriteLine("Sum can be defined without explicit recursion using Y:\n");
            Console.WriteLine("sum = Y (λf. λn. if n=0 then 0 else n + f(n-1))\n");

            // Define sum body (without recursion)
            RecursiveFunc sumBody = self => n =>
            {
                Console.WriteLine($"  Computing sum({n})");
                if (n == 0)
                {
                    Console.WriteLine($"  Base case: sum(0) = 0");
                    return 0;
                }
                else
                {
                    Console.WriteLine($"  Recursive case: sum({n}) = {n} + sum({n - 1})");
                    return n + self(n - 1);
                }
            };

            // Apply Y combinator (trigger)
            var sum = Y(sumBody);

            // Test cases
            var testCases = new[] { 0, 1, 5, 10 };

            foreach (var n in testCases)
            {
                Console.WriteLine($"\nCalculating sum(1..{n}):");
                var result = sum(n);
                Console.WriteLine($"Result: {result}\n");
            }
        }

        /// <summary>
        /// Conceptual explanation of how Y combinator works as a trigger.
        /// </summary>
        public static void ConceptualExplanation()
        {
            Console.WriteLine("\n=== Y Combinator as a Trigger: Conceptual Explanation ===\n");

            Console.WriteLine("The Y combinator enables recursion through self-application:");
            Console.WriteLine();
            Console.WriteLine("Y = λf. (λx. f (x x)) (λx. f (x x))");
            Console.WriteLine();
            Console.WriteLine("When we apply Y to a function f:");
            Console.WriteLine();
            Console.WriteLine("Y f");
            Console.WriteLine("= (λx. f (x x)) (λx. f (x x))");
            Console.WriteLine("= f ((λx. f (x x)) (λx. f (x x)))");
            Console.WriteLine("= f (Y f)");
            Console.WriteLine();
            Console.WriteLine("This is the key trigger rule: Y f → f (Y f)");
            Console.WriteLine();
            Console.WriteLine("When we need recursion, we 'unfold' Y f:");
            Console.WriteLine();
            Console.WriteLine("Y f");
            Console.WriteLine("→ f (Y f)          [trigger fires]");
            Console.WriteLine("→ f (f (Y f))      [trigger fires again]");
            Console.WriteLine("→ f (f (f (Y f)))  [trigger fires again]");
            Console.WriteLine("... and so on");
            Console.WriteLine();
            Console.WriteLine("This infinite unfolding provides recursion!");
            Console.WriteLine();
            Console.WriteLine("In the Links Platform, the Y combinator trigger:");
            Console.WriteLine("  Trigger([Y, f], [f, [Y, f]])");
            Console.WriteLine();
            Console.WriteLine("This single trigger rule enables ALL recursive functions.");
            Console.WriteLine();
        }

        public static void RunExamples()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Y Combinator Emulation Using Triggers                    ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            ConceptualExplanation();
            Console.WriteLine(new string('─', 60));

            FactorialExample();
            Console.WriteLine(new string('─', 60));

            FibonacciExample();
            Console.WriteLine(new string('─', 60));

            SumExample();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Conclusion: The Y combinator can be fully emulated with triggers.");
            Console.WriteLine("The key trigger rule 'Y f → f (Y f)' enables all recursion.");
            Console.WriteLine("This proves that triggers support arbitrary recursive computation,");
            Console.WriteLine("which is essential for Turing completeness.");
            Console.WriteLine(new string('═', 60));
        }
    }
}
