# LinksPlatform Test Data Collection

This directory contains test and example data for the LinksPlatform project. The data serves multiple purposes:
- Training sets for machine learning algorithms
- Test datasets for algorithm validation
- Reference examples demonstrating correct solutions to specific problems
- Real-world usage patterns collected from production systems

## Directory Structure

### `/standard`
Verified and approved test data that has been reviewed and accepted as part of the standard test suite. This data should be stable and well-documented.

### `/community`
Community-submitted test data that is under review or has been accepted but not yet promoted to standard. Anyone can submit examples here.

### `/experimental`
Experimental or automatically collected data from real-world system usage. This data may be noisy or incomplete and requires validation before use.

### `/deprecated`
Outdated or superseded test data kept for historical reference.

## Test Data Format

Each test data entry should be provided as a JSON file with the following structure:

```json
{
  "metadata": {
    "id": "unique-identifier",
    "name": "Descriptive name of the test case",
    "description": "Detailed description of what this test demonstrates",
    "category": "link-operations|sequences|transformations|indexing|other",
    "difficulty": "basic|intermediate|advanced",
    "tags": ["tag1", "tag2"],
    "author": "Author name or system identifier",
    "created": "ISO 8601 timestamp",
    "verified": true|false,
    "verifiedBy": "Reviewer name (if verified)",
    "verifiedDate": "ISO 8601 timestamp (if verified)"
  },
  "input": {
    "description": "Description of the input",
    "data": "Input data in appropriate format"
  },
  "expectedOutput": {
    "description": "Description of expected output",
    "data": "Expected output data"
  },
  "rules": [
    "Rule 1 that should be applied",
    "Rule 2 that should be applied"
  ],
  "notes": "Additional notes or considerations"
}
```

Alternative formats (CSV, XML, binary) are also supported for specific use cases. See the schema documentation for details.

## Submitting Test Data

To submit new test data:

1. Create your test data file following the format specification above
2. Place it in the `/community` directory with a descriptive filename
3. Create a pull request or submit via issue tracker using the test data submission template
4. Await review and approval from maintainers

For automated submissions from production systems, use the collection API (documentation coming soon).

## Validation

All test data should pass validation before being accepted. Use the validation tool:

```bash
# Validate a single test data file
dotnet run --project Platform.TestData.Validator -- validate path/to/testdata.json

# Validate all test data in a directory
dotnet run --project Platform.TestData.Validator -- validate-dir TestData/community
```

## Usage in Testing

Test data can be consumed by test frameworks and ML pipelines:

```csharp
using Platform.TestData;

// Load standard test data
var testDataSet = TestDataLoader.LoadStandard("link-operations");

// Load all test data from a directory
var allData = TestDataLoader.LoadFromDirectory("TestData/standard");

// Filter by category and difficulty
var basicTests = allData.Where(t =>
    t.Metadata.Category == "sequences" &&
    t.Metadata.Difficulty == "basic");
```

## Contribution Guidelines

When contributing test data:

- Ensure data is accurate and well-documented
- Include clear descriptions of input, output, and expected behavior
- Tag appropriately for easy discovery
- Verify that the example demonstrates the intended concept
- Remove any sensitive or proprietary information
- Follow the established format and schema

## Version History

Test data follows semantic versioning. Changes to the schema or significant updates to the standard test set will increment version numbers.

Current version: 1.0.0

## License

Test data in this repository is provided under the same license as the LinksPlatform project (see LICENSE file in root directory).
