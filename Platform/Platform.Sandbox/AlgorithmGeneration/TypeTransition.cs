using System;

namespace Platform.Sandbox.AlgorithmGeneration
{
    /// <summary>
    /// Represents a type transition (operation) from one type to another with associated costs
    /// </summary>
    public class TypeTransition
    {
        public string OperationName { get; set; }
        public TypeNode SourceType { get; set; }
        public TypeNode TargetType { get; set; }
        public double CpuCost { get; set; }
        public double MemoryCost { get; set; }

        public TypeTransition(string operationName, TypeNode sourceType, TypeNode targetType, double cpuCost, double memoryCost)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            CpuCost = cpuCost;
            MemoryCost = memoryCost;
        }

        public double TotalCost => CpuCost + MemoryCost;

        public override string ToString() => $"{SourceType.Name} --[{OperationName}]--> {TargetType.Name} (CPU:{CpuCost}, MEM:{MemoryCost})";
    }
}
