# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

EntityTrace is a .NET Standard 2.0 library that enables traceable computations where every value maintains its origin and computation history. The library wraps primitive values in traceable entities that support natural arithmetic syntax while automatically tracking dependencies and generating visual dependency graphs.

## Project Structure

- `EntityTrace/` - Main library project (.NET Standard 2.0)
  - `ITraceable.cs` - Core interface defining traceable entity contract
  - `Traceable.cs` - Main implementation with operator overloading
  - `ITraceableOperators.cs` - Operator interfaces for custom type support
- `EntityTrace.Tests/` - Test project (.NET 10.0)
  - Uses xunit, FluentAssertions, and FsCheck for property-based testing
  - `ExampleCustomTypes.cs` - Example custom types (Vector2D, Money, TriState)
  - `CustomTypeTests.cs` - Tests for custom type support

## Development Commands

### Build
```bash
dotnet build EntityTrace/EntityTrace.csproj
dotnet build EntityTrace.Tests/EntityTrace.Tests.csproj
```

### Run All Tests
```bash
dotnet test EntityTrace.Tests/EntityTrace.Tests.csproj
```

### Run Tests with Coverage
```bash
dotnet test EntityTrace.Tests/EntityTrace.Tests.csproj --collect:"XPlat Code Coverage"
```

### Run Specific Test
```bash
dotnet test EntityTrace.Tests/EntityTrace.Tests.csproj --filter "FullyQualifiedName~TestMethodName"
```

### Clean Build Artifacts
```bash
dotnet clean EntityTrace/EntityTrace.csproj
dotnet clean EntityTrace.Tests/EntityTrace.Tests.csproj
```

### Restore NuGet Packages
```bash
dotnet restore
```

## Architecture

### Core Design Pattern

The library uses **operator overloading** combined with the **composite pattern** to create a tree of traceable entities. Each operation creates a new `Traceable<T>` instance that maintains references to its operands, forming a computation graph.

### Key Concepts

1. **ITraceable<T> Interface**: Defines the contract for all traceable entities
   - `Name`: Identifier for the entity
   - `Description`: Optional human-readable description
   - `Value`: The underlying wrapped value
   - `Dependencies`: String representation of the computation expression
   - `Graph`: Tree-structured visualization of dependencies
   - `Resolve()`: Computes the final value
   - `GetDependencyNames()`: Returns all base entity names in the computation
   - `Reset(T)`: Updates base entity values and propagates changes

2. **Traceable<T> Class**: Generic implementation with type-specific operator overloads
   - Supports numeric types (int, decimal, double) with arithmetic and comparison operators
   - Supports boolean type with logical operators (& for AND, | for OR)
   - Supports string type with concatenation
   - Each operation creates a new composite entity that lazily evaluates its result

3. **Lazy Evaluation**: Values are computed on-demand via `Resolve()`, not at construction time. This allows efficient propagation when base values change.

4. **Dependency Tracking**: Each composite entity maintains references to its operands, enabling:
   - Dependency expression generation (e.g., "A + B - C")
   - Dependency graph visualization as tree structure
   - Base entity enumeration via `GetDependencyNames()`

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

### Custom Type Implementation Pattern

The dispatch pattern for operators follows this priority:
1. **Null validation** - Throws `ArgumentNullException` if operand is null
2. **Built-in primitive check** - Fast path using `typeof(T) == typeof(int)` etc.
3. **Interface implementation check** - Uses `left is ITraceableAddable<T>` pattern
4. **Throw exception** - Helpful error message mentioning both primitives and interfaces

Example from `Add()` method:
```csharp
private static T Add(T left, T right)
{
    // Fast path for primitives
    if (typeof(T) == typeof(int)) { /* boxing/unboxing */ }
    if (typeof(T) == typeof(decimal)) { /* boxing/unboxing */ }
    // ... other primitives

    // Interface dispatch for custom types
    if (left is ITraceableAddable<T> addable)
    {
        return addable.Add(left, right);
    }

    // Helpful error message
    throw new InvalidOperationException(
        $"ADD operation not supported for type {typeof(T).Name}. " +
        $"Type must be int, decimal, double, string, or implement ITraceableAddable<{typeof(T).Name}>");
}
```

This design ensures:
- **Performance**: Primitives use fast path without reflection
- **Extensibility**: Custom types work via interface implementation
- **Flexibility**: Types implement only needed operations (Interface Segregation Principle)
- **Backward Compatibility**: All existing code works unchanged

## Testing Strategy

Tests use:
- **xunit** as the test framework with standard assertions

When adding new operators or types:
1. Write unit tests for basic operations
2. Test dependency tracking and graph generation
3. Verify `Reset()` propagates changes correctly

## Target Framework

The library targets **.NET Standard 2.0** for maximum compatibility across:
- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5.0+
- Mono, Xamarin, and other .NET implementations

The test project uses **.NET 10.0** (latest version) but this can be adjusted for compatibility testing.
