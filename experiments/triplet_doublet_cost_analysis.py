#!/usr/bin/env python3
"""
Experimental analysis of the triplet-to-doublet optimization pattern.

This script verifies the mathematical properties and demonstrates the
cost savings of representing multiple typed relationships as doublets.
"""

import matplotlib
matplotlib.use('Agg')  # Use non-interactive backend
import matplotlib.pyplot as plt
import numpy as np


def traditional_cost(n_types):
    """
    Cost of traditional triplet representation as doublets.
    Each triplet (a, type_i, b) becomes 2 doublets.

    Args:
        n_types: Number of different types between same source and target

    Returns:
        Total number of doublets needed
    """
    return 2 * n_types


def optimized_cost(n_types):
    """
    Cost of optimized doublet representation.
    Base relationship (a, b) is shared, plus one doublet per type.

    Args:
        n_types: Number of different types between same source and target

    Returns:
        Total number of doublets needed
    """
    if n_types == 0:
        return 0
    return n_types + 1


def space_saved(n_types):
    """
    Number of doublets saved by using optimization.

    Args:
        n_types: Number of different types

    Returns:
        Number of doublets saved
    """
    return traditional_cost(n_types) - optimized_cost(n_types)


def efficiency_percentage(n_types):
    """
    Efficiency gain as percentage of traditional cost.

    Args:
        n_types: Number of different types

    Returns:
        Efficiency percentage (0-100)
    """
    if n_types == 0:
        return 0
    trad = traditional_cost(n_types)
    saved = space_saved(n_types)
    return (saved / trad) * 100


def verify_formula():
    """
    Verify the mathematical formula matches the pattern.
    """
    print("=== Formula Verification ===\n")
    print("For n triplets with same source and target but different types:")
    print("  Traditional: 2n doublets")
    print("  Optimized:   n+1 doublets")
    print("  Saved:       n-1 doublets")
    print("  Efficiency:  (n-1)/(2n) * 100%")
    print()

    test_cases = [1, 2, 3, 5, 10, 20, 50, 100, 1000]
    print(f"{'n':>6} | {'Traditional':>11} | {'Optimized':>9} | {'Saved':>6} | {'Efficiency':>10}")
    print("-" * 60)

    for n in test_cases:
        trad = traditional_cost(n)
        opt = optimized_cost(n)
        saved = space_saved(n)
        eff = efficiency_percentage(n)

        # Verify formula
        assert trad == 2 * n, f"Traditional formula failed for n={n}"
        assert opt == n + 1, f"Optimized formula failed for n={n}"
        assert saved == n - 1, f"Saved formula failed for n={n}"

        print(f"{n:6} | {trad:11} | {opt:9} | {saved:6} | {eff:9.2f}%")

    print("\n✓ All formulas verified!")


def analyze_convergence():
    """
    Analyze how efficiency converges to 50% as n increases.
    """
    print("\n=== Convergence Analysis ===\n")
    print("As n approaches infinity, efficiency approaches 50%")
    print()

    large_n_values = [10, 100, 1000, 10000, 100000, 1000000]
    print(f"{'n':>10} | {'Efficiency':>12} | {'Distance from 50%':>18}")
    print("-" * 45)

    for n in large_n_values:
        eff = efficiency_percentage(n)
        distance = abs(50.0 - eff)
        print(f"{n:10} | {eff:11.6f}% | {distance:17.6f}%")

    print("\n✓ Convergence to 50% confirmed!")


