# Platform.Tests.Generator

Automatic test generation tool for LinksPlatform projects to maximize test coverage.

## Overview

This tool analyzes C# source code and automatically generates test scaffolding using xUnit, helping developers quickly bootstrap test projects with basic test structure.

## Features

- Parses C# source files using Roslyn (Microsoft.CodeAnalysis)
- Generates xUnit test classes for public methods
- Creates test scaffolding following Arrange-Act-Assert pattern
- Preserves namespace structure in test projects
- Automatically imports necessary using directives

## Usage

```bash
dotnet run --project Platform.Tests.Generator <source-project-path> <test-project-path>
```

### Example

```bash
dotnet run --project Platform.Tests.Generator ../Platform.Examples ../Platform.Examples.Tests
```

This will:
1. Scan all `.cs` files in `Platform.Examples`
2. Generate corresponding test files in `Platform.Examples.Tests`
3. Create basic test methods for all public methods found

## Generated Test Structure

The tool generates tests following this structure:

```csharp
using System;
using Xunit;

namespace YourNamespace.Tests
{
    public class YourClassTests
    {
        [Fact]
        public void YourMethodTest()
        {
            // Arrange
            // TODO: Set up test data and dependencies

            // Act
            // TODO: Call YourClass.YourMethod

            // Assert
            // TODO: Verify expected behavior
            Assert.True(true, "Test not yet implemented");
        }
    }
}
```

## Integration with CI/CD

The generated tests integrate with:
- **xUnit** for test execution
- **Coverlet** for code coverage collection
- **ReportGenerator** for coverage visualization
- **GitHub Actions** for automated testing

## Next Steps After Generation

1. Review generated test files
2. Implement the TODO sections with actual test logic
3. Add test data and mocks as needed
4. Run `dotnet test` to execute tests
5. Use `dotnet test --collect:"XPlat Code Coverage"` to measure coverage

## Notes

- Generated tests are scaffolding only and require manual implementation
- The tool focuses on public methods as they represent the API surface
- Private methods are tested indirectly through public methods
- Consider using dependency injection to make testing easier
