#!/bin/bash
# Test script for ProjectExecutor

set -e

echo "=== Testing ProjectExecutor ==="
echo

# Test 1: Detect current repository
echo "Test 1: Detecting project type in current repository"
cd /tmp/gh-issue-solver-1760634589553
dotnet run --project Platform/Platform.Data.ConsoleTerminal/Platform.Data.ConsoleTerminal.csproj -- projectexecutor detect .
echo

# Test 2: Generate shell script for Platform.Examples
echo "Test 2: Generating execution script for Platform.Examples"
dotnet run --project Platform/Platform.Data.ConsoleTerminal/Platform.Data.ConsoleTerminal.csproj -- projectexecutor generate Platform/Platform.Examples /tmp/run-examples.sh sh
if [ -f /tmp/run-examples.sh ]; then
    echo "✓ Script generated successfully"
    cat /tmp/run-examples.sh
else
    echo "✗ Script generation failed"
fi
echo

# Test 3: Detect Platform.Examples
echo "Test 3: Detecting Platform.Examples project"
dotnet run --project Platform/Platform.Data.ConsoleTerminal/Platform.Data.ConsoleTerminal.csproj -- projectexecutor detect Platform/Platform.Examples
echo

echo "=== All tests completed ==="
