#!/bin/bash
# Example: Creating a C++ application with cppkg

set -e

echo "=== Creating a C++ application with cppkg ==="
echo

# Create a new binary
echo "1. Creating new binary 'myapp'..."
cd /tmp/cppkg-demo
mkdir -p myapp && cd myapp
python3 ../../cppkg/src/cppkg.py init myapp --binary
echo

# Show generated files
echo "2. Generated files:"
ls -la
echo

echo "3. Package manifest (package.toml):"
cat package.toml
echo

echo "4. Generated main.cpp:"
cat src/main.cpp
echo

echo "5. Generated CMakeLists.txt:"
cat CMakeLists.txt
echo

# Build and run
echo "6. Building and running the application..."
python3 ../../cppkg/src/cppkg.py run
echo

echo "=== Application created and run successfully! ==="
