#!/usr/bin/env python3
"""
cppkg - A simple C++ package manager
Inspired by cargo (Rust) and npm (JavaScript)
"""

import argparse
import json
import os
import sys
import subprocess
import shutil
from pathlib import Path
from typing import Dict, List, Optional

# Try to use built-in tomllib (Python 3.11+) or fall back to toml
try:
    import tomllib
    # tomllib only supports reading, we need a write function
    import tomli_w as toml_write
    def toml_load(f):
        return tomllib.loads(f.read())
    def toml_dump(data, f):
        f.write(toml_write.dumps(data))
except ImportError:
    try:
        import toml
        def toml_load(f):
            return toml.load(f)
        def toml_dump(data, f):
            toml.dump(data, f)
    except ImportError:
        # Simple TOML parser/writer as fallback
        import re
        def toml_load(f):
            data = {}
            current_section = data
            for line in f:
                line = line.strip()
                if not line or line.startswith('#'):
                    continue
                if line.startswith('[') and line.endswith(']'):
                    section_name = line[1:-1]
                    current_section = data.setdefault(section_name, {})
                elif '=' in line:
                    key, value = line.split('=', 1)
                    key = key.strip()
                    value = value.strip().strip('"\'')
                    current_section[key] = value
            return data

        def toml_dump(data, f):
            for section, values in data.items():
                f.write(f'[{section}]\n')
                if isinstance(values, dict):
                    for key, value in values.items():
                        if isinstance(value, str):
                            f.write(f'{key} = "{value}"\n')
                        elif isinstance(value, dict):
                            # Nested structure
                            f.write(f'{key} = {{ ')
                            items = [f'{k} = "{v}"' if isinstance(v, str) else f'{k} = {v}'
                                   for k, v in value.items()]
                            f.write(', '.join(items))
                            f.write(' }\n')
                        else:
                            f.write(f'{key} = {value}\n')
                f.write('\n')


