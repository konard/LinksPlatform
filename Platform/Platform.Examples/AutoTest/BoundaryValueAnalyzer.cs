using System;
using System.Collections.Generic;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Analyzes functions to generate boundary value test cases.
    /// Implements extreme value testing as mentioned in issue #102.
    /// </summary>
    public class BoundaryValueAnalyzer
    {
        /// <summary>
        /// Generates boundary test values for a given parameter type.
        /// </summary>
        public List<object> GenerateBoundaryValues(Type parameterType, ParameterConstraints constraints = null)
        {
            var values = new List<object>();

            if (parameterType == typeof(int))
            {
                values.Add(int.MinValue);
                values.Add(-1);
                values.Add(0);
                values.Add(1);
                values.Add(int.MaxValue);
            }
            else if (parameterType == typeof(long))
            {
                values.Add(long.MinValue);
                values.Add(-1L);
                values.Add(0L);
                values.Add(1L);
                values.Add(long.MaxValue);
            }
            else if (parameterType == typeof(uint))
            {
                values.Add(uint.MinValue);
                values.Add(1u);
                values.Add(uint.MaxValue);
            }
            else if (parameterType == typeof(ulong))
            {
                values.Add(ulong.MinValue);
                values.Add(1UL);
                values.Add(ulong.MaxValue);
            }
            else if (parameterType == typeof(double))
            {
                values.Add(double.MinValue);
                values.Add(-1.0);
                values.Add(0.0);
                values.Add(1.0);
                values.Add(double.MaxValue);
                values.Add(double.NaN);
                values.Add(double.PositiveInfinity);
                values.Add(double.NegativeInfinity);
            }
            else if (parameterType == typeof(float))
            {
                values.Add(float.MinValue);
                values.Add(-1.0f);
                values.Add(0.0f);
                values.Add(1.0f);
                values.Add(float.MaxValue);
                values.Add(float.NaN);
                values.Add(float.PositiveInfinity);
                values.Add(float.NegativeInfinity);
            }
            else if (parameterType == typeof(string))
            {
                values.Add(null);
                values.Add(string.Empty);
                values.Add(" ");
                values.Add("test");
            }
            else if (parameterType == typeof(bool))
            {
                values.Add(true);
                values.Add(false);
            }
            else if (parameterType.IsArray)
            {
                values.Add(null);
                values.Add(Array.CreateInstance(parameterType.GetElementType(), 0));
                values.Add(Array.CreateInstance(parameterType.GetElementType(), 1));
            }
            else if (!parameterType.IsValueType)
            {
                values.Add(null);
            }

            // Apply custom constraints if provided
            if (constraints != null)
            {
                if (constraints.MinValue != null)
                {
                    values.Add(constraints.MinValue);
                }
                if (constraints.MaxValue != null)
                {
                    values.Add(constraints.MaxValue);
                }
                if (constraints.ValidValues != null)
                {
                    values.AddRange(constraints.ValidValues);
                }
            }

            return values;
        }

        /// <summary>
        /// Generates random test values for a parameter type.
        /// </summary>
        public object GenerateRandomValue(Type parameterType, Random random = null)
        {
            random = random ?? new Random();

            if (parameterType == typeof(int))
            {
                return random.Next();
            }
            else if (parameterType == typeof(long))
            {
                return (long)random.Next() * random.Next();
            }
            else if (parameterType == typeof(uint))
            {
                return (uint)random.Next();
            }
            else if (parameterType == typeof(ulong))
            {
                return (ulong)random.Next() * (ulong)random.Next();
            }
            else if (parameterType == typeof(double))
            {
                return random.NextDouble() * random.Next();
            }
            else if (parameterType == typeof(float))
            {
                return (float)(random.NextDouble() * random.Next());
            }
            else if (parameterType == typeof(bool))
            {
                return random.Next(2) == 0;
            }
            else if (parameterType == typeof(string))
            {
                var length = random.Next(0, 100);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = (char)random.Next(32, 127);
                }
                return new string(chars);
            }

            return null;
        }
    }
}
