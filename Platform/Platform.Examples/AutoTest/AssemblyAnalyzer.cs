using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Analyzes assemblies to discover testable functions.
    /// Implements function discovery for automatic test generation.
    /// </summary>
    public class AssemblyAnalyzer
    {
        /// <summary>
        /// Discovers all public methods in an assembly that can be tested.
        /// </summary>
        public List<ITestableFunction> DiscoverTestableFunctions(Assembly assembly, Func<MethodInfo, bool> filter = null)
        {
            var functions = new List<ITestableFunction>();

            foreach (var type in assembly.GetTypes())
            {
                // Skip compiler-generated types
                if (type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null)
                    continue;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    // Skip special methods (properties, events, etc.)
                    if (method.IsSpecialName)
                        continue;

                    // Skip methods from Object
                    if (method.DeclaringType == typeof(object))
                        continue;

                    // Apply custom filter if provided
                    if (filter != null && !filter(method))
                        continue;

                    // Check if method has supported parameter types
                    if (!HasSupportedParameterTypes(method))
                        continue;

                    object instance = null;
                    if (!method.IsStatic)
                    {
                        // Try to create an instance for instance methods
                        try
                        {
                            instance = Activator.CreateInstance(type);
                        }
                        catch
                        {
                            // Skip if we can't create an instance
                            continue;
                        }
                    }

                    functions.Add(new ReflectionTestableFunction(method, instance));
                }
            }

            return functions;
        }

        /// <summary>
        /// Discovers testable functions in a specific type.
        /// </summary>
        public List<ITestableFunction> DiscoverTestableFunctions(Type type, object instance = null)
        {
            var functions = new List<ITestableFunction>();

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (method.IsSpecialName || method.DeclaringType == typeof(object))
                    continue;

                if (!HasSupportedParameterTypes(method))
                    continue;

                object methodInstance = instance;
                if (!method.IsStatic && methodInstance == null)
                {
                    try
                    {
                        methodInstance = Activator.CreateInstance(type);
                    }
                    catch
                    {
                        continue;
                    }
                }

                functions.Add(new ReflectionTestableFunction(method, methodInstance));
            }

            return functions;
        }

        /// <summary>
        /// Checks if a method has parameter types that can be tested automatically.
        /// </summary>
        private bool HasSupportedParameterTypes(MethodInfo method)
        {
            foreach (var parameter in method.GetParameters())
            {
                if (!IsSupportedType(parameter.ParameterType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if a type is supported for automatic testing.
        /// </summary>
        private bool IsSupportedType(Type type)
        {
            // Primitive types
            if (type.IsPrimitive)
                return true;

            // Common types
            if (type == typeof(string) || type == typeof(decimal))
                return true;

            // Arrays of supported types
            if (type.IsArray)
                return IsSupportedType(type.GetElementType());

            // Nullable types
            if (Nullable.GetUnderlyingType(type) != null)
                return IsSupportedType(Nullable.GetUnderlyingType(type));

            // Types with parameterless constructors
            if (type.GetConstructor(Type.EmptyTypes) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Finds functions matching a search query (for web service functionality).
        /// </summary>
        public List<ITestableFunction> SearchFunctions(List<ITestableFunction> functions, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return functions;

            query = query.ToLowerInvariant();
            return functions.Where(f =>
                f.FunctionName.ToLowerInvariant().Contains(query) ||
                f.ReturnType.Name.ToLowerInvariant().Contains(query) ||
                f.ParameterTypes.Any(p => p.Name.ToLowerInvariant().Contains(query))
            ).ToList();
        }
    }
}
