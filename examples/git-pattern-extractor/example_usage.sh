#!/bin/bash
# Example usage of git-pattern-extractor tool
# This script demonstrates the main features of the tool

set -e

TOOL_PATH="../../tools/git-pattern-extractor/git_pattern_extractor.py"
OUTPUT_DIR="output"

echo "=== Git Pattern Extractor Examples ==="
echo ""

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Example 1: Extract patterns from current repository
echo "1. Extracting patterns from current repository..."
python3 "$TOOL_PATH" extract \
    --repo ../.. \
    --output "$OUTPUT_DIR/full_pattern.json"
echo "✓ Pattern extracted to $OUTPUT_DIR/full_pattern.json"
echo ""

# Example 2: Extract patterns from recent commits
echo "2. Extracting patterns from last 5 commits..."
python3 "$TOOL_PATH" extract \
    --repo ../.. \
    --range HEAD~5..HEAD \
    --output "$OUTPUT_DIR/recent_pattern.json"
echo "✓ Recent pattern extracted to $OUTPUT_DIR/recent_pattern.json"
echo ""

# Example 3: Merge patterns
echo "3. Merging patterns..."
if [ -f "$OUTPUT_DIR/full_pattern.json" ] && [ -f "$OUTPUT_DIR/recent_pattern.json" ]; then
    python3 "$TOOL_PATH" merge \
        "$OUTPUT_DIR/full_pattern.json" \
        "$OUTPUT_DIR/recent_pattern.json" \
        --output "$OUTPUT_DIR/merged_pattern.json"
    echo "✓ Patterns merged to $OUTPUT_DIR/merged_pattern.json"
else
    echo "⚠ Skipping merge - pattern files not found"
fi
echo ""

# Example 4: Split pattern by author
echo "4. Splitting pattern by author..."
if [ -f "$OUTPUT_DIR/full_pattern.json" ]; then
    python3 "$TOOL_PATH" split \
        "$OUTPUT_DIR/full_pattern.json" \
        --by author \
        --output-dir "$OUTPUT_DIR/split_by_author/"
    echo "✓ Pattern split by author in $OUTPUT_DIR/split_by_author/"
else
    echo "⚠ Skipping split - pattern file not found"
fi
echo ""

# Example 5: Split pattern by file type
echo "5. Splitting pattern by file type..."
if [ -f "$OUTPUT_DIR/full_pattern.json" ]; then
    python3 "$TOOL_PATH" split \
        "$OUTPUT_DIR/full_pattern.json" \
        --by file_type \
        --output-dir "$OUTPUT_DIR/split_by_type/"
    echo "✓ Pattern split by file type in $OUTPUT_DIR/split_by_type/"
else
    echo "⚠ Skipping split - pattern file not found"
fi
echo ""

# Example 6: Split pattern by commit type
echo "6. Splitting pattern by commit type..."
if [ -f "$OUTPUT_DIR/full_pattern.json" ]; then
    python3 "$TOOL_PATH" split \
        "$OUTPUT_DIR/full_pattern.json" \
        --by commit_type \
        --output-dir "$OUTPUT_DIR/split_by_commit_type/"
    echo "✓ Pattern split by commit type in $OUTPUT_DIR/split_by_commit_type/"
else
    echo "⚠ Skipping split - pattern file not found"
fi
echo ""

# Example 7: Replay pattern (dry-run)
echo "7. Replaying pattern (dry-run)..."
if [ -f "$OUTPUT_DIR/recent_pattern.json" ]; then
    python3 "$TOOL_PATH" replay \
        "$OUTPUT_DIR/recent_pattern.json" \
        --target ../..
    echo "✓ Dry-run completed"
else
    echo "⚠ Skipping replay - pattern file not found"
fi
echo ""

echo "=== All examples completed ==="
echo "Output files are in: $OUTPUT_DIR/"
echo ""
echo "To view extracted patterns:"
echo "  cat $OUTPUT_DIR/full_pattern.json | python3 -m json.tool | less"
echo ""
echo "To clean up:"
echo "  rm -rf $OUTPUT_DIR/"
