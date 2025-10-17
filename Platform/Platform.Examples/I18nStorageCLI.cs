using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for internationalization storage demonstration.
    /// Shows how to store and retrieve translations in multiple languages in a single links file.
    /// </summary>
    public class I18nStorageCLI : ICommandLineInterface
    {
        private const string DefaultI18nFilename = "i18n.links";

        public void Run(params string[] args)
        {
            var filename = args.Length > 0 ? args[0] : DefaultI18nFilename;

            Console.WriteLine($"Internationalization Storage Demo");
            Console.WriteLine($"Using file: {filename}");
            Console.WriteLine();

            try
            {
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(filename, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var frequenciesCache = new LinkFrequenciesCache<ulong>(links);
                    var i18nStorage = new I18nStorage<ulong>(links, indexSequenceBeforeCreation: true, frequenciesCache);

                    // Store sample translations
                    Console.WriteLine("Storing translations...");

                    // English translations
                    i18nStorage.SetTranslation("en", "greeting.hello", "Hello");
                    i18nStorage.SetTranslation("en", "greeting.goodbye", "Goodbye");
                    i18nStorage.SetTranslation("en", "button.submit", "Submit");
                    i18nStorage.SetTranslation("en", "button.cancel", "Cancel");
                    i18nStorage.SetTranslation("en", "app.title", "Links Platform");

                    // Russian translations
                    i18nStorage.SetTranslation("ru", "greeting.hello", "Привет");
                    i18nStorage.SetTranslation("ru", "greeting.goodbye", "До свидания");
                    i18nStorage.SetTranslation("ru", "button.submit", "Отправить");
                    i18nStorage.SetTranslation("ru", "button.cancel", "Отменить");
                    i18nStorage.SetTranslation("ru", "app.title", "Платформа Связей");

                    // Spanish translations
                    i18nStorage.SetTranslation("es", "greeting.hello", "Hola");
                    i18nStorage.SetTranslation("es", "greeting.goodbye", "Adiós");
                    i18nStorage.SetTranslation("es", "button.submit", "Enviar");
                    i18nStorage.SetTranslation("es", "button.cancel", "Cancelar");
                    i18nStorage.SetTranslation("es", "app.title", "Plataforma de Enlaces");

                    Console.WriteLine("Translations stored successfully!");
                    Console.WriteLine();

                    // Retrieve and display translations
                    Console.WriteLine("Retrieving translations...");
                    Console.WriteLine();

                    var languages = new[] { "en", "ru", "es" };
                    var keys = new[] { "greeting.hello", "greeting.goodbye", "button.submit", "button.cancel", "app.title" };

                    foreach (var lang in languages)
                    {
                        Console.WriteLine($"Language: {lang}");
                        foreach (var key in keys)
                        {
                            var translationLink = i18nStorage.GetTranslation(lang, key);
                            Console.WriteLine($"  {key}: Link #{translationLink}");
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine($"Total links in database: {links.Count()}");
                    Console.WriteLine();
                    Console.WriteLine("All translations are stored in a single file: " + filename);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }
        }
    }
}
