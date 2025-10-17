#!/usr/bin/env python3
"""
Test script for cppkg package manager
"""

import os
import sys
import shutil
import tempfile
import subprocess
from pathlib import Path

# Add src to path
sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

def run_cppkg(args):
    """Run cppkg command"""
    cppkg_path = Path(__file__).parent.parent / "src" / "cppkg.py"
    result = subprocess.run([sys.executable, str(cppkg_path)] + args,
                          capture_output=True, text=True)
    return result

def test_init_binary():
    """Test creating a binary package"""
    print("Test: Initialize binary package...")

    with tempfile.TemporaryDirectory() as tmpdir:
        os.chdir(tmpdir)

        result = run_cppkg(["init", "testapp", "--binary"])
        assert result.returncode == 0, f"Init failed: {result.stderr}"

        # Check files were created
        assert Path("package.toml").exists(), "package.toml not created"
        assert Path("CMakeLists.txt").exists(), "CMakeLists.txt not created"
        assert Path("src/main.cpp").exists(), "src/main.cpp not created"

        # Check package.toml content
        content = Path("package.toml").read_text()
        assert "testapp" in content, "Package name not in manifest"
        assert "binary" in content, "Package type not in manifest"

        print("✓ Binary package initialization works")

def test_init_library():
    """Test creating a library package"""
    print("Test: Initialize library package...")

    with tempfile.TemporaryDirectory() as tmpdir:
        os.chdir(tmpdir)

        result = run_cppkg(["init", "mylib"])
        assert result.returncode == 0, f"Init failed: {result.stderr}"

        # Check files were created
        assert Path("package.toml").exists(), "package.toml not created"
        assert Path("CMakeLists.txt").exists(), "CMakeLists.txt not created"
        assert Path("include/mylib.hpp").exists(), "include/mylib.hpp not created"
        assert Path("src/mylib.cpp").exists(), "src/mylib.cpp not created"

        # Check package.toml content
        content = Path("package.toml").read_text()
        assert "mylib" in content, "Package name not in manifest"
        assert "library" in content, "Package type not in manifest"

        print("✓ Library package initialization works")

def test_add_dependency():
    """Test adding a dependency"""
    print("Test: Add dependency...")

    with tempfile.TemporaryDirectory() as tmpdir:
        os.chdir(tmpdir)

        # Initialize project
        run_cppkg(["init", "myproject"])

        # Add dependency
        result = run_cppkg(["add", "mylib", "1.0.0"])
        assert result.returncode == 0, f"Add failed: {result.stderr}"

        # Check manifest was updated
        content = Path("package.toml").read_text()
        assert "mylib" in content, "Dependency not added to manifest"
        assert "1.0.0" in content, "Version not in manifest"

        print("✓ Adding dependencies works")

def test_cmake_generation():
    """Test CMakeLists.txt generation"""
    print("Test: CMake generation...")

    with tempfile.TemporaryDirectory() as tmpdir:
        os.chdir(tmpdir)

        # Create library
        run_cppkg(["init", "mylib"])

        cmake_content = Path("CMakeLists.txt").read_text()
        assert "cmake_minimum_required" in cmake_content, "CMake version not set"
        assert "project(mylib)" in cmake_content, "Project name not set"
        assert "add_library" in cmake_content, "add_library not in CMakeLists"
        assert "target_include_directories" in cmake_content, "Include dirs not set"

        print("✓ CMake generation works")

def test_help():
    """Test help command"""
    print("Test: Help command...")

    result = run_cppkg(["--help"])
    assert result.returncode == 0, "Help command failed"
    assert "cppkg" in result.stdout, "Help text doesn't mention cppkg"
    assert "init" in result.stdout, "Help text doesn't mention init command"

    print("✓ Help command works")

def main():
    """Run all tests"""
    print("=" * 50)
    print("Running cppkg tests")
    print("=" * 50)
    print()

    # Save current directory
    original_dir = os.getcwd()

    try:
        tests = [
            test_help,
            test_init_binary,
            test_init_library,
            test_add_dependency,
            test_cmake_generation,
        ]

        for test in tests:
            try:
                test()
            except AssertionError as e:
                print(f"✗ Test failed: {e}")
                return 1
            except Exception as e:
                print(f"✗ Test error: {e}")
                import traceback
                traceback.print_exc()
                return 1

        print()
        print("=" * 50)
        print(f"All {len(tests)} tests passed! ✓")
        print("=" * 50)
        return 0

    finally:
        # Restore original directory
        os.chdir(original_dir)

if __name__ == "__main__":
    sys.exit(main())
