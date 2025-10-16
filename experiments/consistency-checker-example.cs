/*
 * Doublets Consistency Checker - Usage Example
 *
 * This example demonstrates how to use the DoubletsConsistencyChecker
 * to check database consistency and perform recovery operations.
 *
 * Related to issue #39: https://github.com/konard/LinksPlatform/issues/39
 */

using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace ConsistencyCheckerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example 1: Create a test database and check its consistency
            Console.WriteLine("=== Example 1: Creating a test database ===\n");
            CreateTestDatabase("test-db.links");

            // Example 2: Check consistency
            Console.WriteLine("\n=== Example 2: Checking consistency ===\n");
            CheckDatabaseConsistency("test-db.links");

            // Example 3: Simulate corruption and perform recovery
            Console.WriteLine("\n=== Example 3: Recovery demonstration ===\n");
            Console.WriteLine("(In a real scenario, this would repair a corrupted database)");
            DemonstrateRecovery("test-db.links");
        }

        static void CreateTestDatabase(string dbPath)
        {
            using (var links = new UnitedMemoryLinks<ulong>(dbPath))
            {
                Console.WriteLine($"Created database: {dbPath}");

                // Create some links
                var link1 = links.Create();
                var link2 = links.Create();
                var link3 = links.Create();

                // Update links to reference each other
                links.Update(link1, link1, link2);
                links.Update(link2, link1, link3);
                links.Update(link3, link2, link1);

                Console.WriteLine($"Created {links.Count()} links");
                Console.WriteLine($"Link 1: {links.Format(link1)}");
                Console.WriteLine($"Link 2: {links.Format(link2)}");
                Console.WriteLine($"Link 3: {links.Format(link3)}");
            }
        }

        static void CheckDatabaseConsistency(string dbPath)
        {
            using (var links = new UnitedMemoryLinks<ulong>(dbPath))
            {
                var checker = new DoubletsConsistencyChecker<ulong>(links);

                // Perform consistency check
                var isConsistent = checker.CheckConsistency();

                // Display results
                checker.DisplayResults();

                if (isConsistent)
                {
                    Console.WriteLine("\n✓ Database is consistent!");
                }
                else
                {
                    Console.WriteLine("\n✗ Database has consistency issues!");
                }
            }
        }

        static void DemonstrateRecovery(string dbPath)
        {
            using (var links = new UnitedMemoryLinks<ulong>(dbPath))
            {
                var checker = new DoubletsConsistencyChecker<ulong>(links);

                Console.WriteLine("Performing partial recovery...");
                var success = checker.PerformPartialRecovery();

                if (success)
                {
                    Console.WriteLine("\nVerifying after recovery...");
                    checker.CheckConsistency();
                    checker.DisplayResults();
                }
            }
        }
    }
}

/*
 * Expected Output:
 *
 * === Example 1: Creating a test database ===
 *
 * Created database: test-db.links
 * Created 3 links
 * Link 1: 1:1->2
 * Link 2: 2:1->3
 * Link 3: 3:2->1
 *
 * === Example 2: Checking consistency ===
 *
 * Starting consistency check...
 *   Checking link address validity...
 *   Checking link references...
 *   Checking header consistency...
 *     Link count validated: 3 links
 *   Checking for dangling references...
 * ✓ Consistency check passed. No errors found.
 *
 * ✓ Database is consistent!
 *
 * === Example 3: Recovery demonstration ===
 *
 * (In a real scenario, this would repair a corrupted database)
 * Performing partial recovery...
 *
 * === PARTIAL RECOVERY (FAST) ===
 * This will validate and repair minor inconsistencies.
 *
 * 1. Cleaning incomplete links...
 *    No incomplete links found.
 *
 * 2. Validating index structures...
 *    Validating source index...
 *    Checked 3 links.
 *
 * 3. Checking for transaction log...
 *    (Transaction log replay not yet implemented)
 *
 * ✓ Partial recovery completed successfully.
 *
 * Verifying after recovery...
 * Starting consistency check...
 *   Checking link address validity...
 *   Checking link references...
 *   Checking header consistency...
 *     Link count validated: 3 links
 *   Checking for dangling references...
 * ✓ Consistency check passed. No errors found.
 */
