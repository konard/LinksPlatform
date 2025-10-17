using System;
using System.Collections.Generic;

namespace Platform.Sandbox
{
    /// <summary>
    /// Mock supervisor for testing that returns predefined decisions
    /// </summary>
    public class MockSupervisor : ISupervisor
    {
        private readonly Queue<string> _predefinedActions;

        public MockSupervisor(params string[] predefinedActions)
        {
            _predefinedActions = new Queue<string>(predefinedActions);
        }

        public Decision RequestDecision(ExecutionPath path)
        {
            string action = _predefinedActions.Count > 0
                ? _predefinedActions.Dequeue()
                : "default_action";

            return new Decision(path, action, null, "MockSupervisor");
        }
    }

    /// <summary>
    /// Tests for supervised code generation functionality
    /// </summary>
    public static class SupervisedCodeGenerationTests
    {
        public static void RunAll()
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("    Running Supervised Code Generation Tests");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();

            TestDecisionRecording();
            TestDecisionRetrieval();
            TestExecutionPathReuse();
            TestMultipleUndefinedPaths();
            TestMockSupervisor();

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("    All Tests Passed!");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
        }

        private static void TestDecisionRecording()
        {
            Console.WriteLine("Test 1: Decision Recording");
            var repository = new InMemoryDecisionRepository();
            var path = new ExecutionPath("test_path_1", "Test context");
            var decision = new Decision(path, "test_action");

            repository.RecordDecision(decision);

            Assert(repository.HasDecision(path), "Repository should have the recorded decision");
            Console.WriteLine("✓ Decision recording works correctly");
            Console.WriteLine();
        }

        private static void TestDecisionRetrieval()
        {
            Console.WriteLine("Test 2: Decision Retrieval");
            var repository = new InMemoryDecisionRepository();
            var path = new ExecutionPath("test_path_2", "Test context");
            var decision = new Decision(path, "retrieve_action");

            repository.RecordDecision(decision);
            var retrieved = repository.GetDecision(path);

            Assert(retrieved != null, "Retrieved decision should not be null");
            Assert(retrieved.Action == "retrieve_action", "Retrieved action should match");
            Assert(retrieved.Path.PathId == "test_path_2", "Retrieved path ID should match");
            Console.WriteLine("✓ Decision retrieval works correctly");
            Console.WriteLine();
        }

        private static void TestExecutionPathReuse()
        {
            Console.WriteLine("Test 3: Execution Path Reuse");
            var repository = new InMemoryDecisionRepository();
            var supervisor = new MockSupervisor("first_action");
            var executor = new SupervisedExecutor(repository, supervisor);

            var path1 = new ExecutionPath("reuse_path", "Test context");
            var decision1 = executor.ExecuteAtPath(path1);

            // Second encounter of same path should use recorded decision
            var path2 = new ExecutionPath("reuse_path", "Test context");
            var decision2 = executor.ExecuteAtPath(path2);

            Assert(decision1.Action == "first_action", "First decision should be from supervisor");
            Assert(decision2.Action == "first_action", "Second decision should reuse first decision");
            Assert(decision1.Path.PathId == decision2.Path.PathId, "Path IDs should match");
            Console.WriteLine("✓ Execution path reuse works correctly");
            Console.WriteLine();
        }

        private static void TestMultipleUndefinedPaths()
        {
            Console.WriteLine("Test 4: Multiple Undefined Paths");
            var repository = new InMemoryDecisionRepository();
            var supervisor = new MockSupervisor("action_1", "action_2", "action_3");
            var executor = new SupervisedExecutor(repository, supervisor);

            var path1 = new ExecutionPath("path_1", "Context 1");
            var path2 = new ExecutionPath("path_2", "Context 2");
            var path3 = new ExecutionPath("path_3", "Context 3");

            var decision1 = executor.ExecuteAtPath(path1);
            var decision2 = executor.ExecuteAtPath(path2);
            var decision3 = executor.ExecuteAtPath(path3);

            Assert(decision1.Action == "action_1", "First path should get first action");
            Assert(decision2.Action == "action_2", "Second path should get second action");
            Assert(decision3.Action == "action_3", "Third path should get third action");

            var allDecisions = new List<Decision>(executor.GetRecordedDecisions());
            Assert(allDecisions.Count == 3, "Should have recorded 3 decisions");
            Console.WriteLine("✓ Multiple undefined paths handled correctly");
            Console.WriteLine();
        }

        private static void TestMockSupervisor()
        {
            Console.WriteLine("Test 5: Mock Supervisor");
            var supervisor = new MockSupervisor("mock_action_1", "mock_action_2");
            var path1 = new ExecutionPath("mock_path_1", "Mock context 1");
            var path2 = new ExecutionPath("mock_path_2", "Mock context 2");

            var decision1 = supervisor.RequestDecision(path1);
            var decision2 = supervisor.RequestDecision(path2);

            Assert(decision1.Action == "mock_action_1", "First mock action should match");
            Assert(decision2.Action == "mock_action_2", "Second mock action should match");
            Assert(decision1.DecidedBy == "MockSupervisor", "DecidedBy should be MockSupervisor");
            Console.WriteLine("✓ Mock supervisor works correctly");
            Console.WriteLine();
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Assertion failed: {message}");
            }
        }
    }
}
