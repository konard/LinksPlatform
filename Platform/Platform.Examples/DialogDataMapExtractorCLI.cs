using System;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Dialog Data Map Extractor
    /// </summary>
    public class DialogDataMapExtractorCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();

            try
            {
                switch (command)
                {
                    case "analyze":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Error: 'analyze' command requires input file path and person name.");
                            Console.WriteLine("Usage: analyze <input-file> <person-name> [output-file]");
                            return;
                        }
                        AnalyzeConversation(args[1], args[2], args.Length > 3 ? args[3] : null);
                        break;

                    case "list-participants":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: 'list-participants' command requires input file path.");
                            Console.WriteLine("Usage: list-participants <input-file>");
                            return;
                        }
                        ListParticipants(args[1]);
                        break;

                    case "check-question":
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Error: 'check-question' command requires input file, person, and question text.");
                            Console.WriteLine("Usage: check-question <input-file> <person> <question-text>");
                            return;
                        }
                        CheckQuestion(args[1], args[2], string.Join(" ", args.Skip(3)));
                        break;

                    case "autocomplete":
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Error: 'autocomplete' command requires input file, person, and prefix.");
                            Console.WriteLine("Usage: autocomplete <input-file> <person> <prefix>");
                            return;
                        }
                        GetAutocomplete(args[1], args[2], args[3]);
                        break;

                    case "help":
                        PrintUsage();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void AnalyzeConversation(string inputFile, string person, string outputFile)
        {
            Console.WriteLine($"Analyzing conversation for {person}...");

            var extractor = new DialogDataMapExtractor();
            var messages = extractor.LoadMessagesFromFile(inputFile);

            Console.WriteLine($"Loaded {messages.Count} messages.");

            var report = extractor.GeneratePersonalDataMap(messages, person);

            if (outputFile != null)
            {
                extractor.ExportDataMap(outputFile, person, messages);
                Console.WriteLine($"Report saved to: {outputFile}");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(report);
            }
        }

        private void ListParticipants(string inputFile)
        {
            Console.WriteLine("Loading conversation...");

            var extractor = new DialogDataMapExtractor();
            var messages = extractor.LoadMessagesFromFile(inputFile);
            var participants = extractor.GetParticipants(messages);

            Console.WriteLine($"\nFound {participants.Count} participants:");
            foreach (var participant in participants)
            {
                var messageCount = messages.Count(m => m.Sender.Equals(participant, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"  - {participant} ({messageCount} messages)");
            }
        }

        private void CheckQuestion(string inputFile, string person, string question)
        {
            var extractor = new DialogDataMapExtractor();
            var messages = extractor.LoadMessagesFromFile(inputFile);

            var hasAsked = extractor.HasAskedQuestion(messages, person, question);

            if (hasAsked)
            {
                Console.WriteLine($"✓ {person} has asked a similar question before.");
            }
            else
            {
                Console.WriteLine($"✗ {person} has not asked this question before.");
            }
        }

        private void GetAutocomplete(string inputFile, string person, string prefix)
        {
            var extractor = new DialogDataMapExtractor();
            var messages = extractor.LoadMessagesFromFile(inputFile);

            var suggestions = extractor.GetAutocompleteSuggestions(messages, person, prefix);

            if (suggestions.Any())
            {
                Console.WriteLine($"Autocomplete suggestions for '{prefix}' based on {person}'s vocabulary:");
                foreach (var suggestion in suggestions)
                {
                    Console.WriteLine($"  - {suggestion}");
                }
            }
            else
            {
                Console.WriteLine($"No suggestions found for prefix '{prefix}'.");
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine("Dialog Data Map Extractor CLI");
            Console.WriteLine("=============================");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("  Tool for extracting personal data maps from dialog/chat conversations.");
            Console.WriteLine("  Analyzes questions, answers, and vocabulary usage patterns.");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  analyze <input-file> <person> [output-file]");
            Console.WriteLine("      Generates a comprehensive data map for the specified person.");
            Console.WriteLine("      Includes question-answer pairs and vocabulary analysis.");
            Console.WriteLine();
            Console.WriteLine("  list-participants <input-file>");
            Console.WriteLine("      Lists all participants in the conversation.");
            Console.WriteLine();
            Console.WriteLine("  check-question <input-file> <person> <question-text>");
            Console.WriteLine("      Checks if a person has asked a similar question before.");
            Console.WriteLine();
            Console.WriteLine("  autocomplete <input-file> <person> <prefix>");
            Console.WriteLine("      Gets word suggestions based on person's vocabulary.");
            Console.WriteLine();
            Console.WriteLine("  help");
            Console.WriteLine("      Shows this help message.");
            Console.WriteLine();
            Console.WriteLine("Input File Format:");
            Console.WriteLine("  [YYYY-MM-DD HH:MM:SS] SenderName: Message content");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  [2024-01-15 10:30:45] John: Hello, how are you?");
            Console.WriteLine("  [2024-01-15 10:31:20] Alice: I'm fine, thanks! What about you?");
        }
    }
}
