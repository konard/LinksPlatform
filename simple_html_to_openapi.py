#!/usr/bin/env python3
"""
Simple HTML to OpenAPI Converter (No external dependencies)

This tool converts HTML API documentation to OpenAPI (Swagger) YAML specification
using only Python standard library.

Usage:
    python3 simple_html_to_openapi.py input.html output.yaml
"""

import sys
import re
import json
from html.parser import HTMLParser
from pathlib import Path
import argparse


class HTMLToMarkdownParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.markdown_content = []
        self.current_text = ""
        self.in_heading = False
        self.heading_level = 0
        self.in_paragraph = False
        self.in_code = False
        self.in_strong = False
        self.in_em = False

    def handle_starttag(self, tag, attrs):
        if tag in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
            self.heading_level = int(tag[1])
            self.in_heading = True
            self.markdown_content.append('\n' + '#' * self.heading_level + ' ')
        elif tag == 'p':
            self.in_paragraph = True
            self.markdown_content.append('\n\n')
        elif tag == 'code':
            self.in_code = True
            self.markdown_content.append('`')
        elif tag == 'strong' or tag == 'b':
            self.in_strong = True
            self.markdown_content.append('**')
        elif tag == 'em' or tag == 'i':
            self.in_em = True
            self.markdown_content.append('*')
        elif tag == 'br':
            self.markdown_content.append('\n')
        elif tag == 'ul':
            self.markdown_content.append('\n')
        elif tag == 'li':
            self.markdown_content.append('\n- ')

    def handle_endtag(self, tag):
        if tag in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
            self.in_heading = False
            self.markdown_content.append('\n')
        elif tag == 'p':
            self.in_paragraph = False
        elif tag == 'code':
            self.in_code = False
            self.markdown_content.append('`')
        elif tag == 'strong' or tag == 'b':
            self.in_strong = False
            self.markdown_content.append('**')
        elif tag == 'em' or tag == 'i':
            self.in_em = False
            self.markdown_content.append('*')

    def handle_data(self, data):
        # Clean up whitespace
        data = data.strip()
        if data:
            self.markdown_content.append(data)

    def get_markdown(self):
        return ''.join(self.markdown_content)


