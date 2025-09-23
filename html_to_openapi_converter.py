#!/usr/bin/env python3
"""
HTML to OpenAPI Converter

This tool converts HTML API documentation to OpenAPI (Swagger) YAML specification
through an intermediate Markdown conversion step.

The workflow is:
1. Parse HTML documentation
2. Convert to structured Markdown
3. Extract API information from Markdown
4. Generate OpenAPI YAML specification

Usage:
    python html_to_openapi_converter.py input.html output.yaml
"""

import sys
import re
import yaml
from pathlib import Path
from bs4 import BeautifulSoup
from markdownify import markdownify
import argparse
import json


class HTMLToOpenAPIConverter:
    def __init__(self):
        self.api_info = {
            "openapi": "3.0.0",
            "info": {
                "title": "Generated API",
                "version": "1.0.0",
                "description": "API documentation generated from HTML"
            },
            "paths": {},
            "components": {
                "schemas": {}
            }
        }

    def html_to_markdown(self, html_content):
        """Convert HTML content to Markdown"""
        soup = BeautifulSoup(html_content, 'html.parser')

        # Remove script and style elements
        for script in soup(["script", "style"]):
            script.decompose()

        # Convert to markdown
        markdown = markdownify(str(soup), heading_style="ATX")
        return markdown

    def extract_api_endpoints_from_markdown(self, markdown_content):
        """Extract API endpoint information from markdown content"""
        endpoints = []
        lines = markdown_content.split('\n')

        current_endpoint = None
        in_code_block = False

        for line in lines:
            line = line.strip()

            # Toggle code block state
            if line.startswith('```'):
                in_code_block = not in_code_block
                continue

            # Skip content inside code blocks
            if in_code_block:
                continue

            # Look for HTTP methods and paths
            http_method_pattern = r'^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)\s+(.+)$'
            match = re.match(http_method_pattern, line, re.IGNORECASE)

            if match:
                method = match.group(1).lower()
                path = match.group(2).strip()

                # Clean path - remove any extra formatting
                path = re.sub(r'[*_`]', '', path)
                path = path.split()[0] if ' ' in path else path

                if not path.startswith('/'):
                    path = '/' + path

                current_endpoint = {
                    'method': method,
                    'path': path,
                    'summary': '',
                    'description': '',
                    'parameters': [],
                    'responses': {}
                }
                endpoints.append(current_endpoint)

            # Look for endpoint descriptions
            elif current_endpoint and line and not line.startswith('#'):
                if not current_endpoint['summary']:
                    current_endpoint['summary'] = line
                elif not current_endpoint['description']:
                    current_endpoint['description'] = line

            # Look for parameter descriptions
            elif current_endpoint and 'parameter' in line.lower():
                param_match = re.search(r'(\w+)\s*:\s*(.+)', line)
                if param_match:
                    param_name = param_match.group(1)
                    param_desc = param_match.group(2)
                    current_endpoint['parameters'].append({
                        'name': param_name,
                        'in': 'query',
                        'description': param_desc,
                        'schema': {'type': 'string'}
                    })

        return endpoints

    def generate_openapi_from_endpoints(self, endpoints):
        """Generate OpenAPI specification from extracted endpoints"""
        paths = {}

        for endpoint in endpoints:
            path = endpoint['path']
            method = endpoint['method']

            if path not in paths:
                paths[path] = {}

            operation = {
                'summary': endpoint['summary'] or f"{method.upper()} {path}",
                'description': endpoint['description'] or f"Endpoint for {path}",
                'responses': {
                    '200': {
                        'description': 'Successful response',
                        'content': {
                            'application/json': {
                                'schema': {
                                    'type': 'object',
                                    'properties': {
                                        'result': {
                                            'type': 'string',
                                            'description': 'Response data'
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if endpoint['parameters']:
                operation['parameters'] = endpoint['parameters']

            paths[path][method] = operation

        self.api_info['paths'] = paths
        return self.api_info

    def convert_html_file_to_openapi(self, html_file_path, output_path=None):
        """Convert HTML file to OpenAPI YAML"""
        try:
            with open(html_file_path, 'r', encoding='utf-8') as f:
                html_content = f.read()
        except UnicodeDecodeError:
            with open(html_file_path, 'r', encoding='latin1') as f:
                html_content = f.read()

        # Convert HTML to Markdown
        markdown_content = self.html_to_markdown(html_content)

        # Extract API endpoints
        endpoints = self.extract_api_endpoints_from_markdown(markdown_content)

        # Generate OpenAPI specification
        openapi_spec = self.generate_openapi_from_endpoints(endpoints)

        # If no endpoints found, create a basic spec based on HTML structure
        if not endpoints:
            openapi_spec = self.generate_basic_openapi_from_html(html_content)

        # Save to file if output path provided
        if output_path:
            with open(output_path, 'w') as f:
                yaml.dump(openapi_spec, f, default_flow_style=False, sort_keys=False)

        return openapi_spec, markdown_content

    def generate_basic_openapi_from_html(self, html_content):
        """Generate basic OpenAPI spec when no clear endpoints are found"""
        soup = BeautifulSoup(html_content, 'html.parser')

        # Try to extract title
        title_elem = soup.find('title') or soup.find('h1')
        title = title_elem.get_text().strip() if title_elem else "Generated API"

        # Try to extract description
        desc_elem = soup.find('meta', attrs={'name': 'description'}) or soup.find('p')
        description = ""
        if desc_elem:
            description = desc_elem.get('content') if desc_elem.name == 'meta' else desc_elem.get_text().strip()

        basic_spec = {
            "openapi": "3.0.0",
            "info": {
                "title": title,
                "version": "1.0.0",
                "description": description or "API documentation converted from HTML"
            },
            "paths": {
                "/api/example": {
                    "get": {
                        "summary": "Example endpoint",
                        "description": "This is an example endpoint. Please update with actual API endpoints.",
                        "responses": {
                            "200": {
                                "description": "Successful response",
                                "content": {
                                    "application/json": {
                                        "schema": {
                                            "type": "object",
                                            "properties": {
                                                "message": {
                                                    "type": "string",
                                                    "example": "Hello, World!"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return basic_spec


def main():
    parser = argparse.ArgumentParser(description='Convert HTML documentation to OpenAPI YAML')
    parser.add_argument('input_html', help='Input HTML file path')
    parser.add_argument('output_yaml', help='Output OpenAPI YAML file path')
    parser.add_argument('--markdown-output', help='Optional: Save intermediate markdown to file')

    args = parser.parse_args()

    if not Path(args.input_html).exists():
        print(f"Error: Input file {args.input_html} does not exist")
        sys.exit(1)

    converter = HTMLToOpenAPIConverter()

    try:
        openapi_spec, markdown_content = converter.convert_html_file_to_openapi(
            args.input_html,
            args.output_yaml
        )

        # Save markdown if requested
        if args.markdown_output:
            with open(args.markdown_output, 'w') as f:
                f.write(markdown_content)
            print(f"Intermediate markdown saved to: {args.markdown_output}")

        print(f"OpenAPI specification generated: {args.output_yaml}")
        print(f"Found {len(openapi_spec.get('paths', {}))} API paths")

    except Exception as e:
        print(f"Error during conversion: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()