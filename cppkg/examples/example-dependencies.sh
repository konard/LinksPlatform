#!/bin/bash
# Example: Managing dependencies with cppkg

set -e

echo "=== Managing dependencies with cppkg ==="
echo

cd /tmp/cppkg-demo
mkdir -p myproject && cd myproject

# Initialize project
echo "1. Initializing project..."
python3 ../../cppkg/src/cppkg.py init myproject
echo

# Add dependencies
echo "2. Adding dependencies..."
python3 ../../cppkg/src/cppkg.py add json 3.11.0 --git https://github.com/nlohmann/json.git
echo

echo "3. Updated package.toml:"
cat package.toml
echo

# Install dependencies
echo "4. Installing dependencies..."
python3 ../../cppkg/src/cppkg.py install
echo

echo "=== Dependencies managed successfully! ==="
