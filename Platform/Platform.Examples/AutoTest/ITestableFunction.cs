using System;
using System.Collections.Generic;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Represents metadata about a function that can be automatically tested.
    /// </summary>
    public interface ITestableFunction
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        /// Gets the types of input parameters.
        /// </summary>
        Type[] ParameterTypes { get; }

        /// <summary>
        /// Gets the return type of the function.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Executes the function with the given parameters.
        /// </summary>
        object Execute(params object[] parameters);

        /// <summary>
        /// Gets constraints for input parameters (min/max values, valid ranges).
        /// </summary>
        Dictionary<int, ParameterConstraints> ParameterConstraints { get; }
    }

    /// <summary>
    /// Defines constraints for a parameter.
    /// </summary>
    public class ParameterConstraints
    {
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public object[] ValidValues { get; set; }
        public Func<object, bool> ValidationFunc { get; set; }
    }
}
