using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Sandbox.AlgorithmGeneration
{
    /// <summary>
    /// Represents a complete path through type transformations from input to output
    /// </summary>
    public class TransformationPath
    {
        public List<TypeTransition> Transitions { get; set; }
        public TypeNode StartType { get; set; }
        public TypeNode EndType { get; set; }

        public TransformationPath()
        {
            Transitions = new List<TypeTransition>();
        }

        public TransformationPath(TransformationPath other)
        {
            Transitions = new List<TypeTransition>(other.Transitions);
            StartType = other.StartType;
            EndType = other.EndType;
        }

        public double TotalCpuCost => Transitions.Sum(t => t.CpuCost);
        public double TotalMemoryCost => Transitions.Sum(t => t.MemoryCost);
        public double TotalCost => TotalCpuCost + TotalMemoryCost;

        public void AddTransition(TypeTransition transition)
        {
            if (Transitions.Count == 0)
            {
                StartType = transition.SourceType;
            }
            Transitions.Add(transition);
            EndType = transition.TargetType;
        }

        public override string ToString()
        {
            if (Transitions.Count == 0)
                return "Empty path";

            var sb = new StringBuilder();
            sb.AppendLine($"Path: {StartType.Name} -> {EndType.Name}");
            sb.AppendLine($"Total Cost: {TotalCost:F2} (CPU: {TotalCpuCost:F2}, MEM: {TotalMemoryCost:F2})");
            sb.AppendLine("Steps:");
            foreach (var transition in Transitions)
            {
                sb.AppendLine($"  {transition}");
            }
            return sb.ToString();
        }
    }
}
