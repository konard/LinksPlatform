#!/bin/bash
# Run script for Triple Links Java Example

# Check if JNA jar is provided
if [ -z "$1" ]; then
    echo "Usage: ./run.sh <path-to-jna.jar>"
    echo "Example: ./run.sh jna-5.13.0.jar"
    exit 1
fi

JNA_JAR="$1"

if [ ! -f "$JNA_JAR" ]; then
    echo "Error: JNA jar not found at: $JNA_JAR"
    exit 1
fi

# Check if compiled
if [ ! -f "TripleLinksExample.class" ]; then
    echo "Error: Example not compiled. Run ./compile.sh first."
    exit 1
fi

echo "Running Triple Links Java Example..."
echo "Using JNA jar: $JNA_JAR"
echo ""

# Set library path to include current directory
export LD_LIBRARY_PATH=.:${LD_LIBRARY_PATH}
export DYLD_LIBRARY_PATH=.:${DYLD_LIBRARY_PATH}

java -classpath ".:$JNA_JAR" TripleLinksExample
