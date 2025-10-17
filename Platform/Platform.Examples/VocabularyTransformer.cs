using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    /// <summary>
    /// Transforms text based on vocabulary level by replacing complex words with simpler alternatives
    /// or grouping simple words into more complex terms.
    /// </summary>
    public class VocabularyTransformer
    {
        private readonly Dictionary<string, string> _complexToSimple;
        private readonly Dictionary<string, string> _simpleToComplex;

        /// <summary>
        /// Initializes a new instance of the VocabularyTransformer class.
        /// </summary>
        public VocabularyTransformer()
        {
            _complexToSimple = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _simpleToComplex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a mapping from a complex word to its simpler alternative.
        /// </summary>
        /// <param name="complexWord">The complex word to be replaced.</param>
        /// <param name="simpleAlternative">The simpler alternative phrase or word.</param>
        public void AddComplexToSimpleMapping(string complexWord, string simpleAlternative)
        {
            if (string.IsNullOrWhiteSpace(complexWord))
                throw new ArgumentException("Complex word cannot be null or whitespace.", nameof(complexWord));
            if (string.IsNullOrWhiteSpace(simpleAlternative))
                throw new ArgumentException("Simple alternative cannot be null or whitespace.", nameof(simpleAlternative));

            _complexToSimple[complexWord.Trim()] = simpleAlternative.Trim();
        }

        /// <summary>
        /// Adds a mapping from a simple phrase to its complex equivalent.
        /// </summary>
        /// <param name="simplePhrase">The simple phrase to be replaced.</param>
        /// <param name="complexWord">The complex word equivalent.</param>
        public void AddSimpleToComplexMapping(string simplePhrase, string complexWord)
        {
            if (string.IsNullOrWhiteSpace(simplePhrase))
                throw new ArgumentException("Simple phrase cannot be null or whitespace.", nameof(simplePhrase));
            if (string.IsNullOrWhiteSpace(complexWord))
                throw new ArgumentException("Complex word cannot be null or whitespace.", nameof(complexWord));

            _simpleToComplex[simplePhrase.Trim()] = complexWord.Trim();
        }

        /// <summary>
        /// Adds a bidirectional mapping between a complex word and its simple alternative.
        /// </summary>
        /// <param name="complexWord">The complex word.</param>
        /// <param name="simpleAlternative">The simpler alternative.</param>
        public void AddBidirectionalMapping(string complexWord, string simpleAlternative)
        {
            AddComplexToSimpleMapping(complexWord, simpleAlternative);
            AddSimpleToComplexMapping(simpleAlternative, complexWord);
        }

        /// <summary>
        /// Transforms text by unpacking complex words into simpler alternatives.
        /// </summary>
        /// <param name="text">The text to transform.</param>
        /// <returns>The transformed text with simpler vocabulary.</returns>
        public string UnpackToSimple(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return TransformText(text, _complexToSimple);
        }

        /// <summary>
        /// Transforms text by packaging simple phrases into more complex words.
        /// </summary>
        /// <param name="text">The text to transform.</param>
        /// <returns>The transformed text with more complex vocabulary.</returns>
        public string PackageToComplex(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Sort by length descending to match longer phrases first
            var sortedMappings = _simpleToComplex
                .OrderByDescending(kvp => kvp.Key.Length)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return TransformText(text, sortedMappings);
        }

        /// <summary>
        /// Transforms text to match the specified vocabulary level.
        /// </summary>
        /// <param name="text">The text to transform.</param>
        /// <param name="knownWords">Set of words the person knows.</param>
        /// <returns>The transformed text adapted to the vocabulary.</returns>
        public string AdaptToVocabulary(string text, HashSet<string> knownWords)
        {
            if (string.IsNullOrEmpty(text) || knownWords == null || knownWords.Count == 0)
                return text;

            var words = Regex.Split(text, @"(\W+)");
            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word) || !Regex.IsMatch(word, @"\w"))
                {
                    result.Append(word);
                    continue;
                }

                // If word is known, keep it
                if (knownWords.Contains(word.ToLower()))
                {
                    result.Append(word);
                }
                // If word is not known, try to find a simpler alternative
                else if (_complexToSimple.TryGetValue(word, out var simpleAlternative))
                {
                    // Check if the alternative words are in the known vocabulary
                    var alternativeWords = simpleAlternative.Split(' ');
                    if (alternativeWords.All(w => knownWords.Contains(w.ToLower())))
                    {
                        result.Append(simpleAlternative);
                    }
                    else
                    {
                        result.Append(word); // Keep original if alternative is also unknown
                    }
                }
                else
                {
                    result.Append(word);
                }
            }

            return result.ToString();
        }

        private string TransformText(string text, Dictionary<string, string> mappings)
        {
            var result = text;

            foreach (var mapping in mappings)
            {
                // Use word boundary matching to avoid partial word replacements
                var pattern = $@"\b{Regex.Escape(mapping.Key)}\b";
                result = Regex.Replace(result, pattern, mapping.Value, RegexOptions.IgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// Gets the count of complex-to-simple mappings.
        /// </summary>
        public int ComplexToSimpleMappingCount => _complexToSimple.Count;

        /// <summary>
        /// Gets the count of simple-to-complex mappings.
        /// </summary>
        public int SimpleToComplexMappingCount => _simpleToComplex.Count;
    }
}
