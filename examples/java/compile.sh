#!/bin/bash
# Compile script for Triple Links Java Example

# Check if JNA jar is provided
if [ -z "$1" ]; then
    echo "Usage: ./compile.sh <path-to-jna.jar>"
    echo "Example: ./compile.sh jna-5.13.0.jar"
    echo ""
    echo "Download JNA from: https://github.com/java-native-access/jna/releases"
    exit 1
fi

JNA_JAR="$1"

if [ ! -f "$JNA_JAR" ]; then
    echo "Error: JNA jar not found at: $JNA_JAR"
    exit 1
fi

echo "Compiling Triple Links Java Example..."
echo "Using JNA jar: $JNA_JAR"

javac -classpath ".:$JNA_JAR" TripletsLibrary.java TripleLinksExample.java

if [ $? -eq 0 ]; then
    echo "Compilation successful!"
    echo ""
    echo "To run the example:"
    echo "  export LD_LIBRARY_PATH=.:\$LD_LIBRARY_PATH"
    echo "  java -classpath .:$JNA_JAR TripleLinksExample"
else
    echo "Compilation failed!"
    exit 1
fi
