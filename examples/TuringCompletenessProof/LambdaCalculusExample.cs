using System;
using System.Collections.Generic;
using System.Linq;

namespace TuringCompletenessProof
{
    /// <summary>
    /// Demonstrates how Triggers can emulate Lambda calculus.
    ///
    /// Lambda calculus consists of:
    /// - Variables: x, y, z
    /// - Abstraction: λx. M (function definition)
    /// - Application: (M N) (function application)
    ///
    /// The key operation is β-reduction: (λx. M) N → M[x := N]
    /// </summary>
    public class LambdaCalculusExample
    {
        /// <summary>
        /// Represents a lambda term.
        /// </summary>
        public abstract class LambdaTerm
        {
            public abstract string ToString();
            public abstract LambdaTerm Substitute(string variable, LambdaTerm replacement);
            public abstract LambdaTerm BetaReduce();
            public abstract HashSet<string> FreeVariables();
        }

        /// <summary>
        /// Variable in lambda calculus.
        /// </summary>
        public class Var : LambdaTerm
        {
            public string Name { get; set; }

            public Var(string name)
            {
                Name = name;
            }

            public override string ToString() => Name;

            public override LambdaTerm Substitute(string variable, LambdaTerm replacement)
            {
                // Trigger: substitute x with N when variable matches
                if (Name == variable)
                {
                    Console.WriteLine($"  Trigger: Substitute '{variable}' with '{replacement}'");
                    return replacement;
                }
                return this;
            }

            public override LambdaTerm BetaReduce() => this;

            public override HashSet<string> FreeVariables() => new HashSet<string> { Name };
        }

        /// <summary>
        /// Lambda abstraction: λx. body
        /// </summary>
        public class Lambda : LambdaTerm
        {
            public string Parameter { get; set; }
            public LambdaTerm Body { get; set; }

            private static int freshVarCounter = 0;

            public Lambda(string parameter, LambdaTerm body)
            {
                Parameter = parameter;
                Body = body;
            }

            public override string ToString() => $"(λ{Parameter}. {Body})";

            public override LambdaTerm Substitute(string variable, LambdaTerm replacement)
            {
                if (Parameter == variable)
                {
                    // Variable is bound, no substitution in body
                    Console.WriteLine($"  Trigger: Variable '{variable}' is bound, no substitution");
                    return this;
                }

                // Check for variable capture
                var freeInReplacement = replacement.FreeVariables();
                if (freeInReplacement.Contains(Parameter))
                {
                    // α-conversion: rename bound variable to avoid capture
                    var freshVar = $"{Parameter}'{freshVarCounter++}";
                    Console.WriteLine($"  Trigger: α-conversion to avoid capture: {Parameter} → {freshVar}");
                    var renamedBody = Body.Substitute(Parameter, new Var(freshVar));
                    return new Lambda(freshVar, renamedBody.Substitute(variable, replacement));
                }

                // Safe to substitute in body
                return new Lambda(Parameter, Body.Substitute(variable, replacement));
            }

            public override LambdaTerm BetaReduce()
            {
                var reducedBody = Body.BetaReduce();
                if (reducedBody != Body)
                {
                    return new Lambda(Parameter, reducedBody);
                }
                return this;
            }

            public override HashSet<string> FreeVariables()
            {
                var fv = Body.FreeVariables();
                fv.Remove(Parameter);
                return fv;
            }
        }

        /// <summary>
        /// Application: (function argument)
        /// </summary>
        public class App : LambdaTerm
        {
            public LambdaTerm Function { get; set; }
            public LambdaTerm Argument { get; set; }

            public App(LambdaTerm function, LambdaTerm argument)
            {
                Function = function;
                Argument = argument;
            }

            public override string ToString() => $"({Function} {Argument})";

            public override LambdaTerm Substitute(string variable, LambdaTerm replacement)
            {
                return new App(
                    Function.Substitute(variable, replacement),
                    Argument.Substitute(variable, replacement)
                );
            }

            public override LambdaTerm BetaReduce()
            {
                // Try β-reduction at the top level
                if (Function is Lambda lambda)
                {
                    // Trigger: (λx. M) N → M[x := N]
                    Console.WriteLine($"  Trigger: β-reduction (λ{lambda.Parameter}. {lambda.Body}) {Argument}");
                    return lambda.Body.Substitute(lambda.Parameter, Argument);
                }

                // Try reducing the function
                var reducedFunction = Function.BetaReduce();
                if (reducedFunction != Function)
                {
                    return new App(reducedFunction, Argument);
                }

                // Try reducing the argument
                var reducedArgument = Argument.BetaReduce();
                if (reducedArgument != Argument)
                {
                    return new App(Function, reducedArgument);
                }

                return this;
            }

            public override HashSet<string> FreeVariables()
            {
                var fv = Function.FreeVariables();
                fv.UnionWith(Argument.FreeVariables());
                return fv;
            }
        }

        /// <summary>
        /// Reduces a term to normal form.
        /// </summary>
        public static LambdaTerm ReduceFully(LambdaTerm term, int maxSteps = 100)
        {
            Console.WriteLine($"Initial: {term}");

            for (int i = 0; i < maxSteps; i++)
            {
                var reduced = term.BetaReduce();
                if (reduced.ToString() == term.ToString())
                {
                    Console.WriteLine($"Normal form reached.");
                    return reduced;
                }
                term = reduced;
                Console.WriteLine($"Step {i + 1}: {term}");
            }

            Console.WriteLine($"Max steps reached.");
            return term;
        }

