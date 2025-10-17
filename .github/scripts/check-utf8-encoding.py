#!/usr/bin/env python3
"""
Check that all text files in the repository are encoded in UTF-8.
This script validates both UTF-8 (with BOM) and UTF-8 (without BOM) encodings.
"""

import os
import sys
import chardet
from pathlib import Path

# File extensions to check
TEXT_EXTENSIONS = {
    '.cs', '.md', '.txt', '.json', '.yml', '.yaml', '.xml', '.html', '.cshtml',
    '.css', '.js', '.ts', '.tsx', '.jsx', '.sql', '.sh', '.bat', '.ps1',
    '.config', '.csproj', '.sln', '.props', '.targets', '.resx'
}

# Directories to skip
SKIP_DIRS = {
    '.git', 'node_modules', 'bin', 'obj', '__pycache__', '.vs',
    'packages', '.vscode', '.idea', 'dist', 'build'
}

# Files to skip
SKIP_FILES = {
    '.gitignore', '.gitattributes', '.editorconfig'
}


def is_text_file(filepath):
    """Check if a file should be validated based on its extension."""
    return filepath.suffix.lower() in TEXT_EXTENSIONS


def should_skip_path(path):
    """Check if a path should be skipped."""
    parts = path.parts
    for part in parts:
        if part in SKIP_DIRS:
            return True
    if path.name in SKIP_FILES:
        return True
    return False


def is_utf8_encoded(filepath):
    """
    Check if a file is encoded in UTF-8 (with or without BOM).
    Returns (is_valid, encoding, confidence)
    """
    try:
        # Try reading as UTF-8 with BOM
        with open(filepath, 'rb') as f:
            raw_data = f.read()

        # Check for UTF-8 BOM
        if raw_data.startswith(b'\xef\xbb\xbf'):
            return True, 'UTF-8-SIG (with BOM)', 100

        # Try decoding as UTF-8
        try:
            raw_data.decode('utf-8')
            return True, 'UTF-8', 100
        except UnicodeDecodeError:
            pass

        # Use chardet to detect encoding
        result = chardet.detect(raw_data)
        encoding = result.get('encoding', 'unknown')
        confidence = result.get('confidence', 0) * 100

        # Consider ASCII as valid UTF-8 (ASCII is a subset of UTF-8)
        if encoding and encoding.upper() in ['ASCII', 'UTF-8']:
            return True, encoding, confidence

        return False, encoding, confidence

    except Exception as e:
        return False, f'Error: {str(e)}', 0


def check_repository(repo_path):
    """
    Check all text files in the repository for UTF-8 encoding.
    Returns a list of files that are not UTF-8 encoded.
    """
    repo_path = Path(repo_path)
    non_utf8_files = []
    checked_files = 0

    for root, dirs, files in os.walk(repo_path):
        # Skip directories in-place
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]

        for filename in files:
            filepath = Path(root) / filename

            # Skip based on path
            if should_skip_path(filepath.relative_to(repo_path)):
                continue

            # Check only text files
            if not is_text_file(filepath):
                continue

            checked_files += 1
            is_valid, encoding, confidence = is_utf8_encoded(filepath)

            if not is_valid:
                relative_path = filepath.relative_to(repo_path)
                non_utf8_files.append({
                    'path': str(relative_path),
                    'encoding': encoding,
                    'confidence': confidence
                })
                print(f"❌ {relative_path}")
                print(f"   Detected: {encoding} (confidence: {confidence:.1f}%)")
            else:
                print(f"✓ {filepath.relative_to(repo_path)} [{encoding}]")

    return non_utf8_files, checked_files


def main():
    """Main function to run the encoding check."""
    print("=" * 70)
    print("UTF-8 Encoding Checker")
    print("=" * 70)
    print()

    # Get repository root
    repo_path = os.environ.get('GITHUB_WORKSPACE', '.')

    print(f"Checking files in: {repo_path}")
    print(f"Text file extensions: {', '.join(sorted(TEXT_EXTENSIONS))}")
    print()

    non_utf8_files, checked_files = check_repository(repo_path)

    print()
    print("=" * 70)
    print(f"Total files checked: {checked_files}")

    if non_utf8_files:
        print(f"Files with non-UTF-8 encoding: {len(non_utf8_files)}")
        print()
        print("The following files are not encoded in UTF-8:")
        for file_info in non_utf8_files:
            print(f"  - {file_info['path']}")
            print(f"    Detected: {file_info['encoding']} (confidence: {file_info['confidence']:.1f}%)")
        print()
        print("Please convert these files to UTF-8 encoding.")
        sys.exit(1)
    else:
        print("✓ All text files are properly encoded in UTF-8!")
        print()
        sys.exit(0)


if __name__ == '__main__':
    main()
