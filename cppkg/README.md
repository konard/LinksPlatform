# cppkg - A Simple C++ Package Manager

A modern, easy-to-use package manager for C++ inspired by cargo (Rust) and npm (JavaScript).

## Features

- 🚀 **Simple to use** - Intuitive CLI commands like `init`, `install`, `build`, `run`
- 📦 **Dependency management** - Automatic dependency resolution and installation
- 🔧 **CMake integration** - Automatic CMakeLists.txt generation
- 📝 **TOML manifest** - Clean, readable `package.toml` configuration
- 🔄 **Git support** - Install dependencies directly from Git repositories
- 📤 **Publishing** - Publish both source code libraries and binary applications
- 🌐 **Cross-platform** - Works on Linux, macOS, and Windows

## Installation

### Quick Install

```bash
# Clone or download cppkg
git clone https://github.com/konard/LinksPlatform.git
cd LinksPlatform/cppkg

# Install globally (optional)
sudo cp src/cppkg.py /usr/local/bin/cppkg
sudo chmod +x /usr/local/bin/cppkg

# Or create an alias
echo 'alias cppkg="python3 /path/to/cppkg/src/cppkg.py"' >> ~/.bashrc
source ~/.bashrc
```

### Requirements

- Python 3.6+
- CMake 3.15+
- C++ compiler (GCC, Clang, or MSVC)
- Git (for Git dependencies)

## Quick Start

### Creating a Library

```bash
# Initialize a new library
cppkg init mylib

# Build the library
cppkg build
```

This creates:
```
mylib/
├── package.toml        # Package manifest
├── CMakeLists.txt      # Generated CMake file
├── include/
│   └── mylib.hpp      # Public header
└── src/
    └── mylib.cpp      # Implementation
```

### Creating an Application

```bash
# Initialize a new binary application
cppkg init myapp --binary

# Build and run
cppkg run
```

This creates:
```
myapp/
├── package.toml       # Package manifest
├── CMakeLists.txt     # Generated CMake file
└── src/
    └── main.cpp       # Entry point
```

## Usage

### Commands

#### `cppkg init <name> [--binary]`
Initialize a new C++ package

```bash
cppkg init mylib              # Create a library
cppkg init myapp --binary     # Create an application
```

#### `cppkg add <package> [version] [--git <url>]`
Add a dependency to your package

```bash
cppkg add json 3.11.0 --git https://github.com/nlohmann/json.git
cppkg add mylib latest
```

#### `cppkg install`
Install all dependencies from package.toml

```bash
cppkg install
```

#### `cppkg build`
Build the current package

```bash
cppkg build
```

#### `cppkg run`
Build and run the package (binary packages only)

```bash
cppkg run
```

#### `cppkg publish [--registry <url>]`
Publish package to registry

```bash
cppkg publish
```

## Package Manifest (package.toml)

### Library Example

```toml
[package]
name = "mylib"
version = "0.1.0"
description = "A library package"
authors = []
license = "MIT"
type = "library"

[dependencies]
# Simple version
somelib = "1.0.0"

# Git repository
json = { version = "3.11.0", git = "https://github.com/nlohmann/json.git" }

[build]
standard = "c++17"
cmake_minimum = "3.15"
```

### Binary Example

```toml
[package]
name = "myapp"
version = "0.1.0"
description = "A binary package"
authors = []
license = "MIT"
type = "binary"

[dependencies]
mylib = "0.1.0"

[build]
standard = "c++17"
cmake_minimum = "3.15"
```

## Comparison with Other Tools

| Feature | cppkg | cargo | npm | conan | vcpkg |
|---------|-------|-------|-----|-------|-------|
| Easy to use | ✅ | ✅ | ✅ | ⚠️ | ⚠️ |
| Simple manifest | ✅ | ✅ | ✅ | ⚠️ | ❌ |
| CMake integration | ✅ | N/A | N/A | ✅ | ✅ |
| Git dependencies | ✅ | ✅ | ✅ | ✅ | ✅ |
| Source templates | ✅ | ✅ | ❌ | ✅ | ❌ |
| Binary publishing | ✅ | ✅ | ✅ | ✅ | ✅ |

## Examples

### Example 1: Simple Library

```bash
# Create library
cppkg init mathlib

# Edit include/mathlib.hpp
cat > include/mathlib.hpp << 'EOF'
#pragma once

namespace mathlib {
    int add(int a, int b);
}
EOF

# Edit src/mathlib.cpp
cat > src/mathlib.cpp << 'EOF'
#include "mathlib.hpp"

namespace mathlib {
    int add(int a, int b) {
        return a + b;
    }
}
EOF

# Build
cppkg build
```

### Example 2: Application with Dependencies

```bash
# Create application
cppkg init calculator --binary

# Add dependency
cppkg add mathlib 0.1.0 --git https://github.com/user/mathlib.git

# Install dependencies
cppkg install

# Build and run
cppkg run
```

### Example 3: Publishing

```bash
# After developing your package
cppkg publish

# Your package is now available for others to use
```

## Architecture

cppkg uses a simple architecture:

1. **Manifest Parser** - Reads and validates `package.toml`
2. **Dependency Resolver** - Resolves and fetches dependencies
3. **Build System** - Generates and executes CMake builds
4. **Package Registry** - Manages local and remote package caches

```
~/.cppkg/
├── packages/          # Installed dependencies
│   ├── mylib-1.0.0/
│   └── json-3.11.0/
└── cache/            # Published packages
    └── mylib/
        └── 1.0.0/
```

## Roadmap

- [x] Basic package initialization
- [x] Dependency management
- [x] CMake integration
- [x] Git dependencies
- [x] Local publishing
- [ ] Remote registry support
- [ ] Binary caching
- [ ] Version resolution algorithms
- [ ] Package search command
- [ ] Update command
- [ ] Lock file support
- [ ] Workspace support (multiple packages)
- [ ] Custom build scripts
- [ ] Pre/post build hooks

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## License

MIT License - See LICENSE file for details

## Design Philosophy

cppkg follows these principles:

1. **Simplicity First** - Easy to learn and use, like cargo and npm
2. **Convention over Configuration** - Sensible defaults, minimal boilerplate
3. **CMake Native** - Works with existing CMake projects
4. **Git-Friendly** - Direct Git repository dependencies
5. **Developer Experience** - Fast, intuitive, reliable

## FAQ

**Q: Why another C++ package manager?**
A: Existing tools (conan, vcpkg) are powerful but complex. cppkg aims to be as simple as cargo or npm.

**Q: Can I use cppkg with existing CMake projects?**
A: Yes! Just add a `package.toml` file to your project.

**Q: Does cppkg replace CMake?**
A: No, cppkg generates and uses CMake files. It's a package manager, not a build system.

**Q: How do I publish to a custom registry?**
A: Custom registry support is planned for future versions.

**Q: Can I use cppkg packages with conan/vcpkg?**
A: Yes, cppkg packages are just CMake projects and can be used by other tools.

## Credits

Inspired by:
- [Cargo](https://doc.rust-lang.org/cargo/) - Rust's package manager
- [npm](https://www.npmjs.com/) - JavaScript's package manager
- [Conan](https://conan.io/) - C++ package manager
- [vcpkg](https://vcpkg.io/) - C++ library manager

---

Made with ❤️ for the C++ community
