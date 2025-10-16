using System;
using System.Collections.Generic;
using System.Reflection;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Wraps a method for automatic testing using reflection.
    /// </summary>
    public class ReflectionTestableFunction : ITestableFunction
    {
        private readonly MethodInfo _method;
        private readonly object _instance;

        public string FunctionName { get; }
        public Type[] ParameterTypes { get; }
        public Type ReturnType { get; }
        public Dictionary<int, ParameterConstraints> ParameterConstraints { get; }

        public ReflectionTestableFunction(MethodInfo method, object instance = null)
        {
            _method = method;
            _instance = instance;
            FunctionName = $"{method.DeclaringType?.Name}.{method.Name}";

            var parameters = method.GetParameters();
            ParameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterTypes[i] = parameters[i].ParameterType;
            }

            ReturnType = method.ReturnType;
            ParameterConstraints = new Dictionary<int, ParameterConstraints>();
        }

        public object Execute(params object[] parameters)
        {
            try
            {
                return _method.Invoke(_instance, parameters);
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the inner exception
                throw ex.InnerException ?? ex;
            }
        }

        public void AddConstraint(int parameterIndex, ParameterConstraints constraints)
        {
            ParameterConstraints[parameterIndex] = constraints;
        }
    }
}
