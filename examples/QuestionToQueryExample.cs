using System;
using Platform.Examples.QuestionToQueryTranslator;

namespace Platform.Examples.Examples
{
    /// <summary>
    /// Example demonstrating the Question to Query Translator functionality
    /// </summary>
    public class QuestionToQueryExample
    {
        public static void Main(string[] args)
        {
            var translator = new QuestionToQueryTranslator.QuestionToQueryTranslator();

            Console.WriteLine("=== Question to Query Translator Examples ===");
            Console.WriteLine();

            // Example 1: Simple "what" question
            DemonstrateTranslation(translator, "What is artificial intelligence?");

            // Example 2: "Who" question
            DemonstrateTranslation(translator, "Who created the theory of relativity?");

            // Example 3: "When" question
            DemonstrateTranslation(translator, "When did World War II end?");

            // Example 4: "Where" question
            DemonstrateTranslation(translator, "Where is the Eiffel Tower?");

            // Example 5: "Why" question
            DemonstrateTranslation(translator, "Why is the sky blue?");

            // Example 6: "How" question
            DemonstrateTranslation(translator, "How does a computer work?");

            // Example 7: Ambiguous question
            Console.WriteLine("Example: Ambiguous Question");
            var ambiguousQuestion = "What is that?";
            Console.WriteLine($"Question: {ambiguousQuestion}");
            if (translator.NeedsClarification(ambiguousQuestion))
            {
                Console.WriteLine("This question needs clarification:");
                foreach (var clarification in translator.GetClarificationQuestions(ambiguousQuestion))
                {
                    Console.WriteLine($"  {clarification}");
                }
            }
            Console.WriteLine();
        }

        private static void DemonstrateTranslation(QuestionToQueryTranslator.QuestionToQueryTranslator translator, string question)
        {
            Console.WriteLine($"Question: {question}");
            Console.WriteLine("Generated Queries:");

            var queries = translator.Translate(question);
            int count = 1;
            foreach (var query in queries)
            {
                Console.WriteLine($"  {count}. [{query.Confidence:P0}] {query.Path}");
                Console.WriteLine($"     Description: {query.Description}");
                count++;
            }
            Console.WriteLine();
        }
    }
}
