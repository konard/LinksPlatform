# Separating C# XML Documentation from Code

## Overview

This guide demonstrates how to separate XML documentation from C# source code using the `<include>` tag. This approach allows documentation to be maintained in external XML files, which can be beneficial for:

- Keeping source code files cleaner and more focused on implementation
- Allowing documentation teams to work independently from developers
- Managing translations of documentation more easily
- Reusing documentation across multiple code elements

## How It Works

The XML `<include>` tag allows you to reference documentation stored in external files. The C# compiler and documentation tools (like DocFX) will read these external files and include the documentation as if it were written inline.

### Basic Syntax

```csharp
/// <include file='path/to/docs.xml' path='xpath/to/element/*'/>
public class MyClass { }
```

## Implementation Example

### Step 1: Create External Documentation File

Create an XML file to store your documentation (e.g., `Platform/Documentation/API/QueryExecutorExtensions.xml`):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<docs>
  <members name="QueryExecutorExtensions">
    <QueryExecutorExtensions>
      <summary>
        Представляет класс-контейнер расширений для выполнения произвольных запросов над Links
      </summary>
    </QueryExecutorExtensions>
    <Execute>
      <summary>
        Выполняет запрос query над links и возвращает результат запроса в виде перечисляемого объекта с элементами типа T.
      </summary>
      <typeparam name="T">Тип элемента запроса.</typeparam>
      <param name="links">База данных связей, над которой будет выполняться запрос.</param>
      <param name="query">Запрос в виде Linq-выражения.</param>
      <returns>Результат запроса в виде перечисляемого объекта с элементами типа T.</returns>
    </Execute>
  </members>
</docs>
```

### Step 2: Reference Documentation in C# Code

Replace inline XML documentation with `<include>` tags:

```csharp
/// <include file='../Documentation/API/QueryExecutorExtensions.xml' path='docs/members[@name="QueryExecutorExtensions"]/QueryExecutorExtensions/*'/>
public static class QueryExecutorExtensions
{
    /// <include file='../Documentation/API/QueryExecutorExtensions.xml' path='docs/members[@name="QueryExecutorExtensions"]/Execute/*'/>
    public static IEnumerable<T> Execute<T>(this SynchronizedLinks<ulong> links,
        Expression<Func<SynchronizedLinks<ulong>, IEnumerable<T>>> query)
    {
        // Implementation
    }
}
```

## Best Practices

### File Organization

Organize external documentation files to mirror your code structure:

```
Platform/
├── Documentation/
│   └── API/
│       ├── ClassName1.xml
│       ├── ClassName2.xml
│       └── ...
└── YourProject/
    ├── ClassName1.cs
    ├── ClassName2.cs
    └── ...
```

### XPath Path Structure

Use descriptive XML structures that make XPath queries clear:

```xml
<docs>
  <members name="ClassName">
    <ClassName>
      <!-- Class documentation -->
    </ClassName>
    <MethodName>
      <!-- Method documentation -->
    </MethodName>
  </members>
</docs>
```

### File Paths

- Use relative paths from the C# file to the XML file
- Keep paths consistent across the project
- Consider using the same base directory structure

### When to Use This Approach

**Good use cases:**
- Large codebases with extensive documentation
- Projects with separate documentation teams
- Multilingual documentation requirements
- Documentation that needs frequent updates independent of code changes

**When to keep inline documentation:**
- Small projects with minimal documentation
- Rapidly changing APIs where documentation changes frequently with code
- When documentation and implementation are closely coupled

## Verification

After implementing external documentation:

1. Build the project to ensure C# compiler can find and parse the XML files
2. Generate documentation using DocFX to verify the include tags work correctly
3. Check that IntelliSense in your IDE still shows the documentation

## Tools Support

- **Visual Studio**: Fully supports `<include>` tags for IntelliSense
- **DocFX**: Processes `<include>` tags when generating documentation
- **Sandcastle Help File Builder (SHFB)**: Also supports external documentation files
- **.NET CLI**: The `dotnet build` command processes includes for XML documentation output

## Migration Strategy

For existing projects with inline documentation:

1. Start with a pilot class or namespace
2. Create the external XML file structure
3. Move documentation from code to XML files
4. Update code to use `<include>` tags
5. Test documentation generation
6. Gradually migrate other parts of the codebase

## References

- [Microsoft Docs: include tag](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/include)
- [Recommended XML tags for C# documentation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)
- [DocFX Documentation](https://dotnet.github.io/docfx/)
