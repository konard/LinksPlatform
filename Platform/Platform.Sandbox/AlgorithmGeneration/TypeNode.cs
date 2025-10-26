using System;
using System.Collections.Generic;

namespace Platform.Sandbox.AlgorithmGeneration
{
    /// <summary>
    /// Represents a type node in the type transformation graph
    /// </summary>
    public class TypeNode
    {
        public string Name { get; set; }
        public List<TypeTransition> OutgoingTransitions { get; set; }

        public TypeNode(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            OutgoingTransitions = new List<TypeTransition>();
        }

        public override string ToString() => Name;

        public override bool Equals(object obj)
        {
            return obj is TypeNode node && Name == node.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
