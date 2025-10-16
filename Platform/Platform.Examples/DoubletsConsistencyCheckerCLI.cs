using System;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Doublets consistency checker and recovery tool.
    /// Provides commands to check consistency and recover from database damage.
    /// </summary>
    public class DoubletsConsistencyCheckerCLI : ICommandLineInterface
    {
        /// <summary>
        /// Runs the consistency checker CLI.
        /// </summary>
        /// <param name="args">
        /// Command-line arguments:
        /// - args[0]: Path to the database file
        /// - args[1]: Operation - "check", "full-recovery", or "partial-recovery"
        /// - args[2]: (Optional) "--no-backup" to skip backup during recovery
        /// </param>
        public void Run(params string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    ShowUsage();
                    return;
                }

                var dbPath = args[0];
                var operation = args[1].ToLowerInvariant();
                var createBackup = !Array.Exists(args, arg => arg == "--no-backup");

                Console.WriteLine("=================================================");
                Console.WriteLine("  Doublets Consistency Checker & Recovery Tool");
                Console.WriteLine("=================================================");
                Console.WriteLine($"Database file: {dbPath}");
                Console.WriteLine($"Operation: {operation}");
                Console.WriteLine();

                // Open the database with UInt64 addresses (most common)
                using (var links = new UnitedMemoryLinks<ulong>(dbPath))
                {
                    var checker = new DoubletsConsistencyChecker<ulong>(links);

                    switch (operation)
                    {
                        case "check":
                            PerformCheck(checker);
                            break;

                        case "full-recovery":
                            PerformFullRecovery(checker, createBackup);
                            break;

                        case "partial-recovery":
                            PerformPartialRecovery(checker);
                            break;

                        default:
                            Console.WriteLine($"Unknown operation: {operation}");
                            ShowUsage();
                            return;
                    }
                }

                Console.WriteLine("\nOperation completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            }
        }

        private void PerformCheck(DoubletsConsistencyChecker<ulong> checker)
        {
            var isConsistent = checker.CheckConsistency();
            checker.DisplayResults();

            if (!isConsistent)
            {
                Console.WriteLine("\n=== RECOMMENDED ACTIONS ===");
                Console.WriteLine("To recover from these errors, run:");
                Console.WriteLine("  - For minor issues: partial-recovery");
                Console.WriteLine("  - For major issues: full-recovery (recommended to backup first)");
            }
        }

        private void PerformFullRecovery(DoubletsConsistencyChecker<ulong> checker, bool createBackup)
        {
            if (createBackup)
            {
                Console.WriteLine("⚠ IMPORTANT: Make sure you have backed up your database file before proceeding!");
                Console.WriteLine("Press Enter to continue or Ctrl+C to cancel...");
                Console.ReadLine();
            }

            var success = checker.PerformFullRecovery(createBackup);

            if (success)
            {
                Console.WriteLine("\nRunning consistency check after recovery...");
                var isConsistent = checker.CheckConsistency();
                checker.DisplayResults();

                if (isConsistent)
                {
                    Console.WriteLine("\n✓ Recovery successful! Database is now consistent.");
                }
                else
                {
                    Console.WriteLine("\n⚠ Recovery completed but some issues remain.");
                }
            }
        }

        private void PerformPartialRecovery(DoubletsConsistencyChecker<ulong> checker)
        {
            var success = checker.PerformPartialRecovery();

            if (success)
            {
                Console.WriteLine("\nRunning consistency check after recovery...");
                var isConsistent = checker.CheckConsistency();
                checker.DisplayResults();

                if (isConsistent)
                {
                    Console.WriteLine("\n✓ Recovery successful! Database is now consistent.");
                }
                else
                {
                    Console.WriteLine("\n⚠ Recovery completed but some issues remain.");
                    Console.WriteLine("Consider running full-recovery for a more thorough repair.");
                }
            }
        }

        private void ShowUsage()
        {
            Console.WriteLine("Usage: DoubletsConsistencyChecker <database-file> <operation> [options]");
            Console.WriteLine();
            Console.WriteLine("Operations:");
            Console.WriteLine("  check             - Check database consistency without making changes");
            Console.WriteLine("  partial-recovery  - Perform fast recovery (validates and fixes minor issues)");
            Console.WriteLine("  full-recovery     - Perform thorough recovery (rebuilds indexes, slower)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --no-backup       - Skip backup prompt during recovery");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  DoubletsConsistencyChecker db.links check");
            Console.WriteLine("  DoubletsConsistencyChecker db.links partial-recovery");
            Console.WriteLine("  DoubletsConsistencyChecker db.links full-recovery");
            Console.WriteLine("  DoubletsConsistencyChecker db.links full-recovery --no-backup");
        }
    }
}
