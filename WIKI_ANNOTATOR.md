# Wiki Annotator

A Python tool for automatically annotating markdown text with Wikipedia links. This tool can be used to enhance README files and repository wikis by adding educational Wikipedia links to technical terms and concepts.

## Features

- **Multiple Languages**: Support for English and Russian Wikipedia
- **Smart Detection**: Automatically identifies technical terms that benefit from Wikipedia links
- **Preserves Structure**: Maintains existing markdown formatting, links, and code blocks
- **Customizable**: Add custom term mappings for project-specific terminology
- **CLI and Library**: Use as a command-line tool or Python library
- **Safe Processing**: Never annotates text inside code blocks or existing links

## Installation

The tool is standalone and requires only Python 3.6+. No additional dependencies needed.

```bash
# Make the script executable
chmod +x wiki_annotator.py

# Or run directly with Python
python3 wiki_annotator.py --help
```

## Command-Line Usage

### Basic Usage

```bash
# Annotate a file with English Wikipedia links
python3 wiki_annotator.py input.md -o output.md

# Annotate with Russian Wikipedia links
python3 wiki_annotator.py input.ru.md -o output.ru.md -l ru

# Overwrite the input file
python3 wiki_annotator.py input.md

# Preview without saving (dry-run)
python3 wiki_annotator.py input.md --dry-run
```

### Advanced Usage

```bash
# Add custom term mappings
python3 wiki_annotator.py input.md -o output.md \
  -t "Docker:Docker_(software)" \
  -t "Kubernetes:Kubernetes" \
  -t "REST API:Representational_state_transfer"

# Process from stdin/stdout
echo "This is a framework" | python3 wiki_annotator.py - -o -

# Combine with other tools
cat README.md | python3 wiki_annotator.py - | less
```

## Python Library Usage

### Basic Example

```python
from wiki_annotator import WikiAnnotator

# Create annotator
annotator = WikiAnnotator(language='en')

# Annotate text
text = "This framework uses a modular architecture with a powerful library."
annotated = annotator.annotate(text)
print(annotated)
# Output: This [framework](https://en.wikipedia.org/wiki/Software_framework)
# uses a [modular](https://en.wikipedia.org/wiki/Modular_programming) architecture...
```

### Custom Terms

```python
from wiki_annotator import WikiAnnotator

# Add custom terms
custom_terms = {
    'Docker': 'Docker_(software)',
    'Kubernetes': 'Kubernetes',
    'GraphQL': 'GraphQL'
}

annotator = WikiAnnotator(language='en', custom_terms=custom_terms)
text = "Deploy with Docker and Kubernetes using GraphQL API"
annotated = annotator.annotate(text)
```

### File Processing

```python
from pathlib import Path
from wiki_annotator import WikiAnnotator

annotator = WikiAnnotator(language='en')

# Process a file
input_path = Path('README.md')
output_path = Path('README_annotated.md')
annotator.annotate_file(input_path, output_path)
```

### Russian Language

```python
from wiki_annotator import WikiAnnotator

annotator = WikiAnnotator(language='ru')
text = "Это модульный фреймворк с поддержкой СУБД."
annotated = annotator.annotate(text)
# Output: Это [модульный](https://ru.wikipedia.org/wiki/Модульное_программирование)
# [фреймворк](https://ru.wikipedia.org/wiki/Фреймворк)...
```

## Default Terms

### English

The tool recognizes these terms by default:

- framework → Software_framework
- database → Database
- DBMS → Database
- library → Library_(computing)
- programming language → Programming_language
- implementation → Implementation
- compiler → Compiler
- IDE → Integrated_development_environment
- text editor → Text_editor
- modular → Modular_programming

### Russian

- модульный → Модульное_программирование
- фреймворк → Фреймворк
- СУБД → Система_управления_базами_данных
- реализация → Реализация
- библиотека → Библиотека_(программирование)
- язык программирования → Язык_программирования
- транслятор → Транслятор

## Examples

See the `examples/` directory for complete examples:

- `example_basic.py` - Basic usage examples with different languages
- `example_file_processing.py` - File processing example

Run examples:

```bash
python3 examples/example_basic.py
python3 examples/example_file_processing.py
```

## How It Works

1. **Parsing**: Reads markdown text and identifies code blocks and existing links
2. **Pattern Matching**: Searches for known technical terms using case-insensitive matching
3. **Smart Linking**: Creates Wikipedia links only for unlinked terms outside code blocks
4. **Preservation**: Maintains all existing markdown structure, links, and formatting

## Use Cases

### Repository Documentation

Enhance your README files with educational links:

```bash
# Annotate main README
python3 wiki_annotator.py README.md -o README.md

# Annotate Russian version
python3 wiki_annotator.py README.ru.md -o README.ru.md -l ru
```

### Wiki Pages

Process wiki markdown files before publishing:

```bash
# Batch process wiki pages
for file in wiki/*.md; do
  python3 wiki_annotator.py "$file" -o "$file"
done
```

### CI/CD Integration

Add to your documentation build process:

```yaml
# .github/workflows/docs.yml
- name: Annotate documentation
  run: |
    python3 wiki_annotator.py docs/README.md -o docs/README.md
```

## Limitations

- Only processes markdown text (not other formats)
- Requires exact term matching (case-insensitive)
- Does not validate Wikipedia links
- Does not handle all edge cases in complex markdown structures

## Contributing

To add more default terms, edit the `_get_default_terms()` method in `wiki_annotator.py`.

## License

This tool is part of the LinksPlatform project. See LICENSE for details.