class SimpleHTMLToOpenAPIConverter:
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
        """Convert HTML content to Markdown using built-in parser"""
        # Simple cleanup
        html_content = re.sub(r'<script[^>]*>.*?</script>', '', html_content, flags=re.DOTALL)
        html_content = re.sub(r'<style[^>]*>.*?</style>', '', html_content, flags=re.DOTALL)

        parser = HTMLToMarkdownParser()
        parser.feed(html_content)
        return parser.get_markdown()

    def extract_api_info_from_html(self, html_content):
        """Extract API information directly from HTML"""
        # Extract title
        title_match = re.search(r'<title[^>]*>(.*?)</title>', html_content, re.IGNORECASE | re.DOTALL)
        title = title_match.group(1).strip() if title_match else "Generated API"

        # Extract description from meta or first paragraph
        desc_match = re.search(r'<meta[^>]*name=["\']description["\'][^>]*content=["\']([^"\']*)["\'][^>]*>', html_content, re.IGNORECASE)
        if not desc_match:
            desc_match = re.search(r'<p[^>]*>(.*?)</p>', html_content, re.IGNORECASE | re.DOTALL)

        description = desc_match.group(1).strip() if desc_match else "API documentation converted from HTML"
        # Clean HTML tags from description
        description = re.sub(r'<[^>]+>', '', description)

        return title, description

    def extract_endpoints_from_html(self, html_content):
        """Extract API endpoints from HTML content"""
        endpoints = []

        # Look for HTTP method patterns in the HTML
        # Pattern 1: <span class="method">GET</span> <span class="path">/api/path</span>
        method_path_pattern = r'<span[^>]*class=["\']method["\'][^>]*>(\w+)</span>\s*<span[^>]*class=["\']path["\'][^>]*>([^<]+)</span>'
        matches = re.finditer(method_path_pattern, html_content, re.IGNORECASE)

        current_section = ""
        for match in matches:
            method = match.group(1).lower()
            path = match.group(2).strip()

            # Extract the surrounding content for description
            start_pos = max(0, match.start() - 1000)
            end_pos = min(len(html_content), match.end() + 2000)
            section_content = html_content[start_pos:end_pos]

            # Extract summary and description from surrounding content
            summary, description = self.extract_endpoint_description(section_content, method, path)

            endpoint = {
                'method': method,
                'path': path,
                'summary': summary,
                'description': description,
                'parameters': self.extract_parameters(section_content),
                'responses': self.generate_default_responses()
            }

            endpoints.append(endpoint)

        # Pattern 2: Look for text like "GET /api/path" in headings or text
        text_pattern = r'(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)\s+(/[^\s<]+)'
        text_matches = re.finditer(text_pattern, html_content, re.IGNORECASE)

        for match in text_matches:
            method = match.group(1).lower()
            path = match.group(2).strip()

            # Skip if we already have this endpoint
            if any(ep['method'] == method and ep['path'] == path for ep in endpoints):
                continue

            start_pos = max(0, match.start() - 1000)
            end_pos = min(len(html_content), match.end() + 2000)
            section_content = html_content[start_pos:end_pos]

            summary, description = self.extract_endpoint_description(section_content, method, path)

            endpoint = {
                'method': method,
                'path': path,
                'summary': summary,
                'description': description,
                'parameters': self.extract_parameters(section_content),
                'responses': self.generate_default_responses()
            }

            endpoints.append(endpoint)

        return endpoints

    def extract_endpoint_description(self, section_content, method, path):
        """Extract summary and description for an endpoint"""
        # Clean HTML tags for text analysis
        clean_text = re.sub(r'<[^>]+>', ' ', section_content)

        # Look for summary/description patterns
        summary_patterns = [
            r'<strong>Summary:</strong>\s*([^<\n]+)',
            r'Summary:\s*([^\n]+)',
            r'<h[3-6][^>]*>([^<]+)</h[3-6]>'
        ]

        summary = f"{method.upper()} {path}"
        for pattern in summary_patterns:
            match = re.search(pattern, section_content, re.IGNORECASE)
            if match:
                summary = match.group(1).strip()
                break

        desc_patterns = [
            r'<strong>Description:</strong>\s*([^<\n]+)',
            r'Description:\s*([^\n]+)',
            r'<p[^>]*>([^<]+)</p>'
        ]

        description = f"Endpoint for {path}"
        for pattern in desc_patterns:
            match = re.search(pattern, section_content, re.IGNORECASE)
            if match:
                desc_text = match.group(1).strip()
                if len(desc_text) > 10 and desc_text != summary:  # Avoid duplicates
                    description = desc_text
                    break

        return summary, description

    def extract_parameters(self, section_content):
        """Extract parameter information from section content"""
        parameters = []

        # Look for parameter patterns
        param_patterns = [
            r'<div[^>]*class=["\']parameter["\'][^>]*>[\s\S]*?<strong>(\w+):</strong>\s*([^<]+)',
            r'(\w+):\s*([^\n]+(?:integer|string|boolean|required|optional)[^\n]*)'
        ]

        for pattern in param_patterns:
            matches = re.finditer(pattern, section_content, re.IGNORECASE)
            for match in matches:
                param_name = match.group(1).strip()
                param_desc = match.group(2).strip()

                param_type = 'string'
                param_in = 'query'
                required = False

                if 'integer' in param_desc.lower():
                    param_type = 'integer'
                elif 'boolean' in param_desc.lower():
                    param_type = 'boolean'

                if 'required' in param_desc.lower():
                    required = True

                if 'path' in param_desc.lower() or '{' in param_name:
                    param_in = 'path'
                    required = True

                parameter = {
                    'name': param_name,
                    'in': param_in,
                    'description': param_desc,
                    'required': required,
                    'schema': {'type': param_type}
                }

                parameters.append(parameter)

        return parameters

    def generate_default_responses(self):
        """Generate default response structure"""
        return {
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
            },
            '400': {
                'description': 'Bad Request'
            },
            '404': {
                'description': 'Not Found'
            },
            '500': {
                'description': 'Internal Server Error'
            }
        }

    def generate_openapi_from_endpoints(self, endpoints, title, description):
        """Generate OpenAPI specification from extracted endpoints"""
        self.api_info['info']['title'] = title
        self.api_info['info']['description'] = description

        paths = {}

        for endpoint in endpoints:
            path = endpoint['path']
            method = endpoint['method']

            if path not in paths:
                paths[path] = {}

            operation = {
                'summary': endpoint['summary'],
                'description': endpoint['description'],
                'responses': endpoint['responses']
            }

            if endpoint['parameters']:
                operation['parameters'] = endpoint['parameters']

            paths[path][method] = operation

        self.api_info['paths'] = paths
        return self.api_info

    def dict_to_yaml(self, data, indent=0):
        """Convert dictionary to YAML format (simple implementation)"""
        yaml_lines = []
        spaces = "  " * indent

        if isinstance(data, dict):
            for key, value in data.items():
                if isinstance(value, (dict, list)):
                    yaml_lines.append(f"{spaces}{key}:")
                    yaml_lines.append(self.dict_to_yaml(value, indent + 1))
                else:
                    if isinstance(value, str) and ('\n' in value or ':' in value or value.startswith(' ')):
                        # Multi-line or special characters
                        yaml_lines.append(f"{spaces}{key}: |")
                        for line in str(value).split('\n'):
                            yaml_lines.append(f"{spaces}  {line}")
                    else:
                        yaml_lines.append(f"{spaces}{key}: {value}")

        elif isinstance(data, list):
            for item in data:
                if isinstance(item, (dict, list)):
                    yaml_lines.append(f"{spaces}-")
                    yaml_lines.append(self.dict_to_yaml(item, indent + 1))
                else:
                    yaml_lines.append(f"{spaces}- {item}")

        return '\n'.join(yaml_lines)

    def convert_html_file_to_openapi(self, html_file_path, output_path=None):
        """Convert HTML file to OpenAPI YAML"""
        try:
            with open(html_file_path, 'r', encoding='utf-8') as f:
                html_content = f.read()
        except UnicodeDecodeError:
            with open(html_file_path, 'r', encoding='latin1') as f:
                html_content = f.read()

        # Extract basic API info
        title, description = self.extract_api_info_from_html(html_content)

        # Extract endpoints
        endpoints = self.extract_endpoints_from_html(html_content)

        # Generate OpenAPI specification
        if endpoints:
            openapi_spec = self.generate_openapi_from_endpoints(endpoints, title, description)
        else:
            # Generate basic spec if no endpoints found
            openapi_spec = self.generate_basic_openapi(title, description)

        # Convert to markdown for reference
        markdown_content = self.html_to_markdown(html_content)

        # Save to file if output path provided
        if output_path:
            yaml_content = self.dict_to_yaml(openapi_spec)
            with open(output_path, 'w') as f:
                f.write(yaml_content)

        return openapi_spec, markdown_content

    def generate_basic_openapi(self, title, description):
        """Generate basic OpenAPI spec when no endpoints are found"""
        return {
            "openapi": "3.0.0",
            "info": {
                "title": title,
                "version": "1.0.0",
                "description": description
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


def main():
    parser = argparse.ArgumentParser(description='Convert HTML documentation to OpenAPI YAML')
    parser.add_argument('input_html', help='Input HTML file path')
    parser.add_argument('output_yaml', help='Output OpenAPI YAML file path')
    parser.add_argument('--markdown-output', help='Optional: Save intermediate markdown to file')

    args = parser.parse_args()

    if not Path(args.input_html).exists():
        print(f"Error: Input file {args.input_html} does not exist")
        sys.exit(1)

    converter = SimpleHTMLToOpenAPIConverter()

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

        # Show some stats
        total_operations = sum(len(methods) for methods in openapi_spec.get('paths', {}).values())
        print(f"Total operations: {total_operations}")

    except Exception as e:
        print(f"Error during conversion: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()