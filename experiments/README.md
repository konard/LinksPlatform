# LinksPlatform Experiments

This directory contains experimental scripts and analyses for testing and verifying concepts in the LinksPlatform.

## Triplet-to-Doublet Cost Analysis

**File:** `triplet_doublet_cost_analysis.py`

Experimental analysis script that mathematically verifies the optimization pattern for representing multiple typed relationships as doublets.

### Features

1. **Formula Verification** - Validates the mathematical formulas for cost comparison
2. **Convergence Analysis** - Shows how efficiency approaches 50% as the number of types increases
3. **Real-World Examples** - Demonstrates practical scenarios with actual use cases
4. **Visualization** - Generates graphs showing cost comparison and efficiency

### Requirements

```bash
pip install matplotlib numpy
```

### Running the Experiment

```bash
cd experiments
python3 triplet_doublet_cost_analysis.py
```

### Output

The script produces:
- Console output with detailed analysis and tables
- `triplet_doublet_analysis.png` - Four-panel visualization showing:
  - Cost comparison between traditional and optimized approaches
  - Absolute space savings
  - Efficiency percentage over different values of n
  - Cost ratio convergence to 0.5

### Results Summary

The experiment confirms:
- **Traditional cost**: 2n doublets for n types
- **Optimized cost**: n+1 doublets for n types
- **Space saved**: n-1 doublets
- **Efficiency**: Approaches 50% as n increases

Example results:
- 2 types: 25% savings (4→3 doublets)
- 3 types: 33% savings (6→4 doublets)
- 10 types: 45% savings (20→11 doublets)
- 100 types: 49.5% savings (200→101 doublets)

### Related Documentation

- `doc/articles/triplet-to-doublet-optimization.md` - Full documentation
- `examples/MultipleTypesDoubletOptimization.cs` - C# implementation example
