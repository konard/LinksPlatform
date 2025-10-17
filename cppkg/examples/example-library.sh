#!/bin/bash
# Example: Creating a C++ library with cppkg

set -e

echo "=== Creating a C++ library with cppkg ==="
echo

# Create a new library
echo "1. Creating new library 'mathlib'..."
cd /tmp/cppkg-demo
mkdir -p mathlib && cd mathlib
python3 ../../cppkg/src/cppkg.py init mathlib
echo

# Show generated files
echo "2. Generated files:"
ls -la
echo

echo "3. Package manifest (package.toml):"
cat package.toml
echo

echo "4. Generated header file:"
cat include/mathlib.hpp
echo

echo "5. Generated source file:"
cat src/mathlib.cpp
echo

echo "6. Generated CMakeLists.txt:"
cat CMakeLists.txt
echo

# Build the library
echo "7. Building the library..."
python3 ../../cppkg/src/cppkg.py build
echo

echo "=== Library created successfully! ==="
