using System;
using Platform.Examples;

namespace Examples
{
    /// <summary>
    /// Example demonstrating the usage of the International Phonetic Alphabet Translator.
    /// </summary>
    public class IPATranslatorExample
    {
        public static void Main(string[] args)
        {
            // Create an instance of the IPA translator
            var translator = new InternationalPhoneticAlphabetTranslator();

            Console.WriteLine("=== International Phonetic Alphabet Translator Example ===\n");
            Console.WriteLine($"Dictionary contains {translator.DictionarySize} words\n");

            // Example 1: Translate English words
            Console.WriteLine("--- English Words ---");
            TranslateAndPrint(translator, "hello");
            TranslateAndPrint(translator, "world");
            TranslateAndPrint(translator, "phonetic");
            TranslateAndPrint(translator, "alphabet");
            Console.WriteLine();

            // Example 2: Translate Spanish words
            Console.WriteLine("--- Spanish Words ---");
            TranslateAndPrint(translator, "hola");
            TranslateAndPrint(translator, "mundo");
            TranslateAndPrint(translator, "gracias");
            Console.WriteLine();

            // Example 3: Translate French words
            Console.WriteLine("--- French Words ---");
            TranslateAndPrint(translator, "bonjour");
            TranslateAndPrint(translator, "monde");
            TranslateAndPrint(translator, "merci");
            Console.WriteLine();

            // Example 4: Translate German words
            Console.WriteLine("--- German Words ---");
            TranslateAndPrint(translator, "hallo");
            TranslateAndPrint(translator, "welt");
            TranslateAndPrint(translator, "danke");
            Console.WriteLine();

            // Example 5: Translate Japanese words (romanized)
            Console.WriteLine("--- Japanese Words (Romanized) ---");
            TranslateAndPrint(translator, "konnichiwa");
            TranslateAndPrint(translator, "arigato");
            Console.WriteLine();

            // Example 6: Translate Russian words (romanized)
            Console.WriteLine("--- Russian Words (Romanized) ---");
            TranslateAndPrint(translator, "privet");
            TranslateAndPrint(translator, "spasibo");
            Console.WriteLine();

            // Example 7: Try translating a word not in dictionary
            Console.WriteLine("--- Word Not in Dictionary ---");
            TranslateAndPrint(translator, "unknown");
            Console.WriteLine();

            // Example 8: Add a custom word and translate it
            Console.WriteLine("--- Adding Custom Word ---");
            translator.AddOrUpdateTranslation("platform", "ˈplætfɔːrm");
            Console.WriteLine("Added 'platform' to dictionary");
            TranslateAndPrint(translator, "platform");
            Console.WriteLine();

            // Example 9: Check if translation is available
            Console.WriteLine("--- Checking Translation Availability ---");
            CheckTranslation(translator, "hello");
            CheckTranslation(translator, "notaword");
            Console.WriteLine();

            // Example 10: Case-insensitive translation
            Console.WriteLine("--- Case-Insensitive Translation ---");
            TranslateAndPrint(translator, "HELLO");
            TranslateAndPrint(translator, "HeLLo");
            TranslateAndPrint(translator, "hello");
        }

        private static void TranslateAndPrint(IPhoneticTranslator translator, string word)
        {
            var result = translator.Translate(word);
            if (result != null)
            {
                Console.WriteLine($"{word,-15} → {result}");
            }
            else
            {
                Console.WriteLine($"{word,-15} → [Translation not available]");
            }
        }

        private static void CheckTranslation(IPhoneticTranslator translator, string word)
        {
            var canTranslate = translator.CanTranslate(word);
            Console.WriteLine($"Can translate '{word}': {canTranslate}");
        }
    }
}
