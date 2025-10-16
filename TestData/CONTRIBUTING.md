# Contributing Test Data to LinksPlatform

Thank you for your interest in contributing test and example data to the LinksPlatform project!

## Contribution Process

### 1. Prepare Your Test Data

Create your test data file following the [JSON schema specification](schema.json). Use the examples in the `/standard` directory as reference.

### 2. Choose the Right Directory

- **`/community`**: For new submissions or user-contributed test data
- **`/experimental`**: For automatically collected or unverified data
- **`/standard`**: Reserved for approved, verified test data (maintainers only)

### 3. Submission Methods

#### Method A: Pull Request (Recommended)

1. Fork the repository
2. Create a new branch for your test data
3. Add your test data file to the appropriate directory
4. Ensure your file follows the naming convention: `{category}-{name}-{number}.json`
5. Validate your test data (see Validation section below)
6. Commit your changes with a clear message
7. Create a pull request with a description of your test data

#### Method B: Issue Tracker

1. Create a new issue using the "Test Data Submission" template
2. Fill in all required fields
3. Provide complete test data in the issue description
4. Await review and approval

### 4. Review Process

All submissions will be reviewed by maintainers for:

- **Accuracy**: Does the test data correctly represent the described scenario?
- **Completeness**: Are all required fields present and filled in?
- **Clarity**: Is the description clear and understandable?
- **Format**: Does the data follow the schema specification?
- **Quality**: Is the test data useful and well-designed?

### 5. Approval and Promotion

- Community submissions are first added to `/community`
- After verification and approval, high-quality test data may be promoted to `/standard`
- Experimental data requires human verification before being moved to community or standard

## Validation

Before submitting, validate your test data:

### Manual Validation

Check that your JSON file:
- Is valid JSON (use a JSON validator)
- Contains all required fields per the schema
- Has unique ID within the collection
- Includes clear descriptions
- Has appropriate tags and categorization

### Automated Validation

If available, use the validation tool:

```bash
# Validate a single file
python TestData/tools/validate.py path/to/your-test-data.json

# Or using the C# validator
dotnet run --project Platform.TestData.Validator -- validate path/to/your-test-data.json
```

## Test Data Guidelines

### Good Test Data

- **Focused**: Tests one specific concept or operation
- **Clear**: Easy to understand what is being tested
- **Complete**: Includes all necessary input and expected output
- **Documented**: Well-described with clear rules and notes
- **Realistic**: Represents real-world use cases when possible

### Naming Convention

Files should be named: `{category}-{descriptive-name}-{number}.json`

Examples:
- `link-operations-create-basic-001.json`
- `sequences-unicode-text-002.json`
- `transformations-csv-import-001.json`

### Categories

Use these standard categories:
- `link-operations`: Basic link CRUD operations
- `sequences`: Sequence creation and manipulation
- `transformations`: Data transformations and conversions
- `indexing`: Indexing and search operations
- `unicode`: Unicode and text handling
- `import-export`: Data import/export operations
- `performance`: Performance and optimization tests
- `other`: Other test cases

### Tags

Add relevant tags to help others discover your test data:
- Operation type: `create`, `read`, `update`, `delete`, `search`, `transform`
- Data type: `text`, `numeric`, `binary`, `structured`
- Complexity: `edge-case`, `regression`, `integration`
- Domain: `ml`, `nlp`, `graph`, `database`

## What NOT to Submit

- Sensitive or proprietary information
- Personal data or PII
- Large datasets (>1MB) without prior discussion
- Duplicate test cases (check existing data first)
- Test data with unclear or missing descriptions
- Unverified or incorrect expected outputs

## Automated Collection

For systems that automatically collect test data from production:

1. Data should be placed in `/experimental`
2. Include source system identifier in author field
3. Mark as `"verified": false` initially
4. Include timestamp and any relevant metadata
5. Filter out sensitive information before submission

## Questions?

If you have questions about contributing test data:

- Check the [README](README.md) for format details
- Review the [schema specification](schema.json)
- Look at examples in `/standard` directory
- Open an issue with the "question" label

Thank you for helping improve the LinksPlatform test data collection!
