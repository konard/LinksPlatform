using System;
using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Translates words from various languages into International Phonetic Alphabet (IPA) notation.
    /// This implementation uses a dictionary-based approach with pre-defined phonetic mappings.
    /// </summary>
    public class InternationalPhoneticAlphabetTranslator : IPhoneticTranslator
    {
        private readonly Dictionary<string, string> _phoneticDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternationalPhoneticAlphabetTranslator"/> class.
        /// </summary>
        public InternationalPhoneticAlphabetTranslator()
        {
            _phoneticDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            InitializePhoneticDictionary();
        }

        /// <summary>
        /// Initializes the phonetic dictionary with common words from multiple languages.
        /// </summary>
        private void InitializePhoneticDictionary()
        {
            // English words
            _phoneticDictionary["hello"] = "həˈloʊ";
            _phoneticDictionary["world"] = "wɜːrld";
            _phoneticDictionary["phonetic"] = "fəˈnɛtɪk";
            _phoneticDictionary["alphabet"] = "ˈælfəˌbɛt";
            _phoneticDictionary["international"] = "ˌɪntərˈnæʃənəl";
            _phoneticDictionary["translator"] = "trænsˈleɪtər";
            _phoneticDictionary["water"] = "ˈwɔːtər";
            _phoneticDictionary["cat"] = "kæt";
            _phoneticDictionary["dog"] = "dɔːɡ";
            _phoneticDictionary["house"] = "haʊs";
            _phoneticDictionary["computer"] = "kəmˈpjuːtər";
            _phoneticDictionary["language"] = "ˈlæŋɡwɪdʒ";
            _phoneticDictionary["beautiful"] = "ˈbjuːtɪfəl";
            _phoneticDictionary["morning"] = "ˈmɔːrnɪŋ";
            _phoneticDictionary["night"] = "naɪt";
            _phoneticDictionary["friend"] = "frɛnd";
            _phoneticDictionary["book"] = "bʊk";
            _phoneticDictionary["music"] = "ˈmjuːzɪk";
            _phoneticDictionary["time"] = "taɪm";
            _phoneticDictionary["love"] = "lʌv";

            // Spanish words
            _phoneticDictionary["hola"] = "ˈola";
            _phoneticDictionary["mundo"] = "ˈmundo";
            _phoneticDictionary["gracias"] = "ˈɡɾaθjas";
            _phoneticDictionary["agua"] = "ˈaɣwa";
            _phoneticDictionary["casa"] = "ˈkasa";
            _phoneticDictionary["amigo"] = "aˈmiɣo";
            _phoneticDictionary["libro"] = "ˈliβɾo";
            _phoneticDictionary["tiempo"] = "ˈtjempo";
            _phoneticDictionary["amor"] = "aˈmoɾ";

            // French words
            _phoneticDictionary["bonjour"] = "bɔ̃ʒuʁ";
            _phoneticDictionary["monde"] = "mɔ̃d";
            _phoneticDictionary["merci"] = "mɛʁsi";
            _phoneticDictionary["eau"] = "o";
            _phoneticDictionary["maison"] = "mɛzɔ̃";
            _phoneticDictionary["ami"] = "ami";
            _phoneticDictionary["livre"] = "livʁ";
            _phoneticDictionary["temps"] = "tɑ̃";
            _phoneticDictionary["amour"] = "amuʁ";

            // German words
            _phoneticDictionary["hallo"] = "ˈhalo";
            _phoneticDictionary["welt"] = "vɛlt";
            _phoneticDictionary["danke"] = "ˈdaŋkə";
            _phoneticDictionary["wasser"] = "ˈvasɐ";
            _phoneticDictionary["haus"] = "haʊs";
            _phoneticDictionary["freund"] = "fʁɔʏnt";
            _phoneticDictionary["buch"] = "buːx";
            _phoneticDictionary["zeit"] = "tsaɪt";
            _phoneticDictionary["liebe"] = "ˈliːbə";

            // Italian words
            _phoneticDictionary["ciao"] = "tʃaʊ";
            _phoneticDictionary["mondo"] = "ˈmondo";
            _phoneticDictionary["grazie"] = "ˈɡrattsje";
            _phoneticDictionary["acqua"] = "ˈakkwa";
            _phoneticDictionary["casa"] = "ˈkasa";
            _phoneticDictionary["amico"] = "aˈmiko";
            _phoneticDictionary["libro"] = "ˈlibro";
            _phoneticDictionary["tempo"] = "ˈtɛmpo";
            _phoneticDictionary["amore"] = "aˈmore";

            // Russian words (romanized)
            _phoneticDictionary["privet"] = "prʲɪˈvʲet";
            _phoneticDictionary["mir"] = "mʲir";
            _phoneticDictionary["spasibo"] = "spɐˈsʲibə";
            _phoneticDictionary["voda"] = "vɐˈda";
            _phoneticDictionary["dom"] = "dom";
            _phoneticDictionary["drug"] = "druk";
            _phoneticDictionary["kniga"] = "ˈknʲiɡə";
            _phoneticDictionary["vremya"] = "ˈvrʲemʲə";
            _phoneticDictionary["lyubov"] = "lʲʊˈbofʲ";

            // Japanese words (romanized)
            _phoneticDictionary["konnichiwa"] = "koɲːit͡ɕiɰa";
            _phoneticDictionary["sekai"] = "sekai";
            _phoneticDictionary["arigato"] = "aɾiɡatoː";
            _phoneticDictionary["mizu"] = "mizɯ";
            _phoneticDictionary["ie"] = "ie";
            _phoneticDictionary["tomodachi"] = "tomodat͡ɕi";
            _phoneticDictionary["hon"] = "hoɴ";
            _phoneticDictionary["jikan"] = "d͡ʑikaɴ";
            _phoneticDictionary["ai"] = "ai";

            // Portuguese words
            _phoneticDictionary["ola"] = "ˈɔla";
            _phoneticDictionary["obrigado"] = "obɾiˈɡadu";
            _phoneticDictionary["agua"] = "ˈaɡwɐ";
            _phoneticDictionary["livro"] = "ˈlivɾu";

            // Dutch words
            _phoneticDictionary["dag"] = "dɑx";
            _phoneticDictionary["wereld"] = "ˈʋeːrəlt";
            _phoneticDictionary["dank"] = "dɑŋk";
            _phoneticDictionary["water"] = "ˈʋaːtər";
            _phoneticDictionary["huis"] = "hœys";

            // Chinese words (romanized Pinyin)
            _phoneticDictionary["nihao"] = "niˈxaʊ";
            _phoneticDictionary["shijie"] = "ʂɨˈtɕjɛ";
            _phoneticDictionary["xiexie"] = "ɕjɛˈɕjɛ";
            _phoneticDictionary["shui"] = "ʂweɪ";
            _phoneticDictionary["pengyou"] = "pʰəŋˈjoʊ";
        }

        /// <summary>
        /// Translates a word to its International Phonetic Alphabet (IPA) representation.
        /// </summary>
        /// <param name="word">The word to translate. Case-insensitive.</param>
        /// <returns>The IPA representation of the word, or null if the word is not in the dictionary.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the word parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the word parameter is empty or whitespace.</exception>
        public string Translate(string word)
        {
            if (word == null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException("Word cannot be empty or whitespace.", nameof(word));
            }

            var normalizedWord = word.Trim();

            if (_phoneticDictionary.TryGetValue(normalizedWord, out var phoneticRepresentation))
            {
                return phoneticRepresentation;
            }

            return null;
        }

        /// <summary>
        /// Checks if a translation is available for the given word.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <returns>True if a translation is available, false otherwise.</returns>
        public bool CanTranslate(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            return _phoneticDictionary.ContainsKey(word.Trim());
        }

        /// <summary>
        /// Adds a new word-to-IPA mapping to the dictionary or updates an existing one.
        /// </summary>
        /// <param name="word">The word to add.</param>
        /// <param name="ipaRepresentation">The IPA representation of the word.</param>
        /// <exception cref="ArgumentNullException">Thrown when word or ipaRepresentation is null.</exception>
        /// <exception cref="ArgumentException">Thrown when word or ipaRepresentation is empty or whitespace.</exception>
        public void AddOrUpdateTranslation(string word, string ipaRepresentation)
        {
            if (word == null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException("Word cannot be empty or whitespace.", nameof(word));
            }

            if (ipaRepresentation == null)
            {
                throw new ArgumentNullException(nameof(ipaRepresentation));
            }

            if (string.IsNullOrWhiteSpace(ipaRepresentation))
            {
                throw new ArgumentException("IPA representation cannot be empty or whitespace.", nameof(ipaRepresentation));
            }

            _phoneticDictionary[word.Trim()] = ipaRepresentation.Trim();
        }

        /// <summary>
        /// Gets the total number of words in the phonetic dictionary.
        /// </summary>
        public int DictionarySize => _phoneticDictionary.Count;
    }
}
