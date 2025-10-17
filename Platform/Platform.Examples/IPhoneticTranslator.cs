namespace Platform.Examples
{
    /// <summary>
    /// Defines the contract for phonetic translation services.
    /// </summary>
    public interface IPhoneticTranslator
    {
        /// <summary>
        /// Translates a word to its phonetic representation.
        /// </summary>
        /// <param name="word">The word to translate.</param>
        /// <returns>The phonetic representation of the word, or null if translation is not available.</returns>
        string Translate(string word);

        /// <summary>
        /// Checks if a translation is available for the given word.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <returns>True if translation is available, false otherwise.</returns>
        bool CanTranslate(string word);
    }
}
