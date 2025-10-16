# Multilingual XML Documentation Guide

This guide explains how to write multilingual XML documentation comments for C# code in the LinksPlatform project.

## Approach

The recommended approach is to use the `xml:lang` attribute within XML documentation comments to specify different language versions. This allows developers to provide documentation in multiple languages directly in the source code.

## Syntax

Use the `xml:lang` attribute on documentation tags to specify the language:

```csharp
/// <summary xml:lang="en">English description</summary>
/// <summary xml:lang="ru">Описание на русском языке</summary>
public class MyClass
{
    /// <remarks xml:lang="en">
    /// English remarks about the method.
    /// </remarks>
    /// <remarks xml:lang="ru">
    /// Примечания о методе на русском языке.
    /// </remarks>
    public void MyMethod() { }
}
```

## Supported Tags

The `xml:lang` attribute can be used with any XML documentation tag:

- `<summary>` - Brief description
- `<remarks>` - Detailed remarks
- `<param>` - Parameter descriptions
- `<returns>` - Return value description
- `<exception>` - Exception documentation
- `<example>` - Code examples

## Language Codes

Use ISO 639-1 language codes:

- `en` - English
- `ru` - Russian (Русский)
- `zh` - Chinese (中文)
- `ja` - Japanese (日本語)
- `de` - German (Deutsch)
- `fr` - French (Français)
- `es` - Spanish (Español)

## Complete Example

```csharp
/// <summary xml:lang="en">
/// Represents a link between two data elements.
/// </summary>
/// <summary xml:lang="ru">
/// Представляет связь между двумя элементами данных.
/// </summary>
/// <remarks xml:lang="en">
/// This class provides a fundamental building block for the Links data structure.
/// </remarks>
/// <remarks xml:lang="ru">
/// Этот класс предоставляет основной строительный блок для структуры данных Links.
/// </remarks>
public class Link
{
    /// <summary xml:lang="en">
    /// Gets or sets the source of the link.
    /// </summary>
    /// <summary xml:lang="ru">
    /// Получает или устанавливает источник связи.
    /// </summary>
    public ulong Source { get; set; }

    /// <summary xml:lang="en">
    /// Gets or sets the target of the link.
    /// </summary>
    /// <summary xml:lang="ru">
    /// Получает или устанавливает цель связи.
    /// </summary>
    public ulong Target { get; set; }

    /// <summary xml:lang="en">
    /// Creates a new link with the specified source and target.
    /// </summary>
    /// <summary xml:lang="ru">
    /// Создает новую связь с указанными источником и целью.
    /// </summary>
    /// <param name="source" xml:lang="en">The source element identifier.</param>
    /// <param name="source" xml:lang="ru">Идентификатор элемента-источника.</param>
    /// <param name="target" xml:lang="en">The target element identifier.</param>
    /// <param name="target" xml:lang="ru">Идентификатор элемента-цели.</param>
    /// <returns xml:lang="en">A new Link instance.</returns>
    /// <returns xml:lang="ru">Новый экземпляр Link.</returns>
    public static Link Create(ulong source, ulong target)
    {
        return new Link { Source = source, Target = target };
    }
}
```

## Best Practices

1. **Always provide English version** - Include `xml:lang="en"` as the default language
2. **Maintain consistency** - Use the same structure for all language versions
3. **Keep translations synchronized** - Update all language versions when changing documentation
4. **Use clear, concise descriptions** - Make documentation easy to understand in all languages
5. **Document public APIs** - Focus on public classes, methods, and properties
6. **Consider context** - Ensure translations convey the same technical meaning

## Integration with DocFX

DocFX, the documentation generation tool used by this project, processes XML comments with `xml:lang` attributes. While DocFX may not automatically generate separate documentation files for each language, the multilingual comments are preserved in the generated XML documentation files, allowing tools and IDEs that support multiple languages to display appropriate versions.

## IDE Support

Modern IDEs like Visual Studio and JetBrains Rider can display XML documentation comments. While native support for `xml:lang` varies, the comments remain accessible in IntelliSense and documentation viewers.

## Alternative Approaches

If the `xml:lang` attribute approach doesn't fully meet your needs, consider these alternatives:

1. **Separate XML files** - Maintain language-specific XML documentation files
2. **Post-build processing** - Use tools like [XML Comment Localization](http://www.surviveplus.net/en/archives/39) to generate localized XML files
3. **Resource files** - Store translations in resource files and reference them in comments

## Contributing

When contributing code to LinksPlatform:

1. Document all public APIs with at least English documentation
2. If you're comfortable with Russian, add Russian translations using `xml:lang="ru"`
3. Follow the examples in this guide for consistent formatting
4. Ensure XML documentation generation is enabled in project files (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)

## References

- [How to localize the documentation of a .NET library](http://stackoverflow.com/questions/6221140/how-to-localize-the-documentation-of-a-net-library)
- [XML Comment Localization by surviveplus](http://www.surviveplus.net/en/archives/39)
- [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB)
- [DocFX Documentation](https://dotnet.github.io/docfx/)
