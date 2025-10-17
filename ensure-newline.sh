#!/bin/bash
# Script to ensure all text files have a newline at the end of the file
# Usage: ./ensure-newline.sh [--check|--fix]

set -e

MODE="${1:---check}"
EXIT_CODE=0
FILES_WITHOUT_NEWLINE=()

# Define text file extensions to check
TEXT_EXTENSIONS=(
    "*.md"
    "*.txt"
    "*.cs"
    "*.csproj"
    "*.sln"
    "*.json"
    "*.yml"
    "*.yaml"
    "*.xml"
    "*.sh"
    "*.js"
    "*.css"
    "*.html"
    "*.cshtml"
    "*.config"
)

# Build find command with all extensions
FIND_CMD="find . -type f \( "
FIRST=true
for ext in "${TEXT_EXTENSIONS[@]}"; do
    if [ "$FIRST" = true ]; then
        FIND_CMD="$FIND_CMD -name \"$ext\""
        FIRST=false
    else
        FIND_CMD="$FIND_CMD -o -name \"$ext\""
    fi
done
FIND_CMD="$FIND_CMD \)"

# Exclude common directories
FIND_CMD="$FIND_CMD -not -path \"*/.*\" -not -path \"*/bin/*\" -not -path \"*/obj/*\" -not -path \"*/node_modules/*\" -not -path \"*/packages/*\""

# Find all text files
echo "Scanning for text files..."
FILES=$(eval $FIND_CMD)

# Check each file
for file in $FILES; do
    # Skip empty files
    if [ ! -s "$file" ]; then
        continue
    fi

    # Check if file ends with newline
    if [ -n "$(tail -c 1 "$file")" ]; then
        FILES_WITHOUT_NEWLINE+=("$file")

        if [ "$MODE" = "--fix" ]; then
            echo "Fixing: $file"
            echo "" >> "$file"
        else
            echo "Missing newline: $file"
            EXIT_CODE=1
        fi
    fi
done

# Summary
echo ""
if [ ${#FILES_WITHOUT_NEWLINE[@]} -eq 0 ]; then
    echo "✓ All text files have newline at the end"
else
    echo "Found ${#FILES_WITHOUT_NEWLINE[@]} file(s) without newline at the end"

    if [ "$MODE" = "--fix" ]; then
        echo "✓ All files have been fixed"
    else
        echo ""
        echo "Run with --fix to automatically add missing newlines:"
        echo "  $0 --fix"
    fi
fi

exit $EXIT_CODE
