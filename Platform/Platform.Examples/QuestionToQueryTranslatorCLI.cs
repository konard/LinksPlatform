using System;
using System.Linq;
using Platform.Examples.QuestionToQueryTranslator;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Question to Query Translator
    /// </summary>
    public class QuestionToQueryTranslatorCLI : ICommandLineInterface
    {
        private readonly QuestionToQueryTranslator.QuestionToQueryTranslator _translator;

        public QuestionToQueryTranslatorCLI()
        {
            _translator = new QuestionToQueryTranslator.QuestionToQueryTranslator();
        }

        public void Run(params string[] args)
        {
            Console.WriteLine("=== Question to Query Translator ===");
            Console.WriteLine("This tool translates natural language questions into executable queries.");
            Console.WriteLine("Type 'exit' or 'quit' to stop.");
            Console.WriteLine();

            if (args.Length > 0)
            {
                // Process single question from command line
                var question = string.Join(" ", args);
                ProcessQuestion(question);
            }
            else
            {
                // Interactive mode
                RunInteractiveMode();
            }
        }

        private void RunInteractiveMode()
        {
            while (true)
            {
                Console.Write("Enter your question: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input.Trim().ToLower() == "exit" || input.Trim().ToLower() == "quit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                try
                {
                    ProcessQuestion(input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine();
            }
        }

        private void ProcessQuestion(string questionText)
        {
            Console.WriteLine();
            Console.WriteLine($"Question: {questionText}");
            Console.WriteLine(new string('-', 60));

            // Check if clarification is needed
            if (_translator.NeedsClarification(questionText))
            {
                Console.WriteLine("Status: Needs clarification");
                Console.WriteLine();
                var clarifications = _translator.GetClarificationQuestions(questionText);
                foreach (var clarification in clarifications)
                {
                    Console.WriteLine(clarification);
                }
            }
            else
            {
                Console.WriteLine("Status: Clear interpretation found");
            }

            Console.WriteLine();
            Console.WriteLine("Possible query interpretations (ordered by confidence):");
            Console.WriteLine();

            var queries = _translator.Translate(questionText).ToList();

            if (!queries.Any())
            {
                Console.WriteLine("  No queries could be generated for this question.");
            }
            else
            {
                for (int i = 0; i < queries.Count; i++)
                {
                    var query = queries[i];
                    Console.WriteLine($"  [{i + 1}] Confidence: {query.Confidence:P0}");
                    Console.WriteLine($"      Query Path: {query.Path}");
                    Console.WriteLine($"      Description: {query.Description}");
                    Console.WriteLine();
                }
            }
        }
    }
}