def generate_visualization():
    """
    Generate plots showing the cost comparison and efficiency.
    """
    print("\n=== Generating Visualizations ===\n")

    n_values = np.arange(1, 101)
    traditional_costs = [traditional_cost(n) for n in n_values]
    optimized_costs = [optimized_cost(n) for n in n_values]
    savings = [space_saved(n) for n in n_values]
    efficiencies = [efficiency_percentage(n) for n in n_values]

    # Create figure with subplots
    fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(14, 10))
    fig.suptitle('Triplet-to-Doublet Optimization Analysis', fontsize=16, fontweight='bold')

    # Plot 1: Cost Comparison
    ax1.plot(n_values, traditional_costs, label='Traditional (2n)', linewidth=2, color='red')
    ax1.plot(n_values, optimized_costs, label='Optimized (n+1)', linewidth=2, color='green')
    ax1.fill_between(n_values, optimized_costs, traditional_costs, alpha=0.3, color='yellow', label='Saved')
    ax1.set_xlabel('Number of Types (n)')
    ax1.set_ylabel('Total Doublets Required')
    ax1.set_title('Cost Comparison: Traditional vs Optimized')
    ax1.legend()
    ax1.grid(True, alpha=0.3)

    # Plot 2: Space Saved
    ax2.plot(n_values, savings, linewidth=2, color='blue')
    ax2.fill_between(n_values, 0, savings, alpha=0.3, color='blue')
    ax2.set_xlabel('Number of Types (n)')
    ax2.set_ylabel('Doublets Saved (n-1)')
    ax2.set_title('Absolute Space Savings')
    ax2.grid(True, alpha=0.3)

    # Plot 3: Efficiency Percentage
    ax3.plot(n_values, efficiencies, linewidth=2, color='purple')
    ax3.axhline(y=50, color='red', linestyle='--', label='50% Asymptote')
    ax3.fill_between(n_values, 0, efficiencies, alpha=0.3, color='purple')
    ax3.set_xlabel('Number of Types (n)')
    ax3.set_ylabel('Efficiency (%)')
    ax3.set_title('Space Efficiency Percentage')
    ax3.set_ylim([0, 55])
    ax3.legend()
    ax3.grid(True, alpha=0.3)

    # Plot 4: Ratio Comparison
    ratios = [optimized_cost(n) / traditional_cost(n) for n in n_values]
    ax4.plot(n_values, ratios, linewidth=2, color='orange')
    ax4.axhline(y=0.5, color='red', linestyle='--', label='0.5 Asymptote')
    ax4.set_xlabel('Number of Types (n)')
    ax4.set_ylabel('Optimized/Traditional Ratio')
    ax4.set_title('Cost Ratio: Approaches 0.5 as n → ∞')
    ax4.set_ylim([0, 1.1])
    ax4.legend()
    ax4.grid(True, alpha=0.3)

    plt.tight_layout()
    output_file = '/tmp/gh-issue-solver-1761471718035/experiments/triplet_doublet_analysis.png'
    plt.savefig(output_file, dpi=150, bbox_inches='tight')
    print(f"✓ Visualization saved to: {output_file}")


def real_world_examples():
    """
    Demonstrate real-world scenarios.
    """
    print("\n=== Real-World Examples ===\n")

    examples = [
        ("Social Network: Person relationships", 5, ["knows", "works_with", "lives_near", "studied_with", "related_to"]),
        ("Knowledge Graph: Entity connections", 8, ["is_a", "part_of", "located_in", "created_by", "similar_to", "causes", "precedes", "requires"]),
        ("Type System: Variable annotations", 3, ["type:int", "type:string", "nullable"]),
        ("Metadata: Document tags", 12, ["verified", "published", "reviewed", "archived", "featured", "trending", "premium", "urgent", "confidential", "draft", "final", "approved"]),
    ]

    for name, n_types, type_list in examples:
        trad = traditional_cost(n_types)
        opt = optimized_cost(n_types)
        saved = space_saved(n_types)
        eff = efficiency_percentage(n_types)

        print(f"{name}")
        print(f"  Types: {n_types} ({', '.join(type_list[:3])}{'...' if len(type_list) > 3 else ''})")
        print(f"  Traditional: {trad} doublets")
        print(f"  Optimized:   {opt} doublets")
        print(f"  Saved:       {saved} doublets ({eff:.1f}%)")
        print()


def main():
    """
    Run all experimental analyses.
    """
    print("=" * 70)
    print("TRIPLET-TO-DOUBLET OPTIMIZATION: EXPERIMENTAL ANALYSIS")
    print("=" * 70)
    print()

    verify_formula()
    analyze_convergence()
    real_world_examples()
    generate_visualization()

    print("\n" + "=" * 70)
    print("CONCLUSION")
    print("=" * 70)
    print("""
The optimization pattern has been mathematically verified:

1. Linear Scaling: O(n+1) vs O(2n)
   - Optimized approach grows linearly with fewer doublets

2. Asymptotic Efficiency: Approaches 50%
   - For large n, saves nearly half the storage space

3. Practical Benefits:
   - Immediate savings starting from n=2 (25% efficiency)
   - Significant gains for common cases (n=3-10: 33-45% efficiency)
   - Maintains full semantic expressiveness

4. Real-World Applicability:
   - Social networks with multiple relationship types
   - Knowledge graphs with varied connections
   - Type systems with union/intersection types
   - Metadata systems with multiple tags

This optimization is a fundamental improvement for doublet-based
associative data stores when representing multi-typed relationships.
    """)


if __name__ == "__main__":
    main()
