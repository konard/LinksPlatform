#!/bin/bash
# Experiment script to test the SequenceUnfoldIndex implementation

echo "=== Testing Sequence Unfold Index Implementation ==="
echo ""

# Set up test environment
TEST_DIR="test_sequence_index"
rm -rf "$TEST_DIR"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

echo "Test directory created: $TEST_DIR"
echo ""

# Build the project
echo "Building Platform.Examples project..."
dotnet build ../../Platform/Platform.sln --configuration Release

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo "Build successful!"
echo ""

# Run the SequenceUnfoldIndexCLI
echo "Running SequenceUnfoldIndexCLI demo..."
echo ""

# Note: This requires adding the CLI to the Sandbox or creating a console app
# For now, we'll document the expected usage
echo "To run the demo, use:"
echo "  dotnet run --project ../../Platform/Platform.Sandbox -- SequenceUnfoldIndexCLI test.links .index 10"
echo ""

# Check if the implementation files exist
echo "Checking implementation files..."
if [ -f "../../Platform/Platform.Examples/SequenceUnfoldIndex.cs" ]; then
    echo "✓ SequenceUnfoldIndex.cs exists"
else
    echo "✗ SequenceUnfoldIndex.cs not found"
fi

if [ -f "../../Platform/Platform.Examples/SequenceUnfolder.cs" ]; then
    echo "✓ SequenceUnfolder.cs exists"
else
    echo "✗ SequenceUnfolder.cs not found"
fi

if [ -f "../../Platform/Platform.Examples/SequenceUnfoldIndexCLI.cs" ]; then
    echo "✓ SequenceUnfoldIndexCLI.cs exists"
else
    echo "✗ SequenceUnfoldIndexCLI.cs not found"
fi

echo ""
echo "Experiment setup complete!"
echo ""
echo "Next steps:"
echo "1. Integrate SequenceUnfoldIndexCLI into Platform.Sandbox/Program.cs"
echo "2. Run the demo to verify functionality"
echo "3. Measure performance improvements for long sequences"

cd ..