class PackageManager:
    """Main package manager class"""

    def __init__(self):
        self.manifest_file = "package.toml"
        self.packages_dir = Path.home() / ".cppkg" / "packages"
        self.cache_dir = Path.home() / ".cppkg" / "cache"
        self.packages_dir.mkdir(parents=True, exist_ok=True)
        self.cache_dir.mkdir(parents=True, exist_ok=True)

    def init(self, name: str, binary: bool = False):
        """Initialize a new C++ package"""
        if Path(self.manifest_file).exists():
            print(f"Error: {self.manifest_file} already exists")
            return False

        package_type = "binary" if binary else "library"

        manifest = {
            "package": {
                "name": name,
                "version": "0.1.0",
                "description": f"A {package_type} package",
                "authors": [],
                "license": "MIT",
                "type": package_type
            },
            "dependencies": {},
            "build": {
                "standard": "c++17",
                "cmake_minimum": "3.15"
            }
        }

        with open(self.manifest_file, "w") as f:
            toml_dump(manifest, f)

        # Create directory structure
        if binary:
            Path("src").mkdir(exist_ok=True)
            Path("src/main.cpp").write_text('''#include <iostream>

int main() {
    std::cout << "Hello from ''' + name + '''!" << std::endl;
    return 0;
}
''')
        else:
            Path("include").mkdir(exist_ok=True)
            Path("src").mkdir(exist_ok=True)
            Path(f"include/{name}.hpp").write_text(f'''#pragma once

namespace {name} {{
    void hello();
}}
''')
            Path("src").mkdir(exist_ok=True)
            Path(f"src/{name}.cpp").write_text(f'''#include "{name}.hpp"
#include <iostream>

namespace {name} {{
    void hello() {{
        std::cout << "Hello from {name}!" << std::endl;
    }}
}}
''')

        # Create CMakeLists.txt
        self._generate_cmake(manifest)

        print(f"Created {package_type} package: {name}")
        return True

    def _generate_cmake(self, manifest: Dict):
        """Generate CMakeLists.txt from manifest"""
        name = manifest["package"]["name"]
        pkg_type = manifest["package"]["type"]
        standard = manifest["build"]["standard"]
        cmake_min = manifest["build"]["cmake_minimum"]

        cmake_content = f'''cmake_minimum_required(VERSION {cmake_min})
project({name})

set(CMAKE_CXX_STANDARD {standard.replace("c++", "")})
set(CMAKE_CXX_STANDARD_REQUIRED ON)

'''

        if pkg_type == "library":
            cmake_content += f'''add_library({name} src/{name}.cpp)
target_include_directories({name} PUBLIC include)
'''
        else:
            cmake_content += f'''add_executable({name} src/main.cpp)
'''

        # Add dependencies
        for dep_name, dep_info in manifest.get("dependencies", {}).items():
            cmake_content += f'''
# Dependency: {dep_name}
find_package({dep_name} REQUIRED)
target_link_libraries({name} PRIVATE {dep_name})
'''

        Path("CMakeLists.txt").write_text(cmake_content)

    def install(self):
        """Install dependencies from package.toml"""
        if not Path(self.manifest_file).exists():
            print(f"Error: {self.manifest_file} not found")
            return False

        with open(self.manifest_file) as f:
            manifest = toml_load(f)

        dependencies = manifest.get("dependencies", {})
        if not dependencies:
            print("No dependencies to install")
            return True

        print(f"Installing {len(dependencies)} dependencies...")

        for dep_name, dep_info in dependencies.items():
            if isinstance(dep_info, str):
                version = dep_info
                source = None
            else:
                version = dep_info.get("version", "latest")
                source = dep_info.get("git") or dep_info.get("path")

            self._install_dependency(dep_name, version, source)

        print("Dependencies installed successfully")
        return True

    def _install_dependency(self, name: str, version: str, source: Optional[str]):
        """Install a single dependency"""
        dep_dir = self.packages_dir / f"{name}-{version}"

        if dep_dir.exists():
            print(f"  {name} {version} - already installed")
            return

        print(f"  Installing {name} {version}...")

        if source and source.startswith("http"):
            # Git repository
            subprocess.run(["git", "clone", source, str(dep_dir)],
                         capture_output=True, check=True)
            if version != "latest":
                subprocess.run(["git", "checkout", version],
                             cwd=str(dep_dir), capture_output=True, check=True)
        elif source:
            # Local path
            shutil.copytree(source, dep_dir)
        else:
            print(f"    Warning: No source specified for {name}, skipping")
            return

        # Build the dependency if it has CMakeLists.txt
        if (dep_dir / "CMakeLists.txt").exists():
            build_dir = dep_dir / "build"
            build_dir.mkdir(exist_ok=True)
            subprocess.run(["cmake", ".."], cwd=str(build_dir),
                         capture_output=True, check=True)
            subprocess.run(["cmake", "--build", "."], cwd=str(build_dir),
                         capture_output=True, check=True)

    def build(self):
        """Build the current package"""
        if not Path(self.manifest_file).exists():
            print(f"Error: {self.manifest_file} not found")
            return False

        # Check if CMake is installed
        if not shutil.which("cmake"):
            print("Error: CMake is not installed")
            print("Please install CMake: https://cmake.org/download/")
            return False

        print("Building package...")

        build_dir = Path("build")
        build_dir.mkdir(exist_ok=True)

        try:
            subprocess.run(["cmake", ".."], cwd=str(build_dir), check=True)
            subprocess.run(["cmake", "--build", "."], cwd=str(build_dir), check=True)
            print("Build completed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"Build failed: {e}")
            return False
        except FileNotFoundError:
            print("Error: CMake not found. Please install CMake.")
            return False

    def run(self):
        """Build and run the package (for binary packages)"""
        if not Path(self.manifest_file).exists():
            print(f"Error: {self.manifest_file} not found")
            return False

        with open(self.manifest_file) as f:
            manifest = toml_load(f)

        if manifest["package"]["type"] != "binary":
            print("Error: 'run' command only works with binary packages")
            return False

        if not self.build():
            return False

        name = manifest["package"]["name"]
        executable = Path("build") / name

        if not executable.exists():
            # Try with .exe extension for Windows
            executable = Path("build") / f"{name}.exe"
            if not executable.exists():
                print(f"Error: Executable not found: {name}")
                return False

        print(f"\nRunning {name}...")
        print("-" * 40)
        subprocess.run([str(executable)])
        return True

    def publish(self, registry: Optional[str] = None):
        """Publish package to registry"""
        if not Path(self.manifest_file).exists():
            print(f"Error: {self.manifest_file} not found")
            return False

        with open(self.manifest_file) as f:
            manifest = toml_load(f)

        name = manifest["package"]["name"]
        version = manifest["package"]["version"]

        print(f"Publishing {name} {version}...")

        # Create package archive
        archive_name = f"{name}-{version}.tar.gz"

        # For now, just create a local copy in cache
        package_cache = self.cache_dir / name / version
        package_cache.mkdir(parents=True, exist_ok=True)

        # Copy package files
        for item in Path(".").iterdir():
            if item.name not in [".git", "build", "__pycache__", ".cppkg"]:
                if item.is_file():
                    shutil.copy2(item, package_cache)
                elif item.is_dir():
                    shutil.copytree(item, package_cache / item.name,
                                  dirs_exist_ok=True)

        print(f"Package published to local cache: {package_cache}")
        print("\nNote: Remote registry publishing will be implemented in future versions")
        return True

    def add(self, package: str, version: str = "latest", git: Optional[str] = None):
        """Add a dependency to package.toml"""
        if not Path(self.manifest_file).exists():
            print(f"Error: {self.manifest_file} not found")
            return False

        with open(self.manifest_file) as f:
            manifest = toml_load(f)

        if "dependencies" not in manifest:
            manifest["dependencies"] = {}

        if git:
            manifest["dependencies"][package] = {"version": version, "git": git}
        else:
            manifest["dependencies"][package] = version

        with open(self.manifest_file, "w") as f:
            toml_dump(manifest, f)

        print(f"Added {package} {version} to dependencies")

        # Regenerate CMakeLists.txt
        self._generate_cmake(manifest)

        return True


def main():
    parser = argparse.ArgumentParser(
        description="cppkg - A simple C++ package manager",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  cppkg init mylib              Create a new library package
  cppkg init myapp --binary     Create a new binary package
  cppkg add mylib 1.0.0         Add a dependency
  cppkg install                 Install dependencies
  cppkg build                   Build the package
  cppkg run                     Build and run (binary packages)
  cppkg publish                 Publish package to registry
        """
    )

    subparsers = parser.add_subparsers(dest="command", help="Available commands")

    # init command
    init_parser = subparsers.add_parser("init", help="Initialize a new package")
    init_parser.add_argument("name", help="Package name")
    init_parser.add_argument("--binary", action="store_true",
                           help="Create a binary package instead of library")

    # install command
    subparsers.add_parser("install", help="Install dependencies")

    # build command
    subparsers.add_parser("build", help="Build the package")

    # run command
    subparsers.add_parser("run", help="Build and run the package")

    # publish command
    publish_parser = subparsers.add_parser("publish", help="Publish package")
    publish_parser.add_argument("--registry", help="Registry URL")

    # add command
    add_parser = subparsers.add_parser("add", help="Add a dependency")
    add_parser.add_argument("package", help="Package name")
    add_parser.add_argument("version", nargs="?", default="latest",
                          help="Package version (default: latest)")
    add_parser.add_argument("--git", help="Git repository URL")

    args = parser.parse_args()

    if not args.command:
        parser.print_help()
        return 1

    pm = PackageManager()

    if args.command == "init":
        success = pm.init(args.name, args.binary)
    elif args.command == "install":
        success = pm.install()
    elif args.command == "build":
        success = pm.build()
    elif args.command == "run":
        success = pm.run()
    elif args.command == "publish":
        success = pm.publish(getattr(args, "registry", None))
    elif args.command == "add":
        success = pm.add(args.package, args.version, getattr(args, "git", None))
    else:
        parser.print_help()
        return 1

    return 0 if success else 1


if __name__ == "__main__":
    sys.exit(main())
