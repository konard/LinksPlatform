#!/bin/bash
# Installation script for cppkg

set -e

echo "Installing cppkg - C++ Package Manager"
echo "======================================"
echo

# Check Python
if ! command -v python3 &> /dev/null; then
    echo "Error: Python 3 is required but not installed"
    exit 1
fi

echo "✓ Python 3 found: $(python3 --version)"

# Check CMake
if ! command -v cmake &> /dev/null; then
    echo "Warning: CMake is not installed. It's required for building packages."
    echo "Install CMake: https://cmake.org/download/"
fi

# Install Python dependencies
echo
echo "Installing Python dependencies..."
python3 -m pip install --user toml 2>/dev/null || {
    echo "Installing toml package..."
    python3 -c "import sys, subprocess; subprocess.check_call([sys.executable, '-m', 'pip', 'install', '--user', 'toml'])"
}

echo "✓ Dependencies installed"

# Determine installation method
echo
echo "Choose installation method:"
echo "1) Install globally (requires sudo)"
echo "2) Install for current user (recommended)"
echo "3) Create alias only"
read -p "Enter choice [1-3]: " choice

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CPPKG_PATH="$SCRIPT_DIR/src/cppkg.py"

case $choice in
    1)
        echo "Installing globally to /usr/local/bin/cppkg..."
        sudo cp "$CPPKG_PATH" /usr/local/bin/cppkg
        sudo chmod +x /usr/local/bin/cppkg
        echo "✓ Installed to /usr/local/bin/cppkg"
        ;;
    2)
        echo "Installing to ~/.local/bin/cppkg..."
        mkdir -p ~/.local/bin
        cp "$CPPKG_PATH" ~/.local/bin/cppkg
        chmod +x ~/.local/bin/cppkg

        # Add to PATH if not already there
        if [[ ":$PATH:" != *":$HOME/.local/bin:"* ]]; then
            echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
            echo "✓ Added ~/.local/bin to PATH in ~/.bashrc"
            echo "  Run: source ~/.bashrc"
        fi

        echo "✓ Installed to ~/.local/bin/cppkg"
        ;;
    3)
        echo "Creating alias..."
        ALIAS_LINE="alias cppkg='python3 $CPPKG_PATH'"

        if ! grep -q "alias cppkg=" ~/.bashrc; then
            echo "$ALIAS_LINE" >> ~/.bashrc
            echo "✓ Added alias to ~/.bashrc"
            echo "  Run: source ~/.bashrc"
        else
            echo "✓ Alias already exists in ~/.bashrc"
        fi
        ;;
    *)
        echo "Invalid choice. Exiting."
        exit 1
        ;;
esac

echo
echo "Installation complete! 🎉"
echo
echo "Try it out:"
echo "  cppkg init myproject"
echo
echo "For more information, see README.md"
