# JSON Schema Inference Examples

This directory contains example JSON documents and test scripts for demonstrating the JSON schema inference functionality.

## Sample JSON Documents

- `sample1.json` - User document with basic fields
- `sample2.json` - User document with additional optional fields
- `sample3.json` - User document demonstrating common required fields

## Test Script

- `test-schema-inference.cs` - C# test program demonstrating various schema inference scenarios

## Usage Example

To infer a schema from the sample documents using the CLI:

```bash
# Build the project first
cd Platform
dotnet build

# Run schema inference on sample documents
dotnet run --project Platform.Examples -- infer ../examples/sample1.json ../examples/sample2.json ../examples/sample3.json -o ../examples/inferred-schema.json
```

## Expected Output

The inferred schema will identify:
- Common required fields (id, name, email, age, isActive, address, tags)
- Optional fields (phone, address.country)
- Proper types for all fields
- Nested object structures
- Array element types

Example inferred schema structure:
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": { "type": "integer" },
    "name": { "type": "string" },
    "email": { "type": "string" },
    "age": { "type": "integer" },
    "isActive": { "type": "boolean" },
    "address": {
      "type": "object",
      "properties": {
        "street": { "type": "string" },
        "city": { "type": "string" },
        "zipCode": { "type": "string" }
      },
      "required": ["street", "city", "zipCode"]
    },
    "tags": {
      "type": "array",
      "items": { "type": "string" }
    }
  },
  "required": ["id", "name", "email", "age", "isActive", "address", "tags"]
}
```
