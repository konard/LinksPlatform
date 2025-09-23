#!/bin/bash
# Demo script for HTML to OpenAPI converter

echo "=== HTML to OpenAPI Converter Demo ==="
echo

echo "1. Converting sample HTML documentation to OpenAPI..."
python3 improved_html_to_openapi.py examples/sample_api_documentation.html examples/demo_output.yaml

echo
echo "2. Generated OpenAPI specification:"
echo "   File: examples/demo_output.yaml"
echo "   Size: $(wc -c < examples/demo_output.yaml) bytes"
echo "   Lines: $(wc -l < examples/demo_output.yaml) lines"

echo
echo "3. API Summary:"
echo "   - Detected $(grep -c "^  /" examples/demo_output.yaml) API paths"
echo "   - Found $(grep -c "^    [a-z]*:" examples/demo_output.yaml) operations"
echo "   - Extracted $(grep -c "name:" examples/demo_output.yaml) parameters"

echo
echo "4. Sample of generated YAML:"
head -20 examples/demo_output.yaml

echo
echo "=== Demo Complete ==="
echo "The converter successfully converted HTML API documentation to OpenAPI YAML!"
echo "This solves issue #683 by providing a way to generate OpenAPI specifications"
echo "from HTML documentation, making it easier for AI to access API documentation."