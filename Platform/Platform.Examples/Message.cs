using System;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a single message in a conversation
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The sender of the message
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// The text content of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The timestamp when the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Indicates whether this message is a question
        /// </summary>
        public bool IsQuestion { get; set; }

        public Message(string sender, string content, DateTime timestamp)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Timestamp = timestamp;
            IsQuestion = DetectQuestion(content);
        }

        private bool DetectQuestion(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var trimmed = text.Trim();

            // Check if ends with question mark
            if (trimmed.EndsWith("?"))
                return true;

            // Check for common question words at the beginning
            var questionWords = new[] { "what", "where", "when", "why", "how", "who", "which", "whose", "whom", "can", "could", "would", "should", "is", "are", "do", "does", "did", "will", "have", "has", "had" };
            var lowerText = trimmed.ToLowerInvariant();

            foreach (var word in questionWords)
            {
                if (lowerText.StartsWith(word + " "))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Sender}: {Content}";
        }
    }
}
