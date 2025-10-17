using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Extracts question-answer pairs from conversation messages
    /// </summary>
    public class QuestionAnswerExtractor
    {
        private readonly int _maxAnswerDistance;

        /// <summary>
        /// Creates a new QuestionAnswerExtractor
        /// </summary>
        /// <param name="maxAnswerDistance">Maximum number of messages to look ahead for an answer (default: 10)</param>
        public QuestionAnswerExtractor(int maxAnswerDistance = 10)
        {
            _maxAnswerDistance = maxAnswerDistance;
        }

        /// <summary>
        /// Extracts all question-answer pairs from a list of messages
        /// </summary>
        /// <param name="messages">The conversation messages</param>
        /// <returns>List of question-answer pairs</returns>
        public List<QuestionAnswerPair> ExtractQuestionAnswerPairs(List<Message> messages)
        {
            var pairs = new List<QuestionAnswerPair>();

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];

                if (message.IsQuestion)
                {
                    // Look for an answer in the next messages
                    Message answer = FindAnswer(messages, i, message.Sender);
                    pairs.Add(new QuestionAnswerPair(message, answer));
                }
            }

            return pairs;
        }

        /// <summary>
        /// Finds an answer to a question by looking at subsequent messages
        /// </summary>
        private Message FindAnswer(List<Message> messages, int questionIndex, string questionSender)
        {
            int searchLimit = Math.Min(questionIndex + _maxAnswerDistance, messages.Count);

            for (int i = questionIndex + 1; i < searchLimit; i++)
            {
                var potentialAnswer = messages[i];

                // Answer should be from a different sender
                if (potentialAnswer.Sender != questionSender)
                {
                    return potentialAnswer;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all questions asked by a specific person
        /// </summary>
        public List<QuestionAnswerPair> GetQuestionsByPerson(List<Message> messages, string person)
        {
            var allPairs = ExtractQuestionAnswerPairs(messages);
            return allPairs.Where(pair => pair.Question.Sender.Equals(person, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Checks if a specific question has been asked before
        /// </summary>
        public bool HasQuestionBeenAsked(List<Message> messages, string questionText, string sender = null)
        {
            var normalizedQuestion = NormalizeText(questionText);

            foreach (var message in messages)
            {
                if (message.IsQuestion)
                {
                    if (sender == null || message.Sender.Equals(sender, StringComparison.OrdinalIgnoreCase))
                    {
                        if (NormalizeText(message.Content).Contains(normalizedQuestion) ||
                            normalizedQuestion.Contains(NormalizeText(message.Content)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.Trim().ToLowerInvariant()
                .Replace("?", "")
                .Replace("!", "")
                .Replace(".", "")
                .Replace(",", "");
        }
    }
}
