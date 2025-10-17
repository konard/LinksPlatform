#!/bin/bash

# Script to compile and run the Matrix Indices demonstration

echo "=== Compiling Matrix Indices Demo ==="
csc MatrixIndicesDemo.cs -out:MatrixIndicesDemo.exe

if [ $? -eq 0 ]; then
    echo ""
    echo "=== Running Matrix Indices Demo ==="
    mono MatrixIndicesDemo.exe || dotnet MatrixIndicesDemo.exe || ./MatrixIndicesDemo.exe
else
    echo "Compilation failed. Trying with dotnet..."
    # Create a minimal project file
    cat > MatrixIndicesDemo.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
</Project>
EOF

    echo "Running with dotnet..."
    dotnet run
fi
