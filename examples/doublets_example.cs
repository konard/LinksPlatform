// Example: Using Doublets for Graph-like Data Storage
// This demonstrates how to use Doublets to create and query link-based structures

using System;
using System.Diagnostics;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace DoubletsExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create or open a links database file
            using var links = new UnitedMemoryLinks<uint>("example.links");

            Console.WriteLine("=== Doublets Example: Creating a Knowledge Graph ===\n");

            // Example 1: Create basic entities
            Console.WriteLine("1. Creating basic entities (points):");
            var personConcept = CreatePoint(links, "Person");
            var companyConcept = CreatePoint(links, "Company");
            var cityC concept = CreatePoint(links, "City");

            // Example 2: Create specific instances
            var alice = CreatePoint(links, "Alice");
            var bob = CreatePoint(links, "Bob");
            var techCorp = CreatePoint(links, "TechCorp");
            var sanFrancisco = CreatePoint(links, "San Francisco");

            // Example 3: Create relationships
            Console.WriteLine("\n2. Creating relationships:");
            var worksAt = CreatePoint(links, "works_at");
            var livesIn = CreatePoint(links, "lives_in");
            var locatedIn = CreatePoint(links, "located_in");

            // Alice works at TechCorp
            var aliceWorksAtTechCorp = links.GetOrCreate(
                links.GetOrCreate(alice, worksAt),
                techCorp
            );
            Console.WriteLine($"Created: Alice works_at TechCorp (link {aliceWorksAtTechCorp})");

            // Bob works at TechCorp
            var bobWorksAtTechCorp = links.GetOrCreate(
                links.GetOrCreate(bob, worksAt),
                techCorp
            );
            Console.WriteLine($"Created: Bob works_at TechCorp (link {bobWorksAtTechCorp})");

            // Alice lives in San Francisco
            var aliceLivesInSF = links.GetOrCreate(
                links.GetOrCreate(alice, livesIn),
                sanFrancisco
            );
            Console.WriteLine($"Created: Alice lives_in San Francisco (link {aliceLivesInSF})");

            // TechCorp located in San Francisco
            var techCorpInSF = links.GetOrCreate(
                links.GetOrCreate(techCorp, locatedIn),
                sanFrancisco
            );
            Console.WriteLine($"Created: TechCorp located_in San Francisco (link {techCorpInSF})");

            // Example 4: Query - Find all links
            Console.WriteLine("\n3. Querying all links:");
            var count = 0;
            links.Each(links.All(), link => {
                count++;
                Console.WriteLine($"  Link {link[0]}: Source={link[1]}, Target={link[2]}");
                return true; // Continue iteration
            });
            Console.WriteLine($"Total links: {count}");

            // Example 5: Create a sequence
            Console.WriteLine("\n4. Creating a sequence: [1, 2, 3, 4, 5]:");
            var numbers = new uint[] {
                CreatePoint(links, "1"),
                CreatePoint(links, "2"),
                CreatePoint(links, "3"),
                CreatePoint(links, "4"),
                CreatePoint(links, "5")
            };

            // Build balanced binary tree sequence
            var sequence = BuildSequence(links, numbers);
            Console.WriteLine($"Sequence created as link {sequence}");

            // Example 6: Performance test
            Console.WriteLine("\n5. Performance test: Creating 10,000 links:");
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                var link = links.Create();
                links.Update(link, link, link); // Point
            }
            sw.Stop();
            Console.WriteLine($"Created 10,000 points in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average: {sw.ElapsedMilliseconds / 10.0}µs per link");

            // Example 7: Performance test - link creation
            Console.WriteLine("\n6. Performance test: Creating 10,000 pairs:");
            var points = new uint[100];
            for (int i = 0; i < 100; i++)
            {
                points[i] = CreatePoint(links, $"P{i}");
            }

            sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                var source = points[i % 100];
                var target = points[(i + 1) % 100];
                links.GetOrCreate(source, target);
            }
            sw.Stop();
            Console.WriteLine($"Created 10,000 pairs in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average: {sw.ElapsedMilliseconds / 10.0}µs per pair");

            Console.WriteLine("\n=== Example Complete ===");
            Console.WriteLine($"Database file: example.links");
            Console.WriteLine($"Total links in database: {links.Count()}");
        }

        static uint CreatePoint(ILinks<uint> links, string label)
        {
            var point = links.Create();
            point = links.Update(point, point, point);
            Console.WriteLine($"  Created point '{label}': {point}");
            return point;
        }

        static uint BuildSequence(ILinks<uint> links, uint[] elements)
        {
            if (elements.Length == 0)
                throw new ArgumentException("Cannot build sequence from empty array");
            if (elements.Length == 1)
                return elements[0];

            // Build balanced binary tree
            int mid = elements.Length / 2;
            var left = BuildSequence(links, elements[0..mid]);
            var right = BuildSequence(links, elements[mid..]);
            return links.GetOrCreate(left, right);
        }
    }
}
