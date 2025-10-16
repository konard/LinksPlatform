#!/bin/bash
# Example script demonstrating how to use the code preference extractor

echo "==================================="
echo "Code Preference Extractor Examples"
echo "==================================="
echo ""

# Example 1: Analyze a local directory
echo "Example 1: Analyzing the Platform directory"
echo "Command: python3 code_preference_extractor.py Platform --pretty --output examples/platform_preferences.json"
python3 ../code_preference_extractor.py ../Platform --pretty --output platform_preferences.json
echo ""

# Example 2: Show summary of extracted preferences
echo "Example 2: Extracted preferences summary"
cat platform_preferences.json | python3 -c "
import sys, json
prefs = json.load(sys.stdin)
print(f\"Files analyzed: {prefs['summary']['files_analyzed']}\")
print(f\"Total lines: {prefs['summary']['total_lines']}\")
print(f\"\\nKey preferences:\")
print(f\"  - Indentation: {prefs['indentation']['style']}\")
if 'classes' in prefs['naming_conventions']:
    print(f\"  - Class naming: {prefs['naming_conventions']['classes']['preferred_style']}\")
print(f\"  - Brace style: {prefs['brace_style']['style']}\")
print(f\"  - Average line length: {prefs['line_length']['average']} chars\")
"
echo ""

echo "Done! Check examples/platform_preferences.json for full details."
