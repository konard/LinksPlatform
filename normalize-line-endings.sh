#!/bin/bash
# Cross-platform script to normalize line endings in all text files
# Works on Linux, macOS, and Windows (via Git Bash, MSYS2, or WSL)

echo "Normalizing line endings for all files in the repository..."
echo "This will reset all files according to .gitattributes settings."
echo ""

# Check if .gitattributes exists
if [ ! -f ".gitattributes" ]; then
    echo "Error: .gitattributes file not found!"
    echo "Please ensure .gitattributes is present in the repository root."
    exit 1
fi

# Save current changes (if any)
echo "Step 1: Checking for uncommitted changes..."
if ! git diff-index --quiet HEAD --; then
    echo "Warning: You have uncommitted changes."
    read -p "Do you want to continue? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Aborted."
        exit 1
    fi
fi

# Remove all files from Git's index
echo "Step 2: Removing all files from Git index..."
git rm --cached -r . > /dev/null 2>&1

# Reset the index
echo "Step 3: Re-adding files with normalized line endings..."
git reset > /dev/null 2>&1

# Re-add all files (Git will normalize them according to .gitattributes)
git add -A

# Show what changed
echo ""
echo "Step 4: Checking for changes..."
if git diff --cached --quiet; then
    echo "No line ending changes needed. All files already have correct line endings."
else
    echo "The following files will have their line endings normalized:"
    git diff --cached --name-only
    echo ""
    echo "To commit these changes, run:"
    echo "  git commit -m \"Normalize line endings\""
fi

echo ""
echo "Done! Line endings have been normalized according to .gitattributes."
