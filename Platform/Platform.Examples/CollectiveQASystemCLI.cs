using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Collective Q&A System.
    /// Demonstrates how to use the collective decision-making system.
    /// </summary>
    public class CollectiveQASystemCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Collective Decision Making & Q&A System ===");
            Console.WriteLine("A system for collective decision making based on facts, not opinions.\n");

            var system = new CollectiveQASystem();

            // Demo: Venus Project activist scenario from the issue
            DemoVenusProjectScenario(system);

            Console.WriteLine("\n=== System Summary ===");
            DisplaySystemSummary(system);
        }

        private void DemoVenusProjectScenario(CollectiveQASystem system)
        {
            Console.WriteLine("--- Scenario: Jacque Fresco's approach to decision making ---\n");

            // Register participants
            var alice = system.RegisterParticipant("Alice", "alice@example.com");
            var bob = system.RegisterParticipant("Bob", "bob@example.com");
            var charlie = system.RegisterParticipant("Charlie", "charlie@example.com");

            Console.WriteLine($"Registered participants: {alice.Name}, {bob.Name}, {charlie.Name}\n");

            // Create a question
            var question = system.CreateQuestion(
                "How should decisions be made in a resource-based economy?",
                alice
            );
            Console.WriteLine($"Question created by {alice.Name}:");
            Console.WriteLine($"  '{question.QuestionText}'\n");

            // Alice adds a fact with source
            var fact1 = system.AddFact(
                question,
                "Jacque Fresco stated that we should not rely on opinions, but instead share how we arrived at our conclusions.",
                "Jacque Fresco interview",
                alice
            );
            Console.WriteLine($"{alice.Name} added fact:");
            Console.WriteLine($"  '{fact1.Description}'");
            Console.WriteLine($"  Source: {fact1.Source}\n");

            // Bob adds supporting fact
            var fact2 = system.AddFact(
                question,
                "We should share what we know about a topic, and based on all voiced information, we should arrive at unified conclusions.",
                "Jacque Fresco lectures",
                bob
            );
            Console.WriteLine($"{bob.Name} added fact:");
            Console.WriteLine($"  '{fact2.Description}'");
            Console.WriteLine($"  Source: {fact2.Source}\n");

            // Bob adds another fact about automation
            var fact3 = system.AddFact(
                question,
                "The process can be automated through a database system that stores collective knowledge and conclusions.",
                "Analysis of Venus Project principles",
                bob
            );
            Console.WriteLine($"{bob.Name} added fact:");
            Console.WriteLine($"  '{fact3.Description}'");
            Console.WriteLine($"  Source: {fact3.Source}\n");

            // Create a conclusion based on the facts
            var conclusion = system.AddOrUpdateConclusion(
                question,
                "Decisions should be based on verifiable facts and data, not personal opinions. A system should allow new people to read conclusions and supporting facts, and if new contradicting data appears, all contributors should be notified to review and potentially update the conclusion.",
                new List<Fact> { fact1, fact2, fact3 },
                bob
            );
            Console.WriteLine($"{bob.Name} proposed conclusion:");
            Console.WriteLine($"  '{conclusion.ConclusionText}'\n");

            // Charlie discovers new contradicting information
            Console.WriteLine($"{charlie.Name} found new information that challenges the conclusion.\n");
            var contradictingFact = system.AddFact(
                question,
                "In practice, purely data-driven decisions can miss important human context and ethical considerations that are not easily quantifiable.",
                "Recent social science research",
                charlie
            );

            system.ChallengeConclusion(question, contradictingFact, charlie);
            Console.WriteLine($"{charlie.Name} challenged the conclusion with a contradicting fact:");
            Console.WriteLine($"  '{contradictingFact.Description}'");
            Console.WriteLine($"  Source: {contradictingFact.Source}\n");

            // Show notifications
            Console.WriteLine("Notifications sent:");
            foreach (var participant in new[] { alice, bob })
            {
                if (participant.Notifications.Any())
                {
                    Console.WriteLine($"  {participant.Name} received: {participant.Notifications.Last().Title}");
                }
            }
            Console.WriteLine();

            // Participants vote on the contradicting fact
            Console.WriteLine("Participants voting on the contradicting fact:");
            system.VoteOnContradictingFact(question, contradictingFact, alice, true);
            Console.WriteLine($"  {alice.Name} voted to accept the new fact");

            system.VoteOnContradictingFact(question, contradictingFact, bob, true);
            Console.WriteLine($"  {bob.Name} voted to accept the new fact");
            Console.WriteLine();

            // Check for consensus notifications
            if (bob.Notifications.Count > 1)
            {
                Console.WriteLine($"Consensus reached! All participants notified.");
                Console.WriteLine($"  Latest notification: {bob.Notifications.Last().Title}\n");
            }

            Console.WriteLine("Outcome: The conclusion should be updated to incorporate the new fact,");
            Console.WriteLine("acknowledging both the importance of data-driven decisions and human context.\n");
        }

        private void DisplaySystemSummary(CollectiveQASystem system)
        {
            var summary = system.GetQuestionsSummary();

            Console.WriteLine($"Total questions: {summary.Count}");
            Console.WriteLine($"Total participants: {system.Participants.Count}\n");

            foreach (var q in summary)
            {
                Console.WriteLine($"Question: {q.QuestionText}");
                Console.WriteLine($"  Author: {q.Author}");
                Console.WriteLine($"  Facts: {q.FactsCount}");
                Console.WriteLine($"  Current conclusion: {(q.CurrentConclusion != null ? "Yes" : "No")}");
                Console.WriteLine($"  Has contradicting facts: {q.HasContradictingFacts}");
                Console.WriteLine();
            }
        }
    }
}
