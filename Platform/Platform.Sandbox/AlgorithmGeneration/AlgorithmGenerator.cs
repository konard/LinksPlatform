using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Sandbox.AlgorithmGeneration
{
    /// <summary>
    /// Algorithm generator that searches for optimal paths through type transitions
    /// Based on the concept of searching paths through type transformation graph
    /// </summary>
    public class AlgorithmGenerator
    {
        private Dictionary<string, TypeNode> _typeNodes;

        public AlgorithmGenerator()
        {
            _typeNodes = new Dictionary<string, TypeNode>();
        }

        /// <summary>
        /// Gets or creates a type node by name
        /// </summary>
        public TypeNode GetOrCreateType(string typeName)
        {
            if (!_typeNodes.ContainsKey(typeName))
            {
                _typeNodes[typeName] = new TypeNode(typeName);
            }
            return _typeNodes[typeName];
        }

        /// <summary>
        /// Adds a type transformation/operation to the graph
        /// </summary>
        public void AddTransformation(string operationName, string sourceTypeName, string targetTypeName, double cpuCost, double memoryCost)
        {
            var sourceType = GetOrCreateType(sourceTypeName);
            var targetType = GetOrCreateType(targetTypeName);

            var transition = new TypeTransition(operationName, sourceType, targetType, cpuCost, memoryCost);
            sourceType.OutgoingTransitions.Add(transition);
        }

        /// <summary>
        /// Finds the optimal path from source type to target type using Dijkstra's algorithm
        /// </summary>
        public TransformationPath FindOptimalPath(string sourceTypeName, string targetTypeName)
        {
            if (!_typeNodes.ContainsKey(sourceTypeName))
                throw new ArgumentException($"Source type '{sourceTypeName}' not found");

            if (!_typeNodes.ContainsKey(targetTypeName))
                throw new ArgumentException($"Target type '{targetTypeName}' not found");

            var startNode = _typeNodes[sourceTypeName];
            var endNode = _typeNodes[targetTypeName];

            // Dijkstra's algorithm implementation
            var distances = new Dictionary<TypeNode, double>();
            var previousTransitions = new Dictionary<TypeNode, TypeTransition>();
            var unvisited = new HashSet<TypeNode>(_typeNodes.Values);

            // Initialize distances
            foreach (var node in _typeNodes.Values)
            {
                distances[node] = double.MaxValue;
            }
            distances[startNode] = 0;

            while (unvisited.Count > 0)
            {
                // Find unvisited node with minimum distance
                var current = unvisited.OrderBy(n => distances[n]).First();

                if (distances[current] == double.MaxValue)
                    break; // No path exists

                if (current.Equals(endNode))
                    break; // Reached destination

                unvisited.Remove(current);

                // Check all neighbors
                foreach (var transition in current.OutgoingTransitions)
                {
                    if (!unvisited.Contains(transition.TargetType))
                        continue;

                    var altDistance = distances[current] + transition.TotalCost;

                    if (altDistance < distances[transition.TargetType])
                    {
                        distances[transition.TargetType] = altDistance;
                        previousTransitions[transition.TargetType] = transition;
                    }
                }
            }

            // Reconstruct path
            if (!previousTransitions.ContainsKey(endNode))
                return null; // No path found

            var path = new TransformationPath();
            var currentNode = endNode;
            var pathTransitions = new Stack<TypeTransition>();

            while (previousTransitions.ContainsKey(currentNode))
            {
                var transition = previousTransitions[currentNode];
                pathTransitions.Push(transition);
                currentNode = transition.SourceType;
            }

            foreach (var transition in pathTransitions)
            {
                path.AddTransition(transition);
            }

            return path;
        }

        /// <summary>
        /// Finds all possible paths from source to target type (limited by max depth to avoid infinite loops)
        /// </summary>
        public List<TransformationPath> FindAllPaths(string sourceTypeName, string targetTypeName, int maxDepth = 10)
        {
            if (!_typeNodes.ContainsKey(sourceTypeName))
                throw new ArgumentException($"Source type '{sourceTypeName}' not found");

            if (!_typeNodes.ContainsKey(targetTypeName))
                throw new ArgumentException($"Target type '{targetTypeName}' not found");

            var startNode = _typeNodes[sourceTypeName];
            var endNode = _typeNodes[targetTypeName];

            var allPaths = new List<TransformationPath>();
            var currentPath = new TransformationPath();
            var visited = new HashSet<TypeNode>();

            FindPathsRecursive(startNode, endNode, currentPath, allPaths, visited, maxDepth);

            return allPaths.OrderBy(p => p.TotalCost).ToList();
        }

        private void FindPathsRecursive(TypeNode current, TypeNode target, TransformationPath currentPath,
            List<TransformationPath> allPaths, HashSet<TypeNode> visited, int remainingDepth)
        {
            if (remainingDepth < 0)
                return;

            if (current.Equals(target))
            {
                allPaths.Add(new TransformationPath(currentPath));
                return;
            }

            visited.Add(current);

            foreach (var transition in current.OutgoingTransitions)
            {
                if (!visited.Contains(transition.TargetType))
                {
                    currentPath.AddTransition(transition);
                    FindPathsRecursive(transition.TargetType, target, currentPath, allPaths, visited, remainingDepth - 1);
                    currentPath.Transitions.RemoveAt(currentPath.Transitions.Count - 1);

                    // Update EndType
                    if (currentPath.Transitions.Count > 0)
                        currentPath.EndType = currentPath.Transitions[currentPath.Transitions.Count - 1].TargetType;
                    else
                        currentPath.EndType = null;
                }
            }

            visited.Remove(current);
        }

        /// <summary>
        /// Gets all registered type nodes
        /// </summary>
        public IEnumerable<TypeNode> GetAllTypes() => _typeNodes.Values;

        /// <summary>
        /// Prints the type transformation graph
        /// </summary>
        public void PrintGraph()
        {
            Console.WriteLine("Type Transformation Graph:");
            Console.WriteLine("==========================");
            foreach (var type in _typeNodes.Values.OrderBy(t => t.Name))
            {
                Console.WriteLine($"\n{type.Name}:");
                foreach (var transition in type.OutgoingTransitions)
                {
                    Console.WriteLine($"  -> {transition}");
                }
            }
        }
    }
}
