# Algorithm Generation Through Type Transitions

## Overview

This implementation solves issue #614: "Algorithm generation as searching of path through type transitions".

The system allows automatic algorithm generation by modeling the problem as a graph search through type transformations, where:
- **Nodes** represent types (input types, intermediate types, output types)
- **Edges** represent operations/transformations that convert one type to another
- **Edge weights** represent costs (CPU and Memory usage)
- **Path finding** discovers the optimal sequence of operations to transform input types to output types

## Concept

Based on the diagram from issue #614, the system models algorithm generation as finding optimal paths through a directed graph of type transitions. Each transformation (operation) has associated costs:
- **CPU Cost**: Computational complexity
- **Memory Cost**: Memory usage

The algorithm finds the path with the minimum total cost from input type(s) to output type(s).

## Architecture

### Core Classes

1. **TypeNode** (`TypeNode.cs`)
   - Represents a type in the type system
   - Maintains outgoing transitions to other types

2. **TypeTransition** (`TypeTransition.cs`)
   - Represents an operation that transforms one type to another
   - Contains CPU and memory cost metrics

3. **TransformationPath** (`TransformationPath.cs`)
   - Represents a complete path from input to output type
   - Tracks total costs across all transitions

4. **AlgorithmGenerator** (`AlgorithmGenerator.cs`)
   - Main class for building the type transition graph
   - Implements Dijkstra's algorithm for finding optimal paths
   - Supports finding all possible paths with depth limiting

## Usage

### Basic Example

```csharp
using Platform.Sandbox.AlgorithmGeneration;

// Create the generator
var generator = new AlgorithmGenerator();

// Define transformations
generator.AddTransformation("DirectMap", "Tin1", "Tout1",
    cpuCost: 10, memoryCost: 140);
generator.AddTransformation("OR-Operation", "Tin1", "X",
    cpuCost: 2, memoryCost: 80);
generator.AddTransformation("X-Transform", "X", "Tout1",
    cpuCost: 9, memoryCost: 120);

// Find optimal path
var path = generator.FindOptimalPath("Tin1", "Tout1");
Console.WriteLine(path);
```

### Finding All Paths

```csharp
// Find all possible paths (limited by depth)
var allPaths = generator.FindAllPaths("Tin1", "Tout1", maxDepth: 5);

foreach (var path in allPaths)
{
    Console.WriteLine(path);
}
```

## Algorithm Details

### Optimal Path Finding

The system uses **Dijkstra's shortest path algorithm** to find the optimal (lowest cost) path:

1. Initialize all distances to infinity except the start node (distance = 0)
2. While there are unvisited nodes:
   - Select the unvisited node with minimum distance
   - For each neighbor, calculate alternative distance
   - Update if alternative distance is lower
3. Reconstruct path by backtracking through previous transitions

**Time Complexity**: O((V + E) log V) where V is the number of types and E is the number of transitions

### All Paths Finding

Uses **depth-first search with backtracking** to discover all possible paths:

1. Explore each transition from current node
2. Mark nodes as visited to avoid cycles
3. Recursively explore until target is reached or max depth exceeded
4. Backtrack and try alternative paths
5. Sort results by total cost

**Time Complexity**: O(V!) in worst case (exponential), limited by max depth parameter

## Example Scenarios

### 1. Type Transformation (from diagram #614)

Input types: `Tin1`, `Tin2`
Output types: `Tout1`, `Tout2`
Intermediate types: `X`, `Y`

The algorithm finds the optimal transformation sequence considering both direct and multi-step paths.

### 2. Data Structure Conversions

```csharp
generator.AddTransformation("ToArray", "List", "Array", 5, 50);
generator.AddTransformation("ToList", "Array", "List", 10, 60);
generator.AddTransformation("ToDict", "List", "Dictionary", 20, 100);
```

Finds optimal conversion path between data structures.

## Files

- `TypeNode.cs` - Type node representation
- `TypeTransition.cs` - Transformation/operation definition
- `TransformationPath.cs` - Path representation with costs
- `AlgorithmGenerator.cs` - Main algorithm implementation
- `AlgorithmGenerationExperiment.cs` - Demonstration examples

## Running the Examples

Execute the Platform.Sandbox project:

```bash
cd Platform
dotnet run --project Platform.Sandbox
```

The program will:
1. Run basic examples from the issue #614 diagram
2. Run complex examples with multiple paths
3. Run data structure transformation examples

## Applications

This algorithm generation approach can be applied to:

- **Compiler optimization**: Finding optimal instruction sequences
- **Data transformation**: Converting between data formats
- **API composition**: Chaining API calls to achieve a goal
- **Type coercion**: Automatic type conversion in programming languages
- **Workflow generation**: Creating optimal sequences of operations

## Future Enhancements

Potential improvements:
- Support for parallel transformations (multiple inputs → multiple outputs)
- Heuristic search (A*) for larger graphs
- Dynamic cost estimation based on input size
- Caching of frequently used paths
- Support for conditional transformations
- Integration with actual type systems

## References

- Issue #614: https://github.com/konard/LinksPlatform/issues/614
- Dijkstra's Algorithm: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
- Graph Search Algorithms: https://en.wikipedia.org/wiki/Graph_traversal
