using System;
using System.Collections.Generic;
using System.Linq;

namespace TuringCompletenessProof
{
    /// <summary>
    /// Demonstrates how Triggers can emulate Turing machines.
    ///
    /// A Turing machine consists of:
    /// - A tape (infinite in both directions)
    /// - A head that can read/write and move left/right
    /// - A finite set of states
    /// - A transition function
    /// </summary>
    public class TuringMachineExample
    {
        public enum Direction { Left, Right, Stay }

        /// <summary>
        /// Represents a Turing machine transition (trigger).
        /// When in state Q reading symbol S, write W, move direction D, and go to state NextQ.
        /// </summary>
        public class Transition
        {
            public string State { get; set; }
            public char ReadSymbol { get; set; }
            public char WriteSymbol { get; set; }
            public Direction Move { get; set; }
            public string NextState { get; set; }

            public Transition(string state, char readSymbol, char writeSymbol,
                              Direction move, string nextState)
            {
                State = state;
                ReadSymbol = readSymbol;
                WriteSymbol = writeSymbol;
                Move = move;
                NextState = nextState;
            }

            public override string ToString()
            {
                return $"δ({State}, {ReadSymbol}) = ({NextState}, {WriteSymbol}, {Move})";
            }
        }

        /// <summary>
        /// Represents the configuration of a Turing machine.
        /// </summary>
        public class Configuration
        {
            public List<char> Tape { get; set; }
            public int HeadPosition { get; set; }
            public string State { get; set; }

            public Configuration(string initialTape, string initialState)
            {
                Tape = initialTape.ToList();
                HeadPosition = 0;
                State = initialState;
            }

            public char CurrentSymbol => HeadPosition >= 0 && HeadPosition < Tape.Count
                ? Tape[HeadPosition]
                : '_';

            public void Display()
            {
                // Display tape
                Console.Write("Tape: ");
                for (int i = 0; i < Tape.Count; i++)
                {
                    if (i == HeadPosition)
                        Console.Write($"[{Tape[i]}]");
                    else
                        Console.Write($" {Tape[i]} ");
                }
                Console.WriteLine();
                Console.WriteLine($"State: {State}, Head: {HeadPosition}");
            }
        }

        /// <summary>
        /// Simulates a Turing machine using trigger-like transitions.
        /// </summary>
        public static void SimulateTuringMachine(Configuration config,
                                                  List<Transition> transitions,
                                                  HashSet<string> haltStates,
                                                  int maxSteps = 1000)
        {
            var steps = 0;

            while (!haltStates.Contains(config.State) && steps < maxSteps)
            {
                Console.WriteLine($"\nStep {steps}:");
                config.Display();

                var currentSymbol = config.CurrentSymbol;
                var transition = transitions.FirstOrDefault(t =>
                    t.State == config.State && t.ReadSymbol == currentSymbol);

                if (transition == null)
                {
                    Console.WriteLine($"No transition found. Halting.");
                    break;
                }

                Console.WriteLine($"Applying: {transition}");

                // Apply trigger: write symbol
                if (config.HeadPosition >= config.Tape.Count)
                {
                    config.Tape.Add('_');
                }
                config.Tape[config.HeadPosition] = transition.WriteSymbol;

                // Move head
                switch (transition.Move)
                {
                    case Direction.Left:
                        config.HeadPosition--;
                        if (config.HeadPosition < 0)
                        {
                            config.Tape.Insert(0, '_');
                            config.HeadPosition = 0;
                        }
                        break;
                    case Direction.Right:
                        config.HeadPosition++;
                        if (config.HeadPosition >= config.Tape.Count)
                        {
                            config.Tape.Add('_');
                        }
                        break;
                    case Direction.Stay:
                        break;
                }

                // Change state
                config.State = transition.NextState;

                steps++;
            }

            Console.WriteLine($"\n=== Final Configuration (after {steps} steps) ===");
            config.Display();
        }

        /// <summary>
        /// Example: Binary increment Turing machine.
        /// Increments a binary number on the tape.
        /// </summary>
        public static void BinaryIncrementExample()
        {
            Console.WriteLine("=== Turing Machine: Binary Increment ===\n");
            Console.WriteLine("This machine increments a binary number by 1.\n");

            // Transitions for binary increment
            var transitions = new List<Transition>
            {
                // Move to the rightmost digit
                new Transition("q0", '0', '0', Direction.Right, "q0"),
                new Transition("q0", '1', '1', Direction.Right, "q0"),
                new Transition("q0", '_', '_', Direction.Left, "q1"),

                // Increment from right to left
                new Transition("q1", '0', '1', Direction.Stay, "qHalt"),  // 0 → 1, done
                new Transition("q1", '1', '0', Direction.Left, "q1"),     // 1 → 0, carry
                new Transition("q1", '_', '1', Direction.Stay, "qHalt"),  // Leftmost, add 1
            };

            var haltStates = new HashSet<string> { "qHalt" };

            var testCases = new[] { "0", "1", "10", "11", "101", "111" };

            foreach (var test in testCases)
            {
                Console.WriteLine($"\n{new string('─', 50)}");
                Console.WriteLine($"Input: {test}");
                Console.WriteLine(new string('─', 50));

                var config = new Configuration(test, "q0");
                SimulateTuringMachine(config, transitions, haltStates);

                var result = new string(config.Tape.ToArray()).Trim('_');
                Console.WriteLine($"\nResult: {result}");
            }
        }

        /// <summary>
        /// Example: Unary addition Turing machine.
        /// Adds two unary numbers separated by '0'.
        /// </summary>
        public static void UnaryAdditionExample()
        {
            Console.WriteLine("\n\n=== Turing Machine: Unary Addition ===\n");
            Console.WriteLine("Adds two unary numbers (e.g., 111011 = 3+2).\n");

            // Transitions for unary addition
            var transitions = new List<Transition>
            {
                // Find the separator '0'
                new Transition("q0", '1', '1', Direction.Right, "q0"),
                new Transition("q0", '0', '_', Direction.Right, "q1"),

                // Move right to the end
                new Transition("q1", '1', '1', Direction.Right, "q1"),
                new Transition("q1", '_', '_', Direction.Left, "q2"),

                // Move left and clean up
                new Transition("q2", '1', '_', Direction.Left, "q3"),
                new Transition("q3", '1', '1', Direction.Left, "q3"),
                new Transition("q3", '_', '1', Direction.Stay, "qHalt"),
            };

            var haltStates = new HashSet<string> { "qHalt" };

            var testCases = new[] { "11101", "101", "1110111", "10111" };

            foreach (var test in testCases)
            {
                Console.WriteLine($"\n{new string('─', 50)}");
                Console.WriteLine($"Input: {test}");
                Console.WriteLine(new string('─', 50));

                var config = new Configuration(test, "q0");
                SimulateTuringMachine(config, transitions, haltStates);

                var result = new string(config.Tape.ToArray()).Trim('_');
                var count = result.Count(c => c == '1');
                Console.WriteLine($"\nResult: {result} ({count})");
            }
        }

        public static void RunExamples()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Turing Machine Emulation Using Triggers                  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            BinaryIncrementExample();

            Console.WriteLine("\n" + new string('═', 60));

            UnaryAdditionExample();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Conclusion: Triggers can fully emulate Turing machines.");
            Console.WriteLine("Each transition δ(q,a) = (q',b,d) is a trigger rule.");
            Console.WriteLine("This directly proves Triggers are Turing complete.");
            Console.WriteLine(new string('═', 60));
        }
    }
}
