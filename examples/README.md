# JSON Translation Rules Inference by Example

This directory contains an implementation for Issue #568: Data translation rules inference by example.

## Overview

The `json_translation_inference.py` module provides a tool to automatically infer translation rules between different JSON schemas by analyzing example data. When a source schema changes but the data values remain the same, the tool can detect field mappings by matching data values.

## Problem Statement

When you have:
- A source JSON with data and partial schema (field names, format, placement)
- A target JSON with different schema but related data
- Later, the source schema changes but data values stay the same

This tool helps you:
- Learn translation rules from example data
- Update translation rules when schemas change
- Apply learned rules to translate new data

## Example Usage

### Basic Example

```python
from json_translation_inference import JsonTranslationInference

# Original source and target
source = {"name": "John", "age": 30}
target = {"first_name": "John", "years": 30}

# Learn the mappings
inferencer = JsonTranslationInference()
mappings = inferencer.infer_mappings(source, target)
# Result: name → first_name, age → years

# Apply to new data
new_source = {"name": "Jane", "age": 25}
translated = inferencer.apply_mappings(new_source)
# Result: {"first_name": "Jane", "years": 25}
```

### Handling Schema Changes

```python
# Source schema changes: "name" becomes "fullname"
old_source = {"name": "John", "age": 30}
new_source = {"fullname": "John", "age": 30}

# Update mappings to reflect schema change
updated_mappings = inferencer.update_mappings_with_schema_change(
    old_source, new_source, mappings
)
# Result: fullname → first_name, age → years
```

## Features

- **Automatic Field Mapping**: Infers field mappings by matching data values
- **Nested Structure Support**: Handles deeply nested JSON objects and arrays
- **Schema Change Detection**: Automatically updates mappings when source schema changes
- **Confidence Scoring**: Provides confidence levels for mappings
- **Multiple Export Formats**: Export rules as JSON or human-readable text

## API Reference

### JsonTranslationInference

Main class for inference operations.

#### Methods

- `infer_mappings(source_json, target_json)` - Learn field mappings from examples
- `apply_mappings(new_source_json, mappings=None)` - Apply learned mappings to new data
- `update_mappings_with_schema_change(old_source, new_source, existing_mappings)` - Update mappings when schema changes
- `export_mappings_as_rules(format='json')` - Export mappings as translation rules

### FieldMapping

Data class representing a field mapping.

**Attributes**:
- `source_field` (str): Source field path
- `target_field` (str): Target field path
- `confidence` (float): Confidence level (0.0 to 1.0)
- `matched_values` (List[Any]): Values used to establish the mapping

## Running Examples

```bash
# Run the demonstration
python3 examples/json_translation_inference.py

# Run unit tests
python3 examples/test_json_translation_inference.py
```

## Implementation Details

The tool works by:

1. **Path Extraction**: Extracts all field paths and values from JSON structures
   - Handles nested objects: `user.profile.name`
   - Handles arrays: `users[0].name`

2. **Value Matching**: Matches fields between source and target by comparing values
   - Uses exact value matching
   - Skips None values as they're not distinctive

3. **Schema Change Detection**: When source schema changes:
   - Uses data values as anchors
   - Finds new field names containing the same values
   - Updates mappings with new source field names

4. **Confidence Calculation**: Calculates confidence based on uniqueness
   - Unique value match: confidence = 1.0
   - Multiple matches: confidence = 1.0 / number_of_matches

## Test Coverage

The implementation includes 14 unit tests covering:
- Path extraction for flat, nested, and array structures
- Simple and complex mapping inference
- Handling duplicate values and None values
- Applying mappings to new data
- Schema change detection
- Export functionality
- Complex nested structures

All tests pass successfully.

## Future Enhancements

Potential improvements:
- Fuzzy string matching for field names
- Machine learning-based confidence scoring
- Support for data type transformations
- Handling of value format changes
- Interactive mode for user verification

## Related Issues

- Issue #568: Data translation rules inference by example

## License

See the main repository LICENSE file.
