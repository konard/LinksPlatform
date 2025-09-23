# HTML to OpenAPI Converter

This tool provides a solution for **Issue #683**: Convert HTML to Markdown to convert it to openapi.yaml

## Overview

The HTML to OpenAPI converter automatically generates OpenAPI (Swagger) specifications from HTML API documentation. This simplifies access to API documentation by AI systems and enables better integration with API development tools.

## Features

- **HTML Parsing**: Extracts API endpoints from HTML documentation
- **Markdown Conversion**: Intermediate conversion to Markdown for readability
- **OpenAPI Generation**: Creates valid OpenAPI 3.0.0 YAML specifications
- **Parameter Detection**: Automatically identifies query, path, and header parameters
- **Type Inference**: Infers parameter types (string, integer, boolean) from descriptions
- **Clean Output**: Removes HTML artifacts and generates well-formatted YAML

## Files

- `improved_html_to_openapi.py` - Main converter tool (recommended)
- `simple_html_to_openapi.py` - Simplified version with no external dependencies
- `html_to_openapi_converter.py` - Advanced version with external dependencies
- `examples/sample_api_documentation.html` - Sample input file
- `examples/improved_output.yaml` - Sample output file
- `requirements.txt` - Python dependencies (for advanced version)

## Usage

### Basic Usage

```bash
python3 improved_html_to_openapi.py input.html output.yaml
```

### Example

```bash
python3 improved_html_to_openapi.py examples/sample_api_documentation.html my_api_spec.yaml
```

## Input HTML Format

The converter works best with HTML documentation that follows these patterns:

### Method and Path in Spans
```html
<h3><span class="method">GET</span> <span class="path">/api/users</span></h3>
```

### Method and Path in Headings
```html
<h3>GET /api/users</h3>
```

### Parameters in Divs
```html
<div class="parameter">
    <strong>id:</strong> User identifier (integer, required)
</div>
```

### Parameters in Markdown-style
```html
<strong>id:</strong> User identifier (integer, required)
```

## Output Format

The tool generates OpenAPI 3.0.0 YAML with:

- Automatic title and description extraction from HTML
- Path and operation definitions
- Parameter specifications with types and requirements
- Standard HTTP response codes (200, 400, 404, 500)
- JSON response schemas

## Integration with DocFX

This tool can be integrated into the existing DocFX documentation pipeline:

1. Generate HTML documentation using DocFX
2. Run the converter on the generated HTML files
3. Output OpenAPI specifications alongside the documentation

## Example Integration Script

```bash
#!/bin/bash
# Generate documentation
mono $(ls | grep "docfx.console.")/tools/docfx.exe docfx.json

# Convert HTML to OpenAPI
find doc/generated/site -name "*.html" -exec python3 improved_html_to_openapi.py {} {}.openapi.yaml \;
```

## Supported HTML Patterns

The converter recognizes:

- HTTP methods: GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS
- Path parameters in curly braces: `/users/{id}`
- Parameter types: integer, string, boolean, number
- Required/optional parameter indicators
- Summary and description sections
- Response format descriptions

## Limitations

- Requires structured HTML input with recognizable patterns
- Parameter extraction depends on consistent HTML formatting
- Complex nested schemas are simplified to basic object types
- Authentication and security schemes need manual addition

## Future Enhancements

- Support for more HTML documentation formats
- Better schema inference from examples
- Authentication scheme detection
- Integration with popular documentation generators
- Validation of generated OpenAPI specifications

## Contributing

When making improvements:

1. Test with various HTML documentation formats
2. Ensure generated YAML is valid OpenAPI 3.0.0
3. Add examples for new supported patterns
4. Update this documentation

## Related Issues

- Issue #683: Convert HTML to Markdown to convert it to openapi.yaml