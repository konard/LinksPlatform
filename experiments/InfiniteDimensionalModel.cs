using System;
using System.Collections.Generic;
using System.Linq;

namespace LinksPlatform.Experiments
{
    /// <summary>
    /// Represents a model with an infinite number of dimensions where each dimension
    /// can have a repeating value. Instead of storing all infinite dimensions,
    /// we only store the value that applies to all dimensions and track changes.
    ///
    /// Example:
    /// Zero: 0, 0, 0, ... (infinite zeros)
    /// One: 1, 1, 1, ... (infinite ones)
    ///
    /// Calculations are only needed when values change.
    /// </summary>
    /// <typeparam name="T">The type of value stored in each dimension</typeparam>
    public class InfiniteDimensionalModel<T> where T : IEquatable<T>
    {
        private readonly Dictionary<long, T> _dimensionOverrides;
        private T _defaultValue;

        /// <summary>
        /// Gets or sets the default value that applies to all dimensions
        /// unless specifically overridden.
        /// </summary>
        public T DefaultValue
        {
            get => _defaultValue;
            set => _defaultValue = value;
        }

        /// <summary>
        /// Gets the count of dimensions that have been explicitly set to a different value.
        /// </summary>
        public int OverrideCount => _dimensionOverrides.Count;

        /// <summary>
        /// Initializes a new instance of the InfiniteDimensionalModel with a default value.
        /// </summary>
        /// <param name="defaultValue">The value that applies to all dimensions by default</param>
        public InfiniteDimensionalModel(T defaultValue)
        {
            _defaultValue = defaultValue;
            _dimensionOverrides = new Dictionary<long, T>();
        }

        /// <summary>
        /// Gets the value at the specified dimension index.
        /// </summary>
        /// <param name="dimensionIndex">The dimension index (can be any long value)</param>
        /// <returns>The value at that dimension</returns>
        public T GetValue(long dimensionIndex)
        {
            if (_dimensionOverrides.TryGetValue(dimensionIndex, out T overrideValue))
            {
                return overrideValue;
            }
            return _defaultValue;
        }

        /// <summary>
        /// Sets the value at the specified dimension index.
        /// If the value equals the default value, the override is removed to save space.
        /// </summary>
        /// <param name="dimensionIndex">The dimension index</param>
        /// <param name="value">The value to set</param>
        public void SetValue(long dimensionIndex, T value)
        {
            if (value.Equals(_defaultValue))
            {
                // Remove override if it equals default value
                _dimensionOverrides.Remove(dimensionIndex);
            }
            else
            {
                _dimensionOverrides[dimensionIndex] = value;
            }
        }

        /// <summary>
        /// Gets all dimension indices that have been explicitly overridden.
        /// </summary>
        /// <returns>An enumerable of dimension indices with overrides</returns>
        public IEnumerable<long> GetOverriddenDimensions()
        {
            return _dimensionOverrides.Keys;
        }

        /// <summary>
        /// Clears all dimension overrides, returning all dimensions to the default value.
        /// </summary>
        public void ClearOverrides()
        {
            _dimensionOverrides.Clear();
        }

        /// <summary>
        /// Performs an operation on all overridden dimensions.
        /// The default dimensions (infinite count) are not enumerated for obvious performance reasons.
        /// </summary>
        /// <param name="operation">The operation to perform on each (dimension, value) pair</param>
        public void ForEachOverride(Action<long, T> operation)
        {
            foreach (var kvp in _dimensionOverrides)
            {
                operation(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Creates a snapshot of the current state including default value and all overrides.
        /// </summary>
        /// <returns>A new InfiniteDimensionalModel with the same state</returns>
        public InfiniteDimensionalModel<T> Clone()
        {
            var clone = new InfiniteDimensionalModel<T>(_defaultValue);
            foreach (var kvp in _dimensionOverrides)
            {
                clone._dimensionOverrides[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        /// <summary>
        /// Applies a transformation function to all overridden values.
        /// Note: This only affects overridden dimensions. To transform all dimensions,
        /// also update the DefaultValue property.
        /// </summary>
        /// <param name="transform">The transformation function</param>
        public void TransformOverrides(Func<T, T> transform)
        {
            var keys = _dimensionOverrides.Keys.ToList();
            foreach (var key in keys)
            {
                _dimensionOverrides[key] = transform(_dimensionOverrides[key]);
            }
        }

        /// <summary>
        /// Performs element-wise operation with another InfiniteDimensionalModel.
        /// Only calculates for dimensions that differ from default in either model.
        /// </summary>
        /// <param name="other">The other model</param>
        /// <param name="operation">The operation to perform (this_value, other_value) => result</param>
        /// <param name="resultDefault">The default value for the result model</param>
        /// <returns>A new InfiniteDimensionalModel with the results</returns>
        public InfiniteDimensionalModel<T> OperateWith(
            InfiniteDimensionalModel<T> other,
            Func<T, T, T> operation,
            T resultDefault)
        {
            var result = new InfiniteDimensionalModel<T>(resultDefault);

            // Get all unique dimension indices from both models
            var allDimensions = new HashSet<long>(this.GetOverriddenDimensions());
            allDimensions.UnionWith(other.GetOverriddenDimensions());

            // Calculate for all overridden dimensions
            foreach (var dim in allDimensions)
            {
                var thisValue = this.GetValue(dim);
                var otherValue = other.GetValue(dim);
                var resultValue = operation(thisValue, otherValue);
                result.SetValue(dim, resultValue);
            }

            return result;
        }

        /// <summary>
        /// Returns a string representation showing the default value and override count.
        /// </summary>
        public override string ToString()
        {
            if (_dimensionOverrides.Count == 0)
            {
                return $"[{_defaultValue}, {_defaultValue}, {_defaultValue}, ...] (all dimensions = {_defaultValue})";
            }
            return $"Default: {_defaultValue}, Overrides: {_dimensionOverrides.Count}";
        }
    }
}
