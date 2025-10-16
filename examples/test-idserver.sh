#!/bin/bash

# Test script for Platform.Data.IdServer
# Tests build and basic code validation

echo "=== Platform.Data.IdServer Test Script ==="
echo ""

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "Error: dotnet CLI not found. Please install .NET Core SDK."
    exit 1
fi

# Build the project
echo "Building Platform.Data.IdServer..."
cd Platform
dotnet build Platform.Data.IdServer/Platform.Data.IdServer.csproj -c Release
BUILD_RESULT=$?
cd ..

if [ $BUILD_RESULT -ne 0 ]; then
    echo "Error: Build failed."
    exit 1
fi

echo "Build successful!"
echo ""

# Verify output files exist
echo "Test 1: Verifying build outputs exist"
if [ -f "Platform/Platform.Data.IdServer/bin/Release/netcoreapp2.2/Platform.Data.IdServer.dll" ]; then
    echo "✓ IdServer DLL built successfully"
else
    echo "✗ IdServer DLL not found"
    exit 1
fi

if [ -f "Platform/Platform.Examples/bin/Release/netstandard2.0/Platform.Examples.dll" ]; then
    echo "✓ Platform.Examples DLL built successfully"
else
    echo "✗ Platform.Examples DLL not found"
    exit 1
fi

echo ""
echo "Test 2: Verifying source files exist and contain expected code"
if grep -q "class IdServer" Platform/Platform.Examples/IdServer.cs; then
    echo "✓ IdServer class found in source"
else
    echo "✗ IdServer class not found in source"
    exit 1
fi

if grep -q "class IdServerCLI" Platform/Platform.Examples/IdServerCLI.cs; then
    echo "✓ IdServerCLI class found in source"
else
    echo "✗ IdServerCLI class not found in source"
    exit 1
fi

echo ""
echo "=== All Tests Passed ==="
echo ""
echo "Note: Runtime tests skipped because .NET Core 2.2 runtime is not available."
echo "The IdServer builds successfully and can be tested with .NET 2.2 runtime."
echo ""
echo "Manual testing instructions (requires .NET 2.2 runtime):"
echo "1. Start IdServer: dotnet run --project Platform/Platform.Data.IdServer/Platform.Data.IdServer.csproj"
echo "2. In another terminal, send ID requests using netcat or similar tool:"
echo "   echo 'REQUEST_ID_BLOCK:server1' | nc -u localhost 9999"
echo "3. Monitor responses on port 9998"
echo "4. Type 'STATUS' in IdServer console to see allocated blocks"
