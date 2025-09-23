#!/usr/bin/env python3
"""
Improved HTML to OpenAPI Converter

This tool converts HTML API documentation to OpenAPI (Swagger) YAML specification.
It handles common HTML documentation patterns and produces clean, valid OpenAPI specs.

Usage:
    python3 improved_html_to_openapi.py input.html output.yaml
"""

import sys
import re
import json
from html.parser import HTMLParser
from pathlib import Path
import argparse


class ImprovedHTMLToOpenAPIConverter:
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

    def clean_html_content(self, html_content):
        """Clean and normalize HTML content"""
        # Remove script and style elements
        html_content = re.sub(r'<script[^>]*>.*?</script>', '', html_content, flags=re.DOTALL | re.IGNORECASE)
        html_content = re.sub(r'<style[^>]*>.*?</style>', '', html_content, flags=re.DOTALL | re.IGNORECASE)

        # Remove HTML comments
        html_content = re.sub(r'<!--.*?-->', '', html_content, flags=re.DOTALL)

        return html_content

    def extract_api_info_from_html(self, html_content):
        """Extract API information directly from HTML"""
        # Extract title
        title_match = re.search(r'<title[^>]*>(.*?)</title>', html_content, re.IGNORECASE | re.DOTALL)
        title = title_match.group(1).strip() if title_match else "Generated API"
        title = re.sub(r'<[^>]+>', '', title)  # Remove any HTML tags

        # Extract description from meta or first paragraph
        desc_match = re.search(r'<meta[^>]*name=["\']description["\'][^>]*content=["\']([^"\']*)["\'][^>]*>', html_content, re.IGNORECASE)
        if not desc_match:
            # Look for first meaningful paragraph
            paragraphs = re.findall(r'<p[^>]*>(.*?)</p>', html_content, re.IGNORECASE | re.DOTALL)
            for p in paragraphs:
                clean_p = re.sub(r'<[^>]+>', '', p).strip()
                if len(clean_p) > 20:  # Must be meaningful
                    desc_match = type('obj', (object,), {'group': lambda x, n: clean_p})()
                    break

        description = desc_match.group(1).strip() if desc_match else "API documentation converted from HTML"
        description = re.sub(r'<[^>]+>', '', description)  # Clean HTML tags

        return title, description

    def extract_endpoints_from_html(self, html_content):
        """Extract API endpoints from HTML content with improved parsing"""
        endpoints = []
        seen_endpoints = set()  # Track to avoid duplicates

        # Pattern 1: <span class="method">GET</span> <span class="path">/api/path</span>
        method_path_pattern = r'<span[^>]*class=["\']method["\'][^>]*>(\w+)</span>\s*<span[^>]*class=["\']path["\'][^>]*>([^<]+)</span>'
        matches = re.finditer(method_path_pattern, html_content, re.IGNORECASE)

        for match in matches:
            method = match.group(1).lower()
            path = match.group(2).strip()

            endpoint_key = f"{method}:{path}"
            if endpoint_key in seen_endpoints:
                continue
            seen_endpoints.add(endpoint_key)

            # Extract the surrounding content for description
            start_pos = max(0, match.start() - 2000)
            end_pos = min(len(html_content), match.end() + 3000)
            section_content = html_content[start_pos:end_pos]

            # Extract summary and description from surrounding content
            summary, description = self.extract_endpoint_description(section_content, method, path)
            parameters = self.extract_parameters_improved(section_content)

            endpoint = {
                'method': method,
                'path': path,
                'summary': summary,
                'description': description,
                'parameters': parameters,
                'responses': self.generate_default_responses()
            }

            endpoints.append(endpoint)

        # Pattern 2: Look for headings with HTTP methods like "### GET /api/path"
        heading_pattern = r'<h[3-6][^>]*>.*?(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)\s+(/[^\s<]+).*?</h[3-6]>'
        heading_matches = re.finditer(heading_pattern, html_content, re.IGNORECASE)

        for match in heading_matches:
            method = match.group(1).lower()
            path = match.group(2).strip()

            endpoint_key = f"{method}:{path}"
            if endpoint_key in seen_endpoints:
                continue
            seen_endpoints.add(endpoint_key)

            start_pos = max(0, match.start())
            end_pos = min(len(html_content), match.end() + 3000)
            section_content = html_content[start_pos:end_pos]

            summary, description = self.extract_endpoint_description(section_content, method, path)
            parameters = self.extract_parameters_improved(section_content)

            endpoint = {
                'method': method,
                'path': path,
                'summary': summary,
                'description': description,
                'parameters': parameters,
                'responses': self.generate_default_responses()
            }

            endpoints.append(endpoint)

        return endpoints

    def extract_endpoint_description(self, section_content, method, path):
        """Extract summary and description for an endpoint with better parsing"""
        # Clean section content for analysis
        clean_text = re.sub(r'<[^>]+>', ' ', section_content)
        clean_text = re.sub(r'\s+', ' ', clean_text).strip()

        # Look for summary patterns
        summary_patterns = [
            r'Summary:\s*([^\n\r]+)',
            r'<strong[^>]*>Summary:</strong>\s*([^<\n\r]+)',
            r'<h[4-6][^>]*>([^<]+)</h[4-6]>',
        ]

        summary = f"{method.upper()} {path}"
        for pattern in summary_patterns:
            match = re.search(pattern, section_content, re.IGNORECASE)
            if match:
                candidate_summary = re.sub(r'<[^>]+>', '', match.group(1)).strip()
                if len(candidate_summary) > 5 and len(candidate_summary) < 100:
                    summary = candidate_summary
                    break

        # Look for description patterns
        desc_patterns = [
            r'Description:\s*([^\n\r]+)',
            r'<strong[^>]*>Description:</strong>\s*([^<\n\r]+)',
            r'<p[^>]*>([^<]+)</p>'
        ]

        description = f"Endpoint for {path}"
        for pattern in desc_patterns:
            match = re.search(pattern, section_content, re.IGNORECASE)
            if match:
                candidate_desc = re.sub(r'<[^>]+>', '', match.group(1)).strip()
                if len(candidate_desc) > 10 and candidate_desc != summary:
                    description = candidate_desc
                    break

        return summary, description

    def extract_parameters_improved(self, section_content):
        """Extract parameter information with better deduplication"""
        parameters = []
        seen_params = set()

        # Pattern 1: <div class="parameter"><strong>name:</strong> description</div>
        param_pattern1 = r'<div[^>]*class=["\']parameter["\'][^>]*>.*?<strong[^>]*>(\w+):</strong>\s*([^<]+).*?</div>'
        matches1 = re.finditer(param_pattern1, section_content, re.IGNORECASE | re.DOTALL)

        for match in matches1:
            param_name = match.group(1).strip()
            param_desc = match.group(2).strip()

            if param_name in seen_params:
                continue
            seen_params.add(param_name)

            param_type, param_in, required = self.parse_parameter_details(param_desc)

            parameter = {
                'name': param_name,
                'in': param_in,
                'description': param_desc,
                'required': required,
                'schema': {'type': param_type}
            }
            parameters.append(parameter)

        # Pattern 2: **name:** description (in markdown-like format)
        param_pattern2 = r'\*\*(\w+):\*\*\s*([^\n\r*]+)'
        matches2 = re.finditer(param_pattern2, section_content, re.IGNORECASE)

        for match in matches2:
            param_name = match.group(1).strip()
            param_desc = match.group(2).strip()

            if param_name in seen_params:
                continue
            seen_params.add(param_name)

            # Skip if this looks like a section header rather than a parameter
            if param_name.lower() in ['summary', 'description', 'response', 'request', 'body', 'parameters']:
                continue

            param_type, param_in, required = self.parse_parameter_details(param_desc)

            parameter = {
                'name': param_name,
                'in': param_in,
                'description': param_desc,
                'required': required,
                'schema': {'type': param_type}
            }
            parameters.append(parameter)

        return parameters

    def parse_parameter_details(self, param_desc):
        """Parse parameter description to extract type, location, and required status"""
        param_desc_lower = param_desc.lower()

        # Determine type
        param_type = 'string'
        if 'integer' in param_desc_lower or 'int' in param_desc_lower:
            param_type = 'integer'
        elif 'boolean' in param_desc_lower or 'bool' in param_desc_lower:
            param_type = 'boolean'
        elif 'number' in param_desc_lower or 'float' in param_desc_lower:
            param_type = 'number'

        # Determine location
        param_in = 'query'
        if 'path' in param_desc_lower or 'url' in param_desc_lower:
            param_in = 'path'
        elif 'header' in param_desc_lower:
            param_in = 'header'
        elif 'body' in param_desc_lower:
            param_in = 'query'  # Default to query for body params in this simple converter

        # Determine if required
        required = 'required' in param_desc_lower
        if param_in == 'path':  # Path parameters are always required
            required = True

        return param_type, param_in, required

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
                                'data': {
                                    'type': 'object',
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
        """Convert dictionary to YAML format with proper formatting"""
        yaml_lines = []
        spaces = "  " * indent

        if isinstance(data, dict):
            for key, value in data.items():
                if isinstance(value, (dict, list)) and value:
                    yaml_lines.append(f"{spaces}{key}:")
                    yaml_lines.append(self.dict_to_yaml(value, indent + 1))
                elif isinstance(value, list) and not value:
                    yaml_lines.append(f"{spaces}{key}: []")
                elif isinstance(value, dict) and not value:
                    yaml_lines.append(f"{spaces}{key}: {{}}")
                else:
                    if isinstance(value, str) and ('\n' in value or value.count(':') > 1):
                        # Multi-line string
                        yaml_lines.append(f"{spaces}{key}: |")
                        for line in str(value).split('\n'):
                            yaml_lines.append(f"{spaces}  {line}")
                    else:
                        # Handle boolean values
                        if isinstance(value, bool):
                            value = str(value).lower()
                        yaml_lines.append(f"{spaces}{key}: {value}")

        elif isinstance(data, list):
            for item in data:
                if isinstance(item, (dict, list)):
                    yaml_lines.append(f"{spaces}- ")
                    sub_yaml = self.dict_to_yaml(item, indent + 1)
                    # Remove the first spaces from the first line since we already added the dash
                    lines = sub_yaml.split('\n')
                    if lines:
                        yaml_lines.append(lines[0][2:])  # Remove first 2 spaces
                        yaml_lines.extend(lines[1:])
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

        # Clean HTML content
        html_content = self.clean_html_content(html_content)

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

        # Save to file if output path provided
        if output_path:
            yaml_content = self.dict_to_yaml(openapi_spec)
            with open(output_path, 'w') as f:
                f.write(yaml_content)

        return openapi_spec

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

    args = parser.parse_args()

    if not Path(args.input_html).exists():
        print(f"Error: Input file {args.input_html} does not exist")
        sys.exit(1)

    converter = ImprovedHTMLToOpenAPIConverter()

    try:
        openapi_spec = converter.convert_html_file_to_openapi(
            args.input_html,
            args.output_yaml
        )

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