# ProjectExecutor Examples

This directory contains examples demonstrating the usage of the ProjectExecutor tool.

## Overview

The ProjectExecutor is an automated tool that:
- Detects project types (C#/.NET, Rust, Node.js, Python, Makefile-based, etc.)
- Extracts setup instructions from README files
- Generates executable scripts for setting up and running projects
- Executes project setup and run commands automatically

This solves the problem described in [Issue #90](https://github.com/konard/LinksPlatform/issues/90) where manually setting up environments for different repositories was time-consuming.

## Usage

### 1. Detect Project Type

Detect the project type and see the execution plan:

```bash
cd experiments
dotnet run -- detect /path/to/project
```

Example output:
```
Project Type: DotNet
Steps Count: 3

Execution Steps:
  1. Restore dependencies
     Command: dotnet restore
  2. Build project
     Command: dotnet build
  3. Run project
     Command: dotnet run
```

### 2. Generate Execution Script

Generate a shell script that can execute the project:

```bash
cd experiments
dotnet run -- generate /path/to/project output.sh
```

This creates an executable script `output.sh` that automates the setup and execution.

### 3. Execute Project Directly

Automatically execute all setup and run steps:

```bash
cd experiments
dotnet run -- execute /path/to/project
```

## Supported Project Types

- **DotNet**: Detects `.csproj` files and uses `dotnet` commands
- **Rust**: Detects `Cargo.toml` and uses `cargo` commands
- **Node.js**: Detects `package.json` and uses `npm` commands
- **Python**: Detects `setup.py` and uses `pip`/`python` commands
- **Makefile**: Detects `Makefile` and uses `make` commands
- **Generic**: Falls back to parsing README instructions

## How It Works

1. **Project Detection**: Scans the directory for known project files (`.csproj`, `package.json`, etc.)
2. **README Parsing**: Extracts prerequisites, setup instructions, and command blocks from README files
3. **Plan Generation**: Creates an execution plan with ordered steps
4. **Script Generation**: Converts the plan into executable shell or batch scripts
5. **Execution**: Can directly execute the plan or generate scripts for later use

## Example: Testing with Mock Project

```bash
cd experiments
dotnet run
```

This runs automated tests that:
- Create a mock project with a README
- Detect the project type
- Extract instructions from the README
- Generate execution scripts
- Verify the parsing logic

## Integration

To use ProjectExecutor in your own code:

```csharp
using Platform.Examples;

var executor = new ProjectExecutor("/path/to/project");
var plan = executor.DetectAndGeneratePlan();

// Generate shell script
var shellScript = executor.GenerateShellScript(plan);
File.WriteAllText("run.sh", shellScript);

// Or generate batch script for Windows
var batchScript = executor.GenerateBatchScript(plan);
File.WriteAllText("run.bat", batchScript);
```

## Benefits

- **Time Savings**: No need to manually read and follow setup instructions
- **Consistency**: Same setup process works across all repositories
- **Automation**: Can be integrated into CI/CD pipelines
- **Documentation**: Generated scripts serve as executable documentation
- **Cross-platform**: Supports both Unix shell scripts and Windows batch files
