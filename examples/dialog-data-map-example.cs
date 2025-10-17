using System;
using System.Collections.Generic;
using Platform.Examples;

namespace Examples
{
    /// <summary>
    /// Example demonstrating the Dialog Data Map Extractor functionality
    /// This example shows how to:
    /// 1. Load conversation messages
    /// 2. Extract question-answer pairs
    /// 3. Analyze vocabulary usage
    /// 4. Check if questions have been asked before
    /// 5. Get autocomplete suggestions
    /// </summary>
    class DialogDataMapExample
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Dialog Data Map Extractor Example ===\n");

            // Create sample messages programmatically
            var messages = CreateSampleMessages();

            Console.WriteLine($"Total messages: {messages.Count}\n");

            // 1. Extract question-answer pairs
            Console.WriteLine("--- Question-Answer Extraction ---");
            var qaExtractor = new QuestionAnswerExtractor();
            var qaPairs = qaExtractor.ExtractQuestionAnswerPairs(messages);

            Console.WriteLine($"Total questions found: {qaPairs.Count}");
            Console.WriteLine($"Questions with answers: {qaPairs.Count(p => p.HasAnswer)}\n");

            Console.WriteLine("Sample Q&A pairs:");
            foreach (var pair in qaPairs.Take(3))
            {
                Console.WriteLine($"Q: {pair.Question.Content}");
                if (pair.HasAnswer)
                {
                    Console.WriteLine($"A: {pair.Answer.Content}");
                }
                Console.WriteLine();
            }

            // 2. Vocabulary analysis
            Console.WriteLine("--- Vocabulary Analysis ---");
            var vocabExtractor = new VocabularyExtractor();
            var aliceVocab = vocabExtractor.GetTopWords(messages, "Alice", 10);

            Console.WriteLine("Alice's top 10 most used words:");
            foreach (var word in aliceVocab)
            {
                Console.WriteLine($"  {word.Key}: {word.Value} times");
            }
            Console.WriteLine();

            // 3. Check if a question has been asked
            Console.WriteLine("--- Question Checking ---");
            var alreadyAsked = qaExtractor.HasQuestionBeenAsked(messages, "How does it work?", "Alice");
            Console.WriteLine($"Has Alice asked about 'How does it work?': {alreadyAsked}");

            var notAsked = qaExtractor.HasQuestionBeenAsked(messages, "What is the weather?", "Alice");
            Console.WriteLine($"Has Alice asked about 'What is the weather?': {notAsked}\n");

            // 4. Autocomplete suggestions
            Console.WriteLine("--- Autocomplete Suggestions ---");
            var suggestions = vocabExtractor.GetAutocompleteSuggestions(messages, "Bob", "pro", 5);
            Console.WriteLine("Suggestions for 'pro' based on Bob's vocabulary:");
            foreach (var suggestion in suggestions)
            {
                Console.WriteLine($"  - {suggestion}");
            }
            Console.WriteLine();

            // 5. Generate full data map
            Console.WriteLine("--- Full Data Map ---");
            var extractor = new DialogDataMapExtractor();
            var dataMap = extractor.GeneratePersonalDataMap(messages, "Alice");
            Console.WriteLine(dataMap);
        }

        static List<Message> CreateSampleMessages()
        {
            return new List<Message>
            {
                new Message("Alice", "Good morning! How are you doing today?", DateTime.Parse("2024-01-15 10:00:00")),
                new Message("Bob", "I'm doing great, thanks! Just working on some interesting projects.", DateTime.Parse("2024-01-15 10:01:30")),
                new Message("Alice", "That sounds exciting! What kind of projects?", DateTime.Parse("2024-01-15 10:02:15")),
                new Message("Bob", "I'm building a chat analysis tool that extracts personal data maps from conversations.", DateTime.Parse("2024-01-15 10:03:45")),
                new Message("Alice", "Wow, that's fascinating! How does it work?", DateTime.Parse("2024-01-15 10:04:30")),
                new Message("Bob", "It analyzes questions and answers, and also extracts vocabulary patterns.", DateTime.Parse("2024-01-15 10:05:20")),
                new Message("Alice", "Can it remember if someone already asked a question?", DateTime.Parse("2024-01-15 10:06:10")),
                new Message("Bob", "Yes! That's one of the main features. It helps track what questions have been asked before.", DateTime.Parse("2024-01-15 10:07:00")),
                new Message("Alice", "That would be really useful when talking with many people. Do you have any documentation?", DateTime.Parse("2024-01-15 10:08:00")),
                new Message("Bob", "I'm working on it. The tool also extracts personal vocabulary for better communication.", DateTime.Parse("2024-01-15 10:09:15"))
            };
        }
    }
}
