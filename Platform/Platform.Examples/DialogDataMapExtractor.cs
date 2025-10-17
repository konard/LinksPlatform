using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    /// <summary>
    /// Main class for extracting personal data maps from dialog/chat conversations
    /// Combines question-answer extraction and vocabulary analysis
    /// </summary>
    public class DialogDataMapExtractor
    {
        private readonly QuestionAnswerExtractor _qaExtractor;
        private readonly VocabularyExtractor _vocabExtractor;

        public DialogDataMapExtractor()
        {
            _qaExtractor = new QuestionAnswerExtractor();
            _vocabExtractor = new VocabularyExtractor();
        }

        /// <summary>
        /// Loads messages from a simple text file format
        /// Expected format: [YYYY-MM-DD HH:MM:SS] Sender: Message content
        /// </summary>
        public List<Message> LoadMessagesFromFile(string filePath)
        {
            var messages = new List<Message>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var lines = File.ReadAllLines(filePath);

            // Regex to parse: [2024-01-15 10:30:45] John: Hello, how are you?
            var messagePattern = new Regex(@"^\[(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})\]\s+([^:]+):\s+(.+)$");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var match = messagePattern.Match(line);
                if (match.Success)
                {
                    var timestamp = DateTime.Parse(match.Groups[1].Value);
                    var sender = match.Groups[2].Value.Trim();
                    var content = match.Groups[3].Value.Trim();

                    messages.Add(new Message(sender, content, timestamp));
                }
            }

            return messages;
        }

        /// <summary>
        /// Generates a comprehensive report for a specific person's conversation
        /// </summary>
        /// <param name="messages">All conversation messages</param>
        /// <param name="person">The person to analyze</param>
        /// <returns>A formatted text report</returns>
        public string GeneratePersonalDataMap(List<Message> messages, string person)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"=== Personal Data Map for {person} ===");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Total messages analyzed: {messages.Count}");
            sb.AppendLine();

            // Question-Answer Analysis
            sb.AppendLine("--- Questions Asked ---");
            var qaPairs = _qaExtractor.GetQuestionsByPerson(messages, person);
            sb.AppendLine($"Total questions asked: {qaPairs.Count}");
            sb.AppendLine($"Questions with answers: {qaPairs.Count(p => p.HasAnswer)}");
            sb.AppendLine();

            if (qaPairs.Any())
            {
                sb.AppendLine("Recent Questions:");
                var recentPairs = qaPairs.Skip(Math.Max(0, qaPairs.Count - 10)).Take(10);
                foreach (var pair in recentPairs)
                {
                    sb.AppendLine($"  Q: {pair.Question.Content}");
                    if (pair.HasAnswer)
                    {
                        sb.AppendLine($"  A: {pair.Answer.Content}");
                    }
                    else
                    {
                        sb.AppendLine($"  A: [No answer found]");
                    }
                    sb.AppendLine();
                }
            }

            // Vocabulary Analysis
            sb.AppendLine("--- Vocabulary Analysis ---");
            var topWords = _vocabExtractor.GetTopWords(messages, person, 20);
            sb.AppendLine($"Unique words used: {_vocabExtractor.ExtractVocabulary(messages, person).Count}");
            sb.AppendLine();

            sb.AppendLine("Top 20 most used words:");
            foreach (var kvp in topWords)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value} times");
            }
            sb.AppendLine();

            // Unique vocabulary
            var uniqueWords = _vocabExtractor.GetUniqueVocabulary(messages, person);
            sb.AppendLine($"Characteristic words (used uniquely or predominantly): {uniqueWords.Count}");
            if (uniqueWords.Any())
            {
                sb.AppendLine("Top characteristic words:");
                foreach (var kvp in uniqueWords.OrderByDescending(x => x.Value).Take(15))
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value} times");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Exports the data map to a file
        /// </summary>
        public void ExportDataMap(string outputPath, string person, List<Message> messages)
        {
            var report = GeneratePersonalDataMap(messages, person);
            File.WriteAllText(outputPath, report);
        }

        /// <summary>
        /// Checks if a question has been asked to a specific person
        /// </summary>
        public bool HasAskedQuestion(List<Message> messages, string asker, string question)
        {
            return _qaExtractor.HasQuestionBeenAsked(messages, question, asker);
        }

        /// <summary>
        /// Gets autocomplete suggestions based on person's vocabulary
        /// </summary>
        public List<string> GetAutocompleteSuggestions(List<Message> messages, string person, string prefix)
        {
            return _vocabExtractor.GetAutocompleteSuggestions(messages, person, prefix);
        }

        /// <summary>
        /// Gets all unique participants in the conversation
        /// </summary>
        public List<string> GetParticipants(List<Message> messages)
        {
            return messages.Select(m => m.Sender).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
