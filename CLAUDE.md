# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Traceable is a .NET Standard 2.0 library that enables traceable computations where every value maintains its origin and computation history. The library wraps primitive values in traceable entities that support natural arithmetic syntax while automatically tracking dependencies and generating visual dependency graphs.

## Project Structure

- `Traceable/` - Main library project (.NET Standard 2.0)
  - `ITraceable.cs` - Core interfaces (`ITraceableBase`, `ITraceable<T>`)
  - `Traceable.cs` - Main implementation with operator overloading
  - `ITraceableOperators.cs` - Operator interfaces for custom type support
  - `TraceableExtensions.cs` - Transform and AsScope extension methods
  - `TraceableOperations.cs` - Type dispatch helpers (Add, Subtract, etc.)
  - `TraceableScope.cs` - Ambient scope context for conditional dependencies
  - `TraceableGraphRenderer.cs` - ASCII tree rendering
  - `GraphNode.cs` - Data structure for dependency graph representation
- `Traceable.Tests/` - Test project (.NET 10.0)
  - Uses xunit for testing
  - `ExampleCustomTypes.cs` - Example custom types (Vector2D, Money, TriState)
  - `CustomTypeTests.cs` - Tests for custom type support

## Development Commands

### Build
```bash
dotnet build Traceable/Traceable.csproj
dotnet build Traceable.Tests/Traceable.Tests.csproj
```

### Run All Tests
```bash
dotnet test Traceable.Tests/Traceable.Tests.csproj
```

### Run Tests with Coverage
```bash
dotnet test Traceable.Tests/Traceable.Tests.csproj --collect:"XPlat Code Coverage"
```

### Run Specific Test
```bash
dotnet test Traceable.Tests/Traceable.Tests.csproj --filter "FullyQualifiedName~TestMethodName"
```

### Clean Build Artifacts
```bash
dotnet clean Traceable/Traceable.csproj
dotnet clean Traceable.Tests/Traceable.Tests.csproj
```

### Restore NuGet Packages
```bash
dotnet restore
```

## Architecture

### Core Design Pattern

The library uses **operator overloading** combined with the **composite pattern** to create a tree of traceable entities. Each operation creates a new `Traceable<T>` instance that maintains references to its operands, forming a computation graph.

### Key Concepts

1. **Two-Level Interface Hierarchy**:
   - `ITraceableBase`: Non-generic base interface for cross-type operations (enables comparison operators returning `Traceable<bool>`)
   - `ITraceable<T>`: Generic interface with type-specific operations

2. **Base vs Composite Entities**: The `Traceable<T>` class uses a discriminator (`_isBase`) to distinguish:
   - **Base entities**: Created via public constructor, hold a mutable value
   - **Composite entities**: Created via operators/transforms, hold operand references and a compute function

3. **Lazy Evaluation**: Values are computed on-demand via `Resolve()`, not at construction time. This allows efficient propagation when base values change via `Reload()`.

4. **Dependency Tracking**: Each composite entity maintains `ITraceableBase[]` operand references, enabling:
   - Dependency expression generation (e.g., "A + B - C") with proper operator precedence
   - Tree-structured graph visualization via `BuildGraph()` and `PrintConsole()`
   - Base entity enumeration via `GetDependencyNames()`

5. **Transform Extensions** (`TraceableExtensions.cs`): Enable type conversions and custom operations:
   - Single-input: `source.Transform("Round", x => Math.Round(x))`
   - Multi-input: `Transform(a, b, "Sum", (x, y) => x + y)`

6. **State Dictionaries**: Entities can carry additional metadata:
   - `ArbitraryState`: `IReadOnlyDictionary<string, object>` for any metadata
   - `ValueState`: `IReadOnlyDictionary<string, T>` for type-safe state

### Type System Constraints

The implementation uses C# generics with runtime type checking to enable type-specific operators. The library supports two categories of types:

#### Built-in Primitive Types
- Numeric types (int, decimal, double): Arithmetic operators (+, -, *, /)
- Boolean type: Logical operators (& for AND, | for OR)
- String type: Concatenation (+)
- All comparable types: Comparison operators (<, >, <=, >=) via `IComparable`
- All types: Equality operators (==, !=)

#### Custom Types via Operator Interfaces
Custom types can implement optional operator interfaces to enable specific operations:

**Arithmetic Interfaces** (in `ITraceableOperators.cs`):
- `ITraceableAddable<T>` - Enables `+` operator
- `ITraceableSubtractable<T>` - Enables `-` operator
- `ITraceableMultiplicable<T>` - Enables `*` operator
- `ITraceableDividable<T>` - Enables `/` operator
- `ITraceableArithmetic<T>` - Combines all arithmetic operations

**Logical Interface**:
- `ITraceableLogical<T>` - Enables `&` and `|` operators

**Comparison**:
- `System.IComparable<T>` - Enables `>`, `<`, `>=`, `<=` operators (standard .NET)

### Operator Implementation Pattern

The `BinaryOp` helper centralizes operator dispatch:
1. **Null validation** - Throws `ArgumentNullException` if operand is null
2. **Support check** - Verifies type supports the operation via `IsXxxSupported()` methods
3. **Create composite** - Returns new `Traceable<T>` with operand references and compute function

The arithmetic helpers (`Add`, `Subtract`, etc.) follow this priority:
1. **Built-in primitive check** - Fast path using `typeof(T) == typeof(int)` etc.
2. **Interface dispatch** - Uses `left is ITraceableAddable<T>` pattern for custom types
3. **Throw exception** - Helpful error message mentioning both primitives and interfaces

## Testing

Tests use xunit. When adding new operators or types:
1. Test basic operations and return values
2. Test `Dependencies` string generation
3. Test `BuildGraph()` structure
4. Verify `Reload()` propagates changes correctly

## Target Framework

- **Library**: .NET Standard 2.0 (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)
- **Tests**: .NET 10.0
