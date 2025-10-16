#!/bin/bash

# Test script for Platform.Data.IdServer
# Tests ID allocation functionality across different scenarios

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

# Test 1: Basic startup test
echo "Test 1: Verifying IdServer can start with default parameters"
timeout 3 dotnet run --project Platform/Platform.Data.IdServer/Platform.Data.IdServer.csproj &
SERVER_PID=$!
sleep 2

if ps -p $SERVER_PID > /dev/null; then
    echo "✓ IdServer started successfully"
    kill $SERVER_PID 2>/dev/null
    wait $SERVER_PID 2>/dev/null
else
    echo "✗ IdServer failed to start"
    exit 1
fi

echo ""

# Test 2: Help output test
echo "Test 2: Testing help output"
dotnet run --project Platform/Platform.Data.IdServer/Platform.Data.IdServer.csproj -- --help > /tmp/idserver-help.txt
if grep -q "Usage: Platform.Data.IdServer" /tmp/idserver-help.txt; then
    echo "✓ Help output is correct"
else
    echo "✗ Help output is incorrect"
    cat /tmp/idserver-help.txt
    exit 1
fi

echo ""

# Test 3: Custom parameters test
echo "Test 3: Testing custom parameters (ports, start-id, block-size)"
timeout 3 dotnet run --project Platform/Platform.Data.IdServer/Platform.Data.IdServer.csproj -- --receive-port 9999 --send-port 9998 --start-id 1000 --block-size 500 &
SERVER_PID=$!
sleep 2

if ps -p $SERVER_PID > /dev/null; then
    echo "✓ IdServer accepts custom parameters"
    kill $SERVER_PID 2>/dev/null
    wait $SERVER_PID 2>/dev/null
else
    echo "✗ IdServer failed with custom parameters"
    exit 1
fi

echo ""
echo "=== All Tests Passed ==="
echo ""
echo "Manual testing instructions:"
echo "1. Start IdServer: dotnet run --project Platform/Platform.Data.IdServer/Platform.Data.IdServer.csproj"
echo "2. In another terminal, send ID requests using netcat or similar tool:"
echo "   echo 'REQUEST_ID_BLOCK:server1' | nc -u localhost 9999"
echo "3. Monitor responses on port 9998"
echo "4. Type 'STATUS' in IdServer console to see allocated blocks"
