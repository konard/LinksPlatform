using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates continuous search refinement with a practical example.
    /// Example scenario: Searching for programming resources like "C# json"
    /// </summary>
    public class ContinuousSearchRefinementExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Continuous Search Refinement Example ===\n");

            // Create a sample collection of programming resources
            var resources = new ProgrammingResourceCollection();

            // Create a strategy that tries to halve results with each question
            var questionStrategy = new BinaryPartitionQuestionStrategy();

            // Create an interaction handler (console-based for this example)
            var interactionHandler = new ConsoleInteractionHandler();

            // Create the search refinement system
            var searchSystem = new ContinuousSearchRefinement<ProgrammingResource>(
                resources,
                questionStrategy
            );

            Console.WriteLine("Enter your search query (e.g., 'C# json'):");
            var query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query))
            {
                query = "C# json"; // Default example
                Console.WriteLine($"Using default query: {query}");
            }

            Console.WriteLine();

            var result = searchSystem.RefineSearch(query, interactionHandler, targetResultCount: 1);

            Console.WriteLine("\n=== Search Refinement Complete ===");
            Console.WriteLine($"Final results: {result.Results.Count}");

            foreach (var resource in result.Results)
            {
                Console.WriteLine($"\n{resource.Title}");
                Console.WriteLine($"  Type: {resource.Type}");
                Console.WriteLine($"  Language: {resource.Language}");
                Console.WriteLine($"  Topic: {resource.Topic}");
                Console.WriteLine($"  Description: {resource.Description}");
            }

            Console.WriteLine("\n=== Refinement History ===");
            for (int i = 0; i < result.RefinementHistory.Count; i++)
            {
                var step = result.RefinementHistory[i];
                Console.WriteLine($"Step {i + 1}: {step.ResultCount} results");
                if (step.Question != null)
                {
                    Console.WriteLine($"  Q: {step.Question.Text}");
                    Console.WriteLine($"  A: {step.Answer?.Value ?? "(skipped)"}");
                }
            }
        }

        public class ProgrammingResource
        {
            public string Title { get; set; }
            public string Language { get; set; }
            public string Topic { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }

        public class ProgrammingResourceCollection : ISearchableCollection<ProgrammingResource>
        {
            private readonly List<ProgrammingResource> _resources;

            public ProgrammingResourceCollection()
            {
                _resources = new List<ProgrammingResource>
                {
                    new ProgrammingResource
                    {
                        Title = "Parsing JSON in C#",
                        Language = "C#",
                        Topic = "json",
                        Type = "tutorial",
                        Description = "Learn how to parse JSON data using C# and System.Text.Json"
                    },
                    new ProgrammingResource
                    {
                        Title = "Generate C# classes from JSON",
                        Language = "C#",
                        Topic = "json",
                        Type = "tool",
                        Description = "Online tool to generate C# classes based on JSON structure"
                    },
                    new ProgrammingResource
                    {
                        Title = "JSON.NET Documentation",
                        Language = "C#",
                        Topic = "json",
                        Type = "documentation",
                        Description = "Official documentation for Newtonsoft.Json library"
                    },
                    new ProgrammingResource
                    {
                        Title = "C# JSON Serialization Best Practices",
                        Language = "C#",
                        Topic = "json",
                        Type = "article",
                        Description = "Best practices for JSON serialization and deserialization in C#"
                    },
                    new ProgrammingResource
                    {
                        Title = "Python JSON Parser",
                        Language = "Python",
                        Topic = "json",
                        Type = "tutorial",
                        Description = "How to work with JSON in Python using the json module"
                    },
                    new ProgrammingResource
                    {
                        Title = "JavaScript JSON Methods",
                        Language = "JavaScript",
                        Topic = "json",
                        Type = "tutorial",
                        Description = "Using JSON.parse() and JSON.stringify() in JavaScript"
                    }
                };
            }

            public List<ProgrammingResource> Search(string query)
            {
                var keywords = query.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return _resources.Where(r =>
                    keywords.Any(k =>
                        r.Title.ToLower().Contains(k) ||
                        r.Language.ToLower().Contains(k) ||
                        r.Topic.ToLower().Contains(k) ||
                        r.Description.ToLower().Contains(k)
                    )
                ).ToList();
            }

            public List<ProgrammingResource> ApplyConstraint(
                List<ProgrammingResource> currentResults,
                SearchConstraint constraint)
            {
                if (constraint.IsSkipped)
                {
                    return currentResults;
                }

                return currentResults.Where(r =>
                {
                    var propertyValue = GetPropertyValue(r, constraint.Attribute);
                    return propertyValue != null &&
                           propertyValue.Equals(constraint.Value, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }

            private string GetPropertyValue(ProgrammingResource resource, string attribute)
            {
                switch (attribute.ToLower())
                {
                    case "language":
                        return resource.Language;
                    case "topic":
                        return resource.Topic;
                    case "type":
                        return resource.Type;
                    default:
                        return null;
                }
            }
        }

        public class BinaryPartitionQuestionStrategy : IQuestionStrategy<ProgrammingResource>
        {
            public Question GenerateQuestion(
                List<ProgrammingResource> currentResults,
                List<SearchConstraint> existingConstraints)
            {
                var usedAttributes = new HashSet<string>(
                    existingConstraints.Select(c => c.Attribute),
                    StringComparer.OrdinalIgnoreCase
                );

                // Try to find an attribute that will split results closest to 50/50
                var candidateAttributes = new[] { "type", "language", "topic" }
                    .Where(attr => !usedAttributes.Contains(attr))
                    .ToList();

                if (!candidateAttributes.Any())
                {
                    return null; // No more attributes to ask about
                }

                Question bestQuestion = null;
                double bestScore = double.MaxValue;

                foreach (var attribute in candidateAttributes)
                {
                    var values = GetDistinctValues(currentResults, attribute);

                    if (values.Count <= 1)
                    {
                        continue; // No point asking if all have the same value
                    }

                    // Find the value that splits results closest to 50/50
                    foreach (var value in values)
                    {
                        var countWithValue = currentResults.Count(r =>
                            GetPropertyValue(r, attribute)?.Equals(value, StringComparison.OrdinalIgnoreCase) == true
                        );

                        var ratio = (double)countWithValue / currentResults.Count;
                        var deviation = Math.Abs(ratio - 0.5);

                        if (deviation < bestScore)
                        {
                            bestScore = deviation;
                            bestQuestion = CreateQuestion(attribute, values, value);
                        }
                    }
                }

                return bestQuestion;
            }

            private List<string> GetDistinctValues(List<ProgrammingResource> resources, string attribute)
            {
                return resources
                    .Select(r => GetPropertyValue(r, attribute))
                    .Where(v => v != null)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            private string GetPropertyValue(ProgrammingResource resource, string attribute)
            {
                switch (attribute.ToLower())
                {
                    case "language":
                        return resource.Language;
                    case "topic":
                        return resource.Topic;
                    case "type":
                        return resource.Type;
                    default:
                        return null;
                }
            }

            private Question CreateQuestion(string attribute, List<string> allValues, string suggestedValue)
            {
                string questionText;
                switch (attribute.ToLower())
                {
                    case "type":
                        questionText = "What type of resource are you looking for?";
                        break;
                    case "language":
                        questionText = "Which programming language?";
                        break;
                    case "topic":
                        questionText = "Which topic are you interested in?";
                        break;
                    default:
                        questionText = $"What {attribute}?";
                        break;
                }

                return new Question
                {
                    Text = questionText,
                    Attribute = attribute,
                    Options = allValues,
                    Type = allValues.Count <= 5 ? QuestionType.MultipleChoice : QuestionType.Text
                };
            }
        }

        public class ConsoleInteractionHandler : IInteractionHandler
        {
            public Answer AskQuestion(Question question)
            {
                Console.WriteLine($"\n{question.Text}");

                if (question.Type == QuestionType.MultipleChoice && question.Options?.Count > 0)
                {
                    for (int i = 0; i < question.Options.Count; i++)
                    {
                        Console.WriteLine($"  {i + 1}. {question.Options[i]}");
                    }
                    Console.WriteLine($"  {question.Options.Count + 1}. Skip this question");

                    Console.Write("Your choice (number): ");
                    var input = Console.ReadLine();

                    if (int.TryParse(input, out int choice))
                    {
                        if (choice > 0 && choice <= question.Options.Count)
                        {
                            return new Answer
                            {
                                Value = question.Options[choice - 1],
                                IsSkipped = false
                            };
                        }
                        else if (choice == question.Options.Count + 1)
                        {
                            return new Answer { IsSkipped = true };
                        }
                    }

                    Console.WriteLine("Invalid choice. Skipping question.");
                }

                return new Answer { IsSkipped = true };
            }
        }
    }
}
