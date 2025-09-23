#!/bin/bash

# Example script to analyze a repository for code evolution patterns

echo "Code Evolution Analysis Example"
echo "==============================="

# Build the project
echo "Building Platform.CodeEvolution..."
cd Platform/Platform.CodeEvolution
dotnet build

if [ $? -ne 0 ]; then
    echo "Build failed. Please check for compilation errors."
    exit 1
fi

echo "Build successful!"
echo ""

# Analyze the current repository
echo "Analyzing the LinksPlatform repository..."
echo "This will analyze the last 100 commits to find code evolution patterns."
echo ""

dotnet run -- analyze \
    --repository ../.. \
    --max-commits 100 \
    --output ../../analysis-results

echo ""
echo "Analysis complete!"
echo "Check the 'analysis-results' directory for:"
echo "- changes.json: All detected code changes"
echo "- frequencies.json: Most common change patterns"
echo "- chains.json: Code evolution chains"
echo "- recommendations.html: Visual recommendations report"
echo "- report.md: Summary analysis report"
echo ""
echo "You can open recommendations.html in a web browser to see the visual report."