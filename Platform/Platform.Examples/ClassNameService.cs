using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// A service that maintains a database of classes grouped by their interface signatures.
    /// Allows retrieving all names that have been used for classes with the same interface structure.
    /// </summary>
    public class ClassNameService
    {
        private readonly Dictionary<InterfaceSignature, HashSet<string>> _interfaceToNames;
        private readonly Dictionary<string, InterfaceSignature> _nameToSignature;

        public ClassNameService()
        {
            _interfaceToNames = new Dictionary<InterfaceSignature, HashSet<string>>();
            _nameToSignature = new Dictionary<string, InterfaceSignature>();
        }

        /// <summary>
        /// Adds a class type to the database.
        /// </summary>
        /// <param name="type">The type to add.</param>
        public void AddClass(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var signature = new InterfaceSignature(type);
            var className = type.Name;

            if (!_interfaceToNames.ContainsKey(signature))
            {
                _interfaceToNames[signature] = new HashSet<string>();
            }

            _interfaceToNames[signature].Add(className);
            _nameToSignature[className] = signature;
        }

        /// <summary>
        /// Adds a class name with a manually specified signature.
        /// Useful for adding classes from external sources without loading their assemblies.
        /// </summary>
        /// <param name="className">The name of the class.</param>
        /// <param name="signature">The interface signature of the class.</param>
        public void AddClass(string className, InterfaceSignature signature)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentException("Class name cannot be null or empty.", nameof(className));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            if (!_interfaceToNames.ContainsKey(signature))
            {
                _interfaceToNames[signature] = new HashSet<string>();
            }

            _interfaceToNames[signature].Add(className);
            _nameToSignature[className] = signature;
        }

        /// <summary>
        /// Gets all class names that have been used for the same interface form as the specified type.
        /// </summary>
        /// <param name="type">The type to query.</param>
        /// <returns>A collection of class names with the same interface.</returns>
        public IEnumerable<string> GetNamesForInterface(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var signature = new InterfaceSignature(type);
            return GetNamesForSignature(signature);
        }

        /// <summary>
        /// Gets all class names that have been used for the specified interface signature.
        /// </summary>
        /// <param name="signature">The interface signature to query.</param>
        /// <returns>A collection of class names with the same interface.</returns>
        public IEnumerable<string> GetNamesForSignature(InterfaceSignature signature)
        {
            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            if (_interfaceToNames.TryGetValue(signature, out var names))
            {
                return names.ToList();
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets all class names that have the same interface as the specified class name.
        /// </summary>
        /// <param name="className">The class name to query.</param>
        /// <returns>A collection of class names with the same interface, or empty if the class is not found.</returns>
        public IEnumerable<string> GetSimilarNames(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentException("Class name cannot be null or empty.", nameof(className));
            }

            if (_nameToSignature.TryGetValue(className, out var signature))
            {
                return GetNamesForSignature(signature);
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the interface signature for a class name.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>The interface signature, or null if not found.</returns>
        public InterfaceSignature GetSignature(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentException("Class name cannot be null or empty.", nameof(className));
            }

            return _nameToSignature.TryGetValue(className, out var signature) ? signature : null;
        }

        /// <summary>
        /// Gets all unique interface signatures in the database.
        /// </summary>
        /// <returns>A collection of all interface signatures.</returns>
        public IEnumerable<InterfaceSignature> GetAllSignatures()
        {
            return _interfaceToNames.Keys.ToList();
        }

        /// <summary>
        /// Gets the total number of unique interface signatures.
        /// </summary>
        public int SignatureCount => _interfaceToNames.Count;

        /// <summary>
        /// Gets the total number of class names in the database.
        /// </summary>
        public int ClassCount => _nameToSignature.Count;

        /// <summary>
        /// Checks if a class name is in the database.
        /// </summary>
        /// <param name="className">The class name to check.</param>
        /// <returns>True if the class is in the database, false otherwise.</returns>
        public bool Contains(string className)
        {
            return _nameToSignature.ContainsKey(className);
        }

        /// <summary>
        /// Removes a class name from the database.
        /// </summary>
        /// <param name="className">The class name to remove.</param>
        /// <returns>True if the class was removed, false if it was not found.</returns>
        public bool RemoveClass(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return false;
            }

            if (_nameToSignature.TryGetValue(className, out var signature))
            {
                _nameToSignature.Remove(className);

                if (_interfaceToNames.TryGetValue(signature, out var names))
                {
                    names.Remove(className);

                    // Remove signature if no names left
                    if (names.Count == 0)
                    {
                        _interfaceToNames.Remove(signature);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all data from the database.
        /// </summary>
        public void Clear()
        {
            _interfaceToNames.Clear();
            _nameToSignature.Clear();
        }
    }
}
