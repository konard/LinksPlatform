# Internationalization Storage Example

This example demonstrates how to store internationalization (i18n) data inside a Links file, allowing all language translations to be stored in a single file instead of separate files per language.

## Overview

The `I18nStorage` class provides a way to store and retrieve translations for multiple languages in a Links database file (`.links`). This solves issue #561 by consolidating internationalization data.

## Benefits

1. **Single Source of Truth**: All translations in one file (`i18n.links`)
2. **Dynamic Language Switching**: Add/update translations without recompiling
3. **Efficient Storage**: Uses the Links platform's associative data structure
4. **Scalable**: Can handle many languages and translation keys
5. **Consistent**: All languages use the same structure

## Usage Example

### Basic Setup

```csharp
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Examples;

// Open or create a links file for i18n data
using var memoryAdapter = new UInt64UnitedMemoryLinks("i18n.links", 8 * 1024 * 1024);
using var links = new UInt64Links(memoryAdapter);

var frequenciesCache = new LinkFrequenciesCache<ulong>(links);
var i18nStorage = new I18nStorage<ulong>(links, indexSequenceBeforeCreation: true, frequenciesCache);
```

### Storing Translations

```csharp
// English
i18nStorage.SetTranslation("en", "greeting.hello", "Hello");
i18nStorage.SetTranslation("en", "button.submit", "Submit");
i18nStorage.SetTranslation("en", "app.title", "Links Platform");

// Russian
i18nStorage.SetTranslation("ru", "greeting.hello", "Привет");
i18nStorage.SetTranslation("ru", "button.submit", "Отправить");
i18nStorage.SetTranslation("ru", "app.title", "Платформа Связей");

// Spanish
i18nStorage.SetTranslation("es", "greeting.hello", "Hola");
i18nStorage.SetTranslation("es", "button.submit", "Enviar");
i18nStorage.SetTranslation("es", "app.title", "Plataforma de Enlaces");
```

### Retrieving Translations

```csharp
// Get translation for a specific language and key
var helloLink = i18nStorage.GetTranslation("ru", "greeting.hello");
// Returns link to "Привет"

var titleLink = i18nStorage.GetTranslation("es", "app.title");
// Returns link to "Plataforma de Enlaces"
```

## Data Structure

The implementation uses the following structure in the Links database:

```
Markers:
- languageMarker: Identifies language entries
- translationKeyMarker: Identifies translation key entries
- translationMarker: Identifies translation entries

Language:
languageMarker -> languageCodeSequence -> languageLink

Translation Key:
translationKeyMarker -> keyStringSequence -> keyLink

Translation:
translationMarker -> translation
  where translation = (languageLink -> keyLink) -> valueSequence
```

## Running the Demo

To run the demonstration CLI:

```bash
cd Platform/Platform.Examples
dotnet run --project Platform.Examples.csproj I18nStorageCLI
```

Or with a custom filename:

```bash
dotnet run --project Platform.Examples.csproj I18nStorageCLI my-translations.links
```

## Example Output

```
Internationalization Storage Demo
Using file: i18n.links

Storing translations...
Translations stored successfully!

Retrieving translations...

Language: en
  greeting.hello: Link #15234
  greeting.goodbye: Link #15247
  button.submit: Link #15260
  button.cancel: Link #15273
  app.title: Link #15286

Language: ru
  greeting.hello: Link #15299
  greeting.goodbye: Link #15312
  button.submit: Link #15325
  button.cancel: Link #15338
  app.title: Link #15351

Language: es
  greeting.hello: Link #15364
  greeting.goodbye: Link #15377
  button.submit: Link #15390
  button.cancel: Link #15403
  app.title: Link #15416

Total links in database: 15420

All translations are stored in a single file: i18n.links
```

## Advanced Use Cases

### 1. Loading Translations from JSON

You could extend this to load translations from existing JSON/YAML files:

```csharp
var json = File.ReadAllText("translations.json");
var translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

foreach (var (lang, keys) in translations)
{
    foreach (var (key, value) in keys)
    {
        i18nStorage.SetTranslation(lang, key, value);
    }
}
```

### 2. Fallback Language Support

```csharp
public string GetTranslationWithFallback(string lang, string key, string fallbackLang = "en")
{
    var link = i18nStorage.GetTranslation(lang, key);
    if (link == default)
    {
        link = i18nStorage.GetTranslation(fallbackLang, key);
    }
    return link; // Convert to string using appropriate converter
}
```

### 3. Batch Operations

```csharp
public void ImportLanguage(string lang, Dictionary<string, string> translations)
{
    foreach (var (key, value) in translations)
    {
        i18nStorage.SetTranslation(lang, key, value);
    }
}
```

## Migration from File-based i18n

To migrate from the current file-based approach (README.md, README.ru.md, etc.):

1. Parse existing language-specific files
2. Extract translatable strings with keys
3. Store in the i18n.links file using `I18nStorage`
4. Update application to read from links database

## Performance Considerations

- **Caching**: The implementation caches language and key links to avoid repeated lookups
- **Indexing**: Enable sequence indexing for faster lookups on large datasets
- **Batch Loading**: Load translations at startup and cache in memory for runtime performance

## Future Enhancements

Possible extensions to this implementation:

1. **Pluralization Support**: Handle singular/plural forms
2. **Interpolation**: Support variables in translation strings
3. **Namespaces**: Organize translations into hierarchical namespaces
4. **Versioning**: Track translation versions and changes
5. **Export/Import**: Tools to export to standard i18n formats (JSON, YAML, PO files)