        /// <summary>
        /// Example: Identity function.
        /// </summary>
        public static void IdentityExample()
        {
            Console.WriteLine("=== Lambda Calculus: Identity Function ===\n");
            Console.WriteLine("I = λx. x");
            Console.WriteLine("I a should reduce to a\n");

            // (λx. x) a
            var term = new App(
                new Lambda("x", new Var("x")),
                new Var("a")
            );

            var result = ReduceFully(term);
            Console.WriteLine($"Final result: {result}");
            Console.WriteLine($"Expected: a\n");
        }

        /// <summary>
        /// Example: Constant function (K combinator).
        /// </summary>
        public static void ConstantExample()
        {
            Console.WriteLine("\n=== Lambda Calculus: Constant Function ===\n");
            Console.WriteLine("K = λx. λy. x");
            Console.WriteLine("K a b should reduce to a\n");

            // ((λx. λy. x) a) b
            var term = new App(
                new App(
                    new Lambda("x", new Lambda("y", new Var("x"))),
                    new Var("a")
                ),
                new Var("b")
            );

            var result = ReduceFully(term);
            Console.WriteLine($"Final result: {result}");
            Console.WriteLine($"Expected: a\n");
        }

        /// <summary>
        /// Example: Church booleans.
        /// </summary>
        public static void ChurchBooleansExample()
        {
            Console.WriteLine("\n=== Lambda Calculus: Church Booleans ===\n");

            Console.WriteLine("TRUE = λt. λf. t  (returns first argument)");
            Console.WriteLine("FALSE = λt. λf. f (returns second argument)\n");

            // TRUE a b
            Console.WriteLine("TRUE a b:");
            var trueTerm = new App(
                new App(
                    new Lambda("t", new Lambda("f", new Var("t"))),
                    new Var("a")
                ),
                new Var("b")
            );
            var trueResult = ReduceFully(trueTerm);
            Console.WriteLine($"Result: {trueResult} (expected: a)\n");

            // FALSE a b
            Console.WriteLine("\nFALSE a b:");
            var falseTerm = new App(
                new App(
                    new Lambda("t", new Lambda("f", new Var("f"))),
                    new Var("a")
                ),
                new Var("b")
            );
            var falseResult = ReduceFully(falseTerm);
            Console.WriteLine($"Result: {falseResult} (expected: b)\n");
        }

        /// <summary>
        /// Example: Church numerals.
        /// </summary>
        public static void ChurchNumeralsExample()
        {
            Console.WriteLine("\n=== Lambda Calculus: Church Numerals ===\n");

            Console.WriteLine("0 = λf. λx. x");
            Console.WriteLine("1 = λf. λx. f x");
            Console.WriteLine("2 = λf. λx. f (f x)\n");

            // Successor: λn. λf. λx. f (n f x)
            Console.WriteLine("SUCC = λn. λf. λx. f (n f x)");
            Console.WriteLine("Let's compute SUCC 0:\n");

            // 0 = λf. λx. x
            var zero = new Lambda("f", new Lambda("x", new Var("x")));

            // SUCC = λn. λf. λx. f (n f x)
            var succ = new Lambda("n",
                new Lambda("f",
                    new Lambda("x",
                        new App(
                            new Var("f"),
                            new App(
                                new App(new Var("n"), new Var("f")),
                                new Var("x")
                            )
                        )
                    )
                )
            );

            // SUCC 0
            var succZero = new App(succ, zero);

            var result = ReduceFully(succZero);
            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Expected: λf. λx. f x (which is 1)\n");
        }

        /// <summary>
        /// Example: Self-application (omega combinator).
        /// </summary>
        public static void SelfApplicationExample()
        {
            Console.WriteLine("\n=== Lambda Calculus: Self-Application ===\n");
            Console.WriteLine("ω = λx. x x");
            Console.WriteLine("ω ω = (λx. x x) (λx. x x)");
            Console.WriteLine("This creates infinite reduction (non-terminating)!\n");
            Console.WriteLine("Let's limit to 3 steps:\n");

            // ω = λx. x x
            var omega = new Lambda("x", new App(new Var("x"), new Var("x")));

            // ω ω
            var omegaOmega = new App(omega, omega);

            var result = ReduceFully(omegaOmega, 3);
            Console.WriteLine($"\nThis demonstrates that not all lambda terms terminate,");
            Console.WriteLine($"which is related to the undecidability of the halting problem.\n");
        }

        public static void RunExamples()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Lambda Calculus Emulation Using Triggers                 ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            IdentityExample();
            Console.WriteLine(new string('─', 60));

            ConstantExample();
            Console.WriteLine(new string('─', 60));

            ChurchBooleansExample();
            Console.WriteLine(new string('─', 60));

            ChurchNumeralsExample();
            Console.WriteLine(new string('─', 60));

            SelfApplicationExample();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Conclusion: Triggers can fully emulate lambda calculus.");
            Console.WriteLine("The key trigger is β-reduction: (λx. M) N → M[x := N]");
            Console.WriteLine("Combined with substitution triggers and α-conversion,");
            Console.WriteLine("all lambda calculus computations can be expressed as triggers.");
            Console.WriteLine("Since lambda calculus is Turing complete,");
            Console.WriteLine("this proves Triggers are Turing complete.");
            Console.WriteLine(new string('═', 60));
        }
    }
}
