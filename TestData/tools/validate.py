#!/usr/bin/env python3
"""
Test Data Validator for LinksPlatform

This script validates test data files against the JSON schema specification.
"""

import json
import sys
import os
from pathlib import Path
from datetime import datetime

def load_json_file(filepath):
    """Load and parse a JSON file."""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            return json.load(f)
    except json.JSONDecodeError as e:
        print(f"❌ JSON parse error in {filepath}: {e}")
        return None
    except Exception as e:
        print(f"❌ Error loading {filepath}: {e}")
        return None

def validate_required_fields(data, filepath):
    """Validate that all required fields are present."""
    errors = []

    # Check top-level required fields
    required_top = ['metadata', 'input', 'expectedOutput']
    for field in required_top:
        if field not in data:
            errors.append(f"Missing required field: {field}")

    if 'metadata' in data:
        required_metadata = ['id', 'name', 'category', 'author', 'created']
        for field in required_metadata:
            if field not in data['metadata']:
                errors.append(f"Missing required metadata field: {field}")

        # Validate category
        valid_categories = [
            'link-operations', 'sequences', 'transformations',
            'indexing', 'unicode', 'import-export', 'performance', 'other'
        ]
        if 'category' in data['metadata'] and data['metadata']['category'] not in valid_categories:
            errors.append(f"Invalid category: {data['metadata']['category']}. Must be one of {valid_categories}")

        # Validate difficulty if present
        if 'difficulty' in data['metadata']:
            valid_difficulty = ['basic', 'intermediate', 'advanced']
            if data['metadata']['difficulty'] not in valid_difficulty:
                errors.append(f"Invalid difficulty: {data['metadata']['difficulty']}. Must be one of {valid_difficulty}")

    # Check input field
    if 'input' in data and 'data' not in data['input']:
        errors.append("Missing required field: input.data")

    # Check expectedOutput field
    if 'expectedOutput' in data and 'data' not in data['expectedOutput']:
        errors.append("Missing required field: expectedOutput.data")

    return errors

def validate_id_format(data):
    """Validate ID format (lowercase alphanumeric with hyphens)."""
    if 'metadata' not in data or 'id' not in data['metadata']:
        return []

    test_id = data['metadata']['id']
    if not all(c.isalnum() or c == '-' for c in test_id):
        return [f"Invalid ID format: {test_id}. Must contain only lowercase letters, numbers, and hyphens."]

    if not test_id.islower():
        return [f"Invalid ID format: {test_id}. Must be lowercase."]

    return []

def validate_timestamp(data):
    """Validate timestamp format."""
    errors = []

    if 'metadata' in data:
        if 'created' in data['metadata']:
            try:
                datetime.fromisoformat(data['metadata']['created'].replace('Z', '+00:00'))
            except ValueError:
                errors.append(f"Invalid timestamp format in 'created': {data['metadata']['created']}")

        if 'verifiedDate' in data['metadata']:
            try:
                datetime.fromisoformat(data['metadata']['verifiedDate'].replace('Z', '+00:00'))
            except ValueError:
                errors.append(f"Invalid timestamp format in 'verifiedDate': {data['metadata']['verifiedDate']}")

    return errors

def validate_file(filepath):
    """Validate a single test data file."""
    print(f"\nValidating: {filepath}")

    data = load_json_file(filepath)
    if data is None:
        return False

    errors = []

    # Run all validations
    errors.extend(validate_required_fields(data, filepath))
    errors.extend(validate_id_format(data))
    errors.extend(validate_timestamp(data))

    if errors:
        print("❌ Validation failed with errors:")
        for error in errors:
            print(f"   - {error}")
        return False
    else:
        print("✅ Validation passed")

        # Print summary
        if 'metadata' in data:
            metadata = data['metadata']
            print(f"   ID: {metadata.get('id', 'N/A')}")
            print(f"   Name: {metadata.get('name', 'N/A')}")
            print(f"   Category: {metadata.get('category', 'N/A')}")
            print(f"   Verified: {metadata.get('verified', False)}")

        return True

def validate_directory(dirpath):
    """Validate all JSON files in a directory."""
    json_files = list(Path(dirpath).rglob('*.json'))

    if not json_files:
        print(f"No JSON files found in {dirpath}")
        return True

    print(f"\nFound {len(json_files)} JSON file(s) to validate")

    results = []
    for filepath in json_files:
        # Skip schema.json
        if filepath.name == 'schema.json':
            continue
        results.append(validate_file(str(filepath)))

    # Summary
    passed = sum(results)
    total = len(results)
    print(f"\n{'='*60}")
    print(f"Validation Summary: {passed}/{total} files passed")
    print(f"{'='*60}")

    return all(results)

def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python validate.py <file_or_directory>")
        print("       python validate.py validate path/to/testdata.json")
        print("       python validate.py validate-dir path/to/directory")
        sys.exit(1)

    command = sys.argv[1] if len(sys.argv) > 2 else 'validate'
    target = sys.argv[2] if len(sys.argv) > 2 else sys.argv[1]

    if not os.path.exists(target):
        print(f"❌ Path not found: {target}")
        sys.exit(1)

    if command == 'validate' or os.path.isfile(target):
        success = validate_file(target)
    elif command == 'validate-dir' or os.path.isdir(target):
        success = validate_directory(target)
    else:
        print(f"❌ Unknown command: {command}")
        sys.exit(1)

    sys.exit(0 if success else 1)

if __name__ == '__main__':
    main()
