using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Basic implementation of question parser that extracts question components
    /// </summary>
    public class BasicQuestionParser : IQuestionParser
    {
        private static readonly string[] QuestionWords = { "who", "what", "when", "where", "why", "how", "which", "whose" };

        public IQuestion Parse(string questionText)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                throw new ArgumentException("Question text cannot be null or empty", nameof(questionText));
            }

            var cleanedQuestion = questionText.Trim().TrimEnd('?', '.', '!');
            var words = cleanedQuestion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var questionType = ExtractQuestionType(words);
            var (subject, predicate, context) = ExtractComponents(cleanedQuestion, questionType);

            return new Question(questionText, questionType, subject, predicate, context);
        }

        private string ExtractQuestionType(string[] words)
        {
            var firstWord = words.FirstOrDefault()?.ToLower() ?? "";
            return QuestionWords.Contains(firstWord) ? firstWord : "unknown";
        }

        private (string subject, string predicate, string[] context) ExtractComponents(string questionText, string questionType)
        {
            var lowerQuestion = questionText.ToLower();

            // Remove question word from beginning
            var withoutQuestionWord = Regex.Replace(lowerQuestion, $"^{questionType}\\s+", "", RegexOptions.IgnoreCase).Trim();

            // Try to identify verb (predicate)
            var commonVerbs = new[] { "is", "are", "was", "were", "has", "have", "had", "do", "does", "did", "can", "could", "will", "would", "should" };
            string predicate = "";
            string subject = "";
            string[] context = new string[0];

            foreach (var verb in commonVerbs)
            {
                var verbPattern = $"\\b{verb}\\b";
                if (Regex.IsMatch(withoutQuestionWord, verbPattern))
                {
                    var parts = Regex.Split(withoutQuestionWord, verbPattern, RegexOptions.IgnoreCase);
                    if (parts.Length >= 2)
                    {
                        subject = parts[0].Trim();
                        predicate = verb;
                        if (parts.Length > 1)
                        {
                            context = new[] { parts[1].Trim() };
                        }
                    }
                    break;
                }
            }

            // If no verb found, treat entire phrase as subject
            if (string.IsNullOrEmpty(predicate))
            {
                subject = withoutQuestionWord;
            }

            return (subject, predicate, context);
        }
    }
}
