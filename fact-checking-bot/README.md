# Wikipedia Fact-Checking Bot

A Python-based tool that scans Wikipedia articles to verify that references actually contain the facts cited in the article text. This bot helps identify potential errors where a reference does not support the claim it's supposed to verify.

## Overview

The Wikipedia Fact-Checking Bot analyzes Wikipedia articles by:

1. **Extracting Citations**: Identifies claims in the article text that are backed by references
2. **Fetching References**: Retrieves the actual content from reference URLs
3. **Verifying Claims**: Compares the cited text against the reference content to check if the reference supports the claim
4. **Generating Reports**: Produces detailed reports showing which citations are verified, unverified, or problematic

## Features

- **Automated Citation Extraction**: Parses Wikipedia wikitext to find all citations
- **Reference Validation**: Checks if references actually contain the facts they're cited for
- **Similarity Scoring**: Uses text analysis to calculate how well references support claims
- **Detailed Reporting**: Generates comprehensive reports with statistics and per-citation analysis
- **Configurable**: Limit the number of citations to check, enable verbose logging
- **Well-Tested**: Includes comprehensive unit tests

## Installation

### Requirements

- Python 3.7 or higher
- pip (Python package manager)

### Install Dependencies

```bash
cd fact-checking-bot
pip install -r requirements.txt
```

The bot requires:
- `requests`: For fetching Wikipedia articles and reference content
- `beautifulsoup4`: For parsing HTML content from references
- `mwparserfromhell`: For parsing Wikipedia wikitext

## Usage

### Command Line

Basic usage:

```bash
python fact_checker.py "Article Title"
```

Check a specific article with options:

```bash
python fact_checker.py "Python (programming language)" --max-citations 10 --verbose
```

Save report to a file:

```bash
python fact_checker.py "Artificial intelligence" --output report.txt
```

### Command Line Options

- `article`: Wikipedia article title to check (required)
- `--max-citations N`: Maximum number of citations to check (default: 10)
- `--verbose`: Enable verbose logging
- `--output FILE`: Save report to file instead of printing to stdout

### As a Python Module

```python
from fact_checker import WikipediaFactChecker

# Create fact checker
checker = WikipediaFactChecker(verbose=True)

# Check an article
results = checker.check_article("Python (programming language)", max_citations=5)

# Generate report
report = checker.generate_report(results)
print(report)
```

### Advanced Usage

Check individual citations:

```python
from fact_checker import WikipediaFactChecker, Citation, Reference

checker = WikipediaFactChecker()

# Define a citation
citation = Citation(
    text="Paris is the capital of France",
    ref_name="france_ref",
    ref_content="",
    position=100
)

# Define a reference
reference = Reference(
    name="france_ref",
    url="https://example.com/france-facts",
    title="France Facts",
    content="Reference content"
)

# Verify the citation
result = checker.verify_citation(citation, reference)

print(f"Status: {result.status}")
print(f"Similarity: {result.similarity_score:.2%}")
```

## Examples

See the `examples/` directory for detailed usage examples:

```bash
cd examples
python fact_check_example.py
```

The example script demonstrates:
- Basic fact-checking workflow
- Custom citation analysis
- Single citation verification
- Similarity calculation testing

## How It Works

### 1. Fetching Articles

The bot uses the Wikipedia API to fetch article content in wikitext format:

```python
wikitext = checker.fetch_article("Article Title")
```

### 2. Extracting Citations

Citations are extracted from wikitext using pattern matching and the `mwparserfromhell` parser:

```python
citations = checker.extract_citations(wikitext)
references = checker.extract_references(wikitext)
```

Citations can be:
- Inline: `text<ref>reference content</ref>`
- Named: `text<ref name="ref1">content</ref>`
- Named references: `text<ref name="ref1" />`

### 3. Verifying Claims

For each citation:
1. Find the corresponding reference
2. If the reference has a URL, fetch its content
3. Calculate similarity between the cited text and reference content
4. Determine verification status based on similarity score

```python
result = checker.verify_citation(citation, reference)
```

### 4. Similarity Calculation

The bot uses a simple Jaccard similarity metric based on word overlap:

```
similarity = |words_in_common| / |all_unique_words|
```

A citation is considered "verified" if similarity > 0.1 (10%).

**Note**: This is a basic implementation. More sophisticated NLP techniques (like semantic similarity using embeddings) could improve accuracy.

## Report Format

The bot generates reports with:

### Summary Section
- Total citations checked
- Number verified
- Number unverified
- References not found
- Errors encountered

### Detailed Results
For each citation:
- Citation text (first 150 characters)
- Verification status
- Similarity score
- Reference details (name, URL, title)
- Additional details

## Testing

Run the test suite:

```bash
cd fact-checking-bot
python -m unittest test_fact_checker.py
```

Or run with verbose output:

```bash
python -m unittest test_fact_checker.py -v
```

Tests cover:
- Citation extraction
- Reference extraction
- URL and title parsing
- Similarity calculation
- Verification logic
- Report generation

## Limitations

1. **Simple Similarity Metric**: Uses basic word overlap; could be improved with semantic analysis
2. **Reference Access**: Can only verify references with accessible URLs
3. **Rate Limiting**: Makes HTTP requests to fetch references; may hit rate limits on large articles
4. **Wikitext Parsing**: May miss some citation formats or complex reference structures
5. **Language**: Currently optimized for English Wikipedia articles

## Future Improvements

- Implement semantic similarity using word embeddings or transformers
- Add support for citations without URLs (books, offline sources)
- Implement caching to avoid re-fetching the same references
- Add multi-language support
- Create a web interface
- Implement batch processing for multiple articles
- Add machine learning to improve verification accuracy
- Integration with Wikipedia's citation templates

## Contributing

Contributions are welcome! Areas for improvement:

- Better similarity algorithms
- Support for more citation formats
- Performance optimizations
- Additional test coverage
- Documentation improvements

## License

This project is part of the LinksPlatform repository. See the repository's LICENSE file for details.

## Use Cases

- **Quality Assurance**: Identify potentially problematic citations in Wikipedia articles
- **Research**: Analyze citation quality across multiple articles
- **Education**: Learn about citation verification and fact-checking
- **Automation**: Batch-check articles for citation quality

## Related Work

- [Wikipedia Citation needed](https://en.wikipedia.org/wiki/Wikipedia:Citation_needed)
- [Wikipedia Verifiability](https://en.wikipedia.org/wiki/Wikipedia:Verifiability)
- [Fact-checking](https://en.wikipedia.org/wiki/Fact-checking)

## Support

For issues, questions, or contributions, please open an issue in the LinksPlatform repository.
