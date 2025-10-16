using System;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace Platform.Experiments
{
    /// <summary>
    /// Example demonstrating persistent search and goal completion assist functionality.
    /// This experiment shows how users can subscribe to search results and be notified
    /// when their goals are achieved.
    /// </summary>
    public class PersistentSearchExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Persistent Search / Goal Completion Assist Example ===\n");

            // Create an in-memory links database
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var persistentSearch = new PersistentSearch<uint>(links);

                // Simulate User IDs
                var user1 = links.Create();
                var user2 = links.Create();

                Console.WriteLine($"Created users: User1={user1}, User2={user2}\n");

                // Example 1: User subscribes to search for connections between specific links
                Console.WriteLine("Example 1: User1 searches for links connecting concepts A and B");
                var conceptA = links.Create();
                var conceptB = links.Create();

                Console.WriteLine($"  Concept A: {conceptA}");
                Console.WriteLine($"  Concept B: {conceptB}");

                var subscription1 = persistentSearch.Subscribe(
                    userId: user1,
                    searchQuery: new[] { conceptA, conceptB },
                    onResultFound: (resultLink) =>
                    {
                        Console.WriteLine($"  [NOTIFICATION] User1: New result found! Link {resultLink}");
                        Console.WriteLine($"    Source: {links.GetSource(resultLink)}, Target: {links.GetTarget(resultLink)}");
                    },
                    onGoalComplete: () =>
                    {
                        Console.WriteLine($"  [COMPLETE] User1: Search goal achieved!");
                    }
                );

                Console.WriteLine($"  Subscription created: {subscription1}\n");

                // Simulate no matches initially
                Console.WriteLine("  Creating unrelated links...");
                var unrelatedLink = links.Create(conceptA, links.Create());
                persistentSearch.CheckSubscriptions(unrelatedLink);
                Console.WriteLine($"    Created link {unrelatedLink} (no match)\n");

                // Simulate a matching result appearing later
                Console.WriteLine("  Creating matching link...");
                var matchingLink = links.Create(conceptA, conceptB);
                persistentSearch.CheckSubscriptions(matchingLink);
                Console.WriteLine();

                // Example 2: Multiple users searching for the same thing
                Console.WriteLine("Example 2: Multiple users interested in same results");
                var targetConcept = links.Create();
                Console.WriteLine($"  Target Concept: {targetConcept}");

                var subscription2a = persistentSearch.Subscribe(
                    userId: user1,
                    searchQuery: new[] { targetConcept },
                    onResultFound: (resultLink) =>
                    {
                        Console.WriteLine($"  [NOTIFICATION] User1: Found reference to target concept in link {resultLink}");
                    }
                );

                var subscription2b = persistentSearch.Subscribe(
                    userId: user2,
                    searchQuery: new[] { targetConcept },
                    onResultFound: (resultLink) =>
                    {
                        Console.WriteLine($"  [NOTIFICATION] User2: Found reference to target concept in link {resultLink}");
                    }
                );

                Console.WriteLine($"  User1 subscription: {subscription2a}");
                Console.WriteLine($"  User2 subscription: {subscription2b}\n");

                Console.WriteLine("  Creating link with target concept...");
                var sharedResult = links.Create(targetConcept, links.Create());
                persistentSearch.CheckSubscriptions(sharedResult);
                Console.WriteLine();

                // Example 3: User satisfaction / goal completion
                Console.WriteLine("Example 3: User confirms satisfaction with results");
                Console.WriteLine($"  User1 found the answer they needed in link {matchingLink}");
                persistentSearch.MarkAsSatisfied(subscription1, matchingLink);
                Console.WriteLine();

                // Example 4: Listing active subscriptions
                Console.WriteLine("Example 4: List active subscriptions");
                var user1Subs = persistentSearch.GetUserSubscriptions(user1);
                var user2Subs = persistentSearch.GetUserSubscriptions(user2);

                Console.WriteLine($"  User1 active subscriptions: {user1Subs.Count}");
                foreach (var subId in user1Subs)
                {
                    var info = persistentSearch.GetSubscriptionInfo(subId);
                    Console.WriteLine($"    - Subscription {subId}: {info.FoundResults.Count} results found");
                }

                Console.WriteLine($"  User2 active subscriptions: {user2Subs.Count}");
                foreach (var subId in user2Subs)
                {
                    var info = persistentSearch.GetSubscriptionInfo(subId);
                    Console.WriteLine($"    - Subscription {subId}: {info.FoundResults.Count} results found");
                }
                Console.WriteLine();

                // Example 5: Unsubscribe
                Console.WriteLine("Example 5: User2 unsubscribes from search");
                persistentSearch.Unsubscribe(subscription2b);
                Console.WriteLine($"  Unsubscribed from {subscription2b}");

                Console.WriteLine($"  Creating another matching link...");
                var anotherMatch = links.Create(links.Create(), targetConcept);
                persistentSearch.CheckSubscriptions(anotherMatch);
                Console.WriteLine("  (Note: Only User1 was notified, User2 unsubscribed)\n");

                Console.WriteLine("=== Example Complete ===");
            }
        }
    }
}
