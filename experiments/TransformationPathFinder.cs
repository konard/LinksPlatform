using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.CodeGeneration.Experiments
{
    /// <summary>
    /// Represents a method signature with parameters and return type.
    /// </summary>
    public class MethodSignature
    {
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public Type ReturnType { get; set; }

        public override string ToString()
        {
            var parameters = string.Join(", ", Parameters.Select(p => $"{p.Type.Name} {p.Name}"));
            return $"{ReturnType.Name} {Name}({parameters})";
        }
    }

    /// <summary>
    /// Represents a method parameter.
    /// </summary>
    public class Parameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool HasDefaultValue { get; set; }
        public object DefaultValue { get; set; }
    }

    /// <summary>
    /// Base interface for all transformations.
    /// </summary>
    public interface ITransformation
    {
        /// <summary>
        /// Gets the source type this transformation converts from.
        /// </summary>
        Type From { get; }

        /// <summary>
        /// Gets the target type this transformation converts to.
        /// </summary>
        Type To { get; }

        /// <summary>
        /// Gets the cost/complexity of this transformation (lower is better).
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// Generates code for this transformation.
        /// </summary>
        /// <param name="input">The input expression to transform.</param>
        /// <returns>The generated code as a string.</returns>
        string GenerateCode(string input);
    }

    /// <summary>
    /// Transformation that casts between compatible types.
    /// </summary>
    public class TypeCastTransformation : ITransformation
    {
        public Type From { get; }
        public Type To { get; }
        public int Cost { get; } = 1;

        public TypeCastTransformation(Type from, Type to)
        {
            From = from;
            To = to;
        }

        public string GenerateCode(string input) => $"({To.Name}){input}";
    }

    /// <summary>
    /// Transformation that converts async Task to sync result.
    /// </summary>
    public class AsyncToSyncTransformation : ITransformation
    {
        public Type From { get; }
        public Type To { get; }
        public int Cost { get; } = 3; // Higher cost as it's blocking

        public AsyncToSyncTransformation(Type taskType, Type resultType)
        {
            From = taskType;
            To = resultType;
        }

        public string GenerateCode(string input) => $"{input}.Result";
    }

    /// <summary>
    /// Transformation that wraps sync call in Task.
    /// </summary>
    public class SyncToAsyncTransformation : ITransformation
    {
        public Type From { get; }
        public Type To { get; }
        public int Cost { get; } = 2;

        public SyncToAsyncTransformation(Type resultType, Type taskType)
        {
            From = resultType;
            To = taskType;
        }

        public string GenerateCode(string input) => $"Task.FromResult({input})";
    }

    /// <summary>
    /// Transformation that unwraps Result&lt;T&gt; to T.
    /// </summary>
    public class ResultUnwrapTransformation : ITransformation
    {
        public Type From { get; }
        public Type To { get; }
        public int Cost { get; } = 2;

        public ResultUnwrapTransformation(Type resultType, Type valueType)
        {
            From = resultType;
            To = valueType;
        }

        public string GenerateCode(string input) => $"{input}.Value";
    }

    /// <summary>
    /// Transformation that calls ToString() on an object.
    /// </summary>
    public class ToStringTransformation : ITransformation
    {
        public Type From { get; }
        public Type To { get; } = typeof(string);
        public int Cost { get; } = 1;

        public ToStringTransformation(Type from)
        {
            From = from;
        }

        public string GenerateCode(string input) => $"{input}.ToString()";
    }

    /// <summary>
    /// Finds transformation paths between method signatures.
    /// </summary>
    public class TransformationPathFinder
    {
        private readonly Dictionary<(Type From, Type To), ITransformation> _directTransformations = new();
        private readonly List<ITransformation> _allTransformations = new();

        public TransformationPathFinder()
        {
            RegisterDefaultTransformations();
        }

        private void RegisterDefaultTransformations()
        {
            // Register common transformations
            RegisterTransformation(new TypeCastTransformation(typeof(int), typeof(long)));
            RegisterTransformation(new TypeCastTransformation(typeof(int), typeof(double)));
            RegisterTransformation(new TypeCastTransformation(typeof(long), typeof(int)));
            RegisterTransformation(new TypeCastTransformation(typeof(float), typeof(double)));
            RegisterTransformation(new TypeCastTransformation(typeof(double), typeof(float)));

            // String conversions
            RegisterTransformation(new ToStringTransformation(typeof(int)));
            RegisterTransformation(new ToStringTransformation(typeof(long)));
            RegisterTransformation(new ToStringTransformation(typeof(object)));
        }

        /// <summary>
        /// Registers a transformation.
        /// </summary>
        public void RegisterTransformation(ITransformation transformation)
        {
            _directTransformations[(transformation.From, transformation.To)] = transformation;
            _allTransformations.Add(transformation);
        }

        /// <summary>
        /// Finds the best transformation path from source to target signature.
        /// </summary>
        public TransformationPath FindPath(MethodSignature source, MethodSignature target)
        {
            var path = new TransformationPath
            {
                Source = source,
                Target = target
            };

            // 1. Match and transform parameters
            for (int i = 0; i < target.Parameters.Count; i++)
            {
                var targetParam = target.Parameters[i];
                var sourceParam = FindMatchingParameter(source.Parameters, targetParam, i);

                if (sourceParam == null)
                {
                    // Need to provide default value
                    path.AddParameterTransformation(i, null, $"default({targetParam.Type.Name})", "Default value");
                }
                else if (sourceParam.Type == targetParam.Type)
                {
                    // Direct match, no transformation needed
                    path.AddParameterTransformation(i, sourceParam.Name, sourceParam.Name, "Direct match");
                }
                else
                {
                    // Need transformation
                    var transformations = FindTypeTransformationPath(sourceParam.Type, targetParam.Type);
                    var code = ApplyTransformations(sourceParam.Name, transformations);
                    var description = string.Join(" → ", transformations.Select(t => $"{t.From.Name} to {t.To.Name}"));
                    path.AddParameterTransformation(i, sourceParam.Name, code, description);
                    path.Transformations.AddRange(transformations);
                }
            }

            // 2. Transform return type
            if (source.ReturnType != target.ReturnType)
            {
                var transformations = FindTypeTransformationPath(source.ReturnType, target.ReturnType);
                var code = ApplyTransformations("result", transformations);
                var description = string.Join(" → ", transformations.Select(t => $"{t.From.Name} to {t.To.Name}"));
                path.ReturnTransformationCode = code;
                path.ReturnTransformationDescription = description;
                path.Transformations.AddRange(transformations);
            }

            // Calculate total cost
            path.TotalCost = path.Transformations.Sum(t => t.Cost);

            return path;
        }

        /// <summary>
        /// Finds transformation path between two types using Dijkstra's algorithm.
        /// </summary>
        private List<ITransformation> FindTypeTransformationPath(Type from, Type to)
        {
            if (from == to)
                return new List<ITransformation>();

            // Try direct transformation first
            if (_directTransformations.TryGetValue((from, to), out var direct))
                return new List<ITransformation> { direct };

            // Use simplified Dijkstra's algorithm for multi-step transformations
            var distances = new Dictionary<Type, int>();
            var previous = new Dictionary<Type, (Type, ITransformation)>();
            var unvisited = new HashSet<Type>();

            distances[from] = 0;
            unvisited.Add(from);

            foreach (var transformation in _allTransformations)
            {
                unvisited.Add(transformation.From);
                unvisited.Add(transformation.To);
                if (!distances.ContainsKey(transformation.From))
                    distances[transformation.From] = int.MaxValue;
                if (!distances.ContainsKey(transformation.To))
                    distances[transformation.To] = int.MaxValue;
            }

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(t => distances[t]).First();
                unvisited.Remove(current);

                if (current == to)
                    break;

                if (distances[current] == int.MaxValue)
                    break;

                foreach (var transformation in _allTransformations.Where(t => t.From == current))
                {
                    var neighbor = transformation.To;
                    var alternateDistance = distances[current] + transformation.Cost;

                    if (alternateDistance < distances[neighbor])
                    {
                        distances[neighbor] = alternateDistance;
                        previous[neighbor] = (current, transformation);
                    }
                }
            }

            // Reconstruct path
            var path = new List<ITransformation>();
            var step = to;

            while (previous.ContainsKey(step))
            {
                var (prevType, transformation) = previous[step];
                path.Insert(0, transformation);
                step = prevType;
            }

            return path.Count > 0 ? path : null;
        }

        private Parameter FindMatchingParameter(List<Parameter> sourceParams, Parameter targetParam, int position)
        {
            // Try exact name match first
            var nameMatch = sourceParams.FirstOrDefault(p => p.Name.Equals(targetParam.Name, StringComparison.OrdinalIgnoreCase));
            if (nameMatch != null)
                return nameMatch;

            // Try position match
            if (position < sourceParams.Count)
                return sourceParams[position];

            return null;
        }

        private string ApplyTransformations(string input, List<ITransformation> transformations)
        {
            if (transformations == null || transformations.Count == 0)
                return input;

            var result = input;
            foreach (var transformation in transformations)
            {
                result = transformation.GenerateCode(result);
            }
            return result;
        }
    }

    /// <summary>
    /// Represents a complete transformation path from source to target method.
    /// </summary>
    public class TransformationPath
    {
        public MethodSignature Source { get; set; }
        public MethodSignature Target { get; set; }
        public List<ITransformation> Transformations { get; } = new List<ITransformation>();
        public Dictionary<int, ParameterTransformation> ParameterTransformations { get; } = new Dictionary<int, ParameterTransformation>();
        public string ReturnTransformationCode { get; set; }
        public string ReturnTransformationDescription { get; set; }
        public int TotalCost { get; set; }

        public void AddParameterTransformation(int position, string sourceParam, string code, string description)
        {
            ParameterTransformations[position] = new ParameterTransformation
            {
                Position = position,
                SourceParameter = sourceParam,
                GeneratedCode = code,
                Description = description
            };
        }

        public override string ToString()
        {
            var lines = new List<string>
            {
                $"Transformation Path (Cost: {TotalCost})",
                $"From: {Source}",
                $"To:   {Target}",
                "",
                "Parameter Transformations:"
            };

            foreach (var kvp in ParameterTransformations.OrderBy(x => x.Key))
            {
                var pt = kvp.Value;
                lines.Add($"  [{kvp.Key}] {pt.SourceParameter ?? "(default)"} → {pt.GeneratedCode} ({pt.Description})");
            }

            if (!string.IsNullOrEmpty(ReturnTransformationDescription))
            {
                lines.Add("");
                lines.Add("Return Transformation:");
                lines.Add($"  {ReturnTransformationDescription}");
                lines.Add($"  Code: {ReturnTransformationCode}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Generates the complete method implementation code.
        /// </summary>
        public string GenerateImplementation()
        {
            var parameters = string.Join(", ", Target.Parameters.Select(p => $"{p.Type.Name} {p.Name}"));
            var returnType = Target.ReturnType.Name;

            var code = new List<string>
            {
                $"public {returnType} {Target.Name}({parameters})",
                "{",
                $"    // Auto-generated implementation",
                $"    // Source: {Source}",
                ""
            };

            // Generate parameter preparation
            if (ParameterTransformations.Any())
            {
                code.Add("    // Prepare parameters");
                foreach (var kvp in ParameterTransformations.OrderBy(x => x.Key))
                {
                    var pt = kvp.Value;
                    var targetParam = Target.Parameters[kvp.Key];
                    code.Add($"    var param{kvp.Key} = {pt.GeneratedCode}; // {pt.Description}");
                }
                code.Add("");
            }

            // Generate method call
            var args = string.Join(", ", ParameterTransformations.OrderBy(x => x.Key).Select(x => $"param{x.Key}"));
            code.Add($"    // Call source method");
            code.Add($"    var result = sourceInstance.{Source.Name}({args});");

            // Generate return transformation
            if (!string.IsNullOrEmpty(ReturnTransformationCode))
            {
                code.Add("");
                code.Add($"    // Transform return value: {ReturnTransformationDescription}");
                code.Add($"    return {ReturnTransformationCode};");
            }
            else
            {
                code.Add($"    return result;");
            }

            code.Add("}");

            return string.Join(Environment.NewLine, code);
        }
    }

    public class ParameterTransformation
    {
        public int Position { get; set; }
        public string SourceParameter { get; set; }
        public string GeneratedCode { get; set; }
        public string Description { get; set; }
    }
}
