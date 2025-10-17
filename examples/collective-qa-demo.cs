/*
 * Collective Q&A System Demo
 *
 * This example demonstrates how the Collective Q&A System works.
 * It shows the process of:
 * 1. Creating questions
 * 2. Adding facts with sources
 * 3. Forming conclusions based on facts
 * 4. Challenging conclusions with new data
 * 5. Reaching consensus through voting
 *
 * Usage:
 * Compile and run this as part of Platform.Examples to see the system in action.
 *
 * The system implements Jacque Fresco's approach to decision making:
 * - Instead of opinions, participants share verifiable facts
 * - Conclusions are derived from collected facts
 * - New contradicting facts trigger notifications to all contributors
 * - Consensus is reached through collective agreement
 * - Knowledge base grows over time with verified information
 */

using System;
using Platform.Examples;

class CollectiveQADemo
{
    static void Main(string[] args)
    {
        var cli = new CollectiveQASystemCLI();
        cli.Run(args);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
