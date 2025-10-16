using System;

namespace Platform.Data.Triplets.Memory.Examples
{
    /// <summary>
    /// Demonstrates basic usage of the triplet links implementation.
    /// </summary>
    public static class BasicUsageExample
    {
        /// <summary>
        /// Runs example scenarios showing how to create and work with triplet links.
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== Basic Triplet Links Usage Example ===\n");

            // Example 1: Creating simple links
            Console.WriteLine("Example 1: Creating Simple Links");
            var thing = Link.CreateLinkLinkingItself();
            var isA = Link.CreateOutcomingSelfLinker(null);
            var link = Link.CreateCycleSelfLink(isA);
            isA.Target = link;

            Console.WriteLine($"Created 'thing': {thing}");
            Console.WriteLine($"Created 'isA': {isA}");
            Console.WriteLine($"Created 'link': {link}\n");

            // Example 2: Creating relationships
            Console.WriteLine("Example 2: Creating Relationships");
            var apple = Link.CreateOutcomingSelfLink(isA, thing);
            var banana = Link.CreateOutcomingSelfLink(isA, thing);
            var fruit = Link.CreateOutcomingSelfLink(isA, thing);

            var appleIsFruit = Link.Create(apple, isA, fruit);
            var bananaIsFruit = Link.Create(banana, isA, fruit);

            Console.WriteLine($"Created: apple is a fruit");
            Console.WriteLine($"Created: banana is a fruit\n");

            // Example 3: Querying referers
            Console.WriteLine("Example 3: Querying Referers");
            Console.WriteLine($"Links that have 'fruit' as target:");
            foreach (var referer in fruit.ReferersByTarget)
            {
                Console.WriteLine($"  - {referer}");
            }
            Console.WriteLine();

            // Example 4: Using extensions
            Console.WriteLine("Example 4: Using Extension Methods");
            Console.WriteLine($"Total referers of 'fruit': {fruit.CountReferers()}");
            Console.WriteLine($"'thing' has referers: {thing.HasReferers()}");
            Console.WriteLine($"'link' is self-reference: {link.IsSelfReference()}");
            Console.WriteLine($"'thing' is complete self-loop: {thing.IsCompleteSelfLoop()}\n");

            // Example 5: Finding specific links
            Console.WriteLine("Example 5: Finding Specific Links");
            var found = fruit.FindReferer(apple, isA, fruit);
            Console.WriteLine($"Found link (apple, isA, fruit): {found != null}\n");

            // Example 6: Deletion
            Console.WriteLine("Example 6: Deletion");
            Console.WriteLine($"Before deletion - banana has referers: {banana.HasReferers()}");
            bananaIsFruit.Delete();
            Console.WriteLine($"After deletion - banana has referers: {banana.HasReferers()}\n");

            Console.WriteLine("=== Example Complete ===");
        }
    }
}
