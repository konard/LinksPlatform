using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    /// <summary>
    /// Simple sentence parser for detecting subject-predicate-object patterns.
    /// Maps sentences to pair links (subject-predicate) and triplet links (subject-verb-object).
    /// </summary>
    public class SentenceParser
    {
        private static readonly HashSet<string> CommonVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "is", "are", "was", "were", "am", "be", "been", "being",
            "has", "have", "had", "do", "does", "did",
            "can", "could", "will", "would", "shall", "should", "may", "might", "must",
            "go", "goes", "went", "come", "comes", "came", "make", "makes", "made",
            "take", "takes", "took", "get", "gets", "got", "see", "sees", "saw",
            "know", "knows", "knew", "think", "thinks", "thought", "say", "says", "said",
            "tell", "tells", "told", "ask", "asks", "asked", "work", "works", "worked",
            "seem", "seems", "seemed", "feel", "feels", "felt", "try", "tries", "tried",
            "leave", "leaves", "left", "call", "calls", "called", "eat", "eats", "ate",
            "run", "runs", "ran", "walk", "walks", "walked", "talk", "talks", "talked",
            "bark", "barks", "barked", "fly", "flies", "flew", "flow", "flows", "flowed",
            "read", "reads", "drink", "drinks", "drank", "jump", "jumps", "jumped",
            "study", "studies", "studied", "write", "writes", "wrote", "sing", "sings", "sang",
            "dance", "dances", "danced", "play", "plays", "played", "learn", "learns", "learned",
            "teach", "teaches", "taught", "help", "helps", "helped", "show", "shows", "showed",
            "use", "uses", "used", "find", "finds", "found", "give", "gives", "gave",
            "bring", "brings", "brought", "sit", "sits", "sat", "stand", "stands", "stood"
        };

        private static readonly HashSet<string> Articles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the"
        };

        private static readonly HashSet<string> Prepositions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "in", "on", "at", "to", "for", "of", "with", "by", "from", "about", "into", "through", "during"
        };

        /// <summary>
        /// Represents a parsed sentence component.
        /// </summary>
        public class SentenceComponents
        {
            public string Subject { get; set; }
            public string Predicate { get; set; }
            public string Object { get; set; }
            public SentenceType Type { get; set; }

            public override string ToString()
            {
                return Type == SentenceType.SubjectPredicate
                    ? $"[Pair] Subject: '{Subject}', Predicate: '{Predicate}'"
                    : $"[Triplet] Subject: '{Subject}', Verb: '{Predicate}', Object: '{Object}'";
            }
        }

        public enum SentenceType
        {
            SubjectPredicate,   // Maps to pair link
            SubjectVerbObject   // Maps to triplet link
        }

        /// <summary>
        /// Parses a sentence into subject-predicate or subject-verb-object components.
        /// </summary>
        public static SentenceComponents Parse(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                return null;
            }

            // Clean and normalize the sentence
            sentence = sentence.Trim().TrimEnd('.', '!', '?');
            var words = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
            {
                return null;
            }

            // Find the main verb
            int verbIndex = FindVerbIndex(words);

            if (verbIndex == -1)
            {
                // No verb found, treat entire sentence as subject-predicate
                return new SentenceComponents
                {
                    Subject = sentence,
                    Predicate = "",
                    Type = SentenceType.SubjectPredicate
                };
            }

            // Extract subject (everything before the verb)
            var subjectWords = words.Take(verbIndex).ToArray();
            var subject = string.Join(" ", subjectWords);

            // Extract predicate/verb
            var verb = words[verbIndex];

            // Extract object (everything after the verb)
            var remainingWords = words.Skip(verbIndex + 1).ToArray();

            if (remainingWords.Length == 0)
            {
                // Subject-Predicate pattern (no object)
                return new SentenceComponents
                {
                    Subject = subject,
                    Predicate = verb,
                    Type = SentenceType.SubjectPredicate
                };
            }

            // Subject-Verb-Object pattern
            var objectPart = string.Join(" ", remainingWords);

            return new SentenceComponents
            {
                Subject = subject,
                Predicate = verb,
                Object = objectPart,
                Type = SentenceType.SubjectVerbObject
            };
        }

        /// <summary>
        /// Finds the index of the main verb in the word array.
        /// </summary>
        private static int FindVerbIndex(string[] words)
        {
            // Skip articles at the beginning
            int startIndex = 0;
            while (startIndex < words.Length && Articles.Contains(words[startIndex]))
            {
                startIndex++;
            }

            // Look for the first verb after potential subject
            for (int i = startIndex; i < words.Length; i++)
            {
                if (CommonVerbs.Contains(words[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Parses multiple sentences from text.
        /// </summary>
        public static IEnumerable<SentenceComponents> ParseMultiple(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            // Split by sentence delimiters
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");

            foreach (var sentence in sentences)
            {
                var parsed = Parse(sentence);
                if (parsed != null)
                {
                    yield return parsed;
                }
            }
        }

        /// <summary>
        /// Determines if a sentence can be mapped to a pair link (subject-predicate).
        /// </summary>
        public static bool IsPairLink(SentenceComponents components)
        {
            return components?.Type == SentenceType.SubjectPredicate;
        }

        /// <summary>
        /// Determines if a sentence can be mapped to a triplet link (subject-verb-object).
        /// </summary>
        public static bool IsTripletLink(SentenceComponents components)
        {
            return components?.Type == SentenceType.SubjectVerbObject;
        }
    }
}
