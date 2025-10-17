# Graph Package Manager Examples

This directory contains examples demonstrating the concept of graph-based package management with circular dependencies.

## Example 1: UI Framework and Layout Engine

A common real-world scenario where circular dependencies are natural:

- **ui-framework**: Provides UI components that need layout
- **layout-engine**: Provides layout calculations that render UI

See the subdirectories for implementation examples.

## Example 2: Parser and AST

Another common pattern:

- **parser**: Parses source code into AST
- **ast**: AST nodes that can contain embedded parsers

## Running Examples

Each example includes:
- `package-graph.json`: Dependency specification
- `build-order.txt`: Compilation stages
- `resolution.txt`: How versions are resolved

These are conceptual examples to illustrate how graph-based package management works.
