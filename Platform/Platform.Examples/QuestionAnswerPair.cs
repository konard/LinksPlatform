using System;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a question and its corresponding answer from a conversation
    /// </summary>
    public class QuestionAnswerPair
    {
        /// <summary>
        /// The question message
        /// </summary>
        public Message Question { get; set; }

        /// <summary>
        /// The answer message (if found)
        /// </summary>
        public Message Answer { get; set; }

        /// <summary>
        /// Indicates whether an answer was found for this question
        /// </summary>
        public bool HasAnswer => Answer != null;

        public QuestionAnswerPair(Message question, Message answer = null)
        {
            Question = question ?? throw new ArgumentNullException(nameof(question));
            Answer = answer;
        }

        public override string ToString()
        {
            if (HasAnswer)
            {
                return $"Q: {Question.Content}\nA: {Answer.Content}";
            }
            else
            {
                return $"Q: {Question.Content}\nA: [No answer found]";
            }
        }
    }
}
