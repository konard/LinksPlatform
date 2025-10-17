#!/bin/bash
# Example: TypoBot Pattern Implementation
# This demonstrates the GitHub Automation Pattern from issue #471
# See: doc/articles/github-automation-pattern.md

set -e

# Pattern Step 1: Problem Discovery
echo "Step 1: Problem Discovery"
TYPO="внезависимости"
CORRECT="вне зависимости"
echo "Typo found: '$TYPO'"
echo "Correct form: '$CORRECT'"
echo ""

# Pattern Step 2: Scale Assessment
echo "Step 2: Scale Assessment"
echo "Searching GitHub for this typo..."
# Note: This would use GitHub API in production
# gh search code "$TYPO" --json repository,path | jq -r '.[] | "\(.repository.name): \(.path)"'
echo "Example search command:"
echo "  gh api search/code?q=$(echo $TYPO | jq -sRr @uri)"
echo ""

# Pattern Step 3: Manual Fix Limitation
echo "Step 3: Manual Fix Limitation"
echo "If hundreds of repositories have this typo, manual fixes are impractical."
echo ""

# Pattern Step 4: Automation Solution
echo "Step 4: Automation Solution"
echo "Creating automated fix process..."
echo ""

# Example: Detect typo in local repository
detect_typo() {
    local search_path="${1:-.}"
    echo "Scanning for typo in: $search_path"

    # Find files containing the typo
    if command -v rg &> /dev/null; then
        rg "$TYPO" "$search_path" --files-with-matches 2>/dev/null || echo "No matches found"
    else
        grep -r "$TYPO" "$search_path" --files-with-matches 2>/dev/null || echo "No matches found"
    fi
}

# Example: Fix typo in a file
fix_typo() {
    local file="$1"
    if [ ! -f "$file" ]; then
        echo "File not found: $file"
        return 1
    fi

    echo "Fixing typo in: $file"
    # Use sed to replace typo (would be more sophisticated in production)
    sed -i "s/$TYPO/$CORRECT/g" "$file"
    echo "✓ Fixed"
}

# Example: Create PR for the fix
create_pr() {
    local branch_name="fix-typo-$(date +%s)"

    echo "Creating branch: $branch_name"
    # git checkout -b "$branch_name"

    echo "Committing changes..."
    # git add .
    # git commit -m "Fix typo: '$TYPO' → '$CORRECT'"

    echo "Creating pull request..."
    # gh pr create --title "Fix typo: $TYPO → $CORRECT" \
    #              --body "This PR fixes a common typo found in the codebase."

    echo "✓ PR would be created (dry run)"
}

# Main execution
echo "=== TypoBot Example (Dry Run) ==="
echo ""
detect_typo "."
echo ""
echo "In production, this would:"
echo "1. Search all target repositories"
echo "2. Clone each repository"
echo "3. Detect and fix the typo"
echo "4. Create a pull request"
echo "5. Respect rate limits and community guidelines"
echo ""
echo "See .github/workflows/pattern-automation-template.yml.example for workflow implementation"
