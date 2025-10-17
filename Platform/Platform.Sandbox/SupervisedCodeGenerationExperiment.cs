using System;
using System.Collections.Generic;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents a unique execution path in the program
    /// </summary>
    public class ExecutionPath
    {
        public string PathId { get; set; }
        public string Context { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public ExecutionPath(string pathId, string context, Dictionary<string, object> parameters = null)
        {
            PathId = pathId;
            Context = context;
            Parameters = parameters ?? new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return $"Path: {PathId}, Context: {Context}";
        }
    }

    /// <summary>
    /// Represents a decision made for a specific execution path
    /// </summary>
    public class Decision
    {
        public ExecutionPath Path { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string DecidedBy { get; set; }

        public Decision(ExecutionPath path, string action, Dictionary<string, object> data = null, string decidedBy = "System")
        {
            Path = path;
            Action = action;
            Data = data ?? new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
            DecidedBy = decidedBy;
        }

        public override string ToString()
        {
            return $"Decision for {Path.PathId}: {Action} (by {DecidedBy} at {Timestamp})";
        }
    }

    /// <summary>
    /// Interface for decision storage and retrieval
    /// </summary>
    public interface IDecisionRepository
    {
        void RecordDecision(Decision decision);
        Decision GetDecision(ExecutionPath path);
        bool HasDecision(ExecutionPath path);
        IEnumerable<Decision> GetAllDecisions();
    }

    /// <summary>
    /// In-memory implementation of decision repository
    /// </summary>
    public class InMemoryDecisionRepository : IDecisionRepository
    {
        private readonly Dictionary<string, Decision> _decisions = new Dictionary<string, Decision>();

        public void RecordDecision(Decision decision)
        {
            _decisions[decision.Path.PathId] = decision;
        }

        public Decision GetDecision(ExecutionPath path)
        {
            return _decisions.TryGetValue(path.PathId, out var decision) ? decision : null;
        }

        public bool HasDecision(ExecutionPath path)
        {
            return _decisions.ContainsKey(path.PathId);
        }

        public IEnumerable<Decision> GetAllDecisions()
        {
            return _decisions.Values;
        }
    }

    /// <summary>
    /// Interface for asking supervisor for decisions
    /// </summary>
    public interface ISupervisor
    {
        Decision RequestDecision(ExecutionPath path);
    }

    /// <summary>
    /// Console-based supervisor implementation
    /// </summary>
    public class ConsoleSupervisor : ISupervisor
    {
        public Decision RequestDecision(ExecutionPath path)
        {
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         UNDEFINED EXECUTION PATH ENCOUNTERED                  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"Path ID: {path.PathId}");
            Console.WriteLine($"Context: {path.Context}");

            if (path.Parameters.Count > 0)
            {
                Console.WriteLine("Parameters:");
                foreach (var param in path.Parameters)
                {
                    Console.WriteLine($"  - {param.Key}: {param.Value}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Please provide a decision (action name):");
            Console.Write("> ");
            string action = Console.ReadLine();

            Console.WriteLine("Provide any additional data (JSON format, or press Enter to skip):");
            Console.Write("> ");
            string dataInput = Console.ReadLine();

            var data = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(dataInput))
            {
                data["raw_input"] = dataInput;
            }

            return new Decision(path, action, data, "ConsoleSupervisor");
        }
    }

    /// <summary>
    /// Main supervised execution engine
    /// </summary>
    public class SupervisedExecutor
    {
        private readonly IDecisionRepository _repository;
        private readonly ISupervisor _supervisor;

        public SupervisedExecutor(IDecisionRepository repository, ISupervisor supervisor)
        {
            _repository = repository;
            _supervisor = supervisor;
        }

        /// <summary>
        /// Executes an action at a decision point. If the path is undefined, asks supervisor.
        /// </summary>
        public Decision ExecuteAtPath(ExecutionPath path)
        {
            // Check if we already have a decision for this path
            if (_repository.HasDecision(path))
            {
                var existingDecision = _repository.GetDecision(path);
                Console.WriteLine($"[KNOWN PATH] Using recorded decision: {existingDecision.Action}");
                return existingDecision;
            }

            // Path is undefined - ask supervisor
            Console.WriteLine($"[UNDEFINED PATH] Execution paused at: {path.PathId}");
            var decision = _supervisor.RequestDecision(path);

            // Record the decision for future use
            _repository.RecordDecision(decision);
            Console.WriteLine($"[DECISION RECORDED] {decision.Action} will be used for future encounters of this path");

            return decision;
        }

        /// <summary>
        /// Gets all recorded decisions
        /// </summary>
        public IEnumerable<Decision> GetRecordedDecisions()
        {
            return _repository.GetAllDecisions();
        }
    }

    /// <summary>
    /// Example demonstrating supervised dynamic code generation at execution.
    /// Implements the concept from issue #520.
    /// </summary>
    public static class SupervisedCodeGenerationExperiment
    {
        public static void Run()
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("    Supervised Dynamic Code Generation Experiment");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("This demonstrates a system where undefined execution paths");
            Console.WriteLine("are resolved interactively by asking a supervisor for decisions.");
            Console.WriteLine();

            // Create the supervised executor
            var repository = new InMemoryDecisionRepository();
            var supervisor = new ConsoleSupervisor();
            var executor = new SupervisedExecutor(repository, supervisor);

            // Simulate various execution scenarios
            Console.WriteLine("Scenario 1: Processing unknown user input");
            var path1 = new ExecutionPath(
                "user_input_handler",
                "Unknown user input received",
                new Dictionary<string, object> { { "input", "hello world" } }
            );
            var decision1 = executor.ExecuteAtPath(path1);
            Console.WriteLine($"Action taken: {decision1.Action}");
            Console.WriteLine();

            Console.WriteLine("Press any key to continue to Scenario 2...");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("Scenario 2: Encountering same path again");
            var path1Again = new ExecutionPath(
                "user_input_handler",
                "Unknown user input received",
                new Dictionary<string, object> { { "input", "hello again" } }
            );
            var decision1Again = executor.ExecuteAtPath(path1Again);
            Console.WriteLine($"Action taken: {decision1Again.Action}");
            Console.WriteLine();

            Console.WriteLine("Press any key to continue to Scenario 3...");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("Scenario 3: New undefined path - validation error");
            var path2 = new ExecutionPath(
                "data_validator",
                "Invalid data format detected",
                new Dictionary<string, object> { { "data", "invalid_format" } }
            );
            var decision2 = executor.ExecuteAtPath(path2);
            Console.WriteLine($"Action taken: {decision2.Action}");
            Console.WriteLine();

            Console.WriteLine("Press any key to see all recorded decisions...");
            Console.ReadKey();
            Console.WriteLine();

            // Display all recorded decisions
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("    All Recorded Decisions");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            foreach (var decision in executor.GetRecordedDecisions())
            {
                Console.WriteLine(decision);
                Console.WriteLine($"  Context: {decision.Path.Context}");
                if (decision.Data.Count > 0)
                {
                    Console.WriteLine("  Additional Data:");
                    foreach (var item in decision.Data)
                    {
                        Console.WriteLine($"    - {item.Key}: {item.Value}");
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("Experiment completed successfully!");
        }
    }
}
